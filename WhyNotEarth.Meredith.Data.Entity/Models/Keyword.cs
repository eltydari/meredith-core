﻿namespace WhyNotEarth.Meredith.Data.Entity.Models
{
    public class Keyword
    {
        public int Id { get; set; }

        public string Value { get; set; } = null!;

        public int PageId { get; set; }

        public Page Page { get; set; } = null!;
    }
}
