using System;
using System.Runtime.Serialization;

namespace WhyNotEarth.Meredith.Data.Entity.Models
{
    [Flags]
    public enum NotificationType : byte
    {
        [EnumMember(Value = "email")]
        Email = 1,

        [EnumMember(Value = "whatsapp")]
        Whatsapp = 2,

        [EnumMember(Value = "text")]
        Text = 4
    }
}