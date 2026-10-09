namespace TE.DomainStorytellingApplied.Domain.Tests;

public class PriceListTests
{
    private static Money Sek(decimal amount)
    {
        return new Money(amount, "SEK");
    }

    [Fact]
    public void GivenAssociationPriceInAnotherCurrency_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new PriceList(Sek(400m), new Money(20m, "EUR"), Sek(100m)));
    }

    [Fact]
    public void GivenRegionPriceInAnotherCurrency_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new PriceList(Sek(400m), Sek(200m), new Money(10m, "EUR")));
    }

    [Fact]
    public void GivenPricesInTheSameCurrency_WhenCreating_ShouldKeepAllPrices()
    {
        var prices = new PriceList(Sek(400m), Sek(200m), Sek(100m));

        prices.PrivatePerHour.ShouldBe(Sek(400m));
        prices.AssociationPerHour.ShouldBe(Sek(200m));
        prices.RegionPerHour.ShouldBe(Sek(100m));
    }

    [Fact]
    public void GivenTwoPriceListsWithSamePrices_WhenComparing_ShouldBeEqual()
    {
        var first = new PriceList(Sek(400m), Sek(200m), Sek(100m));
        var second = new PriceList(Sek(400m), Sek(200m), Sek(100m));

        first.ShouldBe(second);
    }
}
