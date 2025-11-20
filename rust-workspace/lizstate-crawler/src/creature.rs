use crate::commands::Command;
use crate::commands::ExitStateCommand;
use crate::commands::WakeupCommand;
use crate::entity::Entity;
use crate::entity::get_six_bit_color;
use crate::spatial::grid::GridLike;
use crate::spatial::grid::GridPosition;
use crate::spatial::grid::KingStep;

#[derive(Debug, Clone)]
#[non_exhaustive]
#[must_use]
pub struct Creature<Pos: GridLike = GridPosition> {
    pos: Pos,
    state: CreatureState,
    color: u32,
    team: u8,
}

impl<Pos: GridLike> Creature<Pos> {
    pub fn new(pos: Pos, first_round: u32, color: u32, team: u8) -> Self {
        Self {
            pos,
            state: CreatureState::Safe { round: first_round },
            color,
            team,
        }
    }

    /// Not a default!
    pub fn new_garbage() -> Self {
        const ANNOYING_MAGENTA: u32 = 0xFF00_FFFF;
        Self::new(Pos::origin(), u32::MAX, ANNOYING_MAGENTA, u8::MAX)
    }

    pub fn step(&mut self, dir: KingStep) {
        self.pos = self.pos.step_king(dir);
    }

    pub fn get_position(&self) -> Pos {
        self.pos
    }

    pub fn set_position(&mut self, pos: Pos) {
        self.pos = pos;
    }

    pub fn get_team(&self) -> impl Eq {
        self.team
    }

    pub fn get_occupied_position(&self) -> Option<Pos> {
        self.state.occupies_space().then_some(self.pos)
    }

    pub fn get_round(&self) -> Option<u32> {
        self.state.get_round()
    }

    pub fn get_state(&self) -> &CreatureState {
        &self.state
    }

    pub fn get_state_mut(&mut self) -> &mut CreatureState {
        &mut self.state
    }

    /// Instantly become safe (if not downed), without affecting creature's next turn.
    pub fn become_safe(&mut self) {
        if let Some(round) = self.get_round() {
            self.state = CreatureState::Safe { round }
        }
    }

    /// Instantly become downed.
    pub fn become_downed(&mut self) {
        self.state = CreatureState::Downed {}
    }
}

impl<Pos: GridLike> Entity for &Creature<Pos> {
    fn get_char(&self) -> char {
        '@'
    }

    fn get_fg_color(&self) -> u8 {
        const REMAP: u8 = 43; // ceil(256/6);

        let yeah = self.color.to_be_bytes();
        get_six_bit_color(yeah[0] / REMAP, yeah[1] / REMAP, yeah[2] / REMAP)
    }

    fn get_flat_position(&self) -> (i32, i32) {
        self.pos.flatten()
    }
}

#[derive(Debug, PartialEq, Eq, Clone)]
#[must_use]
pub enum CreatureState {
    Safe { round: u32 },
    Hitstun { round: u32 },
    Knockdown { round: u32 },
    Committed { round: u32 },
    Cancelable { round: u32, activate: u32 },
    Stance { round: u32 },
    Punishable { round: u32 },
    Downed {},
}

impl CreatureState {
    /// The command this creature must successfully execute (or wait).
    #[must_use]
    pub fn forced_command(&self) -> Option<&Command> {
        match self {
            CreatureState::Knockdown { .. } => Some(&Command::Wakeup(WakeupCommand)),
            CreatureState::Hitstun { .. } | CreatureState::Punishable { .. } => {
                Some(&const { Command::ExitState(ExitStateCommand::new_pub_crate()) })
            }
            CreatureState::Committed { .. } => {
                println!("hello world!!!");
                Some(&const { Command::ExitState(ExitStateCommand::new_pub_crate()) })
            }
            CreatureState::Cancelable { round, activate } => {
                if round == activate {
                    Some(&const { Command::ExitState(ExitStateCommand::new_pub_crate()) })
                } else {
                    None
                }
            }
            CreatureState::Safe { .. }
            | CreatureState::Stance { .. }
            | CreatureState::Downed {} => None,
        }
    }

    pub fn replace_hotkey_commands(&self) -> &[Command] {
        match self {
            CreatureState::Stance { .. } => {
                &[const { Command::ExitState(ExitStateCommand::new_pub_crate()) }]
            }
            _ => &[],
        }
    }

    #[must_use]
    pub fn get_round(&self) -> Option<u32> {
        match self {
            CreatureState::Safe { round }
            | CreatureState::Hitstun { round }
            | CreatureState::Knockdown { round }
            | CreatureState::Committed { round }
            | CreatureState::Cancelable { round, .. }
            | CreatureState::Stance { round }
            | CreatureState::Punishable { round } => Some(*round),
            CreatureState::Downed {} => None,
        }
    }

    fn occupies_space(&self) -> bool {
        match self {
            CreatureState::Safe { .. }
            | CreatureState::Hitstun { .. }
            | CreatureState::Committed { .. }
            | CreatureState::Cancelable { .. }
            | CreatureState::Stance { .. }
            | CreatureState::Punishable { .. } => true,
            CreatureState::Downed {} | CreatureState::Knockdown { .. } => false,
        }
    }
}
