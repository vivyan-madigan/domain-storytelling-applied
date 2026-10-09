namespace TE.DomainStorytellingApplied.Domain.Tests;

public class OpeningHoursTests
{
    [Fact]
    public void GivenClosesBeforeOpens_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new OpeningHours(new TimeOnly(22, 0), new TimeOnly(8, 0)));
    }

    [Fact]
    public void GivenClosesEqualToOpens_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new OpeningHours(new TimeOnly(8, 0), new TimeOnly(8, 0)));
    }

    [Fact]
    public void GivenValidOpensAndCloses_WhenCreating_ShouldKeepBothValues()
    {
        var openingHours = new OpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0));

        openingHours.Opens.ShouldBe(new TimeOnly(8, 0));
        openingHours.Closes.ShouldBe(new TimeOnly(22, 0));
    }

    [Fact]
    public void GivenTwoOpeningHoursWithSameValues_WhenComparing_ShouldBeEqual()
    {
        var first = new OpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0));
        var second = new OpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0));

        first.ShouldBe(second);
    }
}
