using System.Net;
using WayaPay;
using WayaPay.Models.collection;
using WayaPay.Models.Identity;
using WayaPay.Models.Payout;

namespace Wayapay.Tests.Helpers;

internal static class Factory
{
    internal static WayaPayClient Client(HttpMessageHandler handler, string env = "production") =>
        new(new WayaPayOptions
        {
            MerchantId  = "MER_TEST",
            SecretKey   = "WAYASECK_TEST_key",
            Environment = env,
            HttpClient  = new HttpClient(handler),
        });

    internal static PayoutRequestModel PayoutRequest(string reference = "REF-001") => new()
    {
        Amount        = 5000m,
        Currency      = "NGN",
        AccountNumber = "0123456789",
        BankCode      = "044",
        AccountName   = "JOHN DOE",
        Reference     = reference,
        Narration     = "Test payout",
    };

    internal static PayoutVerifyRequestModel VerifyRequest(
        string enquiryType = "OTHERS", string? bankCode = "044") => new()
    {
        AccountNumber = "0123456789",
        EnquiryType   = enquiryType,
        BankCode      = bankCode,
    };

    internal static BvnIdentityRequestModel BvnRequest(string bvn = "22500809037") =>
        new() { Bvn = bvn };

    internal static CollectionRequestModel CollectionRequest(string txId = "TXN-001") => new()
    {
        Amount        = "5000.00",
        Currency      = "NGN",
        Email         = "test@example.com",
        TransactionId = txId,
        FirstName     = "John",
        LastName      = "Doe",
        Phone         = "08012345678",
        Description   = "Test payment",
    };

    internal static StubHandler OkStub(string data) =>
        new(HttpStatusCode.OK, $$"""{"success":true,"code":"00","data":{{data}}}""");

    internal static StubHandler ErrorStub(string code, string message, HttpStatusCode status) =>
        new(status, $$"""{"success":false,"code":"{{code}}","message":"{{message}}"}""");
}
