using System;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Templates;
using valleyfold.Templates.Events;
using valleyfold.Utils;
using Xunit;

namespace Testing.Templates;

// TemplateSession emits a global EventBus event on completion, so tests share
// the bus and unsubscribe on dispose like FoldAnimatorTests.
[CollectionDefinition(nameof(TemplateSessionTests), DisableParallelization = true)]
public class TemplateSessionCollection { }

[Collection(nameof(TemplateSessionTests))]
public class TemplateSessionTests : IDisposable
{
    private readonly Action<TemplateCompletedEvent> _handler;
    private TemplateCompletedEvent? _lastCompletion;
    private int _completionCount;

    protected TemplateSessionTests()
    {
        _handler = e => { _lastCompletion = e; _completionCount++; };
        EventBus.Register<TemplateCompletedEvent>(_handler);
    }

    public void Dispose()
    {
        EventBus.Deregister<TemplateCompletedEvent>(_handler);
    }

    private static BoatTemplate.FoldStep[] TwoSteps() => new[]
    {
        new BoatTemplate.FoldStep(new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0.5f, 0.1f), ChangeType.ValleyFold),
        new BoatTemplate.FoldStep(new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0.5f, 0.9f), ChangeType.MountainFold),
    };

    [Collection(nameof(TemplateSessionTests))]
    public class Initial : TemplateSessionTests
    {
        [Fact]
        public void CurrentStepIsFirstStep()
        {
            var steps = TwoSteps();
            var session = new TemplateSession("boat", "default", steps);
            Assert.Equal(steps[0], session.CurrentStep);
        }

        [Fact]
        public void IsNotComplete()
        {
            var session = new TemplateSession("boat", "default", TwoSteps());
            Assert.False(session.IsComplete);
        }

        [Fact]
        public void DoesNotEmitCompletion()
        {
            var _ = new TemplateSession("boat", "default", TwoSteps());
            Assert.Equal(0, _completionCount);
        }
    }

    [Collection(nameof(TemplateSessionTests))]
    public class AfterAdvance : TemplateSessionTests
    {
        [Fact]
        public void CurrentStepMovesToNext()
        {
            var steps = TwoSteps();
            var session = new TemplateSession("boat", "default", steps);
            session.Advance();
            Assert.Equal(steps[1], session.CurrentStep);
        }

        [Fact]
        public void IsNotCompleteUntilFinalStepAdvanced()
        {
            var session = new TemplateSession("boat", "default", TwoSteps());
            session.Advance();
            Assert.False(session.IsComplete);
        }

        [Fact]
        public void DoesNotEmitCompletionMidSequence()
        {
            var session = new TemplateSession("boat", "default", TwoSteps());
            session.Advance();
            Assert.Equal(0, _completionCount);
        }
    }

    [Collection(nameof(TemplateSessionTests))]
    public class OnCompletion : TemplateSessionTests
    {
        [Fact]
        public void IsCompleteAfterLastAdvance()
        {
            var session = new TemplateSession("boat", "default", TwoSteps());
            session.Advance();
            session.Advance();
            Assert.True(session.IsComplete);
        }

        [Fact]
        public void EmitsCompletionEventOnce()
        {
            var session = new TemplateSession("boat", "default", TwoSteps());
            session.Advance();
            session.Advance();
            Assert.Equal(1, _completionCount);
        }

        [Fact]
        public void CompletionEventCarriesTemplateAndPaperIds()
        {
            var session = new TemplateSession("boat", "blue", TwoSteps());
            session.Advance();
            session.Advance();
            Assert.Equal("boat", _lastCompletion!.TemplateId);
            Assert.Equal("blue", _lastCompletion.PaperId);
        }

        [Fact]
        public void AdvancePastEndIsNoOp()
        {
            var session = new TemplateSession("boat", "default", TwoSteps());
            session.Advance();
            session.Advance();
            session.Advance(); // extra
            Assert.Equal(1, _completionCount);
            Assert.True(session.IsComplete);
        }
    }
}
