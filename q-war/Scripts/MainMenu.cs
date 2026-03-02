using Godot;

namespace QWar;

/// <summary>
/// 主界面：标题、开始游戏、设置、玩法说明入口。开始游戏切换至 TestScene；设置/玩法说明本期为占位弹窗。
/// </summary>
public partial class MainMenu : Control
{
	private const string GameScenePath = "res://Scenes/TestScene.tscn";

	private Button _startButton;
	private Button _settingsButton;
	private Button _howToPlayButton;
	private AcceptDialog _placeholderDialog;

	public override void _Ready()
	{
		var vbox = GetNode<VBoxContainer>("MarginContainer/CenterContainer/VBox");
		_startButton = vbox.GetNode<Button>("StartButton");
		_settingsButton = vbox.GetNode<Button>("SettingsButton");
		_howToPlayButton = vbox.GetNode<Button>("HowToPlayButton");

		_startButton.Pressed += OnStartGamePressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_howToPlayButton.Pressed += OnHowToPlayPressed;

		_placeholderDialog = new AcceptDialog();
		_placeholderDialog.Title = "";
		AddChild(_placeholderDialog);
	}

	private void OnStartGamePressed()
	{
		GetTree().ChangeSceneToFile(GameScenePath);
	}

	private void OnSettingsPressed()
	{
		_placeholderDialog.DialogText = "敬请期待";
		_placeholderDialog.PopupCentered();
	}

	private void OnHowToPlayPressed()
	{
		_placeholderDialog.DialogText = "敬请期待";
		_placeholderDialog.PopupCentered();
	}
}
