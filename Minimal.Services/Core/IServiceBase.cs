using Microsoft.EntityFrameworkCore.Storage;
using Minimal.Domain.Core;
using Minimal.Domain.Core.RequestFeatures;
using Minimal.Domain.Core.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Services.Core
{
    public interface IServiceBase<U> where U : BaseDto
    {
        Task<Response<PagedList<U>>> GetPaged(RequestParameters requestParams);

        Task<Response<IEnumerable<U>>> GetAll(RequestParameters requestParams = null!);

        Task<Response<int>> GetCount(Expression<Func<U, bool>>? predict = null);

        Task<Response<bool>> Any(Expression<Func<U, bool>>? predict = null);

        IEnumerable<R> GetAs<R>(Expression<Func<U, R>> selector, Expression<Func<U, bool>> predict);

        Task<Response<U>> Get(int id);

        Task<Response<U>> Create(U entityDto);

        Task<Response<IEnumerable<U>>> CreateRange(IEnumerable<U> entityDtos);

        Task<Response<U>> Update(U entityDto);

        Task<Response<IEnumerable<U>>> UpdateRange(IEnumerable<U> entityDtos, IEnumerable<int>? currIds = null);

        //Task<List<W>> MapUpate(List<W> dtoList, List<W> currList);

        Task<Response<string>> Delete(int id);

        Task<Response<string>> DeleteRange(Expression<Func<U, bool>> predict);


        IDbContextTransaction? BeginTransaction();
        Task Commit(IDbContextTransaction? transaction);
        void RollBack(IDbContextTransaction? transaction);
    }
}
