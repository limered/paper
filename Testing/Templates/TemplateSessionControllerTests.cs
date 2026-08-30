using System;
using Godot;
using valleyfold;
using valleyfold.Templates;
using valleyfold.Templates.Events;
using valleyfold.TwoDeeModels;
using valleyfold.Ui.Events;
using valleyfold.Utils;
using Xunit;

namespace Testing.Templates;

// TemplateSessionController uses the global EventBus and Statics, so tests
// share both and unsubscribe / reset state on dispose like other bus tests.
[CollectionDefinition(nameof(TemplateSessionControllerTests), DisableParallelization = true)]
public class TemplateSessionControllerCollection { }

[Collection(nameof(TemplateSessionControllerTests))]
public class TemplateSessionControllerTests : IDisposable
{
    private readonly Action<ResetPaperEvent> _resetHandler;
    private int _resetCount;

    protected TemplateSessionControllerTests()
    {
        _resetHandler = _ => _resetCount++;
        EventBus.Register<ResetPaperEvent>(_resetHandler);
        Statics.TemplateSession = null;
    }

    public void Dispose()
    {
        EventBus.Deregister<ResetPaperEvent>(_resetHandler);
        Statics.TemplateSession = null;
    }

    [Collection(nameof(TemplateSessionControllerTests))]
    public class InvalidFrame : TemplateSessionControllerTests
    {
        public InvalidFrame()
        {
            Statics.Frame = new Frame(); // empty, not the unfolded square
        }

        [Fact]
        public void EmitsResetPaperEvent()
        {
            TemplateSessionController.StartBoatSession(
                new StartTemplateSessionEvent { TemplateId = "boat", PaperId = "default" });
            Assert.Equal(1, _resetCount);
        }

        [Fact]
        public void StartsSession()
        {
            TemplateSessionController.StartBoatSession(
                new StartTemplateSessionEvent { TemplateId = "boat", PaperId = "default" });
            Assert.NotNull(Statics.TemplateSession);
        }
    }

    [Collection(nameof(TemplateSessionControllerTests))]
    public class ValidFrame : TemplateSessionControllerTests
    {
        public ValidFrame()
        {
            Statics.Frame = new Frame();
            Statics.Frame.InitializePaper();
        }

        [Fact]
        public void DoesNotEmitResetPaperEvent()
        {
            TemplateSessionController.StartBoatSession(
                new StartTemplateSessionEvent { TemplateId = "boat", PaperId = "default" });
            Assert.Equal(0, _resetCount);
        }

        [Fact]
        public void StartsSession()
        {
            TemplateSessionController.StartBoatSession(
                new StartTemplateSessionEvent { TemplateId = "boat", PaperId = "default" });
            Assert.NotNull(Statics.TemplateSession);
        }
    }

    [Collection(nameof(TemplateSessionControllerTests))]
    public class ActiveSession : TemplateSessionControllerTests
    {
        [Fact]
        public void DoesNotStartNewSession()
        {
            var existing = new TemplateSession("boat", "default", Array.Empty<BoatTemplate.FoldStep>());
            Statics.TemplateSession = existing;
            TemplateSessionController.StartBoatSession(
                new StartTemplateSessionEvent { TemplateId = "boat", PaperId = "default" });
            Assert.Same(existing, Statics.TemplateSession);
        }
    }
}
