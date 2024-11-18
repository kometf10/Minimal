using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.DataAccess.Converters
{
    public class TimeOnlyConverter : ValueConverter<TimeOnly, TimeSpan>
    {
        /// <summary>
        /// Creates a new instance of this converter.
        /// </summary>
        public TimeOnlyConverter() : base(
                t => t.ToTimeSpan(),
                t => TimeOnly.FromTimeSpan(t))
        { }
    }
}
