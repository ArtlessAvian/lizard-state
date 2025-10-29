use ratatui::buffer::Cell;
use ratatui::layout::Offset;
use ratatui::prelude::*;

pub struct Camera<'a> {
    pub look_at: (i32, i32),
    pub render_target: Rect,
    pub buffer: &'a mut Buffer,
}

impl Camera<'_> {
    fn offset_look_at_to_center(&self) -> Offset {
        Offset {
            x: self.render_target.x as i32 + (self.render_target.width as i32 - 1) / 2
                - self.look_at.0,
            y: self.render_target.y as i32 + (self.render_target.height as i32 - 1) / 2
                - self.look_at.1,
        }
    }

    // fn screen_to_world(&self, screen: Position) -> (i32, i32) {
    //     let undo_me = self.offset_look_at_to_center();
    //     (screen.x as i32 - undo_me.x, screen.y as i32 - undo_me.y)
    // }

    fn world_to_screen(&self, world: (i32, i32)) -> Option<Position> {
        let offset = self.offset_look_at_to_center();

        let screen = (world.0 + offset.x, world.1 + offset.y);
        let screen = Position::new(screen.0 as u16, screen.1 as u16);

        if self.render_target.contains(screen) {
            Some(screen)
        } else {
            None
        }
    }

    pub fn cell_mut(&mut self, world: (i32, i32)) -> Option<&mut Cell> {
        self.world_to_screen(world)
            .and_then(|screen| self.buffer.cell_mut(screen))
    }

    // pub fn iter_screen(&self) -> impl Iterator<Item = Position> + use<> {
    //     let left = self.render_target.left();
    //     let right = self.render_target.right();

    //     let top = self.render_target.top();
    //     let bottom = self.render_target.bottom();

    //     (top..bottom).flat_map(move |y| (left..right).map(move |x| Position::new(x, y)))
    // }

    pub fn iter_world(&self) -> impl Iterator<Item = (i32, i32)> + use<> {
        let left: i32 = self.look_at.0 - (self.render_target.width as i32 - 1) / 2;
        let right = left + self.render_target.width as i32;

        let top: i32 = self.look_at.1 - (self.render_target.height as i32 - 1) / 2;
        let bottom = top + self.render_target.height as i32;

        (top..bottom).flat_map(move |y| (left..right).map(move |x| (x, y)))
    }

    pub fn iter_mut_cell(&mut self) -> impl Iterator<Item = &mut Cell> {
        let all = self
            .buffer
            .content
            .chunks_exact_mut(self.buffer.area.width.into());

        let lines = all
            .skip(self.render_target.y as usize)
            .take(self.render_target.height as usize);

        lines.flat_map(|line| {
            line.iter_mut()
                .skip(self.render_target.x as usize)
                .take(self.render_target.width as usize)
        })
    }

    pub fn iter_mut_cell_and_world(&mut self) -> impl Iterator<Item = (&mut Cell, (i32, i32))> {
        let iter_world = self.iter_world();
        let iter_mut_cell = self.iter_mut_cell();
        iter_mut_cell.zip(iter_world)
    }
}

#[cfg(test)]
mod tests {
    use ratatui::buffer::Buffer;
    use ratatui::layout::Position;
    use ratatui::layout::Rect;

    use crate::camera::Camera;

    #[test]
    fn rect_center_is_look_at() {
        let render_target = Rect::new(2, 3, 5, 5);
        let mut buffer = Buffer::empty(Rect::new(0, 0, 10, 10));

        let camera = Camera {
            look_at: (4, 5),
            render_target,
            buffer: &mut buffer,
        };

        assert_eq!(
            camera.world_to_screen((4, 5)),
            Some(Position::new(2 + 2, 3 + 2))
        );

        let render_target = Rect::new(2, 3, 7, 7);
        let mut buffer = Buffer::empty(Rect::new(0, 0, 10, 10));

        let camera = Camera {
            look_at: (4, 5),
            render_target,
            buffer: &mut buffer,
        };

        assert_eq!(
            camera.world_to_screen((4, 5)),
            Some(Position::new(2 + 3, 3 + 3))
        );
    }

    #[test]
    fn rect_center_bias_top_left() {
        let render_target = Rect::new(2, 3, 6, 6);
        let mut buffer = Buffer::empty(Rect::new(0, 0, 10, 10));

        let camera = Camera {
            look_at: (4, 5),
            render_target,
            buffer: &mut buffer,
        };

        assert_eq!(
            camera.world_to_screen((4, 5)),
            Some(Position::new(2 + 2, 3 + 2))
        );
    }

    #[test]
    fn cell_mut() {
        let render_target = Rect::new(2, 3, 6, 6);
        let mut buffer = Buffer::empty(Rect::new(0, 0, 10, 10));

        let mut camera = Camera {
            look_at: (4, 5),
            render_target,
            buffer: &mut buffer,
        };

        if let Some(cell) = camera.cell_mut((4, 5)) {
            cell.set_char('@');
        }

        if let Some(cell) = camera.cell_mut((6, 7)) {
            cell.set_char('k');
        }

        let expected = Buffer::with_lines(vec![
            "          ",
            "          ",
            "          ",
            "          ",
            "          ",
            "    @     ",
            "          ",
            "      k   ",
            "          ",
            "          ",
        ]);
        assert_eq!(buffer, expected);
    }

    #[test]
    fn iter_world() {
        let render_target = Rect::new(2, 3, 6, 6);
        let mut buffer = Buffer::empty(Rect::new(0, 0, 10, 10));

        let camera = Camera {
            look_at: (4, 5),
            render_target,
            buffer: &mut buffer,
        };

        for world in camera.iter_world() {
            assert!(2 <= world.0);
            assert!(world.0 < 8);
            assert!(3 <= world.1);
            assert!(world.1 < 9);
        }

        for world in camera.iter_world() {
            assert!(
                camera
                    .render_target
                    .contains(camera.world_to_screen(world).unwrap())
            );
        }
    }

    #[test]
    fn iter_mut_cell() {
        let render_target = Rect::new(2, 3, 6, 6);
        let mut buffer = Buffer::empty(Rect::new(0, 0, 10, 10));

        let mut camera = Camera {
            look_at: (4, 5),
            render_target,
            buffer: &mut buffer,
        };

        for cell in camera.iter_mut_cell() {
            cell.set_char('.');
        }

        let expected = Buffer::with_lines(vec![
            "          ",
            "          ",
            "          ",
            "  ......  ",
            "  ......  ",
            "  ......  ",
            "  ......  ",
            "  ......  ",
            "  ......  ",
            "          ",
        ]);
        assert_eq!(buffer, expected);
    }
}
