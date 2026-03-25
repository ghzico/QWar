using Godot;
using System.Collections.Generic;

namespace QWar;

/// <summary>
/// 棋将包 UI：显示在战斗场面下方，从 General.xlsx 加载可选棋将，供拖拽布阵使用。
/// </summary>
public partial class DeckPanel : Control
{
	/// <summary>战斗棋盘引用，由 TestScene 或父场景设置；用于拖拽放置与确认出战</summary>
	public FightBoard? Board { get; set; }

	private HBoxContainer _cardsContainer = null!;
	private Button _confirmButton = null!;
	private readonly List<int> _deployedConfigIds = new();

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(0, 120);
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 8);
		AddChild(vbox);

		var scroll = new ScrollContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			HorizontalScrollMode = ScrollContainer.ScrollMode.ShowAlways,
			VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
		};
		_cardsContainer = new HBoxContainer();
		_cardsContainer.AddThemeConstantOverride("separation", 12);
		scroll.AddChild(_cardsContainer);
		vbox.AddChild(scroll);

		_confirmButton = new Button
		{
			Text = "确认出战",
			Disabled = true
		};
		_confirmButton.Pressed += OnConfirmPressed;
		vbox.AddChild(_confirmButton);

		LoadAndShowHeroes();
	}

	private static string NormalizePortraitPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path)) return path;
		path = path.Trim().Replace('\\', '/');
		return path.StartsWith("res://", System.StringComparison.Ordinal) ? path : "res://" + path.TrimStart('/');
	}

	private void LoadAndShowHeroes()
	{
		var configs = GeneralConfigLoader.LoadHeroConfigs();
		foreach (var kv in configs)
		{
			int configId = kv.Key;
			var config = kv.Value;
			var card = CreateHeroCard(configId, config);
			_cardsContainer.AddChild(card);
		}
	}

	private Control CreateHeroCard(int configId, HeroConfig config)
	{
		var card = new DeckHeroCard { ConfigId = configId };
		card.CustomMinimumSize = new Vector2(80, 90);
		card.SetMeta("hero_config_id", configId);

		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 4);

		Texture2D? tex = null;
		if (!string.IsNullOrWhiteSpace(config.PortraitPath))
		{
			string path = NormalizePortraitPath(config.PortraitPath);
			tex = GD.Load<Texture2D>(path);
		}
		var texRect = new TextureRect
		{
			CustomMinimumSize = new Vector2(64, 64),
			ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Texture = tex
		};
		vbox.AddChild(texRect);

		var label = new Label { Text = $"ID:{configId}" };
		vbox.AddChild(label);

		card.AddChild(vbox);
		return card;
	}

	private void OnConfirmPressed()
	{
		Board?.StartBattle();
		Visible = false;
	}

	/// <summary>布阵阶段是否允许放置到指定格子（仅第 0、1 列即最左侧两列且为空）</summary>
	public static bool IsValidDeployCell(FightBoard board, int row, int col)
	{
		if (col != 0 && col != 1) return false;
		return board.GetUnitAt(row, col) == null;
	}

	/// <summary>标记某配置 ID 已上场，从包内移除或标记不可再拖拽</summary>
	public void MarkDeployed(int configId)
	{
		if (_deployedConfigIds.Contains(configId)) return;
		_deployedConfigIds.Add(configId);
		UpdateCardVisibility();
	}

	/// <summary>某棋将是否已被拖拽上场（本场仅可上场一次）</summary>
	public bool IsDeployed(int configId) => _deployedConfigIds.Contains(configId);

	private void UpdateCardVisibility()
	{
		foreach (var child in _cardsContainer.GetChildren())
		{
			if (child is PanelContainer card && card.HasMeta("hero_config_id"))
			{
				int id = (int)card.GetMeta("hero_config_id");
				card.Visible = !_deployedConfigIds.Contains(id);
			}
		}
		_confirmButton.Disabled = _deployedConfigIds.Count == 0;
	}
}
