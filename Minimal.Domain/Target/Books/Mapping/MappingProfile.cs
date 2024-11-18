using AutoMapper;
using Minimal.Domain.Target.Books;
using Minimal.Domain.Target.Books.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Mapping
{
    public partial class MappingProfile : Profile
    {
        public void BooksMappingProfile()
        {
            CreateMap<Book, BookDto>()
                .ReverseMap();
        }
    }
}
