using Godot;

namespace QWar;

/// <summary>
/// 棋将包内单个棋将卡片，支持拖拽到地图第 0/1 列（最左侧两列）。
/// </summary>
public partial class DeckHeroCard : PanelContainer
{
	/// <summary>General.xlsx 中的棋将配置 ID</summary>
	public int ConfigId { get; set; }

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		return Variant.CreateFrom(ConfigId);
	}
}
