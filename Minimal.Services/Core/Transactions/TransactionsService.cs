using Microsoft.EntityFrameworkCore.Storage;
using Minimal.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Services.Core.Transactions
{
    public class TransactionsService : ITransactionsService
    {
        private readonly AppDbContext DbContext;
        private int Count;
        private bool TransactionCompleted;
        public TransactionsService(AppDbContext dbContext)
        {
            DbContext = dbContext;
            Count = 0;
        }

        public IDbContextTransaction? BeginTransaction()
        {
            var transaction = DbContext.Database.CurrentTransaction ?? DbContext.Database.BeginTransaction();

            Count++;

            return transaction;
        }

        public async Task Commit(IDbContextTransaction? transaction)
        {
            Count--;

            if (transaction != null && Count == 0)
            {
                transaction.Commit();
                TransactionCompleted = true;
                await Task.CompletedTask;
            }
        }

        public void RollBack(IDbContextTransaction? transaction)
        {
            if (transaction != null && !TransactionCompleted)
            {
                transaction.Rollback();
                TransactionCompleted = true;
            }
        }
    }
}
