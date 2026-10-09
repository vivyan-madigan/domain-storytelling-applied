namespace TE.DomainStorytellingApplied.Domain;

/// <summary>
/// An entity: a place that can be booked, for example a sports hall.
/// It is a class and not a record, because a venue is identified by its Id and not by its values.
/// Two venues with the same name and capacity are still two different venues.
/// </summary>
public sealed class Venue
{
    // Private setters: a venue can change over time, but only through its own methods.
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int Capacity { get; private set; }

    public OpeningHours OpeningHours { get; private set; }

    public PriceList Prices { get; private set; }

    public Venue(string name, int capacity, OpeningHours openingHours, PriceList prices)
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
    }

    public bool IsOpenDuring(TimeSlot timeSlot)
    {
        return OpeningHours.Covers(timeSlot);
    }
}
