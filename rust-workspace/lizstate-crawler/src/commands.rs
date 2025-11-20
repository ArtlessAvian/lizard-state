use crate::floor::Floor;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::KingStep;

#[derive(Debug)]
#[non_exhaustive]
pub enum CommandError {
    InTheWay(u8),
    TargetIdDoesntExist(u8),
    MacroFallthrough,
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

/// Wait is a command that explicitly ALWAYS works.
pub struct WaitCommand;

impl WaitCommand {
    /// # Panics
    /// Turntaker is invalid.
    pub fn do_infallible(turntaker: &Turntaker) -> Floor {
        let mut mut_floor = turntaker.get_floor().clone();
        let me = mut_floor
            .get_creature_list_mut()
            .get_creature_mut_or_insert(turntaker.get_id(), turntaker.get_creature());
        me.set_round(
            turntaker
                .get_now()
                .skip_rounds(1)
                .coming_round_for(turntaker.get_id()),
        );

        mut_floor
    }
}

impl CommandTrait for WaitCommand {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        Ok(Self::do_infallible(turntaker))
    }
}

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

            clone.set_round(
                turntaker
                    .get_now()
                    .skip_rounds(1)
                    .coming_round_for(turntaker.get_id()),
            );

            Ok(clone)
        })
    }
}

/// Swaps position, even at a distance.
/// Consumes YOUR turn, but not the swapped creature's.
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
        me.set_round(
            turntaker
                .get_now()
                .skip_rounds(1)
                .coming_round_for(turntaker.get_id()),
        );

        Ok(mut_floor)
    }
}

pub struct StepMacro(pub KingStep);

impl CommandTrait for StepMacro {
    fn do_command(&self, turntaker: &Turntaker) -> Result<Floor, CommandError> {
        let step = StepCommand(self.0).do_command(turntaker);

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
    }
}
