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
    
    public partial class UserDetail
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public Nullable<bool> Gender { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
    
        public virtual User_ User_ { get; set; }
    }
}
