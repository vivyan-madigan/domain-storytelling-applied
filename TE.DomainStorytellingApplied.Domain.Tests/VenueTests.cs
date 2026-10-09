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

    [Fact]
    public void GivenATimeSlotWithinOpeningHours_WhenAskingIsOpenDuring_ShouldBeTrue()
    {
        var venue = CreateVenue();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc));

        venue.IsOpenDuring(timeSlot).ShouldBeTrue();
    }

    [Fact]
    public void GivenATimeSlotBeforeOpening_WhenAskingIsOpenDuring_ShouldBeFalse()
    {
        var venue = CreateVenue();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 10, 20, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 7, 0, 0, DateTimeKind.Utc));

        venue.IsOpenDuring(timeSlot).ShouldBeFalse();
    }

    [Theory]
    [InlineData(BookerType.Private, 400)]
    [InlineData(BookerType.Association, 200)]
    [InlineData(BookerType.Region, 100)]
    public void GivenAOneHourSlot_WhenCalculatingFee_ShouldBeTheHourlyPriceForThatBookerType(BookerType bookerType, int expectedAmount)
    {
        var venue = CreateVenue();
        var oneHour = SlotStartingAtTen(minutes: 60);

        venue.CalculateFee(oneHour, bookerType).ShouldBe(new Money(expectedAmount, "SEK"));
    }

    [Fact]
    public void GivenANinetyMinuteSlot_WhenCalculatingFee_ShouldBeOneAndAHalfTimesTheHourlyPrice()
    {
        var venue = CreateVenue();
        var ninetyMinutes = SlotStartingAtTen(minutes: 90);

        venue.CalculateFee(ninetyMinutes, BookerType.Private).ShouldBe(new Money(600m, "SEK"));
    }

    [Fact]
    public void GivenAFiftyMinuteSlot_WhenCalculatingFee_ShouldBeRoundedToTwoDecimals()
    {
        var venue = CreateVenue();
        var fiftyMinutes = SlotStartingAtTen(minutes: 50);

        // 400 * 50 / 60 = 333.333..., which is rounded to 333.33.
        venue.CalculateFee(fiftyMinutes, BookerType.Private).ShouldBe(new Money(333.33m, "SEK"));
    }

    private static TimeSlot SlotStartingAtTen(int minutes)
    {
        var start = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        return new TimeSlot(start, start.AddMinutes(minutes));
    }
}
