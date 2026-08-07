using System;
using valleyfold.Templates.Events;
using valleyfold.Ui;
using valleyfold.Utils;
using Xunit;

namespace Testing.Ui;

// DeskSelection emits a global EventBus event on Begin, so tests share the
// bus and unsubscribe on dispose like TemplateSessionTests.
[CollectionDefinition(nameof(DeskSelectionTests), DisableParallelization = true)]
public class DeskSelectionCollection { }

[Collection(nameof(DeskSelectionTests))]
public class DeskSelectionTests : IDisposable
{
    private readonly Action<StartTemplateSessionEvent> _handler;
    private StartTemplateSessionEvent? _lastEvent;
    private int _eventCount;

    protected DeskSelectionTests()
    {
        _handler = e => { _lastEvent = e; _eventCount++; };
        EventBus.Register<StartTemplateSessionEvent>(_handler);
    }

    public void Dispose()
    {
        EventBus.Deregister<StartTemplateSessionEvent>(_handler);
    }

    public class Defaults : DeskSelectionTests
    {
        [Fact]
        public void NothingSelectedAndCannotBegin()
        {
            var selection = new DeskSelection();

            Assert.Empty(selection.SelectedTemplateId);
            Assert.Empty(selection.SelectedPaperId);
            Assert.False(selection.CanBegin);
        }

        [Fact]
        public void DefaultPaperIsSelectable()
        {
            var selection = new DeskSelection();

            Assert.True(selection.IsPaperSelectable(DeskSelection.DefaultPaperId));
        }

        [Fact]
        public void LockedPaperIsNotSelectable()
        {
            var selection = new DeskSelection();

            Assert.False(selection.IsPaperSelectable(DeskSelection.LockedPaperId));
        }
    }

    public class Selecting : DeskSelectionTests
    {
        [Fact]
        public void BoatAndDefaultPaperCanBegin()
        {
            var selection = new DeskSelection();
            selection.SelectTemplate(DeskSelection.BoatTemplateId);
            selection.SelectPaper(DeskSelection.DefaultPaperId);

            Assert.True(selection.CanBegin);
        }

        [Fact]
        public void LockedPaperCannotBeSelected()
        {
            var selection = new DeskSelection();
            selection.SelectTemplate(DeskSelection.BoatTemplateId);
            selection.SelectPaper(DeskSelection.LockedPaperId);

            Assert.Empty(selection.SelectedPaperId);
            Assert.False(selection.CanBegin);
        }
    }

    public class Beginning : DeskSelectionTests
    {
        [Fact]
        public void WithoutSelection_DoesNotEmit()
        {
            var selection = new DeskSelection();
            selection.Begin();

            Assert.Equal(0, _eventCount);
        }

        [Fact]
        public void WithSelection_EmitsStartTemplateSessionEvent()
        {
            var selection = new DeskSelection();
            selection.SelectTemplate(DeskSelection.BoatTemplateId);
            selection.SelectPaper(DeskSelection.DefaultPaperId);
            selection.Begin();

            Assert.Equal(1, _eventCount);
            Assert.Equal(DeskSelection.BoatTemplateId, _lastEvent!.TemplateId);
            Assert.Equal(DeskSelection.DefaultPaperId, _lastEvent.PaperId);
        }
    }
}
