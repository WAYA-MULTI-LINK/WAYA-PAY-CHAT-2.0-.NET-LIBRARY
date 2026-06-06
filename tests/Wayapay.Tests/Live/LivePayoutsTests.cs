using Wayapay.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayapay.Tests.Live;

[Trait("Category", "Live")]
public sealed class LivePayoutsTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ListBanks_ReturnsBanks()
    {
        using var client = LiveFactory.Client();

        var banks = await client.Payouts.ListBanksAsync();

        Assert.NotEmpty(banks);
        Assert.All(banks, b => Assert.False(string.IsNullOrWhiteSpace(b.Name)));
        output.WriteLine("DONE: ListBanks_ReturnsBanks");
    }

    [Fact]
    public async Task VerifyAccount_ReturnsAccountDetails()
    {
        using var client = LiveFactory.Client();

        var result = await client.Payouts.VerifyAccountAsync(new()
        {
            AccountNumber = "0123456789",
            EnquiryType   = "OTHERS",
            BankCode      = "044",
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccountNumber));
        output.WriteLine("DONE: VerifyAccount_ReturnsAccountDetails");
    }
}
