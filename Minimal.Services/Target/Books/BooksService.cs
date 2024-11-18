using Microsoft.AspNetCore.Http;
using Minimal.DataAccess;
using Minimal.Domain.Target.Books;
using Minimal.Domain.Target.Books.Dtos;
using Minimal.Services.Core;
using Minimal.Services.Core.Logging;
using Minimal.Services.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Services.Target.Books
{
    public class BooksService : ServiceBase<Book, BookDto>, IBooksService
    {
        public BooksService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILoggerService logger, ITransactionsService transactionsService) : base(dbContext, httpContextAccessor, logger, transactionsService)
        {
        }
    }
}
