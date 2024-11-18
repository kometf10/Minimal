using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Minimal.DataAccess;
using Minimal.Domain.Core;
using Minimal.Domain.Core.AutoMapper;
using Minimal.Domain.Core.Logging;
using Minimal.Domain.Core.RequestFeatures;
using Minimal.Domain.Core.Responces;
using Minimal.Services.Core.Logging;
using Minimal.Services.Core.Transactions;
using Minimal.Domain.DataFilter;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Minimal.Domain.Core.Reflection;
namespace Minimal.Services.Core
{
    public class ServiceBase<T, U> : IServiceBase<U> where T: BaseEntity, new() where U : BaseDto, new() 
    {
        protected readonly DbSet<T> DbSet;
        protected readonly AppDbContext DbContext;
        protected readonly IHttpContextAccessor HttpContextAccessor;
        protected readonly ILoggerService? Logger;
        protected readonly ITransactionsService? TransactionsService;

        public ServiceBase(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILoggerService logger, ITransactionsService transactionsService)
        {
            HttpContextAccessor = httpContextAccessor;
            DbContext = dbContext;
            Logger = logger;
            TransactionsService = transactionsService;
            DbSet = dbContext.Set<T>();
        }

        public async Task<Response<PagedList<U>>> GetPaged(RequestParameters requestParams)
        {
            var result = new Response<PagedList<U>>();
            try
            {
                var dtoQueryable = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>());

                dtoQueryable = FilterAndOrder(dtoQueryable, requestParams);

                var query = dtoQueryable.ToQueryString();

                var pagedList = PagedList<U>.ToPagedList(dtoQueryable, requestParams.PageNumber, requestParams.PageSize);
                HttpContextAccessor.HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(pagedList.PagingData));

                result.Result = pagedList;

            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<IEnumerable<U>>> GetAll(RequestParameters requestParams = null)
        {
            var result = new Response<IEnumerable<U>>();
            try
            {

                var dtoQueryable = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>());

                dtoQueryable = requestParams != null ? FilterAndOrder(dtoQueryable, requestParams) : dtoQueryable;

                var query = dtoQueryable.ToQueryString();

                result.Result = dtoQueryable.ToList();

            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<U>> Get(int id)
        {
            var result = new Response<U>();
            try
            {
                var idFilterParam = new List<FilterParam>();
                idFilterParam.Add(new FilterParam { ColumnName = "Id", FilterOption = FilterOptions.IsEqualTo, FilterValue = id.ToString() });

                var dtoQueryable = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>());
                dtoQueryable = dtoQueryable.DynamicFilter(idFilterParam);

                var query = dtoQueryable.ToQueryString();

                var entityDto = dtoQueryable.FirstOrDefault();
                

                if (entityDto == null)
                    throw new Exception($"Entity ({typeof(U).Name}) Not Found for Id ({id})");


            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }
            return result;
        }

        public IEnumerable<R> GetAs<R>(Expression<Func<U, R>> selector, Expression<Func<U, bool>> predict)
        {
            try
            {
                var query = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>(), selector);
                var q = query.ToQueryString();
                var list = query.Where(predict.Compile()).ToList();

                return list.Select(selector.Compile()).ToList();
            }
            catch
            {
                return new List<R>();
            }
        }

