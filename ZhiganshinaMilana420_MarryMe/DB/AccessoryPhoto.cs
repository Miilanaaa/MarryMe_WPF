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
    
    public partial class AccessoryPhoto
    {
        public int Id { get; set; }
        public int AccessoryId { get; set; }
        public byte[] Photo { get; set; }
    
        public virtual Accessory Accessory { get; set; }
    }
}
