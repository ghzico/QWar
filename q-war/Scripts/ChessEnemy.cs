using Godot;
using System;

namespace QWar;

/// <summary>
/// 敌将。可被添加到棋盘，被棋将攻击时扣血。不可移动、不可攻击，无回合，10点血。
/// </summary>
public partial class ChessEnemy : Control
{
	private const int MaxHp = 10;

	private int _hp = MaxHp;
	private ColorRect _colorRect = null!;
	private Label _label = null!;

	public int GridRow { get; private set; }
	public int GridCol { get; private set; }
	public int Hp => _hp;
	public bool IsAlive => _hp > 0;

	public override void _Ready()
	{
	_colorRect = GetNode<ColorRect>("ColorRect");
	_label = GetNode<Label>("Label");
	_colorRect.Color = new Color(0.8f, 0.2f, 0.2f); // 红色表示敌将
	UpdateHpDisplay();
	}

	public void SetGridPosition(int row, int col)
	{
		GridRow = row;
		GridCol = col;
	}

	public void TakeDamage(int amount)
	{
		_hp = Math.Max(0, _hp - amount);
		UpdateHpDisplay();
		if (!IsAlive)
		{
			Node n = GetParent();
			while (n != null)
			{
				if (n is FightBoard board)
				{
					board.RemoveUnitAt(GridRow, GridCol);
					break;
				}
				n = n.GetParent();
			}
			QueueFree();
		}
	}

	private void UpdateHpDisplay()
	{
		if (_label != null)
			_label.Text = _hp.ToString();
	}
}
