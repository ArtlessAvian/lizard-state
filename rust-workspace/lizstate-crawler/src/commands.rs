use crate::floor::Floor;
use crate::floor::turntaker::Turntaker;
use crate::spatial::grid::KingStep;

#[non_exhaustive]
pub enum CommandError {
    InTheWay,
}

pub trait CommandTrait {
    /// # Errors
    /// Any reason for the Command to be impossible.
    fn do_command(&self, turntaker: Turntaker) -> Result<Floor, CommandError>;
}

pub trait SuggestionTrait {
    fn try_suggestion(&self, turntaker: Turntaker) -> Floor;
}

impl<T: SuggestionTrait> CommandTrait for T {
    fn do_command(&self, turntaker: Turntaker) -> Result<Floor, CommandError> {
        Ok(self.try_suggestion(turntaker))
    }
}

pub struct StepCommand(pub KingStep);

impl CommandTrait for StepCommand {
    fn do_command(&self, turntaker: Turntaker) -> Result<Floor, CommandError> {
        turntaker.map_independent(|creature, floor| {
            let mut clone = creature.clone();
            clone.step(self.0);

            let position = clone.get_position();
            if floor
                .get_creatures()
                .any(|x| x.1.get_position() == position)
            {
                return Err(CommandError::InTheWay);
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
