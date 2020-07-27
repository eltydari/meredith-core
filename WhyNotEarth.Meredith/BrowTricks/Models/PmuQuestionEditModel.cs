﻿using System.Diagnostics.CodeAnalysis;
using WhyNotEarth.Meredith.Validation;

namespace WhyNotEarth.Meredith.BrowTricks.Models
{
    public class PmuQuestionEditModel
    {
        [NotNull]
        [Mandatory]
        public string Question { get; set; } = null!;
    }
}