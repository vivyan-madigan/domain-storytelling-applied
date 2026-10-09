namespace TE.DomainStorytellingApplied.Domain.Tests;

/// <summary>
/// Ready-made domain objects that several test classes need.
/// </summary>
public static class TestData
{
    public static readonly OpeningHours OpeningHours = new(new TimeOnly(8, 0), new TimeOnly(22, 0));

    // Per hour: 400 SEK for private persons, 200 SEK for associations, 100 SEK for regions.
    public static readonly PriceList Prices = new(
        new Money(400m, "SEK"),
        new Money(200m, "SEK"),
        new Money(100m, "SEK"));

    // Sporthallen Norr: open 08:00 to 22:00, room for 30 people.
    // Private persons and associations may book. Regions may not.
    // Every call gives a new venue with its own Id.
    public static Venue CreateVenue(string name = "Sporthallen Norr", int capacity = 30)
    {
        return new Venue(name, capacity, OpeningHours, Prices, [BookerType.Private, BookerType.Association]);
    }
}
