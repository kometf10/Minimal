using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Services.Core.Transactions
{
    public interface ITransactionsService
    {
        IDbContextTransaction? BeginTransaction();

        Task Commit(IDbContextTransaction? transaction);

        void RollBack(IDbContextTransaction? transaction);
    }
}
