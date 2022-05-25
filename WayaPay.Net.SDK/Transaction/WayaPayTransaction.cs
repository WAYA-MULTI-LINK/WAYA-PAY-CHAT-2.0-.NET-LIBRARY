using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WayaPay.Net.SDK.Interface;
using WayaPay.Net.SDK.Model;

namespace WayaPay.Net.SDK.Transaction
{
    public class WayaPayTransaction : IWayaPayTransaction
    {
        bool _isProduction;
        string _merchatID;
        string _publicKey;
        public WayaPayTransaction(string merchantID, string publicKey,bool isProduction = false)
        {
            _isProduction = isProduction;
            _merchatID = merchantID;
            _publicKey = publicKey;
        }
        public async Task<InitilizationResponse> InitiateTransfer(string amount, string description , string customerName , string customerEmail, string customerPhoneNumber,int currency = 566, int fee = 1)
        {
            var client = HttpConnection.CreateClient(this._isProduction);
            var initilizationRequest = new InitilizeRequest
            {
                Description = description,
                Fee = fee,
                Amount = amount,
                Currency = currency,
                MerchantId = _merchatID,
                WayaPublicKey = _publicKey,
                Customer = new CustomerDetails
                {
                    Email = customerEmail,
                    Name = customerName,
                    phoneNumber = customerPhoneNumber
                }
            };
            var body = JsonConvert.SerializeObject(initilizationRequest);
            var data = new StringContent(JsonConvert.SerializeObject(initilizationRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(BaseConstants.WayPayInitializeTransactionEndPoint, data);
            var json = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<InitilizeResponse>(json);

            if(resp.Data == null)
            {
                return new InitilizationResponse
                {
                    TimeStamp = resp.TimeStamp,
                    Status = resp.Status,
                    Message = resp.Message,
                };
            }

            if (resp.Status && !string.IsNullOrWhiteSpace(resp.Data.TranId))
            {

                return new InitilizationResponse
                {
                    TimeStamp = resp.TimeStamp,
                    Status = resp.Status,
                    Message = resp.Message,
                    Data = new SubData
                    {
                        Authorization_url = _isProduction ? BaseConstants.WapPayFrontEndProduction.Replace("{TranID}", resp.Data.TranId) : BaseConstants.WapPayFrontEndStaging.Replace("{TranID}", resp.Data.TranId),
                        CustomerAvoid = resp.Data.CustomerAvoid,
                        CustomerId = resp.Data.CustomerId,
                        Name = resp.Data.Name,
                        TranId = resp.Data.TranId
                    }
                };
            }
            else
            {
                return new InitilizationResponse
                {
                    TimeStamp = resp.TimeStamp,
                    Status = resp.Status,
                    Message = resp.Message,
                };
            }
            
        }



        public async Task<VerifyTransactionResponse> Verify(string transactionID)
        {
            var url = $"{BaseConstants.WayPayTransactionVerificationEndPoint}/{transactionID}";
            var client = HttpConnection.CreateClient(this._isProduction);
            var response = await client.GetAsync($"{BaseConstants.WayPayTransactionVerificationEndPoint}/{transactionID}");

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<VerifyTransactionResponse>(json);
        }
    }
}
