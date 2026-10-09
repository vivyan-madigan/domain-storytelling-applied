namespace TE.DomainStorytellingApplied.Domain.Tests;

public class VenueTests
{
    private static readonly OpeningHours EightToTen = new(new TimeOnly(8, 0), new TimeOnly(22, 0));

    private static readonly PriceList Prices = new(
        new Money(400m, "SEK"),
        new Money(200m, "SEK"),
        new Money(100m, "SEK"));

    private static Venue CreateVenue(string name = "Sporthallen Norr", int capacity = 30)
    {
        return new Venue(name, capacity, EightToTen, Prices);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyName_WhenCreating_ShouldThrow(string name)
    {
        Should.Throw<ArgumentException>(() => CreateVenue(name: name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenCapacityBelowOne_WhenCreating_ShouldThrow(int capacity)
    {
        Should.Throw<ArgumentException>(() => CreateVenue(capacity: capacity));
    }

    [Fact]
    public void GivenCapacityOfOne_WhenCreating_ShouldBeAllowed()
    {
        var venue = CreateVenue(capacity: 1);

        venue.Capacity.ShouldBe(1);
    }

    [Fact]
    public void GivenValidInput_WhenCreating_ShouldKeepAllValues()
    {
        var venue = CreateVenue();

        venue.Name.ShouldBe("Sporthallen Norr");
        venue.Capacity.ShouldBe(30);
        venue.OpeningHours.ShouldBe(EightToTen);
        venue.Prices.ShouldBe(Prices);
    }

    [Fact]
    public void GivenANewVenue_WhenCreating_ShouldGetAnId()
    {
        var venue = CreateVenue();

        venue.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void GivenTwoVenuesWithSameValues_WhenComparing_ShouldNotBeEqual()
    {
        var first = CreateVenue();
        var second = CreateVenue();

        first.ShouldNotBe(second);
        first.Id.ShouldNotBe(second.Id);
    }
}
