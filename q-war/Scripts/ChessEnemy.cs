using Godot;
using System;
using System.Collections.Generic;

namespace QWar;

/// <summary>
/// 敌将。可被添加到棋盘，被棋将攻击时扣血。支持 Slime 动画集（Slime1/Slime2/Slime3），无动画时使用静态立绘。
/// </summary>
public partial class ChessEnemy : Control
{
	private const int MaxHp = 10;
	/// <summary>精灵表仅使用第一行（向玩家面）</summary>
	private const int AnimationRowIndex = 0;

	private int _hp = MaxHp;
	private TextureRect _textureRect = null!;
	private Label _label = null!;
	private AnimatedSprite2D _animatedSprite = null!;
	private bool _animationFinishedConnected;

	/// <summary>敌将立绘，可在场景中导出或运行时通过 SetPortrait 设置</summary>
	[Export] public Texture2D? PortraitTexture { get; set; }

	/// <summary>动画集文件夹名，如 Slime1、Slime2、Slime3。为空或加载失败时仅显示立绘。</summary>
	[Export] public string AnimationSetName { get; set; } = "";

	public int GridRow { get; private set; }
	public int GridCol { get; private set; }
	public int Hp => _hp;
	public bool IsAlive => _hp > 0;

	/// <summary>是否已成功加载并可使用动画（有任一动作即可）</summary>
	public bool HasAnimation => _animatedSprite != null && _animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.GetAnimationNames().Length > 0;

