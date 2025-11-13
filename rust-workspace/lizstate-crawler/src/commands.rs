use crate::floor::Floor;
use crate::floor::Turntaker;
use crate::spatial::grid::KingStep;

#[non_exhaustive]
pub enum CommandError {}

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
        turntaker.map_independent(|creature, _floor| {
            let mut clone = creature.clone();
            clone.step(self.0);
            Ok(clone)
        })
    }
}
