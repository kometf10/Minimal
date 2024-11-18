using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.RequestFeatures
{
    public class FilterParam
    {
        public int Id { get; set; }

        [Required]
        public string ColumnName { get; set; } = string.Empty;

        [Required]
        public string FilterValue { get; set; } = string.Empty;
        [Required]
        public FilterOptions FilterOption { get; set; } = FilterOptions.IsEqualTo;

        public string? FilterOptionsStr { get; set; }

        public string? FilterValueDisplay { get; set; }


        public int FilterTemplateId { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