	/// <summary>攻击动画播放结束时发出（供 FightBoard 等待后结算伤害）</summary>
	[Signal] public delegate void AttackAnimationFinishedEventHandler();

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_label = GetNode<Label>("Label");
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (PortraitTexture != null)
			_textureRect.Texture = PortraitTexture;
		TryLoadSlimeAnimation();
		UpdateHpDisplay();
	}

	/// <summary>运行时设置立绘（与导出 PortraitTexture 二选一，用于同一场景不同资源）</summary>
	public void SetPortrait(Texture2D texture)
	{
		PortraitTexture = texture;
		if (_textureRect != null)
			_textureRect.Texture = texture;
	}

	/// <summary>设置动画集名称（如 Slime1、Slime2、Slime3）并重新加载动画；在入树前调用有效，入树后需在 _Ready 之后调用会触发重载。</summary>
	public void SetAnimationSet(string setName)
	{
		AnimationSetName = setName ?? "";
		if (IsInsideTree() && _animatedSprite != null)
			TryLoadSlimeAnimation();
	}

	private void TryLoadSlimeAnimation()
	{
		if (string.IsNullOrWhiteSpace(AnimationSetName) || _animatedSprite == null)
			return;
		string name = AnimationSetName.Trim();
		var frames = new SpriteFrames();
		// 约定：文件名 怪物名序号_动作.png，网格见 Res/Enemy/README.md，仅使用第 0 行
		var actions = new[] { ("Idle", 4, 6), ("Walk", 4, 8), ("Run", 4, 8), ("Attack", 4, 10), ("Hurt", 4, 5), ("Death", 4, 10) };
		int added = 0;
		foreach (var (action, vframes, hframes) in actions)
		{
			string? path = GetSlimeTexturePath(name, action);
			if (path == null) continue;
			Texture2D? tex = GD.Load<Texture2D>(path);
			if (tex == null) continue;
			var rowFrames = BuildRowFrames(tex, hframes, vframes, AnimationRowIndex);
			if (rowFrames.Count == 0) continue;
			string animName = action.ToLower();
			frames.AddAnimation(animName);
			foreach (var atlas in rowFrames)
				frames.AddFrame(animName, atlas, 0.08f);
			// 单次播放的动画必须设为不循环，否则 animation_finished 不会触发，导致攻击不结算、死亡不移除
			frames.SetAnimationLoop(animName, animName == "idle" || animName == "walk" || animName == "run");
			added++;
		}
		if (added > 0)
		{
			_animatedSprite.SpriteFrames = frames;
			_animatedSprite.Visible = true;
			if (_textureRect != null) _textureRect.Visible = false;
			_animatedSprite.Play("idle");
			if (_animationFinishedConnected)
			{
				_animatedSprite.AnimationFinished -= OnEnemyAnimationFinished;
				_animationFinishedConnected = false;
			}
			_animatedSprite.AnimationFinished += OnEnemyAnimationFinished;
			_animationFinishedConnected = true;
		}
		else
		{
			_animatedSprite.Visible = false;
			if (_textureRect != null) _textureRect.Visible = true;
		}
	}

	/// <summary>解析怪物动画贴图路径：Res/Enemy/SlimeN/SlimeN_Action.png；仅当文件存在时返回路径，避免 GD.Load 报错。</summary>
	private static string? GetSlimeTexturePath(string slimeName, string action)
	{
		string path = $"res://Res/Enemy/{slimeName}/{slimeName}_{action}.png";
		return FileAccess.FileExists(path) ? path : null;
	}

	private void OnEnemyAnimationFinished()
	{
		if (!HasAnimation) return;
		string name = _animatedSprite.Animation;
		if (name == "attack")
			EmitSignal(ChessEnemy.SignalName.AttackAnimationFinished);
		else if (name == "hurt" && !IsAlive)
		{
			if (_animatedSprite.SpriteFrames.HasAnimation("death"))
			{
				_animatedSprite.Play("death");
				// 最大等待约 3 秒，避免 death 动画异常时卡住
				GetTree().CreateTimer(3.0).Timeout += () => { if (IsInsideTree()) DoRemoveAndFree(); };
			}
			else
				DoRemoveAndFree();
		}
		else if (name == "death")
			DoRemoveAndFree();
		else if (name != "idle" && name != "walk")
			_animatedSprite.Play("idle");
	}

	private void DoRemoveAndFree()
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

	private static List<AtlasTexture> BuildRowFrames(Texture2D atlasTex, int hframes, int vframes, int rowIndex)
	{
		var list = new List<AtlasTexture>();
		int w = atlasTex.GetWidth();
		int h = atlasTex.GetHeight();
		if (w <= 0 || h <= 0 || hframes <= 0 || vframes <= 0 || rowIndex < 0 || rowIndex >= vframes) return list;
		float cw = (float)w / hframes;
		float ch = (float)h / vframes;
		int y = (int)(rowIndex * ch);
		int fh = (int)ch;
		for (int col = 0; col < hframes; col++)
		{
			int x = (int)(col * cw);
			int fw = (int)cw;
			var atlas = new AtlasTexture { Atlas = atlasTex, Region = new Rect2(x, y, fw, fh) };
			list.Add(atlas);
		}
		return list;
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
		if (HasAnimation)
			PlayHurt();
		else if (!IsAlive)
			DoRemoveAndFree();
	}

	/// <summary>播放待机动画（循环）</summary>
	public void PlayIdle() { if (HasAnimation) _animatedSprite.Play("idle"); }
	/// <summary>播放移动动画（循环）</summary>
	public void PlayWalk() { if (HasAnimation) _animatedSprite.Play("walk"); }
	/// <summary>播放攻击动画（单次）</summary>
	public void PlayAttack() { if (HasAnimation) _animatedSprite.Play("attack"); }
	/// <summary>播放受击动画（单次），若已死亡则播完后播 death，再移除</summary>
	public void PlayHurt() { if (HasAnimation) _animatedSprite.Play("hurt"); }
	/// <summary>播放死亡动画（单次），播完后移除</summary>
	public void PlayDeath() { if (HasAnimation) _animatedSprite.Play("death"); else DoRemoveAndFree(); }

	private void UpdateHpDisplay()
	{
		if (_label != null)
			_label.Text = _hp.ToString();
	}
}
