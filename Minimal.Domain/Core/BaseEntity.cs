using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? CreatedAt { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? LastModefiedAt { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(255)]
        public string? CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(255)]
        public string? LastModifiedBy { get; set; } //UserId

        [ScaffoldColumn(false)]
        public bool IsDeleted { get; set; } = false;

        [StringLength(255)]
        [ScaffoldColumn(false)]
        public string? DataAccessKey { get; set; }

        public string? ConcurrencyStamp { get; set; }
    }
}
