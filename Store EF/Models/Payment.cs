//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Store_EF.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string Code { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public Nullable<System.DateTime> Expiry { get; set; }
        public string TransactionId { get; set; }
        public string Bank { get; set; }
        public string Account { get; set; }
    
        public virtual Order_ Order_ { get; set; }
    }
}
