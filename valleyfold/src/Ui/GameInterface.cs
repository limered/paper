using Godot;
using valleyfold.Templates.Events;
using valleyfold.TwoDeeModels;
using valleyfold.Ui.Events;
using valleyfold.Utils;

namespace valleyfold.Ui;

public partial class GameInterface : Control
{
    private Label _activeFoldLabel;

    public override void _Ready()
    {
        var resetButton = GetNode<Button>("Sidepane/reset");
        resetButton.Pressed += () => EventBus.Emit(new ResetPaperEvent());

        var startButton = GetNode<Button>("Sidepane/start_template");
        startButton.Pressed += () => EventBus.Emit(new StartTemplateSessionEvent());

        var valleyfoldButton = GetNode<Button>("Sidepane/valleyfold");
        valleyfoldButton.Pressed += ValleyfoldButtonOnPressed;

        var mountainfoldButton = GetNode<Button>("Sidepane/mountainfold");
        mountainfoldButton.Pressed += MountainfoldButtonOnPressed;

        var unspecifiedButton = GetNode<Button>("Sidepane/unfold");
        unspecifiedButton.Pressed += UnspecifiedButtonOnPressed;

        var refoldButton = GetNode<Button>("Sidepane/refold");
        refoldButton.Pressed += RefoldButtonOnPressed;

        _activeFoldLabel = GetNode<Label>("Sidepane/active_fold_mode");
        _activeFoldLabel.Text = "Valleyfold";
        EventBus.Register<FoldModeChange>(MapFoldModeToText);
    }

    private void MapFoldModeToText(FoldModeChange msg)
    {
        switch (msg.NextFoldMode)
        {
            case Assignment.M:
                _activeFoldLabel.Text = "Mountainfold";
                break;
            case Assignment.V:
                _activeFoldLabel.Text = "Valleyfold";
                break;
            case Assignment.F:
                _activeFoldLabel.Text = "Unfold";
                break;
            case Assignment.R:
                _activeFoldLabel.Text = "Refold";
                break;
            default:
                _activeFoldLabel.Text = "Unspecified";
                break;
        }
    }

    private static void UnspecifiedButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange { NextFoldMode = Assignment.F });
    }

    private static void RefoldButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange { NextFoldMode = Assignment.R });
    }

    private static void ValleyfoldButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange { NextFoldMode = Assignment.V });
    }

    private static void MountainfoldButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange { NextFoldMode = Assignment.M });
    }
}
