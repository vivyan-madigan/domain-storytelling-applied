namespace TE.DomainStorytellingApplied.Domain.Tests;

public class PaymentTests
{
    private static readonly DateTime PaidAt = new(2026, 10, 12, 9, 5, 0, DateTimeKind.Utc);

    [Fact]
    public void GivenAmountMethodAndTime_WhenCreating_ShouldKeepAllValues()
    {
        var payment = new Payment(new Money(400m, "SEK"), PaymentMethod.Swish, PaidAt);

        payment.Amount.ShouldBe(new Money(400m, "SEK"));
        payment.Method.ShouldBe(PaymentMethod.Swish);
        payment.PaidAt.ShouldBe(PaidAt);
    }

    [Fact]
    public void GivenTwoPaymentsWithSameValues_WhenComparing_ShouldBeEqual()
    {
        var first = new Payment(new Money(400m, "SEK"), PaymentMethod.Swish, PaidAt);
        var second = new Payment(new Money(400m, "SEK"), PaymentMethod.Swish, PaidAt);

        first.ShouldBe(second);
    }

    [Fact]
    public void GivenTwoPaymentsWithDifferentMethods_WhenComparing_ShouldNotBeEqual()
    {
        var swish = new Payment(new Money(400m, "SEK"), PaymentMethod.Swish, PaidAt);
        var card = new Payment(new Money(400m, "SEK"), PaymentMethod.Card, PaidAt);

        swish.ShouldNotBe(card);
    }
}
