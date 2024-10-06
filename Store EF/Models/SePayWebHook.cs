using Newtonsoft.Json;
using System;

namespace Store_EF.Models
{
    public class SePayWebHook
    {
        [JsonProperty("gateway")]
        public string Gateway { get; set; }

        [JsonProperty("transactionDate")]
        public DateTimeOffset TransactionDate { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("transferAmount")]
        public long TransferAmount { get; set; }

        [JsonProperty("subAccount")]
        public string SubAccount { get; set; }

        [JsonProperty("referenceCode")]
        public string ReferenceCode { get; set; }
    }
}