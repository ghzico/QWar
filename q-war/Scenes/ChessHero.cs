using Godot;
using System;
using System.Collections.Generic;

namespace QWar;

/// <summary>
/// 棋将。玩家可控制移动与攻击，可被添加到棋盘并按格移动。
/// </summary>
public partial class ChessHero : Control
{
	public int GridRow { get; private set; }
	public int GridCol { get; private set; }

	private ColorRect _colorRect = null!;
	private FightBoard _board = null!;
	private PopupPanel _actionPopup = null!;
	private Button _btnMove = null!;
	private Button _btnAttack = null!;
	private bool _waitingMoveTarget;
	private bool _waitingAttackTarget;

	public override void _Ready()
	{
		_colorRect = GetNode<ColorRect>("ColorRect");
		_colorRect.Color = new Color(0.2f, 0.4f, 0.9f); // 蓝色表示棋将

		// 棋将可能在 FightBoard/UnitsContainer 下，向上找棋盘
		Node n = GetParent();
		while (n != null)
		{
			if (n is FightBoard fb) { _board = fb; break; }
			n = n.GetParent();
		}
		if (_board == null)
			_board = GetTree().CurrentScene?.GetNode<FightBoard>("FightBoard")!;

		_actionPopup = GetNode<PopupPanel>("ActionPopup");
		_btnMove = GetNode<Button>("ActionPopup/Margin/VBox/Move");
		_btnAttack = GetNode<Button>("ActionPopup/Margin/VBox/Attack");

		_btnMove.Pressed += OnMovePressed;
		_btnAttack.Pressed += OnAttackPressed;
	}

	/// <summary>由棋盘在点击该格时调用，弹出【移动|攻击】选项</summary>
	public void ShowActionPopup()
	{
		_actionPopup.PopupCentered();
	}

	public void SetGridPosition(int row, int col)
	{
		GridRow = row;
		GridCol = col;
		if (_board != null)
			Position = _board.GetCellPosition(row, col);
	}

	private void OnMovePressed()
	{
		_actionPopup.Hide();
		StartMoveTargetMode();
	}

	private void OnAttackPressed()
	{
		_actionPopup.Hide();
		StartAttackTargetMode();
	}

	private void StartMoveTargetMode()
	{
		_waitingMoveTarget = true;
		_waitingAttackTarget = false;
		_board.SelectedHero = this;
		var list = _board.GetEmptyNeighbourCells(GridRow, GridCol);
		_board.SetHighlightCells(list);
	}

	private void StartAttackTargetMode()
	{
		_waitingAttackTarget = true;
		_waitingMoveTarget = false;
		_board.SelectedHero = this;
		var list = GetAttackableEnemyCells();
		_board.SetHighlightCells(list);
	}

	/// <summary>可攻击的敌将所在格子（相邻格子的敌将）</summary>
	private List<(int row, int col)> GetAttackableEnemyCells()
	{
		var result = new List<(int row, int col)>();
		foreach (var (nr, nc) in _board.GetNeighbourCells(GridRow, GridCol))
		{
			var unit = _board.GetUnitAt(nr, nc);
			if (unit is ChessEnemy)
				result.Add((nr, nc));
		}
		return result;
	}

	/// <summary>由棋盘或测试场景调用：点击了某格子</summary>
	public void OnCellClicked(int row, int col)
	{
		if (_waitingMoveTarget)
		{
			var empty = _board.GetEmptyNeighbourCells(GridRow, GridCol);
			if (empty.Contains((row, col)))
			{
				_waitingMoveTarget = false;
				_board.ClearHighlight();
				_board.SelectedHero = null;
				_board.MoveUnit(GridRow, GridCol, row, col);
			}
		}
		else if (_waitingAttackTarget)
		{
			var unit = _board.GetUnitAt(row, col);
			if (unit is ChessEnemy enemy)
			{
				_waitingAttackTarget = false;
				_board.ClearHighlight();
				_board.SelectedHero = null;
				enemy.TakeDamage(1);
			}
		}
	}

	public void CancelTargetMode()
	{
		_waitingMoveTarget = false;
		_waitingAttackTarget = false;
		_board.ClearHighlight();
		_board.SelectedHero = null;
	}
}
