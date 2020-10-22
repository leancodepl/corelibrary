using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF
{
    public class DateConverter : ValueConverter<Date, DateTime>
    {
        public static readonly DateConverter Instance = new DateConverter { };
        public DateConverter()
            : base(d => d.ToDateTimeAtMidnight(), d => d.GetDate(), null)
        { }
    }
}
