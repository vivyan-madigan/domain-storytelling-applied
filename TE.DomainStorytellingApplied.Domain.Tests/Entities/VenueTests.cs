namespace TE.DomainStorytellingApplied.Domain.Tests;

public class VenueTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyName_WhenCreating_ShouldThrow(string name)
    {
        Should.Throw<ArgumentException>(() => TestData.CreateVenue(name: name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenCapacityBelowOne_WhenCreating_ShouldThrow(int capacity)
    {
        Should.Throw<ArgumentException>(() => TestData.CreateVenue(capacity: capacity));
    }

    [Fact]
    public void GivenCapacityOfOne_WhenCreating_ShouldBeAllowed()
    {
        var venue = TestData.CreateVenue(capacity: 1);

        venue.Capacity.ShouldBe(1);
    }

    [Fact]
    public void GivenValidInput_WhenCreating_ShouldKeepAllValues()
    {
        var venue = TestData.CreateVenue();

        venue.Name.ShouldBe("Sporthallen Norr");
        venue.Capacity.ShouldBe(30);
        venue.OpeningHours.ShouldBe(TestData.OpeningHours);
        venue.Prices.ShouldBe(TestData.Prices);
    }

    [Fact]
    public void GivenANewVenue_WhenCreating_ShouldGetAnId()
    {
        var venue = TestData.CreateVenue();

        venue.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void GivenTwoVenuesWithSameValues_WhenComparing_ShouldNotBeEqual()
    {
        var first = TestData.CreateVenue();
        var second = TestData.CreateVenue();

        first.ShouldNotBe(second);
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void GivenATimeSlotWithinOpeningHours_WhenAskingIsOpenDuring_ShouldBeTrue()
    {
        var venue = TestData.CreateVenue();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc));

        venue.IsOpenDuring(timeSlot).ShouldBeTrue();
    }

    [Fact]
    public void GivenATimeSlotBeforeOpening_WhenAskingIsOpenDuring_ShouldBeFalse()
    {
        var venue = TestData.CreateVenue();
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
        var venue = TestData.CreateVenue();
        var oneHour = SlotStartingAtTen(minutes: 60);

        venue.CalculateFee(oneHour, bookerType).ShouldBe(new Money(expectedAmount, "SEK"));
    }

    [Fact]
    public void GivenANinetyMinuteSlot_WhenCalculatingFee_ShouldBeOneAndAHalfTimesTheHourlyPrice()
    {
        var venue = TestData.CreateVenue();
        var ninetyMinutes = SlotStartingAtTen(minutes: 90);

        venue.CalculateFee(ninetyMinutes, BookerType.Private).ShouldBe(new Money(600m, "SEK"));
    }

    [Fact]
    public void GivenAFiftyMinuteSlot_WhenCalculatingFee_ShouldBeRoundedToTwoDecimals()
    {
        var venue = TestData.CreateVenue();
        var fiftyMinutes = SlotStartingAtTen(minutes: 50);

        // 400 * 50 / 60 = 333.333..., which is rounded to 333.33.
        venue.CalculateFee(fiftyMinutes, BookerType.Private).ShouldBe(new Money(333.33m, "SEK"));
    }

    [Fact]
    public void GivenANewVenue_WhenCreating_ShouldBeActive()
    {
        var venue = TestData.CreateVenue();

        venue.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(BookerType.Private)]
    [InlineData(BookerType.Association)]
    public void GivenAnAllowedBookerType_WhenAskingCanBeBookedBy_ShouldBeTrue(BookerType bookerType)
    {
        var venue = TestData.CreateVenue();

        venue.CanBeBookedBy(bookerType).ShouldBeTrue();
    }

    [Fact]
    public void GivenABookerTypeThatIsNotAllowed_WhenAskingCanBeBookedBy_ShouldBeFalse()
    {
        var venue = TestData.CreateVenue();

        venue.CanBeBookedBy(BookerType.Region).ShouldBeFalse();
    }

    [Fact]
    public void GivenADeactivatedVenue_WhenAskingCanBeBookedBy_ShouldBeFalse()
    {
        var venue = TestData.CreateVenue();

        venue.Deactivate();

        venue.IsActive.ShouldBeFalse();
        venue.CanBeBookedBy(BookerType.Private).ShouldBeFalse();
    }

    // ----- Change opening hours -----

    [Fact]
    public void GivenNoBookings_WhenChangingOpeningHours_ShouldReplaceThemAndStayTheSameVenue()
    {
        var venue = TestData.CreateVenue();
        var idBefore = venue.Id;

        venue.ChangeOpeningHours(ShorterHours, [], Now);

        venue.OpeningHours.ShouldBe(ShorterHours);
        venue.Id.ShouldBe(idBefore);
    }

    [Fact]
    public void GivenChangedOpeningHours_WhenAskingIsOpenDuring_ShouldAnswerWithTheNewHours()
    {
        var venue = TestData.CreateVenue();

        venue.ChangeOpeningHours(ShorterHours, [], Now);

        // 20:00 to 21:00 was open before the change, when the venue closed at 22:00.
        venue.IsOpenDuring(EveningSlot).ShouldBeFalse();
    }

    [Fact]
    public void GivenABookingOutsideTheNewHours_WhenChangingOpeningHours_ShouldThrowAndKeepTheOldHours()
    {
        var venue = TestData.CreateVenue();
        var eveningBooking = ReserveBooking(venue, EveningSlot);

        Should.Throw<InvalidOperationException>(() => venue.ChangeOpeningHours(ShorterHours, [eveningBooking], Now));

        venue.OpeningHours.ShouldBe(TestData.OpeningHours);
    }

    [Fact]
    public void GivenABookingWithinTheNewHours_WhenChangingOpeningHours_ShouldBeAllowed()
    {
        var venue = TestData.CreateVenue();
        var middayBooking = ReserveBooking(venue, SlotStartingAtTen(minutes: 60));

        venue.ChangeOpeningHours(ShorterHours, [middayBooking], Now);

        venue.OpeningHours.ShouldBe(ShorterHours);
    }

    [Fact]
    public void GivenACancelledBookingOutsideTheNewHours_WhenChangingOpeningHours_ShouldBeAllowed()
    {
        var venue = TestData.CreateVenue();
        var eveningBooking = ReserveBooking(venue, EveningSlot);
        eveningBooking.Cancel(Now);

        venue.ChangeOpeningHours(ShorterHours, [eveningBooking], Now);

        venue.OpeningHours.ShouldBe(ShorterHours);
    }

    [Fact]
    public void GivenABookingOutsideTheNewHoursThatIsOver_WhenChangingOpeningHours_ShouldBeAllowed()
    {
        var venue = TestData.CreateVenue();
        var eveningBooking = ReserveBooking(venue, EveningSlot);
        var theDayAfter = EveningSlot.End.AddDays(1);

        venue.ChangeOpeningHours(ShorterHours, [eveningBooking], theDayAfter);

        venue.OpeningHours.ShouldBe(ShorterHours);
    }

    [Fact]
    public void GivenABookingOutsideTheNewHoursInAnotherVenue_WhenChangingOpeningHours_ShouldBeAllowed()
    {
        var venue = TestData.CreateVenue();
        var otherVenue = TestData.CreateVenue();
        var eveningBooking = ReserveBooking(otherVenue, EveningSlot);

        venue.ChangeOpeningHours(ShorterHours, [eveningBooking], Now);

        venue.OpeningHours.ShouldBe(ShorterHours);
    }

    [Fact]
    public void GivenADeactivatedVenue_WhenChangingOpeningHours_ShouldThrow()
    {
        var venue = TestData.CreateVenue();
        venue.Deactivate();

        Should.Throw<InvalidOperationException>(() => venue.ChangeOpeningHours(ShorterHours, [], Now));
    }

    private static readonly DateTime Now = new(2026, 10, 12, 9, 0, 0, DateTimeKind.Utc);

    // Open 09:00 to 20:00, where TestData has 08:00 to 22:00.
    private static readonly OpeningHours ShorterHours = new(new TimeOnly(9, 0), new TimeOnly(20, 0));

    // 20:00 to 21:00: open with the hours in TestData, closed with ShorterHours.
    private static readonly TimeSlot EveningSlot = new(
        new DateTime(2026, 10, 20, 20, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 20, 21, 0, 0, DateTimeKind.Utc));

    private static Booking ReserveBooking(Venue venue, TimeSlot timeSlot)
    {
        var contact = new ContactDetails("Anna Andersson", "anna@example.com", "070-000 00 00");

        return Booking.Reserve(venue, BookerType.Private, timeSlot, 10, contact, Now);
    }

    private static TimeSlot SlotStartingAtTen(int minutes)
    {
        var start = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        return new TimeSlot(start, start.AddMinutes(minutes));
    }
}
