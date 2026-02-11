using Godot;
using System;
using System.Collections.Generic;

namespace QWar;

/// <summary>
/// 棋将。玩家可控制移动与攻击，可设置移动距离与攻击距离；第二、三个棋将拥有技能。
/// </summary>
public partial class ChessHero : Control
{
	public int GridRow { get; private set; }
	public int GridCol { get; private set; }

	/// <summary>攻击力（普攻与技能伤害基数）</summary>
	[Export] public int Attack { get; set; } = 1;
	/// <summary>移动距离（格数，曼哈顿）</summary>
	[Export] public int MoveRange { get; set; } = 1;
	/// <summary>攻击距离（格数，曼哈顿）</summary>
	[Export] public int AttackRange { get; set; } = 1;
	/// <summary>棋将序号：0=普通，1=AOE技能，2=下次攻击×3</summary>
	public int HeroIndex { get; private set; }

	/// <summary>棋将立绘，可在场景中导出或运行时通过 SetPortrait 设置</summary>
	[Export] public Texture2D? PortraitTexture { get; set; }

	/// <summary>形象资源路径（由配置加载器设置，_Ready 时若尚无贴图会用此路径再加载一次以确保显示）</summary>
	internal string? PortraitPath { get; private set; }

	private TextureRect _textureRect = null!;
	private FightBoard _board = null!;
	private PopupPanel _actionPopup = null!;
	private Button _btnMove = null!;
	private Button _btnAttack = null!;
	private Button? _btnSkill;

	private bool _waitingMoveTarget;
	private bool _waitingAttackTarget;
	private bool _waitingSkillAoeTarget;

	/// <summary>下次普攻伤害倍率（第三棋将技能用）</summary>
	private int _nextAttackDamageMultiplier = 1;

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		// 优先用已设置的贴图；若无则用配置路径再加载一次（解决先 ApplyToHero 再入树时贴图未绑定的问题）
		if (PortraitTexture != null)
			_textureRect.Texture = PortraitTexture;
		else if (!string.IsNullOrEmpty(PortraitPath))
		{
			var tex = GD.Load<Texture2D>(PortraitPath);
			if (tex != null)
			{
				PortraitTexture = tex;
				_textureRect.Texture = tex;
			}
		}

		Node n = GetParent();
		while (n != null)
		{
			if (n is FightBoard fb) { _board = fb; break; }
			n = n.GetParent();
		}
		if (_board == null)
			_board = GetTree().CurrentScene?.GetNode<FightBoard>("MainVBox/BoardCenter/FightBoard")!;

		_actionPopup = GetNode<PopupPanel>("ActionPopup");
		_btnMove = GetNode<Button>("ActionPopup/Margin/VBox/Move");
		_btnAttack = GetNode<Button>("ActionPopup/Margin/VBox/Attack");
		_btnSkill = GetNodeOrNull<Button>("ActionPopup/Margin/VBox/Skill");

		_btnMove.Pressed += OnMovePressed;
		_btnAttack.Pressed += OnAttackPressed;
		if (_btnSkill != null)
			_btnSkill.Pressed += OnSkillPressed;

