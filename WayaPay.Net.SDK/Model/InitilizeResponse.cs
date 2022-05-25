using System;
using System.Collections.Generic;
using System.Text;

namespace WayaPay.Net.SDK.Model
{
    public class InitilizeResponse
    {
        public long TimeStamp { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public InitilizeDetails Data { get; set; }
    }

    public class InitilizeDetails
    {
        public string TranId { get; set; }
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public bool CustomerAvoid { get; set; }
    }
}
