using Godot;

namespace QWar;

/// <summary>
/// 战斗结果界面：胜利时显示「战斗胜利！」+ 回到主菜单；失败时显示「战斗失败...」+ 重新挑战、回到主菜单。结果图从 Res/BattleResult/ 加载，缺失时隐藏图片区域。
/// </summary>
public partial class BattleResultPanel : Control
{
	private const string VictoryImagePath = "res://Res/BattleResult/victory.png";
	private const string DefeatImagePath = "res://Res/BattleResult/defeat.png";
	private const string MainMenuPath = "res://Scenes/MainMenu.tscn";
	private const string BattleScenePath = "res://Scenes/TestScene.tscn";

	private Label _titleLabel = null!;
	private TextureRect _resultTexture = null!;
	private Button _retryButton = null!;
	private Button _backToMenuButton = null!;

	public override void _Ready()
	{
		var vbox = GetNode<VBoxContainer>("CenterContainer/VBox");
		_titleLabel = vbox.GetNode<Label>("TitleLabel");
		_resultTexture = vbox.GetNode<TextureRect>("ResultTexture");
		var buttonsHBox = vbox.GetNode<HBoxContainer>("ButtonsHBox");
		_retryButton = buttonsHBox.GetNode<Button>("RetryButton");
		_backToMenuButton = buttonsHBox.GetNode<Button>("BackToMenuButton");

		_retryButton.Pressed += OnRetryPressed;
		_backToMenuButton.Pressed += OnBackToMenuPressed;

		Visible = false;
	}

	/// <summary>显示战斗胜利界面：标题、胜利图（若有）、回到主菜单。</summary>
	public void ShowVictory()
	{
		_titleLabel.Text = "战斗胜利！";
		LoadResultImage(VictoryImagePath);
		_retryButton.Visible = false;
		_backToMenuButton.Visible = true;
		Visible = true;
	}

	/// <summary>显示战斗失败界面：标题、失败图（若有）、重新挑战、回到主菜单。</summary>
	public void ShowDefeat()
	{
		_titleLabel.Text = "战斗失败...";
		LoadResultImage(DefeatImagePath);
		_retryButton.Visible = true;
		_backToMenuButton.Visible = true;
		Visible = true;
	}

	private void LoadResultImage(string path)
	{
		var tex = GD.Load<Texture2D>(path);
		if (tex != null)
		{
			_resultTexture.Texture = tex;
			_resultTexture.Visible = true;
		}
		else
		{
			_resultTexture.Texture = null;
			_resultTexture.Visible = false;
		}
	}

	private void OnRetryPressed()
	{
		GetTree().ChangeSceneToFile(BattleScenePath);
	}

	private void OnBackToMenuPressed()
	{
		GetTree().ChangeSceneToFile(MainMenuPath);
	}
}
