namespace WhyNotEarth.Meredith.Data.Entity.Models.Modules.Hotel
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class Bed : IEntityTypeConfiguration<Bed>
    {
        public enum BedTypes
        {
            King,
            Queen,
            Twin
        };

        public BedTypes BedType { get; set; }

        public int Count { get; set; }

        public Hotel Hotel { get; set; }

        public Guid HotelId { get; set; }

        public Guid Id { get; set; }

        public void Configure(EntityTypeBuilder<Bed> builder)
        {
            builder.ToTable("Beds", "ModuleHotel");
        }
    }
}