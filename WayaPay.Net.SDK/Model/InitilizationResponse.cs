using System;
using System.Collections.Generic;
using System.Text;

namespace WayaPay.Net.SDK.Model
{
    public class InitilizationResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long TimeStamp { get; set; }
        public SubData Data { get; set; }
    }

    public class SubData
    {
        public string Authorization_url { get; set; }
        public string TranId { get; set; }
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public bool CustomerAvoid { get; set; }
    }
}
