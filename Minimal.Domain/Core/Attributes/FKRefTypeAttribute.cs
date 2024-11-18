using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FKRefTypeAttribute : Attribute
    {
        public Type? RefType { get; set; }

        public FKRefTypeAttribute(Type? refType)
        {
            RefType = refType;
        }
    }
}