        public async Task<Response<int>> GetCount(Expression<Func<U, bool>>? predict = null)
        {
            var result = new Response<int>();
            try
            {
                var dtoQueryable = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>());
                result.Result = predict != null ? dtoQueryable.Count(predict) : dtoQueryable.Count();            
            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<bool>> Any(Expression<Func<U, bool>>? predict = null)
        {
            var result = new Response<bool>();
            try
            {

                var dtoQueryable = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>());

                result.Result = predict != null ? dtoQueryable.Any(predict) : dtoQueryable.Any();

            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<U>> Create(U entityDto)
        {
            var result = new Response<U>();
            try
            {
                //ValidateUniqFields(entityDto);

                var entity = Mapping.Mapper.Map<T>(entityDto);

                DbSet.Add(entity);

                //AttachEntityCreatedDomainEvent(entity);
                var c = await DbContext.SaveChangesAsync();

                result.HasErrors = (c <= 0);
                result.Result = Mapping.Mapper.Map<U>(entity);

                //if (CacheManager != null && !result.HasErrors)
                //    CacheManager.Invalidate(CacheKey!);
            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<IEnumerable<U>>> CreateRange(IEnumerable<U> entityDtos)
        {
            var result = new Response<IEnumerable<U>>();
            try
            {
                if (!entityDtos.Any())
                    return result;

                //entityDtos.ForEach(item => ValidateUniqFields(item));

                var entities = Mapping.Mapper.Map<List<T>>(entityDtos);

                DbSet.AddRange(entities);

                //entities.ForEach(e => AttachEntityCreatedDomainEvent(e));
                var c = await DbContext.SaveChangesAsync();

                result.HasErrors = (c <= 0);
                result.Result = Mapping.Mapper.Map<List<U>>(entities);

                //if (CacheManager != null && !result.HasErrors)
                //    CacheManager.Invalidate(CacheKey!);
            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<string>> Delete(int id)
        {
            var result = new Response<string>();
            try
            {
                var entity = await DbSet.FindAsync(id);
                if (entity == null)
                {
                    result.HasErrors = true;
                    result.AddValidationError("Id", "entity not found");
                    return result;
                }

                DbSet.Remove(entity);

                //AttachEntityDeletedDomainEvent(entity);
                var c = await DbContext.SaveChangesAsync();

                result.HasErrors = (c < 0);
                result.Result = string.Empty;

                //if (CacheManager != null && !result.HasErrors)
                //    CacheManager.Invalidate(CacheKey!);
            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<string>> DeleteRange(Expression<Func<U, bool>> predict)
        {
            var result = new Response<string>();
            try
            {
                var query = Mapping.Mapper.ProjectTo<U>(DbSet.AsQueryable<T>()).Where(predict);
                var dtoList = query.ToList();

                var list = Mapping.Mapper.Map<List<T>>(dtoList);
                DbSet.RemoveRange(list);

                //list.ForEach(e => AttachEntityDeletedDomainEvent(e));
                var c = await DbContext.SaveChangesAsync();

                result.HasErrors = c < 0;
                result.Result = string.Empty;

                //if (CacheManager != null && !result.HasErrors)
                //    CacheManager.Invalidate(CacheKey!);

            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public async Task<Response<U>> Update(U entityDto)
        {
            var result = new Response<U>();
            try
            {
                var id = entityDto.GetType().GetProperty("Id")!.GetValue(entityDto);
                if (id == null || (int)id == 0)
                    throw new Exception($"Id Property Not Found For ({typeof(T).Name})");

                var entity = DbSet.Find(id);
                _ = entity ?? throw new Exception($"{typeof(T).Name} Entity Not Found For Id ({(int)id})");

                var oldEntity = ReflectionAccessor.Clone(entity);

                //ValidateUniqFields(entityDto, isUpdate: true);

                Mapping.Mapper.Map(entityDto, entity);
                //[TODO]: Double Check
                DbContext.Entry(entity).State = EntityState.Modified;

                //AttachEntityUpdatedDomainEvent(oldEntity, entity);
                var c = await DbContext.SaveChangesAsync();


                result.HasErrors = (c < 0);
                result.Result = Mapping.Mapper.Map<U>(entity);

                //if (CacheManager != null && !result.HasErrors)
                //    CacheManager.Invalidate(CacheKey!);
            }
            catch (Exception e)
            {
                await HandleException(e, result);
            }

            return result;
        }

        public Task<Response<IEnumerable<U>>> UpdateRange(IEnumerable<U> entityDtos, IEnumerable<int>? currIds = null)
        {
            throw new NotImplementedException();
        }

        public IDbContextTransaction? BeginTransaction()
            => TransactionsService != null ? TransactionsService.BeginTransaction() : null;


        public async Task Commit(IDbContextTransaction? transaction)
        {
            if (TransactionsService != null)
            {
                await TransactionsService.Commit(transaction);
                //if (CacheManager != null)
                //    CacheManager.Invalidate(CacheKey!);
            }
        }
        public void RollBack(IDbContextTransaction? transaction)
        {
            if (TransactionsService != null)
            {
                TransactionsService.RollBack(transaction);
                //if (CacheManager != null)
                //    CacheManager.Invalidate(CacheKey!);
            }
        }

        public async Task HandleException<V>(Exception e, Response<V> result)
        {
            if (e is ValidationException)
                result.ValidationErrors = (e as ValidationException)?.ValidationErrors!;
            else if (Logger != null)
                await Logger.Log(e);

            result.HasErrors = true;
        }

        public void PassValidationException<V>(Response<V> result)
        {
            if (result.HasErrors || result.Result == null)
                throw new ValidationException(result.ValidationErrors);
        }


        private IQueryable<U> FilterAndOrder(IQueryable<U> dtoQueryable, RequestParameters requestParams)
        {
            if (requestParams.FilterParams != null && requestParams.FilterParams.Any())
                dtoQueryable = dtoQueryable.DynamicFilter(requestParams.FilterParams!, requestParams.Gather);

            if (!string.IsNullOrEmpty(requestParams.QuickFilter))
                dtoQueryable = dtoQueryable.QuickFilter(requestParams.QuickFilter);

            dtoQueryable = dtoQueryable.Sort(requestParams.OrderColumn, requestParams.OrderType);

            var query = dtoQueryable.ToQueryString();

            return dtoQueryable;
        }
    }
}
