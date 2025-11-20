use crate::creature::CreatureState;
use crate::floor::Floor;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::KingStep;

#[derive(Debug)]
#[non_exhaustive]
pub enum CommandError {
    InTheWay(u8),
    TargetIdDoesntExist(u8),
    MacroFallthrough,
    InvalidTurntakerState(CreatureState),
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
    fn do_infallible(turntaker: &Turntaker) -> Floor {
        let mut mut_floor = turntaker.get_floor().clone();
        let me = mut_floor
            .get_creature_list_mut()
            .get_creature_mut_or_insert(turntaker.get_id(), turntaker.get_creature());

        // We couldn't become safe, so we just wait.
        if let Some(round) = me.get_state_mut().get_round_mut() {
            *round += 1;
        }
        mut_floor
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
        turntaker.map_independent(|creature, floor| {
            let mut clone = creature.clone();
            clone.step(self.0);

            let position = clone.get_position();
            if let Some((id, _)) = floor
                .get_creature_list()
                .iter_indices_nonempty()
                .find(|x| x.1.get_position() == position)
            {
                return Err(CommandError::InTheWay(id));
            }

            if matches!(clone.get_state(), CreatureState::Safe { .. }) {
                *clone.get_state_mut() = CreatureState::Safe {
                    round: turntaker
                        .get_now()
                        .skip_rounds(1)
                        .coming_round_for(turntaker.get_id()),
                }
            } else {
                return Err(CommandError::InvalidTurntakerState(
                    clone.get_state().clone(),
                ));
            }

            Ok(clone)
        })
    }
}

/// Swaps position, even at a distance.
/// Consumes YOUR turn, but not the swapped creature's.
#[derive(Debug)]
pub struct TagOutCommand(pub u8);

impl CommandTrait for TagOutCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        let mut mut_floor = turntaker.get_floor().clone();

        let target = mut_floor
            .get_creature_list_mut()
            .get_creature_mut(self.0)
            .ok_or(CommandError::TargetIdDoesntExist(self.0))?;

        let position = target.get_position();
        target.set_position(turntaker.get_creature().get_position());

        let me = mut_floor
            .get_creature_list_mut()
            .get_creature_mut_or_insert(turntaker.get_id(), turntaker.get_creature());
        me.set_position(position);

        if let CreatureState::Safe { .. } = me.get_state() {
            *me.get_state_mut() = CreatureState::Safe {
                round: turntaker
                    .get_now()
                    .skip_rounds(1)
                    .coming_round_for(turntaker.get_id()),
            };
        } else {
            return Err(CommandError::InvalidTurntakerState(me.get_state().clone()));
        }

        Ok(mut_floor)
    }
}

#[derive(Debug)]
pub struct StepMacro(pub Option<KingStep>);

impl CommandTrait for StepMacro {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        if let Some(step) = self.0 {
            let step = StepCommand(step).do_command(turntaker);

            if let Ok(floor) = step {
                return Ok(floor);
            }

            if let Err(CommandError::InTheWay(who)) = step {
                let tagout = TagOutCommand(who).do_command(turntaker);
                if let Ok(floor) = tagout {
                    return Ok(floor);
                }
            }

            Err(CommandError::MacroFallthrough)
        } else {
            WaitCommand.do_command(turntaker)
        }
    }
}

#[derive(Debug)]
pub struct WakeupCommand;

impl CommandTrait for WakeupCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        turntaker.map_independent(|me, floor| {
            if matches!(me.get_state(), CreatureState::Knockdown { .. }) {
                if let Some((id, _)) = floor
                    .get_creature_list()
                    .iter_indices_nonempty()
                    .filter_map(|(id, creature)| {
                        creature.get_occupied_position().map(|pos| (id, pos))
                    })
                    .find(|(_, pos)| *pos == me.get_position())
                {
                    Err(CommandError::InTheWay(id))
                } else {
                    let mut clone = me.clone();
                    *clone.get_state_mut() = CreatureState::Safe {
                        round: turntaker.get_now().coming_round_for(turntaker.get_id()),
                    };
                    Ok(clone)
                }
            } else {
                Err(CommandError::InvalidTurntakerState(me.get_state().clone()))
            }
        })
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
        if turntaker.get_creature().get_occupied_position().is_some() {
            let mut mut_floor = turntaker.get_floor().clone();
            let clone = mut_floor
                .get_creature_list_mut()
                .get_creature_mut_or_insert(turntaker.get_id(), turntaker.get_creature());
            *clone.get_state_mut() = CreatureState::Safe {
                round: turntaker.get_now().coming_round_for(turntaker.get_id()),
            };
            Ok(mut_floor)
        } else {
            Err(CommandError::InvalidTurntakerState(
                turntaker.get_creature().get_state().clone(),
            ))
        }
    }
}
