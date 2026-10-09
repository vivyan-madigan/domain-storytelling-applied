namespace TE.DomainStorytellingApplied.Domain;

/// <summary>
/// An entity: a place that can be booked, for example a sports hall.
/// It is a class and not a record, because a venue is identified by its Id and not by its values.
/// Two venues with the same name and capacity are still two different venues.
/// </summary>
public sealed class Venue
{
    private readonly List<BookerType> _allowedBookerTypes;

    // Private setters: a venue can change over time, but only through its own methods.
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int Capacity { get; private set; }

    public OpeningHours OpeningHours { get; private set; }

    public PriceList Prices { get; private set; }

    public bool IsActive { get; private set; }

    public Venue(string name, int capacity, OpeningHours openingHours, PriceList prices, IEnumerable<BookerType> allowedBookerTypes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A venue must have a name.", nameof(name));
        }

        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be at least 1.", nameof(capacity));
        }

        Id = Guid.NewGuid();
        Name = name;
        Capacity = capacity;
        OpeningHours = openingHours;
        Prices = prices;
        IsActive = true;

        // A copy, so nobody can change the allowed types from outside after the venue is created.
        _allowedBookerTypes = new List<BookerType>(allowedBookerTypes);
    }

    public bool CanBeBookedBy(BookerType bookerType)
    {
        return IsActive && _allowedBookerTypes.Contains(bookerType);
    }

    public bool IsOpenDuring(TimeSlot timeSlot)
    {
        return OpeningHours.Covers(timeSlot);
    }

    // The fee is the hourly price for this type of booker, times the length of the slot in hours.
    public Money CalculateFee(TimeSlot timeSlot, BookerType bookerType)
    {
        var hours = (decimal)timeSlot.Duration.TotalHours;

        return Prices.PerHourFor(bookerType).Multiply(hours);
    }

    // A deactivated venue cannot be booked. Bookings already made are not touched.
    public void Deactivate()
    {
        IsActive = false;
    }
}
