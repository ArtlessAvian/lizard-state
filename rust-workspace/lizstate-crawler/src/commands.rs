use crate::creature::CreatureState;
use crate::floor::Floor;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::GridLike;
use crate::spatial::grid::KingStep;

#[derive(Debug)]
#[non_exhaustive]
pub enum CommandError {
    InTheWay(u8),
    TargetIdDoesntExist(u8),
    MacroFallthrough,
    InvalidTurntakerState(CreatureState),
    NoTarget,
    TargetNotFriendly,
}

pub trait CommandTrait {
    /// # Errors
    /// Any reason for the Command to be impossible.
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError>;

    fn do_or_wait(&self, turntaker: &Turntaker) -> Floor {
        self.do_command(turntaker)
            .unwrap_or_else(|_| WaitCommand::do_infallible(turntaker))
    }
}

#[derive(Debug)]
#[must_use]
pub enum Command {
    Wait(WaitCommand),
    Step(StepCommand),
    TagOut(TagOutCommand),
    Wakeup(WakeupCommand),
    ExitState(ExitStateCommand),
}

impl CommandTrait for Command {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        match self {
            Command::Wait(x) => x.do_command(turntaker),
            Command::Step(x) => x.do_command(turntaker),
            Command::TagOut(x) => x.do_command(turntaker),
            Command::Wakeup(x) => x.do_command(turntaker),
            Command::ExitState(x) => x.do_command(turntaker),
        }
    }
}

/// Wait is a command that explicitly ALWAYS works.
///
/// Without any knowledge of how the game works, increments the creature's next turn.
#[derive(Debug)]
pub struct WaitCommand;

impl WaitCommand {
    fn get_round_mut(state: &mut CreatureState) -> Option<&mut u32> {
        match state {
            CreatureState::Safe { round }
            | CreatureState::Hitstun { round }
            | CreatureState::Knockdown { round }
            | CreatureState::Committed { round }
            | CreatureState::Cancelable { round, .. }
            | CreatureState::Stance { round }
            | CreatureState::Punishable { round } => Some(round),
            CreatureState::Downed {} => None,
        }
    }

    fn do_infallible(turntaker: &Turntaker) -> Floor {
        turntaker.clone_and_modify_creature(|me| {
            if let Some(round) = WaitCommand::get_round_mut(me.get_state_mut()) {
                *round += 1;
            }
        })
    }
}

impl CommandTrait for WaitCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        Ok(WaitCommand::do_infallible(turntaker))
    }
}

#[derive(Debug)]
pub struct StepCommand(pub KingStep);

impl CommandTrait for StepCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        let CreatureState::Safe { .. } = turntaker.get_creature().get_state() else {
            return Err(CommandError::InvalidTurntakerState(
                turntaker.get_creature().get_state().clone(),
            ));
        };

        // TODO: Move to validation step?
        let pos = turntaker.get_creature().get_position().step_king(self.0);
        if let Some((id, _)) = turntaker
            .get_floor()
            .get_creature_list()
            .iter()
            .find(|x| x.1.get_occupied_position() == Some(pos))
        {
            return Err(CommandError::InTheWay(id));
        }

        Ok(turntaker.clone_and_modify_creature(|clone| {
            clone.step(self.0);
            *clone.get_state_mut() = CreatureState::Safe {
                round: turntaker
                    .get_now()
                    .skip_rounds(1)
                    .coming_round_for(turntaker.get_id()),
            };
        }))
    }
}

/// Swaps position, even at a distance.
/// Consumes YOUR turn, but not the swapped creature's.
#[derive(Debug)]
pub struct TagOutCommand(pub u8);

