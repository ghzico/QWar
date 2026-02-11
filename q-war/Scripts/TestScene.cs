using Godot;

namespace QWar;

/// <summary>
/// 测试场景：5 行 10 列棋盘，左侧棋将由 config/General.xlsx 按 ID 决定显示与数值。
/// 在表格中修改数据或新增 ID 后，只需在此处或关卡配置中指定要出战的 ID 列表即可，无需改其它逻辑。
/// </summary>
public partial class TestScene : Control
{
	/// <summary>本场出战的棋将配置 ID 列表，与 General.xlsx 中的 ID 列对应。增删或改顺序即可换将、加将。</summary>
	private static readonly int[] HeroConfigIds = { 1, 2, 3 };

	public override void _Ready()
	{
		var board = GetNode<FightBoard>("MainVBox/BoardCenter/FightBoard");
		board.BoardRows = 5;
		board.BoardCols = 10;

		var heroScene = GD.Load<PackedScene>("res://Scenes/ChessHero.tscn");
		var enemyScene = GD.Load<PackedScene>("res://Scenes/ChessEnemy.tscn");

		var heroConfigs = GeneralConfigLoader.LoadHeroConfigs();

		for (int i = 0; i < HeroConfigIds.Length; i++)
		{
			var hero = heroScene.Instantiate<ChessHero>();
			int configId = HeroConfigIds[i];
			if (!GeneralConfigLoader.ApplyToHero(hero, configId, heroConfigs))
				GD.PushWarning($"棋将配置 ID {configId} 在 General.xlsx 中未找到，使用默认属性。");
			hero.SetHeroIndex(i); // 技能类型仍按出场顺序：0=普通 1=AOE 2=下次攻击×3
			board.PlaceUnit(hero, i, 0);
		}

		// 三个敌将立绘：monster1, monster2, monster3（来自 Res/Enemy/）
		var enemyTextures = new[]
		{
			GD.Load<Texture2D>("res://Res/Enemy/monster1.png"),
			GD.Load<Texture2D>("res://Res/Enemy/monster2.png"),
			GD.Load<Texture2D>("res://Res/Enemy/monster3.png")
		};

		for (int i = 0; i < 3; i++)
		{
			var enemy = enemyScene.Instantiate<ChessEnemy>();
			enemy.SetPortrait(enemyTextures[i]);
			board.PlaceUnit(enemy, i, 9);
		}
	}
}
