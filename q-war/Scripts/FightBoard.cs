using Godot;
using System;
using System.Collections.Generic;

namespace QWar;

/// <summary>
/// 战斗棋盘。可配置行数和列数，管理格子坐标与单位占用。
/// </summary>
public partial class FightBoard : Control
{
	[Export] public int BoardRows { get; set; } = 5;
	[Export] public int BoardCols { get; set; } = 10;
	[Export] public int CellSizePx { get; set; } = 64;

	/// <summary>当前选中等待选目标的棋将（移动/攻击）</summary>
	public ChessHero? SelectedHero { get; set; }

	/// <summary>当前是否为我方回合</summary>
	public bool IsPlayerTurn => _isPlayerTurn;
	/// <summary>本回合剩余行为次数（我方回合时有效）</summary>
	public int ActionsRemainingThisTurn => _actionsRemainingThisTurn;

	private bool _isPlayerTurn = true;
	private int _actionsRemainingThisTurn = 2;
	private readonly Dictionary<(int row, int col), Node> _gridUnits = new();
	private Control _cellsContainer = null!;
	private Control _unitsContainer = null!;
	private Control _clickLayer = null!;
	private Control _highlightContainer = null!;
	private readonly List<ColorRect> _highlightRects = new();
	private Button _cancelButton = null!;

	public override void _Ready()
	{
		_cellsContainer = GetNodeOrNull<Control>("CellsContainer") ?? this;
		_unitsContainer = GetNodeOrNull<Control>("UnitsContainer") ?? this;

		CustomMinimumSize = new Vector2(BoardCols * CellSizePx, BoardRows * CellSizePx);
		BuildCells();

		// 点击层：置于最上层以接收格子点击，尺寸与棋盘一致，不绘制任何内容
		var boardSize = new Vector2(BoardCols * CellSizePx, BoardRows * CellSizePx);
		_clickLayer = new Control
		{
			Position = Vector2.Zero,
			Size = boardSize,
			CustomMinimumSize = boardSize,
			MouseFilter = Control.MouseFilterEnum.Stop,
			ZIndex = 10
		};
		_clickLayer.GuiInput += OnClickLayerInput;

		_highlightContainer = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
		AddChild(_highlightContainer);
		AddChild(_clickLayer); // 最后添加，保证在最上层，点击由此处统一处理

		// 统一行动取消按钮：在移动/攻击/技能选目标时显示，点击后取消当前操作
		_cancelButton = new Button
		{
			Text = "取消",
			Visible = false,
			Position = new Vector2(boardSize.X - 76, 8),
			Size = new Vector2(68, 36)
		};
		_cancelButton.Pressed += OnCancelActionPressed;
		AddChild(_cancelButton);
	}

	/// <summary>显示取消按钮（进入选目标状态时调用）</summary>
	public void ShowCancelButton()
	{
		_cancelButton.Visible = true;
	}

	/// <summary>隐藏取消按钮（取消或完成操作时调用）</summary>
	public void HideCancelButton()
	{
		_cancelButton.Visible = false;
	}

	private void OnCancelActionPressed()
	{
		SelectedHero?.CancelTargetMode();
	}

