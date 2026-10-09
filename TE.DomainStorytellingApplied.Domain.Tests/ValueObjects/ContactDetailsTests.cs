namespace TE.DomainStorytellingApplied.Domain.Tests;

public class ContactDetailsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenEmptyName_WhenCreating_ShouldThrow(string name)
    {
        Should.Throw<ArgumentException>(() => new ContactDetails(name, "anna@example.com", "070-000 00 00"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("anna.example.com")]
    public void GivenEmailWithoutAtSign_WhenCreating_ShouldThrow(string email)
    {
        Should.Throw<ArgumentException>(() => new ContactDetails("Anna Andersson", email, "070-000 00 00"));
    }

    [Fact]
    public void GivenValidDetails_WhenCreating_ShouldKeepAllValues()
    {
        var contact = new ContactDetails("Anna Andersson", "anna@example.com", "070-000 00 00");

        contact.Name.ShouldBe("Anna Andersson");
        contact.Email.ShouldBe("anna@example.com");
        contact.Phone.ShouldBe("070-000 00 00");
    }

    [Fact]
    public void GivenTwoContactDetailsWithSameValues_WhenComparing_ShouldBeEqual()
    {
        var first = new ContactDetails("Anna Andersson", "anna@example.com", "070-000 00 00");
        var second = new ContactDetails("Anna Andersson", "anna@example.com", "070-000 00 00");

        first.ShouldBe(second);
    }
}
