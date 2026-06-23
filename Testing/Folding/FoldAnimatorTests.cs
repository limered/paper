using System;
using valleyfold.ChangeTracking;
using valleyfold.Folding;
using valleyfold.Render.ThreeDee.Events;
using valleyfold.Utils;

namespace Testing.Folding;

[CollectionDefinition(nameof(FoldAnimatorTests), DisableParallelization = true)]
public class FoldAnimatorCollection { }

[Collection(nameof(FoldAnimatorTests))]
public class FoldAnimatorTests : IDisposable
{
    // FoldAnimator emits PaperFoldedEvent on the global EventBus, and the bus
    // is a process-wide static. Each test subscribes a counter and unsubscribes
    // on dispose so tests don't bleed into each other.
    private readonly FoldAnimator _animator = new();
    private int _completionEventCount;
    private readonly Action<PaperFoldedEvent> _counter;

    protected FoldAnimatorTests()
    {
        _counter = _ => _completionEventCount++;
        EventBus.Register<PaperFoldedEvent>(_counter);
    }

    public void Dispose()
    {
        EventBus.Deregister<PaperFoldedEvent>(_counter);
    }

    private static ChangeRecord SomeChange() => new()
    {
        ChangeType = ChangeType.ValleyFold,
        TargetAngle = (float)Math.PI
    };

    [Collection(nameof(FoldAnimatorTests))]
    public class Initial : FoldAnimatorTests
    {
        [Fact]
        public void IsNotAnimating()
        {
            Assert.False(_animator.IsAnimating);
        }

        [Fact]
        public void EasedProgressIsZero()
        {
            Assert.Equal(0f, _animator.EasedProgress);
        }

        [Fact]
        public void TickWithoutStartIsInert()
        {
            _animator.Tick(1.0);
            Assert.False(_animator.IsAnimating);
            Assert.Equal(0, _completionEventCount);
        }
    }

    [Collection(nameof(FoldAnimatorTests))]
    public class AfterStart : FoldAnimatorTests
    {
        [Fact]
        public void IsAnimating()
        {
            _animator.Start(SomeChange(), 0.4f);
            Assert.True(_animator.IsAnimating);
        }

        [Fact]
        public void InFlightChangeIsTheStartedChange()
        {
            var change = SomeChange();
            _animator.Start(change, 0.4f);
            Assert.Same(change, _animator.InFlightChange);
        }

        [Fact]
        public void NoCompletionEventYet()
        {
            _animator.Start(SomeChange(), 0.4f);
            Assert.Equal(0, _completionEventCount);
        }

        [Fact]
        public void TickAfterCancelDoesNotInvokeCallback()
        {
            _animator.Start(SomeChange(), 0.4f);
            _animator.Cancel();
            Assert.False(_animator.IsAnimating);
            _animator.Tick(1.0);
            Assert.Equal(0, _completionEventCount);
        }
    }

    [Collection(nameof(FoldAnimatorTests))]
    public class WhileAnimating : FoldAnimatorTests
    {
        [Fact]
        public void EasedProgressRisesAfterTick()
        {
            _animator.Start(SomeChange(), 1.0f);
            _animator.Tick(0.25);
            Assert.True(_animator.EasedProgress > 0f);
            Assert.True(_animator.EasedProgress < 1f);
        }

        [Fact]
        public void HalfwayThroughEaseOutIsPastHalf()
        {
            // Ease-out 1 - (1-t)^2 at t=0.5 is 0.75.
            _animator.Start(SomeChange(), 1.0f);
            _animator.Tick(0.5);
            Assert.True(_animator.EasedProgress > 0.5f);
        }

        [Fact]
        public void StaysAnimatingUntilDurationElapsed()
        {
            _animator.Start(SomeChange(), 1.0f);
            _animator.Tick(0.5);
            Assert.True(_animator.IsAnimating);
        }
    }

    [Collection(nameof(FoldAnimatorTests))]
    public class OnCompletion : FoldAnimatorTests
    {
        [Fact]
        public void IsNotAnimatingAfterFullDuration()
        {
            _animator.Start(SomeChange(), 0.4f);
            _animator.Tick(0.4);
            Assert.False(_animator.IsAnimating);
        }

        [Fact]
        public void IsNotAnimatingAfterOvershoot()
        {
            _animator.Start(SomeChange(), 0.4f);
            _animator.Tick(2.0);
            Assert.False(_animator.IsAnimating);
        }

        [Fact]
        public void InFlightChangeClearsOnCompletion()
        {
            _animator.Start(SomeChange(), 0.4f);
            _animator.Tick(0.5);
            Assert.Null(_animator.InFlightChange);
        }

        [Fact]
        public void EmitsPaperFoldedEventExactlyOnce()
        {
            _animator.Start(SomeChange(), 0.4f);
            _animator.Tick(0.2); // mid-animation
            Assert.Equal(0, _completionEventCount);

            _animator.Tick(0.3); // overshoots completion
            Assert.Equal(1, _completionEventCount);

            _animator.Tick(0.1); // post-completion ticks must not re-emit
            Assert.Equal(1, _completionEventCount);
        }
    }

    [Collection(nameof(FoldAnimatorTests))]
    public class StartingAFreshAnimation : FoldAnimatorTests
    {
        [Fact]
        public void AfterCompletionAnotherStartWorks()
        {
            _animator.Start(SomeChange(), 0.4f);
            _animator.Tick(1.0); // completes

            var next = SomeChange();
            _animator.Start(next, 0.4f);

            Assert.True(_animator.IsAnimating);
            Assert.Same(next, _animator.InFlightChange);
        }
    }
}
