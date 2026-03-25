using Godot;

namespace QWar;

/// <summary>
/// 棋将包内单个棋将卡片，支持拖拽到地图第 0/1 列（最左侧两列）。
/// </summary>
public partial class DeckHeroCard : PanelContainer
{
	/// <summary>General.xlsx 中的棋将配置 ID</summary>
	public int ConfigId { get; set; }

	/// <summary>所属棋将包，由 DeckPanel 在创建时设置；用于接收从棋盘拖入的棋将</summary>
	public DeckPanel? DeckPanel { get; set; }

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		var preview = MakeDragPreview();
		if (preview != null)
			SetDragPreview(preview);
		return Variant.CreateFrom(ConfigId);
	}

	private Control? MakeDragPreview()
	{
		Texture2D? tex = null;
		if (GetChildCount() > 0 && GetChild(0).GetChildCount() > 0 && GetChild(0).GetChild(0) is TextureRect cardTex)
			tex = cardTex.Texture;
		var rect = new TextureRect
		{
			CustomMinimumSize = new Vector2(64, 64),
			ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Texture = tex
		};
		return rect;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		if (DeckPanel?.Board == null || !DeckPanel.Board.IsDeploymentPhase) return false;
		return data.VariantType == Variant.Type.Object && data.AsGodotObject() is ChessHero;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		if (data.VariantType != Variant.Type.Object || data.AsGodotObject() is not ChessHero hero || DeckPanel == null)
			return;
		DeckPanel.HandleHeroDroppedFromBoard(hero);
	}
}
