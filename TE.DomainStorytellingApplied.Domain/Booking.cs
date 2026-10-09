namespace TE.DomainStorytellingApplied.Domain;

/// <summary>
/// An entity: one booking of a venue for a period of time.
/// A booking can only be created through Reserve, which checks the venue's rules first.
/// It remembers the venue by its Id and keeps its own copy of the fee.
/// </summary>
public sealed class Booking
{
    // How long a new booking holds its time before it must be paid.
    private static readonly TimeSpan ReservationHoldTime = TimeSpan.FromMinutes(15);

    // Private setters: a booking can change over time, but only through its own methods.
    public Guid Id { get; private set; }

    public Guid VenueId { get; private set; }

    public TimeSlot TimeSlot { get; private set; }

    public int ParticipantCount { get; private set; }

    public ContactDetails Contact { get; private set; }

    public Money Fee { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTime ReservedUntil { get; private set; }

    // Null until the booking is paid.
    public Payment? Payment { get; private set; }

    // Private: the only way to create a booking from outside is Reserve.
    private Booking(Guid venueId, TimeSlot timeSlot, int participantCount, ContactDetails contact, Money fee, DateTime reservedUntil)
    {
        Id = Guid.NewGuid();
        VenueId = venueId;
        TimeSlot = timeSlot;
        ParticipantCount = participantCount;
        Contact = contact;
        Fee = fee;
        Status = BookingStatus.Reserved;
        ReservedUntil = reservedUntil;
    }

    public static Booking Reserve(
        Venue venue,
        BookerType bookerType,
        TimeSlot timeSlot,
        int participantCount,
        ContactDetails contact,
        DateTime now)
    {
        if (!venue.CanBeBookedBy(bookerType))
        {
            throw new ArgumentException($"{venue.Name} cannot be booked by this type of booker.", nameof(bookerType));
        }

        if (!venue.IsOpenDuring(timeSlot))
        {
            throw new ArgumentException($"{venue.Name} is closed at that time.", nameof(timeSlot));
        }

        if (participantCount < 1)
        {
            throw new ArgumentException("A booking needs at least one participant.", nameof(participantCount));
        }

        if (participantCount > venue.Capacity)
        {
            throw new ArgumentException($"{venue.Name} allows a maximum of {venue.Capacity} people.", nameof(participantCount));
        }

        // The fee is copied, so a later price change on the venue does not touch this booking.
        var fee = venue.CalculateFee(timeSlot, bookerType);

        return new Booking(venue.Id, timeSlot, participantCount, contact, fee, now + ReservationHoldTime);
    }

    // Paying confirms the booking. The current time is passed in, so the rule can be tested without waiting.
    public void Pay(Payment payment, DateTime now)
    {
        if (Status != BookingStatus.Reserved)
        {
            throw new InvalidOperationException("Only a reserved booking can be paid.");
        }

        if (now > ReservedUntil)
        {
            throw new InvalidOperationException("The reservation has run out.");
        }

        if (payment.Amount != Fee)
        {
            throw new ArgumentException("The amount does not match the fee.", nameof(payment));
        }

        Payment = payment;
        Status = BookingStatus.Confirmed;
    }
}
