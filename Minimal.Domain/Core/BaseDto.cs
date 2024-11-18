using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core
{
    public class BaseDto
    {
        public int Id { get; set; }

        public string? ConcurrencyStamp { get; set; }

        public void Init<T>(T item) where T : BaseDto
        {
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(item);
                prop.SetValue(this, value);
            }
        }

        public T BaseClone<T>() where T : BaseDto, new()
        {
            var clone = new T();

            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(this);
                prop.SetValue(clone, value);
            }

            return clone;
        }
    }
}
