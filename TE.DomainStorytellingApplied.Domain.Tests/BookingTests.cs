namespace TE.DomainStorytellingApplied.Domain.Tests;

public class BookingTests
{
    // The moment the booking is reserved, eight days before the slot.
    private static readonly DateTime Now = new(2026, 10, 12, 9, 0, 0, DateTimeKind.Utc);

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

    // A valid booking with a fee of 400 SEK, reserved at Now.
    private static Booking ReserveBooking()
    {
        return Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 10, Contact, Now);
    }

    // The same booking, paid straight away.
    private static Booking ReserveAndPayBooking()
    {
        var booking = ReserveBooking();
        booking.Pay(SwishPayment(400m, Now), Now);

        return booking;
    }

    private static Payment SwishPayment(decimal amount, DateTime paidAt)
    {
        return new Payment(new Money(amount, "SEK"), PaymentMethod.Swish, paidAt);
    }

    // ----- Reserve -----

    [Fact]
    public void GivenValidInput_WhenReserving_ShouldCreateABookingForThatVenue()
    {
        var venue = CreateVenue();

        var booking = Booking.Reserve(venue, BookerType.Private, Slot, 10, Contact, Now);

        booking.Id.ShouldNotBe(Guid.Empty);
        booking.VenueId.ShouldBe(venue.Id);
        booking.TimeSlot.ShouldBe(Slot);
        booking.ParticipantCount.ShouldBe(10);
        booking.Contact.ShouldBe(Contact);
    }

    [Fact]
    public void GivenAPrivateBookerAndAOneHourSlot_WhenReserving_ShouldCopyTheFeeFromTheVenue()
    {
        var booking = Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 10, Contact, Now);

        booking.Fee.ShouldBe(new Money(400m, "SEK"));
    }

    [Fact]
    public void GivenABookerTypeTheVenueDoesNotAllow_WhenReserving_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Region, Slot, 10, Contact, Now));
    }

    [Fact]
    public void GivenADeactivatedVenue_WhenReserving_ShouldThrow()
    {
        var venue = CreateVenue();
        venue.Deactivate();

        Should.Throw<ArgumentException>(() => Booking.Reserve(venue, BookerType.Private, Slot, 10, Contact, Now));
    }

    [Fact]
    public void GivenATimeSlotOutsideOpeningHours_WhenReserving_ShouldThrow()
    {
        var beforeOpening = new TimeSlot(
            new DateTime(2026, 10, 20, 6, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 7, 0, 0, DateTimeKind.Utc));

        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Private, beforeOpening, 10, Contact, Now));
    }

    [Fact]
    public void GivenNoParticipants_WhenReserving_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 0, Contact, Now));
    }

    [Fact]
    public void GivenMoreParticipantsThanCapacity_WhenReserving_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 31, Contact, Now));
    }

    [Fact]
    public void GivenExactlyAsManyParticipantsAsCapacity_WhenReserving_ShouldBeAllowed()
    {
        var booking = Booking.Reserve(CreateVenue(), BookerType.Private, Slot, 30, Contact, Now);

        booking.ParticipantCount.ShouldBe(30);
    }

    [Fact]
    public void GivenANewBooking_WhenReserving_ShouldBeReservedAndHeldForFifteenMinutes()
    {
        var booking = ReserveBooking();

        booking.Status.ShouldBe(BookingStatus.Reserved);
        booking.ReservedUntil.ShouldBe(Now.AddMinutes(15));
        booking.Payment.ShouldBeNull();
    }

    // ----- Pay -----

    [Fact]
    public void GivenAPaymentWithinTheHoldTime_WhenPaying_ShouldConfirmTheBooking()
    {
        var booking = ReserveBooking();
        var paidAt = Now.AddMinutes(5);
        var payment = SwishPayment(400m, paidAt);

        booking.Pay(payment, paidAt);

        booking.Status.ShouldBe(BookingStatus.Confirmed);
        booking.Payment.ShouldBe(payment);
    }

    [Fact]
    public void GivenAPaymentExactlyWhenTheHoldEnds_WhenPaying_ShouldBeAllowed()
    {
        var booking = ReserveBooking();
        var paidAt = Now.AddMinutes(15);

        booking.Pay(SwishPayment(400m, paidAt), paidAt);

        booking.Status.ShouldBe(BookingStatus.Confirmed);
    }

    [Fact]
    public void GivenAPaymentAfterTheHoldTime_WhenPaying_ShouldThrow()
    {
        var booking = ReserveBooking();
        var paidAt = Now.AddMinutes(16);

        Should.Throw<InvalidOperationException>(() => booking.Pay(SwishPayment(400m, paidAt), paidAt));
    }

    [Fact]
    public void GivenAPaymentWithTheWrongAmount_WhenPaying_ShouldThrow()
    {
        var booking = ReserveBooking();

        Should.Throw<ArgumentException>(() => booking.Pay(SwishPayment(399m, Now), Now));
    }

    [Fact]
    public void GivenAnAlreadyPaidBooking_WhenPayingAgain_ShouldThrow()
    {
        var booking = ReserveBooking();
        booking.Pay(SwishPayment(400m, Now), Now);

        Should.Throw<InvalidOperationException>(() => booking.Pay(SwishPayment(400m, Now), Now));
    }

    [Fact]
    public void GivenAFailedPayment_WhenPaying_ShouldLeaveTheBookingReserved()
    {
        var booking = ReserveBooking();

        Should.Throw<ArgumentException>(() => booking.Pay(SwishPayment(399m, Now), Now));

        booking.Status.ShouldBe(BookingStatus.Reserved);
        booking.Payment.ShouldBeNull();
    }

    // ----- Domain events -----

    [Fact]
    public void GivenANewBooking_WhenReserving_ShouldHaveNoDomainEvents()
    {
        var booking = ReserveBooking();

        booking.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAPaymentWithinTheHoldTime_WhenPaying_ShouldRecordBookingConfirmed()
    {
        var booking = ReserveBooking();

        booking.Pay(SwishPayment(400m, Now), Now);

        var confirmed = booking.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<BookingConfirmed>();
        confirmed.BookingId.ShouldBe(booking.Id);
        confirmed.ContactEmail.ShouldBe("anna@example.com");
        confirmed.Fee.ShouldBe(new Money(400m, "SEK"));
    }

    [Fact]
    public void GivenAFailedPayment_WhenPaying_ShouldNotRecordAnEvent()
    {
        var booking = ReserveBooking();

        Should.Throw<ArgumentException>(() => booking.Pay(SwishPayment(399m, Now), Now));

        booking.DomainEvents.ShouldBeEmpty();
    }

    // ----- Cancel -----

    [Fact]
    public void GivenAReservedBooking_WhenCancelling_ShouldBeCancelledAndNotLate()
    {
        var booking = ReserveBooking();

        booking.Cancel(Now);

        booking.Status.ShouldBe(BookingStatus.Cancelled);
        booking.IsLateCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenAConfirmedBookingMoreThan48HoursBeforeStart_WhenCancelling_ShouldNotBeLate()
    {
        var booking = ReserveAndPayBooking();

        booking.Cancel(Slot.Start.AddHours(-49));

        booking.Status.ShouldBe(BookingStatus.Cancelled);
        booking.IsLateCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenAConfirmedBookingExactly48HoursBeforeStart_WhenCancelling_ShouldNotBeLate()
    {
        var booking = ReserveAndPayBooking();

        booking.Cancel(Slot.Start.AddHours(-48));

        booking.IsLateCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenAConfirmedBookingLessThan48HoursBeforeStart_WhenCancelling_ShouldBeLate()
    {
        var booking = ReserveAndPayBooking();

        booking.Cancel(Slot.Start.AddHours(-47));

        booking.Status.ShouldBe(BookingStatus.Cancelled);
        booking.IsLateCancellation.ShouldBeTrue();
    }

    [Fact]
    public void GivenAnUnpaidBookingLessThan48HoursBeforeStart_WhenCancelling_ShouldNotBeLate()
    {
        var booking = ReserveBooking();

        booking.Cancel(Slot.Start.AddHours(-1));

        booking.IsLateCancellation.ShouldBeFalse();
    }

    [Fact]
    public void GivenACancelledBooking_WhenCancellingAgain_ShouldThrow()
    {
        var booking = ReserveBooking();
        booking.Cancel(Now);

        Should.Throw<InvalidOperationException>(() => booking.Cancel(Now));
    }

    [Fact]
    public void GivenACancelledBooking_WhenPaying_ShouldThrow()
    {
        var booking = ReserveBooking();
        booking.Cancel(Now);

        Should.Throw<InvalidOperationException>(() => booking.Pay(SwishPayment(400m, Now), Now));
    }

    // ----- Expire -----

    [Fact]
    public void GivenAnUnpaidBookingAfterTheHoldTime_WhenExpiring_ShouldBeExpired()
    {
        var booking = ReserveBooking();

        booking.Expire(Now.AddMinutes(16));

        booking.Status.ShouldBe(BookingStatus.Expired);
    }

    [Fact]
    public void GivenAnUnpaidBookingExactlyWhenTheHoldEnds_WhenExpiring_ShouldStayReserved()
    {
        var booking = ReserveBooking();

        booking.Expire(Now.AddMinutes(15));

        booking.Status.ShouldBe(BookingStatus.Reserved);
    }

    [Fact]
    public void GivenAnUnpaidBookingWithinTheHoldTime_WhenExpiring_ShouldStayReserved()
    {
        var booking = ReserveBooking();

        booking.Expire(Now.AddMinutes(5));

        booking.Status.ShouldBe(BookingStatus.Reserved);
    }

    [Fact]
    public void GivenAConfirmedBooking_WhenExpiring_ShouldStayConfirmed()
    {
        var booking = ReserveAndPayBooking();

        booking.Expire(Now.AddMinutes(16));

        booking.Status.ShouldBe(BookingStatus.Confirmed);
    }

    [Fact]
    public void GivenACancelledBooking_WhenExpiring_ShouldStayCancelled()
    {
        var booking = ReserveBooking();
        booking.Cancel(Now);

        booking.Expire(Now.AddMinutes(16));

        booking.Status.ShouldBe(BookingStatus.Cancelled);
    }

    [Fact]
    public void GivenAnExpiredBooking_WhenPaying_ShouldThrow()
    {
        var booking = ReserveBooking();
        var later = Now.AddMinutes(16);
        booking.Expire(later);

        Should.Throw<InvalidOperationException>(() => booking.Pay(SwishPayment(400m, later), later));
    }

    [Fact]
    public void GivenAnExpiredBooking_WhenCancelling_ShouldThrow()
    {
        var booking = ReserveBooking();
        var later = Now.AddMinutes(16);
        booking.Expire(later);

        Should.Throw<InvalidOperationException>(() => booking.Cancel(later));
    }
}