	private void OnClickLayerInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
		{
			var pos = _clickLayer.GetLocalMousePosition();
			int c = (int)(pos.X / CellSizePx);
			int r = (int)(pos.Y / CellSizePx);
			if (!IsInBounds(r, c)) return;
			if (SelectedHero != null)
			{
				SelectedHero.OnCellClicked(r, c);
				return;
			}
			var unit = GetUnitAt(r, c);
			if (unit is ChessHero hero && _isPlayerTurn && _actionsRemainingThisTurn > 0)
				hero.ShowActionPopup();
		}
	}

	/// <summary>我方完成一次行为（移动/攻击/技能）后调用；剩余次数减 1，若为 0 则切换怪物回合</summary>
	public void ConsumePlayerAction()
	{
		if (!_isPlayerTurn || _actionsRemainingThisTurn <= 0) return;
		_actionsRemainingThisTurn--;
		if (_actionsRemainingThisTurn <= 0)
			RunMonsterTurn();
	}

	/// <summary>怪物方回合：每只怪物执行最多 2 次行为（攻击或移动），完成后切回我方回合</summary>
	private void RunMonsterTurn()
	{
		_isPlayerTurn = false;
		_actionsRemainingThisTurn = 0;

		var enemies = new List<(int row, int col, ChessEnemy enemy)>();
		foreach (var kv in _gridUnits)
		{
			if (kv.Value is ChessEnemy enemy && enemy.IsAlive)
				enemies.Add((kv.Key.row, kv.Key.col, enemy));
		}
		foreach (var (row, col, enemy) in enemies)
		{
			if (!enemy.IsAlive) continue;
			RunMonsterAi(enemy, row, col, 2);
		}

		_isPlayerTurn = true;
		_actionsRemainingThisTurn = 2;
	}

	/// <summary>单只怪物 AI：最多 actionsLeft 次行为，每次为攻击（伤害 1）或移动一格</summary>
	private void RunMonsterAi(ChessEnemy monster, int monsterRow, int monsterCol, int actionsLeft)
	{
		for (int i = 0; i < actionsLeft; i++)
		{
			var hero = GetNearestAliveHero(monsterRow, monsterCol);
			if (hero == null) break;

			int hr = hero.GridRow, hc = hero.GridCol;
			int dist = ManhattanDistance(monsterRow, monsterCol, hr, hc);
			if (dist <= 1)
			{
				hero.TakeDamage(1);
				if (!hero.IsAlive) return;
				continue;
			}
			var step = GetStepToward(monsterRow, monsterCol, hr, hc);
			if (step != null)
			{
				MoveUnit(monsterRow, monsterCol, step.Value.row, step.Value.col);
				monsterRow = step.Value.row;
				monsterCol = step.Value.col;
			}
		}
	}

	/// <summary>获取距离 (row,col) 曼哈顿距离最近的存活棋将；等距时按行列序取一</summary>
	public ChessHero? GetNearestAliveHero(int row, int col)
	{
		ChessHero? nearest = null;
		int minDist = int.MaxValue;
		foreach (var kv in _gridUnits)
		{
			if (kv.Value is ChessHero hero && hero.IsAlive)
			{
				int d = ManhattanDistance(row, col, hero.GridRow, hero.GridCol);
				if (d < minDist)
				{
					minDist = d;
					nearest = hero;
				}
				else if (d == minDist && nearest != null && (hero.GridRow < nearest.GridRow || (hero.GridRow == nearest.GridRow && hero.GridCol < nearest.GridCol)))
					nearest = hero;
			}
		}
		return nearest;
	}

	/// <summary>从 (fromR,fromC) 向 (toR,toC) 移动一格的合法空格；若无则返回 null</summary>
	private (int row, int col)? GetStepToward(int fromR, int fromC, int toR, int toC)
	{
		var neighbours = GetNeighbourCells(fromR, fromC);
		(int row, int col)? best = null;
		int bestDist = int.MaxValue;
		foreach (var (nr, nc) in neighbours)
		{
			if (GetUnitAt(nr, nc) != null) continue;
			int d = ManhattanDistance(nr, nc, toR, toC);
			if (d < bestDist)
			{
				bestDist = d;
				best = (nr, nc);
			}
		}
		return best;
	}

	/// <summary>高亮一组格子（用于移动/攻击目标）</summary>
	public void SetHighlightCells(IEnumerable<(int row, int col)> cells)
	{
		ClearHighlight();
		foreach (var (r, c) in cells)
		{
			var rect = new ColorRect
			{
				Position = GetCellPosition(r, c) + new Vector2(2, 2),
				Size = new Vector2(CellSizePx - 4, CellSizePx - 4),
				Color = new Color(1f, 1f, 0.3f, 0.5f),
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			_highlightContainer.AddChild(rect);
			_highlightRects.Add(rect);
		}
	}

	public void ClearHighlight()
	{
		foreach (var r in _highlightRects)
			r.QueueFree();
		_highlightRects.Clear();
	}

	private void BuildCells()
	{
		if (_cellsContainer == null || _cellsContainer == this)
			return;

		IReadOnlyDictionary<int, string>? cellResources = null;
		int[,]? grid = null;
		try
		{
			cellResources = MapConfigLoader.LoadMapCellResources();
			grid = MapConfigLoader.LoadMapGrid();
		}
		catch (Exception ex)
		{
			GD.PushWarning($"地图配置加载失败，使用默认格子: {ex.Message}");
		}

		for (int r = 0; r < BoardRows; r++)
		for (int c = 0; c < BoardCols; c++)
		{
			int cellId = (grid != null && r < 5 && c < 10) ? grid[r, c] : 0;
			Texture2D? tex = null;
			if (cellId != 0 && cellResources != null && cellResources.TryGetValue(cellId, out string path))
			{
				tex = GD.Load<Texture2D>(path);
				if (tex == null)
					GD.PushWarning($"地图格贴图加载失败: {path}");
			}

			if (tex != null)
			{
				var texRect = new TextureRect
				{
					Size = new Vector2(CellSizePx - 2, CellSizePx - 2),
					Position = GetCellPosition(r, c) + new Vector2(1, 1),
					ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
					Texture = tex,
					MouseFilter = Control.MouseFilterEnum.Ignore
				};
				_cellsContainer.AddChild(texRect);
			}
			else
			{
				var rect = new ColorRect
				{
					Size = new Vector2(CellSizePx - 2, CellSizePx - 2),
					Position = GetCellPosition(r, c) + new Vector2(1, 1),
					Color = (r + c) % 2 == 0 ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.2f, 0.4f, 0.2f),
					MouseFilter = Control.MouseFilterEnum.Ignore
				};
				_cellsContainer.AddChild(rect);
			}
		}
	}

	/// <summary>获取格子中心像素坐标（相对棋盘左上）</summary>
	public Vector2 GetCellPosition(int row, int col)
	{
		return new Vector2(col * CellSizePx, row * CellSizePx);
	}

	/// <summary>将单位注册到格子并放置到 UnitsContainer</summary>
	public void PlaceUnit(Node unit, int row, int col)
	{
		if (row < 0 || row >= BoardRows || col < 0 || col >= BoardCols)
			return;

		RemoveUnitAt(row, col);

		_gridUnits[(row, col)] = unit;
		if (!unit.IsInsideTree())
			_unitsContainer.AddChild(unit);

		if (unit is Control control)
		{
			control.Position = GetCellPosition(row, col);
			control.Size = new Vector2(CellSizePx, CellSizePx);
		}
		if (unit is ChessHero hero)
			hero.SetGridPosition(row, col);
		else if (unit is ChessEnemy enemy)
			enemy.SetGridPosition(row, col);
	}

	public void RemoveUnitAt(int row, int col)
	{
		if (_gridUnits.TryGetValue((row, col), out var existing) && existing.IsInsideTree())
		{
			existing.GetParent()?.RemoveChild(existing);
			_gridUnits.Remove((row, col));
		}
	}

	public Node? GetUnitAt(int row, int col)
	{
		return _gridUnits.GetValueOrDefault((row, col));
	}

	public bool IsInBounds(int row, int col)
	{
		return row >= 0 && row < BoardRows && col >= 0 && col < BoardCols;
	}

	/// <summary>获取上下左右四格（在范围内的坐标）</summary>
	public List<(int row, int col)> GetNeighbourCells(int row, int col)
	{
		var list = new List<(int row, int col)>();
		foreach (var (dr, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
		{
			int nr = row + dr, nc = col + dc;
			if (IsInBounds(nr, nc))
				list.Add((nr, nc));
		}
		return list;
	}

	/// <summary>曼哈顿距离</summary>
	public static int ManhattanDistance(int r1, int c1, int r2, int c2)
		=> Math.Abs(r2 - r1) + Math.Abs(c2 - c1);

	/// <summary>获取距离 (row,col) 曼哈顿距离 &lt;= range 且在范围内的所有格子（不含自身）</summary>
	public List<(int row, int col)> GetCellsWithinRange(int row, int col, int range)
	{
		var list = new List<(int row, int col)>();
		for (int r = 0; r < BoardRows; r++)
		for (int c = 0; c < BoardCols; c++)
		{
			if (r == row && c == col) continue;
			if (ManhattanDistance(row, col, r, c) <= range)
				list.Add((r, c));
		}
		return list;
	}

	/// <summary>获取在移动距离内的空格（可移动目标）</summary>
	public List<(int row, int col)> GetEmptyCellsInMoveRange(int row, int col, int moveRange)
	{
		var list = new List<(int row, int col)>();
		foreach (var (r, c) in GetCellsWithinRange(row, col, moveRange))
		{
			if (GetUnitAt(r, c) == null)
				list.Add((r, c));
		}
		return list;
	}

	/// <summary>获取在攻击距离内的敌将所在格子（可攻击目标）</summary>
	public List<(int row, int col)> GetEnemyCellsInAttackRange(int row, int col, int attackRange)
	{
		var list = new List<(int row, int col)>();
		foreach (var (r, c) in GetCellsWithinRange(row, col, attackRange))
		{
			if (GetUnitAt(r, c) is ChessEnemy)
				list.Add((r, c));
		}
		return list;
	}

	/// <summary>获取可移动到的相邻空格（兼容旧逻辑，等价于 moveRange=1）</summary>
	public List<(int row, int col)> GetEmptyNeighbourCells(int row, int col)
		=> GetEmptyCellsInMoveRange(row, col, 1);

	public void MoveUnit(int fromRow, int fromCol, int toRow, int toCol)
	{
		if (!_gridUnits.TryGetValue((fromRow, fromCol), out var unit))
			return;
		_gridUnits.Remove((fromRow, fromCol));
		PlaceUnit(unit, toRow, toCol);
	}

	/// <summary>对若干格子内的敌将造成伤害（用于 AOE 技能等）</summary>
	public void ApplyDamageToCells(IEnumerable<(int row, int col)> cells, int damage)
	{
		foreach (var (r, c) in cells)
		{
			if (GetUnitAt(r, c) is ChessEnemy enemy)
				enemy.TakeDamage(damage);
		}
	}
}
