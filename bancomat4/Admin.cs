//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace bancomat4
{
    using System;
    using System.Collections.Generic;
    
    public partial class Admin
    {
        public long Id { get; set; }
        public long IdBanca { get; set; }
        public string NomeUtente { get; set; }
        public string Password { get; set; }
    
        public virtual Banche Banche { get; set; }
    }
}
