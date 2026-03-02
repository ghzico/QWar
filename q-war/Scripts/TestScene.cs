using Godot;

namespace QWar;

/// <summary>
/// 测试场景：5 行 10 列棋盘；先进入布阵阶段，从棋将包拖拽棋将到第 1/2 列，确认出战后开始战斗。
/// 棋将包从 config/General.xlsx 加载；敌将固定放置于右侧。
/// </summary>
public partial class TestScene : Control
{
	public override void _Ready()
	{
		var board = GetNode<FightBoard>("MainVBox/BoardCenter/FightBoard");
		board.BoardRows = 5;
		board.BoardCols = 10;
		board.EnterDeploymentPhase();

		var deckPanel = GetNode<DeckPanel>("MainVBox/DeckPanel");
		deckPanel.Board = board;
		board.DeckPanel = deckPanel;

		var enemyScene = GD.Load<PackedScene>("res://Scenes/ChessEnemy.tscn");
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
