using Godot;

namespace valleyfold.Ui;

public partial class DeskScreen : Control
{
	private readonly DeskSelection _selection = new();

	private Control _drawer;
	private Vector2 _drawerRestPosition;
	private Tween _tween;

	public override void _Ready()
	{
		_drawer = GetNode<Control>("Drawer");
		_drawerRestPosition = _drawer.Position;

		var openDrawer = GetNode<Button>("OpenDrawer");
		var closeDrawer = GetNode<Button>("Drawer/VBoxContainer/CloseButton");
		var beginButton = GetNode<Button>("Drawer/VBoxContainer/BeginButton");
		var backButton = GetNode<Button>("BackButton");
		var defaultPaper = GetNode<Button>("Drawer/VBoxContainer/Papers/DefaultPaper");
		var lockedPaper = GetNode<Button>("Drawer/VBoxContainer/Papers/LockedPaper");

		openDrawer.Pressed += SlideOpen;
		closeDrawer.Pressed += SlideClose;
		backButton.Pressed += () => SceneTransition.To("res://scenes/main.tscn", SceneTransition.TransitionDirection.Down);

		defaultPaper.Pressed += () =>
		{
			_selection.SelectPaper(DeskSelection.DefaultPaperId);
			UpdateSelectionVisuals();
		};

		lockedPaper.Disabled = true;
		lockedPaper.Modulate = new Color(0.5f, 0.5f, 0.5f);

		beginButton.Pressed += () =>
		{
			if (!_selection.CanBegin) return;
			SlideClose();
			_selection.Begin();
		};

		// Boat is the only template in M0.
		_selection.SelectTemplate(DeskSelection.BoatTemplateId);
		UpdateSelectionVisuals();
	}

	private void SlideOpen()
	{
		_tween?.Kill();

		var offScreen = new Vector2(_drawerRestPosition.X, _drawerRestPosition.Y + GetViewportRect().Size.Y);
		if (!_drawer.Visible)
		{
			_drawer.Position = offScreen;
			_drawer.Visible = true;
		}

		_tween = CreateTween()
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(_drawer, "position", _drawerRestPosition, 0.45f);
	}

	private void SlideClose()
	{
		if (!_drawer.Visible) return;

		_tween?.Kill();

		var offScreen = new Vector2(_drawerRestPosition.X, _drawerRestPosition.Y + GetViewportRect().Size.Y);
		_tween = CreateTween()
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.InOut);
		_tween.TweenProperty(_drawer, "position", offScreen, 0.4f);
		_tween.Finished += () => _drawer.Visible = false;
	}

	private void UpdateSelectionVisuals()
	{
		var defaultPaper = GetNode<Button>("Drawer/VBoxContainer/Papers/DefaultPaper");
		defaultPaper.Modulate = _selection.SelectedPaperId == DeskSelection.DefaultPaperId
			? new Color(1, 1, 1)
			: new Color(0.7f, 0.7f, 0.7f);
	}
}
