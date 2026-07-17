using Wayaquick.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Wayaquick.Tests.Live;

[Trait("Category", "Live")]
public sealed class LiveCollectionTests(ITestOutputHelper output)
{
    [Fact]
    public async Task InitiateCollection_ReturnsCheckoutUrl()
    {
        using var client = LiveFactory.Client();

        var result = await client.Collection.InitiateAsync(new()
        {
            Amount        = "100.00",
            Currency      = "NGN",
            Email         = "test@example.com",
            TransactionId = $"TEST-{Guid.NewGuid():N}",
            FirstName     = "John",
            LastName      = "Doe",
            Phone         = "08012345678",
            Description   = "Integration test payment",
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.CheckOutUrl));
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        output.WriteLine("DONE: InitiateCollection_ReturnsCheckoutUrl");
    }
}
