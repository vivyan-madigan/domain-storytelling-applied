namespace TE.DomainStorytellingApplied.Domain.Tests;

public class VenueAvailabilityServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 12, 9, 0, 0, DateTimeKind.Utc);

    // The slot somebody wants to book: 10:00 to 11:00.
    private static readonly TimeSlot RequestedSlot = SlotBetween(10, 11);

    private static TimeSlot SlotBetween(int startHour, int endHour)
    {
        return new TimeSlot(
            new DateTime(2026, 10, 20, startHour, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, endHour, 0, 0, DateTimeKind.Utc));
    }

    private static Venue CreateVenue()
    {
        return new Venue(
            "Sporthallen Norr",
            30,
            new OpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0)),
            new PriceList(new Money(400m, "SEK"), new Money(200m, "SEK"), new Money(100m, "SEK")),
            [BookerType.Private, BookerType.Association]);
    }

    // An unpaid booking of the venue, with a fee of 400 SEK per hour.
    private static Booking ReserveBooking(Venue venue, TimeSlot timeSlot)
    {
        var contact = new ContactDetails("Anna Andersson", "anna@example.com", "070-000 00 00");

        return Booking.Reserve(venue, BookerType.Private, timeSlot, 10, contact, Now);
    }

    [Fact]
    public void GivenNoBookings_WhenCheckingAvailability_ShouldBeAvailable()
    {
        var venue = CreateVenue();

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, []).ShouldBeTrue();
    }

    [Fact]
    public void GivenAReservedBookingAtTheSameTime_WhenCheckingAvailability_ShouldNotBeAvailable()
    {
        var venue = CreateVenue();
        var existing = ReserveBooking(venue, RequestedSlot);

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeFalse();
    }

    [Fact]
    public void GivenAConfirmedBookingAtTheSameTime_WhenCheckingAvailability_ShouldNotBeAvailable()
    {
        var venue = CreateVenue();
        var existing = ReserveBooking(venue, RequestedSlot);
        existing.Pay(new Payment(existing.Fee, PaymentMethod.Swish, Now), Now);

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeFalse();
    }

    [Fact]
    public void GivenACancelledBookingAtTheSameTime_WhenCheckingAvailability_ShouldBeAvailable()
    {
        var venue = CreateVenue();
        var existing = ReserveBooking(venue, RequestedSlot);
        existing.Cancel(Now);

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeTrue();
    }

    [Fact]
    public void GivenAnExpiredBookingAtTheSameTime_WhenCheckingAvailability_ShouldBeAvailable()
    {
        var venue = CreateVenue();
        var existing = ReserveBooking(venue, RequestedSlot);
        existing.Expire(Now.AddMinutes(16));

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeTrue();
    }

    [Fact]
    public void GivenABookingThatEndsWhenTheRequestedSlotStarts_WhenCheckingAvailability_ShouldBeAvailable()
    {
        var venue = CreateVenue();
        var existing = ReserveBooking(venue, SlotBetween(9, 10));

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeTrue();
    }

    [Fact]
    public void GivenABookingThatPartlyOverlaps_WhenCheckingAvailability_ShouldNotBeAvailable()
    {
        var venue = CreateVenue();
        var existing = ReserveBooking(venue, SlotBetween(9, 11));

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeFalse();
    }

    [Fact]
    public void GivenABookingAtTheSameTimeInAnotherVenue_WhenCheckingAvailability_ShouldBeAvailable()
    {
        var venue = CreateVenue();
        var otherVenue = CreateVenue();
        var existing = ReserveBooking(otherVenue, RequestedSlot);

        VenueAvailabilityService.IsAvailable(venue.Id, RequestedSlot, [existing]).ShouldBeTrue();
    }
}
