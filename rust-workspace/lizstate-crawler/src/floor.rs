use std::fmt::Display;
use std::fmt::Write;
use std::ops::IndexMut;
use std::rc::Rc;

use crate::creature::Creature;
use crate::entity::Entity;

pub struct Floor {
    creatures: Vec<Rc<Creature>>,
}

impl Floor {
    fn get_creatures(&self) -> impl Iterator<Item = &Creature> {
        self.creatures.iter().map(Rc::as_ref)
    }
}

impl Display for Floor {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let mut lol: [[char; 80]; 24] = [[' '; 80]; 24];

        for c in self.get_creatures() {
            let (x, y) = c.get_flat_position();
            *lol.index_mut(usize::try_from(x).unwrap())
                .index_mut(usize::try_from(y).unwrap()) = c.get_char();
        }

        for line in &lol {
            f.write_str(&line.iter().collect::<String>())?;
            f.write_char('\n')?;
        }

        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use std::rc::Rc;

    use crate::creature::Creature;
    use crate::floor::Floor;
    use crate::map::GridPos;

    #[test]
    fn yay() {
        let floor = Floor {
            creatures: vec![
                Rc::new(Creature::new(GridPos(3, 3))),
                Rc::new(Creature::new(GridPos(5, 5))),
            ],
        };

        println!("{floor}");
    }
}
