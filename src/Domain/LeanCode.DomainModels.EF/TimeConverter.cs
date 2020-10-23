using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF
{
    public class TimeConverter : ValueConverter<System.Time, TimeSpan>
    {
        public static readonly TimeConverter Instance = new TimeConverter();

        public TimeConverter()
            : base(
                t => t,
                t => (System.Time)t,
                null)
        { }
    }
}
