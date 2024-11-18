using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core
{
    public class IndexData : ICloneable
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ParentId { get; set; }
        public string? StringId { get; set; }

        public Dictionary<string, string?> Extras { get; set; } = new();

        public override bool Equals(object? obj)
        {
            var other = obj as IndexData;

            return other?.Id == Id;
        }

        public override string ToString()
        {
            return Name!;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string GetExtraValue(string key)
        {
            var found = Extras.TryGetValue(key, out var value);

            return found && value != null ? value : string.Empty;
        }

        public object Clone()
        {
            //[TODO]: Clone the Extras Dick
            return MemberwiseClone();
        }
    }
}
