using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.AutoMapper
{
    public class Mapping
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() => {
            var configuration = new MapperConfiguration(config => {
                config.ShouldMapProperty = p => p.GetMethod!.IsPublic || p.GetMethod.IsAssembly;
                config.AddProfile<MappingProfile>();
            });
            return configuration.CreateMapper();
        });

        public static IMapper Mapper => Lazy.Value;
    }
}
