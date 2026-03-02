using Godot;

namespace QWar;

/// <summary>
/// 置于 FightBoard 最上层的点击/拖放层：接收格子点击并转发给棋盘，布阵阶段接收从棋将包拖入的放置。
/// </summary>
public partial class BoardDropZone : Control
{
	private readonly FightBoard _board;

	public BoardDropZone(FightBoard board)
	{
		_board = board;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return _board.CanDropDataAt(atPosition, data);
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		_board.DropDataAt(atPosition, data);
	}
}
