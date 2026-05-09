using EroJRPG.UI.Primitives;
using Godot;
using System;

public partial class MenuOptions : Control
{
    private Cursor ThisCursor;
    private Label ThisLabel;

    public override void _Ready()
	{

		ThisCursor = GetNode<Cursor>("%Cursor");
        ThisLabel = GetNode<Label>("%Label");
        ThisCursor.MoveCursor(ThisLabel);
		
	}
}
