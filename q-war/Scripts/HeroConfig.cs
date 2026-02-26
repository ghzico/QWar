namespace QWar;

/// <summary>
/// 棋将配置（来自 config/General.xlsx 的一行）。
/// 列：ID, 攻击力, 攻击距离, 移动距离, 形象配置（形象资源路径）, 血量（GHp）。
/// </summary>
public class HeroConfig
{
	public int Id { get; set; }
	public int Attack { get; set; }
	public int AttackRange { get; set; }
	public int MoveRange { get; set; }
	/// <summary>形象资源路径，如 res://Res/General/knight.png</summary>
	public string PortraitPath { get; set; } = "";
	/// <summary>血量（GHp），来自表头「血量」或「GHp」列，缺省为 1</summary>
	public int GHp { get; set; } = 1;
}
