using System;
using System.Collections.Generic;
using System.Text;

namespace WayaPay.Net.SDK.Model
{
    public class VerifyTransactionResponse
    {
        public string TimeStamp { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public VerifyTransactionDetails Data { get; set; }
    }


    public class VerifyTransactionDetails
    {
        public string Amount { get; set; }
        public string Description { get; set; }
        public decimal Fee { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string ProductName { get; set; }
        public string BusinessName { get; set; }
        public InitilizeDetails initilizeDetails { get; set; }
    }
  }
