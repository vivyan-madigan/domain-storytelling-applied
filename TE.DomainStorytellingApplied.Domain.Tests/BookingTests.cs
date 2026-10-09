namespace TE.DomainStorytellingApplied.Domain.Tests;

public class BookingTests
{
    // One hour, in the middle of the day.
    private static readonly TimeSlot Slot = new(
        new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc));

    private static readonly ContactDetails Contact = new("Anna Andersson", "anna@example.com", "070-000 00 00");

    // Open 08:00 to 22:00, room for 30 people. Private persons and associations may book. Regions may not.
    private static Venue CreateVenue()
    {
        return new Venue(
            "Sporthallen Norr",
            30,
            new OpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0)),
            new PriceList(new Money(400m, "SEK"), new Money(200m, "SEK"), new Money(100m, "SEK")),
            [BookerType.Private, BookerType.Association]);
    }

    [Fact]
    public void GivenValidInput_WhenReserving_ShouldCreateABookingForThatVenue()
    {
        var venue = CreateVenue();

        var booking = Booking.Reserve(venue, BookerType.Private, Slot, 10, Contact);

        booking.Id.ShouldNotBe(Guid.Empty);
        booking.VenueId.ShouldBe(venue.Id);
        booking.TimeSlot.ShouldBe(Slot);
        booking.ParticipantCount.ShouldBe(10);
        booking.Contact.ShouldBe(Contact);
    }

    [Fact]
    public void GivenAPrivateBookerAndAOneHourSlot_WhenReserving_ShouldCopyTheFeeFromTheVenue()
    {
        var booking = Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 10, Contact);

        booking.Fee.ShouldBe(new Money(400m, "SEK"));
    }

    [Fact]
    public void GivenABookerTypeTheVenueDoesNotAllow_WhenReserving_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Region, Slot, 10, Contact));
    }

    [Fact]
    public void GivenADeactivatedVenue_WhenReserving_ShouldThrow()
    {
        var venue = CreateVenue();
        venue.Deactivate();

        Should.Throw<ArgumentException>(() => Booking.Reserve(venue, BookerType.Private, Slot, 10, Contact));
    }

    [Fact]
    public void GivenATimeSlotOutsideOpeningHours_WhenReserving_ShouldThrow()
    {
        var beforeOpening = new TimeSlot(
            new DateTime(2026, 10, 20, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 7, 0, 0, DateTimeKind.Utc));

        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Private, beforeOpening, 10, Contact));
    }

    [Fact]
    public void GivenNoParticipants_WhenReserving_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 0, Contact));
    }

    [Fact]
    public void GivenMoreParticipantsThanCapacity_WhenReserving_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 31, Contact));
    }

    [Fact]
    public void GivenExactlyAsManyParticipantsAsCapacity_WhenReserving_ShouldBeAllowed()
    {
        var booking = Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 30, Contact);

        booking.ParticipantCount.ShouldBe(30);
    }
}
