using Godot;

namespace QWar;

/// <summary>
/// 布阵阶段覆盖在棋盘上的透明层：负责从己方格拖拽棋将（拖拽源）以及接收拖放到棋盘（放置目标），
/// 避免依赖棋将节点接收鼠标，保证布阵时一定能拖动、可拖入背包。
/// </summary>
public partial class DeploymentDragLayer : Control
{
	private readonly FightBoard _board;

	public DeploymentDragLayer(FightBoard board)
	{
		_board = board;
	}

	/// <summary>由 FightBoard 在进入布阵时调用，构建层与 10 个己方格拖拽源</summary>
	public void Build()
	{
		Position = Vector2.Zero;
		Size = new Vector2(_board.BoardCols * _board.CellSizePx, _board.BoardRows * _board.CellSizePx);
		CustomMinimumSize = Size;
		MouseFilter = MouseFilterEnum.Stop;
		ZIndex = 20;

		for (int r = 0; r < _board.BoardRows; r++)
		for (int c = 0; c < 2; c++)
		{
			var cell = new DeploymentCellControl(_board, r, c);
			cell.Position = _board.GetCellPosition(r, c);
			cell.Size = new Vector2(_board.CellSizePx, _board.CellSizePx);
			cell.CustomMinimumSize = cell.Size;
			cell.MouseFilter = MouseFilterEnum.Stop;
			AddChild(cell);
		}
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

/// <summary>
/// 布阵区单格：作为该格的拖拽源，返回该格上的棋将（若有）供拖到其他格或背包。
/// </summary>
internal partial class DeploymentCellControl : Control
{
	private readonly FightBoard _board;
	private readonly int _row;
	private readonly int _col;

	public DeploymentCellControl(FightBoard board, int row, int col)
	{
		_board = board;
		_row = row;
		_col = col;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (!_board.IsDeploymentPhase) return default;
		var unit = _board.GetUnitAt(_row, _col);
		if (unit is ChessHero hero)
			return Variant.CreateFrom(hero);
		return default;
	}
}
