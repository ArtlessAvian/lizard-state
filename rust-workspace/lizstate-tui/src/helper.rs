use ratatui::buffer::Cell;
use ratatui::prelude::*;

pub fn world_to_screen(area: &Rect, worldspace: (i32, i32)) -> Option<Position> {
    let center: Position = (
        (area.left() + area.right()) / 2,
        (area.top() + area.bottom()) / 2,
    )
        .into();

    let screen: Position = (
        (worldspace.0 + center.x as i32) as u16,
        (worldspace.1 + center.y as i32) as u16,
    )
        .into();

    if area.contains(screen) {
        Some(screen)
    } else {
        None
    }
}

pub fn screen_to_world(area: &Rect, screen: Position) -> (i32, i32) {
    let center: Position = (
        (area.left() + area.right()) / 2,
        (area.top() + area.bottom()) / 2,
    )
        .into();

    (
        screen.x as i32 - center.x as i32,
        screen.y as i32 - center.y as i32,
    )
}

pub fn world_to_cell<'a>(
    area: &Rect,
    buf: &'a mut Buffer,
    worldspace: (i32, i32),
) -> Option<&'a mut Cell> {
    world_to_screen(area, worldspace).and_then(|screen| buf.cell_mut(screen))
}

pub fn zip_cells_and_worldspace<'a>(
    area: &Rect,
    buf: &'a mut Buffer,
) -> impl Iterator<Item = (&'a mut Cell, (i32, i32))> {
    // OPT: Use array slices, or whatever
    buf.content
        .chunks_exact_mut(buf.area.width.into())
        .enumerate()
        .skip(area.y.into())
        .take(area.height.into())
        .flat_map(move |(screen_y, line)| {
            line.iter_mut()
                .enumerate()
                .skip(area.x.into())
                .take(area.width.into())
                .map(move |(screen_x, cell)| {
                    (
                        cell,
                        screen_to_world(area, Position::new(screen_x as u16, screen_y as u16)),
                    )
                })
        })
}
