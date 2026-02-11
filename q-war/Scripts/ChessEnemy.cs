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
	private TextureRect _textureRect = null!;
	private Label _label = null!;

	/// <summary>敌将立绘，可在场景中导出或运行时通过 SetPortrait 设置</summary>
	[Export] public Texture2D? PortraitTexture { get; set; }

	public int GridRow { get; private set; }
	public int GridCol { get; private set; }
	public int Hp => _hp;
	public bool IsAlive => _hp > 0;

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_label = GetNode<Label>("Label");
		if (PortraitTexture != null)
			_textureRect.Texture = PortraitTexture;
		UpdateHpDisplay();
	}

	/// <summary>运行时设置立绘（与导出 PortraitTexture 二选一，用于同一场景不同资源）</summary>
	public void SetPortrait(Texture2D texture)
	{
		PortraitTexture = texture;
		if (_textureRect != null)
			_textureRect.Texture = texture;
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
