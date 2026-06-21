using Godot;
using valleyfold.Interaction;
using valleyfold.Render.Edges;
using valleyfold.Render.ThreeDee;
using valleyfold.Templates.Events;
using valleyfold.Utils;

namespace valleyfold.Templates;

/// <summary>
/// Player-facing template flow (issue 05).
///
/// A <see cref="StartTemplateSessionEvent"/> on the bus (emitted by the
/// Start button in <see cref="Ui.GameInterface"/>) begins a
/// <see cref="TemplateSession"/> over <see cref="BoatTemplate.Steps"/>
/// on the default paper, and assigns <see cref="Statics.TemplateSession"/>
/// — which suppresses the freeform
/// <see cref="Interaction.ThreeDeePaperSelector"/> for the duration.
///
/// While the session is active, a single ghost crease (the next step's
/// fold line) is drawn on top of the paper as an <see cref="EdgeLine"/>.
/// Left-click within <see cref="ClickTolerance"/> of the ghost (measured
/// against the 3D point reported by <see cref="MousePosition"/>) applies
/// the step via <see cref="BoatTemplate.ApplyStep"/> and advances the
/// session. Clicks elsewhere are ignored. Clicks during
/// <see cref="Statics.FoldAnimator"/>.IsAnimating are ignored.
///
/// When the session completes, the ghost hides and the session reverts
/// to <c>null</c> so freeform input resumes.
/// </summary>
public partial class TemplateSessionController : Node3D
{
    [Export] public Area3D MouseCollisionArea;
    [Export] public float ClickTolerance = 0.05f;
    [Export] public float IdleLineWidth = 0.006f;
    [Export] public float HoverLineWidth = 0.018f;

    private const string DefaultPaperId = "default";
    private const string BoatTemplateId = "boat";

    private EdgeLine _ghostLine;
    private MousePosition _mouse;

    public override void _Ready()
    {
        _ghostLine = new EdgeLine();
        AddChild(_ghostLine);

        _mouse = new MousePosition();
        if (MouseCollisionArea != null) _mouse.Init(MouseCollisionArea);

        EventBus.Register<StartTemplateSessionEvent>(_ => StartBoatSession());
    }

    private static void StartBoatSession()
    {
        if (Statics.TemplateSession != null) return;
        if (!BoatTemplate.IsValidStartingFrame(Statics.Frame))
        {
            GD.PrintErr("[TemplateSessionController] frame is not the unfolded square; reset before starting a session.");
            return;
        }
        Statics.TemplateSession = new TemplateSession(
            BoatTemplateId, DefaultPaperId, BoatTemplate.Steps);
    }

    public override void _Input(InputEvent @event)
    {
        if (Statics.TemplateSession == null) return;
        if (@event is not InputEventMouseButton mb) return;
        if (!mb.Pressed || mb.ButtonIndex != MouseButton.Left) return;
        if (Statics.FoldAnimator.IsAnimating) return;

        var session = Statics.TemplateSession;
        if (session.IsComplete) return;

        var endpoints = ResolveGhostEndpoints(session.CurrentStep);
        if (endpoints == null) return;
        if (!GhostCreaseHit.IsNear(_mouse.Current, endpoints.Value.A, endpoints.Value.B, ClickTolerance))
            return;

        if (!BoatTemplate.ApplyStep(session.CurrentStep))
        {
            GD.PrintErr("[TemplateSessionController] step apply failed; session left in place.");
            return;
        }
        session.Advance();
        if (session.IsComplete) Statics.TemplateSession = null;
    }

    public override void _Process(double delta)
    {
        // Keep Frame3D up to date so the ghost reads current 3D coords for
        // recently-folded vertices (same race as ThreeDeePaperSelector).
        if (Statics.TemplateSession != null) PaperRenderer.RebuildFrame3D();
        UpdateGhost();
    }

    private void UpdateGhost()
    {
        _ghostLine.ClearPositions();
        var session = Statics.TemplateSession;
        // EdgeLine.Draw crashes ImmediateMesh on zero positions, so we
        // hide the node instead of drawing an empty surface.
        if (session == null || session.IsComplete)
        {
            _ghostLine.Visible = false;
            return;
        }
        var endpoints = ResolveGhostEndpoints(session.CurrentStep);
        if (endpoints == null)
        {
            _ghostLine.Visible = false;
            return;
        }
        _ghostLine.Visible = true;
        var step = session.CurrentStep;
        var dashes = OrigamiDashPattern.Build(endpoints.Value.A, endpoints.Value.B, step.Direction);
        foreach (var (s, e) in dashes) _ghostLine.AddPositions(s, e);
        var hovering = GhostCreaseHit.IsNear(_mouse.Current,
            endpoints.Value.A, endpoints.Value.B, ClickTolerance);
        _ghostLine.LineWidth(hovering ? HoverLineWidth : IdleLineWidth);
        _ghostLine.Draw();
    }

    private static (Vector3 A, Vector3 B)? ResolveGhostEndpoints(BoatTemplate.FoldStep step)
    {
        var a = PaperPoint3D.Resolve(Statics.Frame, Statics.Frame3d, step.LineA);
        var b = PaperPoint3D.Resolve(Statics.Frame, Statics.Frame3d, step.LineB);
        if (a == null || b == null) return null;
        // Lift slightly above the paper so the line isn't z-fought by face
        // rendering. Tiny offset, manual visual check during play.
        var lift = new Vector3(0, 0.002f, 0);
        return (a.Value + lift, b.Value + lift);
    }
}
