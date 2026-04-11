using Godot;
using System;

namespace EroJRPG.UI.Primitives;
public partial class Cursor : TextureRect
{
	public void MoveCursor(Control someNode)
	{
        Vector2 new_cursor_position = someNode.GlobalPosition;
		new_cursor_position.X -= Size.X + 5;
        new_cursor_position.Y -= (Size.Y / 2) - (someNode.Size.Y / 1.5f);
		GlobalPosition = new_cursor_position;
		Visible = true;
	}
}