		UpdateSkillButtonVisibility();
	}

	/// <summary>由配置加载器设置形象路径，供 _Ready 时兜底加载</summary>
	internal void SetPortraitPath(string path)
	{
		PortraitPath = path;
	}

	/// <summary>运行时设置立绘（与导出 PortraitTexture 二选一，用于同一场景不同资源）</summary>
	public void SetPortrait(Texture2D texture)
	{
		PortraitTexture = texture;
		if (_textureRect != null)
			_textureRect.Texture = texture;
	}

	/// <summary>由 TestScene 在放置后调用，设置棋将序号以启用对应技能</summary>
	public void SetHeroIndex(int index)
	{
		HeroIndex = index;
		UpdateSkillButtonVisibility();
	}

	private void UpdateSkillButtonVisibility()
	{
		if (_btnSkill != null)
			_btnSkill.Visible = HeroIndex == 1 || HeroIndex == 2;
	}

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

	private void OnSkillPressed()
	{
		_actionPopup.Hide();
		if (HeroIndex == 1)
			StartSkillAoeTargetMode();
		else if (HeroIndex == 2)
			ApplyNextAttackBuff();
	}

	/// <summary>第二棋将：选择目标后对目标及上下左右共5格各造成1点伤害</summary>
	private void StartSkillAoeTargetMode()
	{
		_waitingSkillAoeTarget = true;
		_waitingMoveTarget = false;
		_waitingAttackTarget = false;
		_board.SelectedHero = this;
		_board.ShowCancelButton();
		var list = _board.GetEnemyCellsInAttackRange(GridRow, GridCol, AttackRange);
		_board.SetHighlightCells(list);
	}

	/// <summary>第三棋将：下次普攻伤害×3</summary>
	private void ApplyNextAttackBuff()
	{
		_nextAttackDamageMultiplier = 3;
		_board.SelectedHero = null;
	}

	private void StartMoveTargetMode()
	{
		_waitingMoveTarget = true;
		_waitingAttackTarget = false;
		_waitingSkillAoeTarget = false;
		_board.SelectedHero = this;
		_board.ShowCancelButton();
		var list = _board.GetEmptyCellsInMoveRange(GridRow, GridCol, MoveRange);
		_board.SetHighlightCells(list);
	}

	private void StartAttackTargetMode()
	{
		_waitingAttackTarget = true;
		_waitingMoveTarget = false;
		_waitingSkillAoeTarget = false;
		_board.SelectedHero = this;
		_board.ShowCancelButton();
		var list = GetAttackableEnemyCells();
		_board.SetHighlightCells(list);
	}

	/// <summary>可攻击的敌将格子（在攻击距离内）</summary>
	private List<(int row, int col)> GetAttackableEnemyCells()
		=> _board.GetEnemyCellsInAttackRange(GridRow, GridCol, AttackRange);

	public void OnCellClicked(int row, int col)
	{
		if (_waitingMoveTarget)
		{
			var empty = _board.GetEmptyCellsInMoveRange(GridRow, GridCol, MoveRange);
			if (empty.Contains((row, col)))
			{
				_waitingMoveTarget = false;
				_board.ClearHighlight();
				_board.SelectedHero = null;
				_board.HideCancelButton();
				_board.MoveUnit(GridRow, GridCol, row, col);
			}
		}
		else if (_waitingAttackTarget)
		{
			var validTargets = GetAttackableEnemyCells();
			if (validTargets.Contains((row, col)))
			{
				var unit = _board.GetUnitAt(row, col);
				if (unit is ChessEnemy enemy)
				{
					_waitingAttackTarget = false;
					_board.ClearHighlight();
					_board.SelectedHero = null;
					_board.HideCancelButton();
					int damage = Attack * _nextAttackDamageMultiplier;
					_nextAttackDamageMultiplier = 1;
					enemy.TakeDamage(damage);
				}
			}
		}
		else if (_waitingSkillAoeTarget)
		{
			var valid = _board.GetEnemyCellsInAttackRange(GridRow, GridCol, AttackRange);
			if (valid.Contains((row, col)))
			{
				_waitingSkillAoeTarget = false;
				_board.ClearHighlight();
				_board.SelectedHero = null;
				_board.HideCancelButton();
				// 目标格 + 上下左右共5格，每格 Attack 点伤害
				var aoeCells = new List<(int row, int col)> { (row, col) };
				foreach (var (nr, nc) in _board.GetNeighbourCells(row, col))
					aoeCells.Add((nr, nc));
				_board.ApplyDamageToCells(aoeCells, Attack);
			}
		}
	}

	public void CancelTargetMode()
	{
		_waitingMoveTarget = false;
		_waitingAttackTarget = false;
		_waitingSkillAoeTarget = false;
		_board.ClearHighlight();
		_board.SelectedHero = null;
		_board.HideCancelButton();
	}
}
