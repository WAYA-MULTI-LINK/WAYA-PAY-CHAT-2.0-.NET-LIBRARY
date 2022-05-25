using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WayaPay.Net.SDK.Model
{
    public class InitilizeRequest
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "currency")]
        public int Currency { get; set; }
        [JsonProperty(PropertyName = "fee")]
        public int Fee { get; set; }
        [JsonProperty(PropertyName = "customer")]
        public CustomerDetails Customer { get; set; }
        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }
        [JsonProperty(PropertyName = "wayaPublicKey")]
        public string WayaPublicKey { get; set; }
    }

    public class CustomerDetails
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "phoneNumber")]
        public string phoneNumber { get; set; }
    }
}