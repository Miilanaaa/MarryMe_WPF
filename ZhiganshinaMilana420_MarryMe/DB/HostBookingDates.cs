//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZhiganshinaMilana420_MarryMe.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class HostBookingDates
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public System.DateTime BookingDate { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<int> CoupleId { get; set; }
    
        public virtual Couple Couple { get; set; }
        public virtual Host Host { get; set; }
    }
}