impl CommandTrait for TagOutCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        let CreatureState::Safe { .. } = turntaker.get_creature().get_state() else {
            return Err(CommandError::InvalidTurntakerState(
                turntaker.get_creature().get_state().clone(),
            ));
        };

        turntaker.clone_and_try_modify_everyone(|myself, mut everyone_else| {
            let Some(Some(target)) = everyone_else.get_mut(self.0 as usize) else {
                return Err(CommandError::TargetIdDoesntExist(self.0));
            };

            if target.get_team() != turntaker.get_creature().get_team() {
                return Err(CommandError::TargetNotFriendly);
            }

            let target_position = target.get_position();
            target.set_position(turntaker.get_creature().get_position());

            myself.set_position(target_position);
            *myself.get_state_mut() = CreatureState::Safe {
                round: turntaker
                    .get_now()
                    .skip_rounds(1)
                    .coming_round_for(turntaker.get_id()),
            };

            Ok(())
        })
    }
}

#[derive(Debug)]
pub struct StepMacro(pub Option<KingStep>);

impl CommandTrait for StepMacro {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        let Some(step) = self.0 else {
            return WaitCommand.do_command(turntaker);
        };

        let step_result = StepCommand(step).do_command(turntaker);
        let Err(err) = step_result else {
            return step_result;
        };

        let CommandError::InTheWay(who) = err else {
            return Err(CommandError::MacroFallthrough);
        };

        let tag_result = TagOutCommand(who).do_command(turntaker);
        let Err(err) = tag_result else {
            return tag_result;
        };

        let CommandError::TargetNotFriendly = err else {
            return Err(CommandError::MacroFallthrough);
        };

        let bump_command = BumpAttackCommand(step).do_command(turntaker);
        let Err(_) = bump_command else {
            return bump_command;
        };

        Err(CommandError::MacroFallthrough)
    }
}

#[derive(Debug)]
pub struct WakeupCommand;

impl CommandTrait for WakeupCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        let CreatureState::Knockdown { .. } = turntaker.get_creature().get_state() else {
            return Err(CommandError::InvalidTurntakerState(
                turntaker.get_creature().get_state().clone(),
            ));
        };

        if let Some((id, _)) = turntaker
            .get_floor()
            .get_creature_list()
            .iter()
            .filter_map(|(id, creature)| creature.get_occupied_position().map(|pos| (id, pos)))
            .find(|(_, pos)| *pos == turntaker.get_creature().get_position())
        {
            return Err(CommandError::InTheWay(id));
        }

        Ok(turntaker.clone_and_modify_creature(|clone| {
            *clone.get_state_mut() = CreatureState::Safe {
                round: turntaker.get_now().coming_round_for(turntaker.get_id()),
            };
        }))
    }
}

#[non_exhaustive]
#[derive(Debug)]
pub struct ExitStateCommand();

impl ExitStateCommand {
    pub(crate) const fn new_pub_crate() -> Self {
        ExitStateCommand()
    }
}

impl CommandTrait for ExitStateCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        if turntaker.get_creature().get_occupied_position().is_none() {
            return Err(CommandError::InvalidTurntakerState(
                turntaker.get_creature().get_state().clone(),
            ));
        }

        Ok(turntaker.clone_and_modify_creature(|clone| {
            *clone.get_state_mut() = CreatureState::Safe {
                round: turntaker.get_now().coming_round_for(turntaker.get_id()),
            };
        }))
    }
}

pub struct BumpAttackCommand(KingStep);

impl CommandTrait for BumpAttackCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        turntaker.clone_and_try_modify_everyone(|myself, mut everyone_else| {
            let target_pos = myself.get_position().step_king(self.0);

            let Some((target_id, target)) = (0..=255u8)
                .zip(everyone_else.iter_mut())
                .filter_map(|(i, opt)| opt.as_mut().map(|x| (i, x)))
                .find(|(_, x)| x.get_occupied_position() == Some(target_pos))
            else {
                return Err(CommandError::NoTarget);
            };

            *target.get_state_mut() = CreatureState::Knockdown {
                round: turntaker
                    .get_now()
                    .skip_rounds(2)
                    .coming_round_for(target_id),
            };
            target.step(self.0);

            *myself.get_state_mut() = CreatureState::Safe {
                round: turntaker
                    .get_now()
                    .skip_rounds(1)
                    .coming_round_for(turntaker.get_id()),
            };

            Ok(())
        })
    }
}
