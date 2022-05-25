using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WayaPay.Net.SDK.Model;

namespace WayaPay.Net.SDK.Interface
{
    public interface IWayaPayTransaction
    {
        Task<InitilizationResponse> InitiateTransfer(string amount, string description, string customerName, string customerEmail, string customerPhoneNumber, int currency = 566, int fee = 1);
        Task<VerifyTransactionResponse> Verify(string transactionID);

    }
}
