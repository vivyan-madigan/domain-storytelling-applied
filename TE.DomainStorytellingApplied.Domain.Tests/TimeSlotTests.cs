namespace TE.DomainStorytellingApplied.Domain.Tests;

public class TimeSlotTests
{
    private static DateTime At(int hour)
    {
        return new DateTime(2026, 10, 20, hour, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void GivenEndBeforeStart_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new TimeSlot(At(11), At(10)));
    }

    [Fact]
    public void GivenEndEqualToStart_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new TimeSlot(At(10), At(10)));
    }

    [Fact]
    public void GivenValidStartAndEnd_WhenCreating_ShouldKeepBothValues()
    {
        var slot = new TimeSlot(At(10), At(11));

        slot.Start.ShouldBe(At(10));
        slot.End.ShouldBe(At(11));
    }

    [Fact]
    public void GivenTwoTimeSlotsWithSameStartAndEnd_WhenComparing_ShouldBeEqual()
    {
        var first = new TimeSlot(At(10), At(11));
        var second = new TimeSlot(At(10), At(11));

        first.ShouldBe(second);
    }

    [Fact]
    public void GivenAOneHourSlot_WhenAskingForDuration_ShouldBeOneHour()
    {
        var slot = new TimeSlot(At(10), At(11));

        slot.Duration.ShouldBe(TimeSpan.FromHours(1));
    }
}
