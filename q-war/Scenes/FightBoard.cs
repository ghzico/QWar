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

	private readonly Dictionary<(int row, int col), Node> _gridUnits = new();
	private Control _cellsContainer = null!;
	private Control _unitsContainer = null!;
	private Control _clickLayer = null!;
	private Control _highlightContainer = null!;
	private readonly List<ColorRect> _highlightRects = new();

	public override void _Ready()
	{
		_cellsContainer = GetNodeOrNull<Control>("CellsContainer") ?? this;
		_unitsContainer = GetNodeOrNull<Control>("UnitsContainer") ?? this;

		CustomMinimumSize = new Vector2(BoardCols * CellSizePx, BoardRows * CellSizePx);
		BuildCells();

		// 点击层：接收格子点击（尺寸与棋盘一致）
		var boardSize = new Vector2(BoardCols * CellSizePx, BoardRows * CellSizePx);
		_clickLayer = new Control
		{
			Size = boardSize,
			CustomMinimumSize = boardSize,
			MouseFilter = Control.MouseFilterEnum.Stop
		};
		_clickLayer.GuiInput += OnClickLayerInput;

		_highlightContainer = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
		AddChild(_highlightContainer);
		AddChild(_clickLayer); // 放在最上层，统一接收格子点击（含敌将格）
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
			if (unit is ChessHero hero)
				hero.ShowActionPopup();
		}
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

		for (int r = 0; r < BoardRows; r++)
		for (int c = 0; c < BoardCols; c++)
		{
			var rect = new ColorRect
			{
				Size = new Vector2(CellSizePx - 2, CellSizePx - 2),
				Position = GetCellPosition(r, c) + new Vector2(1, 1),
				Color = (r + c) % 2 == 0 ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.2f, 0.4f, 0.2f)
			};
			_cellsContainer.AddChild(rect);
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

	/// <summary>获取可移动到的相邻空格</summary>
	public List<(int row, int col)> GetEmptyNeighbourCells(int row, int col)
	{
		var list = new List<(int row, int col)>();
		foreach (var (nr, nc) in GetNeighbourCells(row, col))
		{
			if (GetUnitAt(nr, nc) == null)
				list.Add((nr, nc));
		}
		return list;
	}

	public void MoveUnit(int fromRow, int fromCol, int toRow, int toCol)
	{
		if (!_gridUnits.TryGetValue((fromRow, fromCol), out var unit))
			return;
		_gridUnits.Remove((fromRow, fromCol));
		PlaceUnit(unit, toRow, toCol);
	}
}
