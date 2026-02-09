using Godot;

namespace QWar;

/// <summary>
/// 测试场景：5 行 10 列棋盘，左侧第一列 3 个棋将，右侧第一列 3 个敌将。
/// </summary>
public partial class TestScene : Control
{
	public override void _Ready()
	{
		var board = GetNode<FightBoard>("FightBoard");
		board.BoardRows = 5;
		board.BoardCols = 10;

		var heroScene = GD.Load<PackedScene>("res://Scenes/ChessHero.tscn");
		var enemyScene = GD.Load<PackedScene>("res://Scenes/ChessEnemy.tscn");

		for (int i = 0; i < 3; i++)
		{
			var hero = heroScene.Instantiate<ChessHero>();
			hero.SetHeroIndex(i); // 0=普通 1=AOE技能 2=下次攻击×3
			hero.MoveRange = 2;   // 可配置：移动距离
			hero.AttackRange = 2; // 可配置：攻击距离
			board.PlaceUnit(hero, i, 0);
		}

		for (int i = 0; i < 3; i++)
		{
			var enemy = enemyScene.Instantiate<ChessEnemy>();
			board.PlaceUnit(enemy, i, 9);
		}
	}
}
