namespace TE.DomainStorytellingApplied.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void GivenNegativeAmount_WhenCreating_ShouldThrow()
    {
        Should.Throw<ArgumentException>(() => new Money(-1m, "SEK"));
    }

    [Fact]
    public void GivenZeroAmount_WhenCreating_ShouldBeAllowed()
    {
        var money = new Money(0m, "SEK");

        money.Amount.ShouldBe(0m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyCurrency_WhenCreating_ShouldThrow(string currency)
    {
        Should.Throw<ArgumentException>(() => new Money(100m, currency));
    }

    [Fact]
    public void GivenValidAmountAndCurrency_WhenCreating_ShouldKeepBothValues()
    {
        var money = new Money(100m, "SEK");

        money.Amount.ShouldBe(100m);
        money.Currency.ShouldBe("SEK");
    }

    [Fact]
    public void GivenTwoMoneyWithSameAmountAndCurrency_WhenComparing_ShouldBeEqual()
    {
        var first = new Money(100m, "SEK");
        var second = new Money(100m, "SEK");

        first.ShouldBe(second);
    }
}
