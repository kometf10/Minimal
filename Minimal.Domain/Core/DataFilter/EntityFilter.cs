using AutoMapper.Execution;
using Minimal.Domain.Core.Reflection;
using Minimal.Domain.Core.RequestFeatures;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Minimal.Domain.DataFilter
{
    public static class EntityFilter
    {
        public static IEnumerable<T> DynamicFilter<T>(this IEnumerable<T> data, IEnumerable<FilterParam> filterParams , string gather = "AND")
        {

            IEnumerable<string> distinctColumns = filterParams.Where(x => !String.IsNullOrEmpty(x.ColumnName)).Select(x => x.ColumnName).Distinct();

            bool firstFilter = true;
            IEnumerable<T> filteredData = (distinctColumns.Any())? Enumerable.Empty<T>() : data;
            foreach (string colName in distinctColumns)
            {
                var filterColumn = typeof(T).GetProperty(colName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (filterColumn != null)
                {
                    IEnumerable<FilterParam> filterValues = filterParams.Where(x => x.ColumnName.Equals(colName)).Distinct();
                    
                    foreach (var val in filterValues)
                    {
                        if (firstFilter)
                        {
                            filteredData = FilterData(val.FilterOption, data, filterColumn, val.FilterValue);
                            firstFilter = false;
                        }
                        else
                        {
                            if(gather == "AND")
                            {
                                filteredData = FilterData(val.FilterOption, filteredData, filterColumn, val.FilterValue);
                            }
                            else
                            {
                                filteredData = filteredData.Concat(FilterData(val.FilterOption, data, filterColumn, val.FilterValue)).Distinct();
                            }
                        }
                    }
                }
            }

            return filteredData;
        }

        public static IEnumerable<T> QuickFilter<T>(this IEnumerable<T> data, string quickFilterValue)
        {
            var properties = ReflectionAccessor.GetQuickFilterProps(typeof(T));

            var filtredData = Enumerable.Empty<T>();

            var firstFilter = true;
            foreach (var prop in properties)
            {
                if (firstFilter)
                {
                    filtredData = FilterData(FilterOptions.Contains, data, prop, quickFilterValue);
                    firstFilter = false;
                }
                else 
                    filtredData = filtredData.Concat(FilterData(FilterOptions.Contains, data, prop, quickFilterValue)).Distinct();
            }

            return filtredData;
        }

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> data, string orderColumnName, string orderType)
        {
            var orderedData = orderType == "ASC" ?
                data.OrderBy(d => d.GetType().GetProperty(orderColumnName)!.GetValue(d)) :
                data.OrderByDescending(d => d.GetType().GetProperty(orderColumnName)!.GetValue(d));

            return orderedData.ToList();
        }
        
        private static IEnumerable<T> FilterData<T>(FilterOptions filterOption, IEnumerable<T> data, PropertyInfo filterColumn, string filterValue)
        {
            int outValue;
            decimal dOutValue;
            DateTime dateValue;
            DateOnly dateOnlyValue;
            switch (filterOption)
            {
                #region [StringDataType]  

                case FilterOptions.StartsWith:
                    data = data.Where(x => filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString().ToLower().StartsWith(filterValue.ToString().ToLower())).ToList();
                    break;
                case FilterOptions.EndsWith:
                    data = data.Where(x => filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString().ToLower().EndsWith(filterValue.ToString().ToLower())).ToList();
                    break;
                case FilterOptions.Contains:
                    data = data.Where(x => filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString().ToLower().Contains(filterValue.ToString().ToLower())).ToList();
                    break;
                case FilterOptions.DoesNotContain:
                    data = data.Where(x => filterColumn.GetValue(x, null) == null ||
                                     (filterColumn.GetValue(x, null) != null && !filterColumn.GetValue(x, null).ToString().ToLower().Contains(filterValue.ToString().ToLower()))).ToList();
                    break;
                case FilterOptions.IsEmpty:
                    data = data.Where(x => filterColumn.GetValue(x, null) == null ||
                                     (filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString() == string.Empty)).ToList();
                    break;
                case FilterOptions.IsNotEmpty:
                    data = data.Where(x => filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString() != string.Empty).ToList();
                    break;
                #endregion

                #region [Custom]  

                case FilterOptions.IsGreaterThan:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)) && Int32.TryParse(filterValue, out outValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) > outValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateTime>) || (filterColumn.PropertyType == typeof(DateTime))) && DateTime.TryParse(filterValue, out dateValue))
                    {
                        data = data.Where(x => Convert.ToDateTime(filterColumn.GetValue(x, null)) > dateValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateOnly>) || (filterColumn.PropertyType == typeof(DateOnly))) && DateOnly.TryParse(filterValue, out dateOnlyValue))
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) != null && DateOnly.Parse(filterColumn.GetValue(x, null).ToString()!) > dateOnlyValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Decimal) || filterColumn.PropertyType == typeof(Nullable<Decimal>)) && Decimal.TryParse(filterValue, out dOutValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) > dOutValue).ToList();
                    }
                    
                    break;

                case FilterOptions.IsGreaterThanOrEqualTo:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)) && Int32.TryParse(filterValue, out outValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) >= outValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateTime>) || (filterColumn.PropertyType == typeof(DateTime))) && DateTime.TryParse(filterValue, out dateValue))
                    {
                        data = data.Where(x => Convert.ToDateTime(filterColumn.GetValue(x, null)) >= dateValue).ToList();
                    }
                    else if((filterColumn.PropertyType == typeof(Nullable<DateOnly>) || (filterColumn.PropertyType == typeof(DateOnly))) && DateOnly.TryParse(filterValue, out dateOnlyValue))
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) != null && DateOnly.Parse(filterColumn.GetValue(x, null).ToString()!) >= dateOnlyValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Decimal) || filterColumn.PropertyType == typeof(Nullable<Decimal>)) && Decimal.TryParse(filterValue, out dOutValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) >= dOutValue).ToList();
                    }
                    break;

                case FilterOptions.IsLessThan:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)) && Int32.TryParse(filterValue, out outValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) < outValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateTime>) || (filterColumn.PropertyType == typeof(DateTime))) && DateTime.TryParse(filterValue, out dateValue))
                    {
                        data = data.Where(x => Convert.ToDateTime(filterColumn.GetValue(x, null)) < dateValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateOnly>) || (filterColumn.PropertyType == typeof(DateOnly))) && DateOnly.TryParse(filterValue, out dateOnlyValue))
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) != null && DateOnly.Parse(filterColumn.GetValue(x, null).ToString()!) < dateOnlyValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Decimal) || filterColumn.PropertyType == typeof(Nullable<Decimal>)) && Decimal.TryParse(filterValue, out dOutValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) < dOutValue).ToList();
                    }
                    break;

                case FilterOptions.IsLessThanOrEqualTo:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)) && Int32.TryParse(filterValue, out outValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) <= outValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateTime>) || (filterColumn.PropertyType == typeof(DateTime))) && DateTime.TryParse(filterValue, out dateValue))
                    {
                        data = data.Where(x => Convert.ToDateTime(filterColumn.GetValue(x, null)) <= dateValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateOnly>) || (filterColumn.PropertyType == typeof(DateOnly))) && DateOnly.TryParse(filterValue, out dateOnlyValue))
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) != null && DateOnly.Parse(filterColumn.GetValue(x, null).ToString()!) <= dateOnlyValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Decimal) || filterColumn.PropertyType == typeof(Nullable<Decimal>)) && Decimal.TryParse(filterValue, out dOutValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) <= dOutValue).ToList();
                    }
                    break;

                case FilterOptions.IsEqualTo:
                    if (filterValue == string.Empty)
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) == null
                                        || (filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString().ToLower() == string.Empty)).ToList();
                    }
                    else
                    {
                        if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)) && Int32.TryParse(filterValue, out outValue))
                        {
                            data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) == outValue).ToList();
                        }
                        else if ((filterColumn.PropertyType == typeof(Nullable<DateTime>) || (filterColumn.PropertyType == typeof(DateTime))) && DateTime.TryParse(filterValue, out dateValue))
                        {
                            data = data.Where(x => Convert.ToDateTime(filterColumn.GetValue(x, null)) == dateValue).ToList();
                        }
                        else if ((filterColumn.PropertyType == typeof(Nullable<DateOnly>) || (filterColumn.PropertyType == typeof(DateOnly))) && DateOnly.TryParse(filterValue, out dateOnlyValue))
                        {
                            data = data.Where(x => filterColumn.GetValue(x, null) != null && DateOnly.Parse(filterColumn.GetValue(x, null).ToString()!) == dateOnlyValue).ToList();
                        }
                        else if ((filterColumn.PropertyType == typeof(Decimal) || filterColumn.PropertyType == typeof(Nullable<Decimal>)) && Decimal.TryParse(filterValue, out dOutValue))
                        {
                            data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) == dOutValue).ToList();
                        }
                        else
                        {
                            data = data.Where(x => filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString().ToLower() == filterValue.ToLower()).ToList();
                        }
                    }
                    break;

                case FilterOptions.IsNotEqualTo:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)) && Int32.TryParse(filterValue, out outValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) != outValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateTime>) || (filterColumn.PropertyType == typeof(DateTime))) && DateTime.TryParse(filterValue, out dateValue))
                    {
                        data = data.Where(x => Convert.ToDateTime(filterColumn.GetValue(x, null)) != dateValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Nullable<DateOnly>) || (filterColumn.PropertyType == typeof(DateOnly))) && DateOnly.TryParse(filterValue, out dateOnlyValue))
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) != null && DateOnly.Parse(filterColumn.GetValue(x, null).ToString()!) != dateOnlyValue).ToList();
                    }
                    else if ((filterColumn.PropertyType == typeof(Decimal) || filterColumn.PropertyType == typeof(Nullable<Decimal>)) && Decimal.TryParse(filterValue, out dOutValue))
                    {
                        data = data.Where(x => Convert.ToInt32(filterColumn.GetValue(x, null)) != dOutValue).ToList();
                    }
                    else
                    {
                        data = data.Where(x => filterColumn.GetValue(x, null) == null ||
                                         (filterColumn.GetValue(x, null) != null && filterColumn.GetValue(x, null).ToString().ToLower() != filterValue.ToLower())).ToList();
                    }
                    break;
                #endregion

                case FilterOptions.In:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)))
                    {
                        var valuesList = filterValue.Split(',').Select(v => Convert.ToInt32(v)).ToList();
                        data = data.Where(x => valuesList.Contains(Convert.ToInt32(filterColumn.GetValue(x, null)))).ToList();
                    }
                    else if (filterColumn.PropertyType == typeof(String))
                    {
                        var valuesList = filterValue.Split(',').ToList();
                        data = data.Where(x => valuesList.Contains(filterColumn.GetValue(x, null)?.ToString()!)).ToList();
                    }
                    break;
                case FilterOptions.NotIn:
                    if ((filterColumn.PropertyType == typeof(Int32) || filterColumn.PropertyType == typeof(Nullable<Int32>)))
                    {
                        var valuesList = filterValue.Split(',').Select(v => Convert.ToInt32(v)).ToList();
                        data = data.Where(x => !valuesList.Contains(Convert.ToInt32(filterColumn.GetValue(x, null)))).ToList();
                    }
                    else if (filterColumn.PropertyType == typeof(String))
                    {
                        var valuesList = filterValue.Split(',').ToList();
                        data = data.Where(x => !valuesList.Contains(filterColumn.GetValue(x, null)?.ToString()!)).ToList();
                    }
                    break;
                case FilterOptions.IsNull:
                    data = data.Where(x => filterColumn.GetValue(x, null) == null).ToList();
                    break;
                case FilterOptions.IsNotNull:
                    data = data.Where(x => filterColumn.GetValue(x, null) != null).ToList();
                    break;
            }
            return data;
        }

    }

    public static class QueryableFilter
    {
        public static IQueryable<T> QuickFilter<T>(this IQueryable<T> data, string filterValue, params Expression<Func<T, string>>[] stringProps)
        {
            if (string.IsNullOrEmpty(filterValue))
                return data;

            Expression propsFilterExpression = null;
            var filterValueExpression = Expression.Constant(filterValue);

            var param = Expression.Parameter(typeof(T), "param");

            var containsMehtod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

            foreach (var stringProp in stringProps)
            {
                var propName = stringProp.Body.GetMemberExpressions().First().Member.Name;
                var propExpression = Expression.PropertyOrField(param, propName);

                var containExpression = Expression.Call(propExpression, containsMehtod!, filterValueExpression);

                propsFilterExpression = BuildOrExpression(propsFilterExpression, containExpression);
            }

            var completeExpression = Expression.Lambda<Func<T, bool>>(propsFilterExpression!, param);

            return data.Where(completeExpression);
        }

        public static IQueryable<T> QuickFilter<T>(this IQueryable<T> data, string filterValue)
        {
            if (string.IsNullOrEmpty(filterValue))
                return data;

            Expression propsFilterExpression = null!;
            var filterValueExpression = Expression.Constant(filterValue);

            var param = Expression.Parameter(typeof(T), "param");

            var containsMehtod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

            var quickFilterProps = ReflectionAccessor.GetQuickFilterProps(typeof(T));
            var stringProps = quickFilterProps?.Select(p => p.Name);

            if (stringProps == null || !stringProps.Any())
                return data;

            foreach (var stringProp in stringProps)
            {
                var propExpression = Expression.PropertyOrField(param, stringProp);

                var containExpression = Expression.Call(propExpression, containsMehtod!, filterValueExpression);

                propsFilterExpression = BuildOrExpression(propsFilterExpression, containExpression);
            }

            var completeExpression = Expression.Lambda<Func<T, bool>>(propsFilterExpression!, param);

            return data.Where(completeExpression);
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> data, string sortProp, string sortDirection)
        {
            var param = Expression.Parameter(typeof(T), "item");
            var propExpression = Expression.PropertyOrField(param, sortProp);

            var sortExpression = Expression.Lambda<Func<T, object>>(Expression.Convert(propExpression, typeof(object)), param);

            return sortDirection.ToLower().Equals("asc") ? data.OrderBy(sortExpression) : data.OrderByDescending(sortExpression);
        }

        public static IQueryable<T> DynamicFilter<T>(this IQueryable<T> data, IEnumerable<FilterParam> filterParams, string gather = "AND")
        {
            var param = Expression.Parameter(typeof(T), "param");
            Expression propsFilterExpression = null;

            if (filterParams == null || !filterParams.Any())
                return data;

            //Distinct Filter Columns Names
            IEnumerable<string> filterColumnNames = filterParams.Where(x => !String.IsNullOrEmpty(x.ColumnName)).Select(x => x.ColumnName).Distinct();


            foreach (var filterColumnName in filterColumnNames)
            {
                var filterColumn = typeof(T).GetProperty(filterColumnName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (filterColumn == null)
                    continue;

                //Distinct Column Filters
                IEnumerable<FilterParam> columnFilters = filterParams.Where(x => x.ColumnName.Equals(filterColumnName)).Distinct();

                //Loop over Column Filters
                foreach (var colFilter in columnFilters)
                {
                    //[TODO:] Fix This Shit 
                    Expression filterExpression = colFilter.FilterOption switch
                    {
                        FilterOptions.In => BuildInExpression(param, colFilter, filterColumn)!,
                        FilterOptions.NotIn => BuildNotInExpression(param, colFilter, filterColumn)!,
                        FilterOptions.IsNull => BuildIsNullExpression(param, filterColumn)!,
                        FilterOptions.IsNotNull => BuildIsNotNullExpression(param, filterColumn)!,
                        _ => GenerateFilterExpression(param, filterColumn, colFilter)!                                        
                    };

                    if (gather == "AND")
                        propsFilterExpression = BuildAndExpression(propsFilterExpression, filterExpression!);
                    else
                        propsFilterExpression = BuildOrExpression(propsFilterExpression, filterExpression!);
                }
            }

            var completeCondition = Expression.Lambda<Func<T, bool>>(propsFilterExpression!, param);
            return data.Where(completeCondition);
        }

        //public static IQueryable<T> FilterBy<T>(this IQueryable<T> data, string filterColumnName, FilterParams filterParam, string gather = "AND")
        //{
        //    var param = Expression.Parameter(typeof(T), "param");
        //    Expression propsFilterExpression = null;

        //    var prop = typeof(T).GetProperty(filterColumnName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

        //    Column Property Expression
        //    var propExpression = Expression.PropertyOrField(param, filterColumnName);

        //    var filterExpression = GenerateFilterExpression(propExpression, prop!, filterParam);

        //    if (gather == "AND")
        //        propsFilterExpression = BuildAndExpression(propsFilterExpression, filterExpression!);
        //    else
        //        propsFilterExpression = BuildOrExpression(propsFilterExpression, filterExpression!);

        //    var completeCondition = Expression.Lambda<Func<T, bool>>(propsFilterExpression!, param);
        //    return data.Where(completeCondition);
        //}

        #region "Not Used (Dynamic Dto Select)"
        public static IQueryable<U> DynamicDtoSelect<T, U>(this IQueryable<T> data) where U: class, new()
        {
            ParameterExpression entityParam = Expression.Parameter(typeof(T), "entityParam");

            MemberInitExpression init = Expression.MemberInit(Expression.New(typeof(U)), Assignments<T, U>(entityParam));

            var selectExpression = Expression.Lambda<Func<T, U>>(init, entityParam);

            var result = data.Select(selectExpression);

            return result;
        }

        public static List<MemberAssignment> Assignments<T, U>(Expression param)
        {
            var dtoProps = typeof(U).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var entityParamExpression = Expression.Parameter(typeof(T), "entityParam");
            var dtoParamExpression = Expression.Parameter(typeof(U), "dtoParam");

            var newExpression = Expression.New(typeof(U));

            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var prop in dtoProps) //UnitCategoryName = param.UnitCategory.Name
            {
                if (prop.IsRefIgnore())
                    continue;

                //dtoParam.Prop
                var dtoPropExpression = Expression.PropertyOrField(dtoParamExpression, prop.Name);

                //entityParam.Prop | entityParam.Ref.Prop
                var propRefName = prop.GetFKRefType().Name;
                var entityPropExpression = Expression.PropertyOrField(entityParamExpression, propRefName);

                var binding = Expression.Bind(prop, entityPropExpression);
                bindings.Add(binding);
            }

            return bindings;
        }

        public static MethodCallExpression InitList<T, U>(Expression param)
        {

            ParameterExpression entityParam = Expression.Parameter(typeof(T), "entityParam");

            MemberInitExpression dtoInit = Expression.MemberInit(Expression.New(typeof(U)), Assignments<T, U>(entityParam));

            LambdaExpression lambda = Expression.Lambda<Func<T, U>>(dtoInit, entityParam);

            // Now we have to specify we are making a call to the select statement and specify the types
            MethodCallExpression dtoSelect = Expression.Call(
                null,
                GetSelect<U>()
                    .MakeGenericMethod(new Type[] { 
				        // Our select "in"
				        typeof(T), 
				        // Our select "out"
				        typeof(U)
                    }),
                new Expression[] {
                    param, 
			        // The lamda body from above
			        lambda
                });

            return dtoSelect;
        }

        public static MethodInfo? GetSelect<U>()
        {
            return typeof(IQueryable<U>).GetMethod("Select");
        }

        #endregion

        #region "Private"
        private static Expression? GenerateFilterExpression(ParameterExpression paramExpression, PropertyInfo prop, FilterParam filter)
        {
            var convertedValue = ReflectionAccessor.ChangeType(filter.FilterValue, prop.PropertyType);
            var nType = Nullable.GetUnderlyingType(prop.PropertyType)?? prop.PropertyType;
            var constantExpression = Expression.Constant(convertedValue, nType);

            var propExpression = Expression.PropertyOrField(paramExpression, prop.Name);
            var convetedPropExpression = Expression.Convert(propExpression, nType);

            Dictionary<FilterOptions, Func<UnaryExpression, ConstantExpression, Expression>> ExpressionBuilderOptions = new Dictionary<FilterOptions, Func<UnaryExpression, ConstantExpression, Expression>>
            {
                { FilterOptions.IsEqualTo, BuildEqualExpression},
                { FilterOptions.IsNotEqualTo, BuildNotEqualExpression},
                { FilterOptions.Contains, BuildContainExpression},
                { FilterOptions.DoesNotContain, BuildNotContainExpression},
                { FilterOptions.StartsWith, BuildStartWithExpression},
                { FilterOptions.EndsWith, BuildEndsWithExpression},
                { FilterOptions.IsEmpty, BuildIsEmptyExpression},
                { FilterOptions.IsNotEmpty, BuildIsNotEmptyExpression},
                { FilterOptions.IsGreaterThan, Expression.GreaterThan},
                { FilterOptions.IsGreaterThanOrEqualTo, Expression.GreaterThanOrEqual },
                { FilterOptions.IsLessThan, Expression.LessThan},
                { FilterOptions.IsLessThanOrEqualTo, Expression.LessThanOrEqual},
            };

            var type = prop.PropertyType;
            if (ExpressionBuilderOptions.ContainsKey(filter.FilterOption))
                return ExpressionBuilderOptions[filter.FilterOption].Invoke(convetedPropExpression, constantExpression);

            return null;
        }

        private static MethodCallExpression BuildContainExpression(UnaryExpression prop, ConstantExpression value)
        {
            var method = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

            return Expression.Call(prop, method!, value);
        }
        private static UnaryExpression BuildNotContainExpression(UnaryExpression prop, ConstantExpression value) => Expression.Not(BuildContainExpression(prop, value));

        private static MethodCallExpression BuildStartWithExpression(UnaryExpression prop, ConstantExpression value)
        {
            var method = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });

            return Expression.Call(prop, method!, value);
        }

        private static MethodCallExpression BuildEndsWithExpression(UnaryExpression prop, ConstantExpression value)
        {
            var method = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

            return Expression.Call(prop, method!, value);
        }
        private static Expression BuildEqualExpression(UnaryExpression prop, ConstantExpression value)
        {           
            var type = value.Type;
            var method = type.GetMethod("Equals", new Type[] { type });

            //if (Nullable.GetUnderlyingType(type) != null)
            //{
            //    var objectValue = Expression.Convert(value, typeof(object));
            //    return Expression.Call(prop, method!, objectValue);
            //}

            return Expression.Call(prop, method!, value);
        }
        private static Expression BuildNotEqualExpression(UnaryExpression prop, ConstantExpression value) => Expression.Not(BuildEqualExpression(prop, value));

        private static Expression BuildIsEmptyExpression(UnaryExpression prop, ConstantExpression value)
        {
            var emptyStrExpression = Expression.Constant(string.Empty);
            var nullExpression = Expression.Constant(null, typeof(object));

            var isEmptyStrExpression = Expression.Equal(prop, emptyStrExpression);
            var isNullExpression = Expression.Equal(prop, nullExpression);

            return BuildOrExpression(isEmptyStrExpression, isNullExpression);
        }

        private static Expression BuildIsNotEmptyExpression(UnaryExpression prop, ConstantExpression value)
        {
            var emptyStrExpression = Expression.Constant(string.Empty);
            var nullExpression = Expression.Constant(null, typeof(object));

            var isNotEmptyStrExpression = Expression.Not(Expression.Equal(prop, emptyStrExpression));
            var isNotNullExpression = Expression.Not(Expression.Equal(prop, nullExpression));

            return BuildAndExpression(isNotEmptyStrExpression, isNotNullExpression);
        }

        public static Expression BuildGreaterThanExpression(MemberExpression prop, ConstantExpression value) => Expression.GreaterThan(prop, value);

        public static MethodCallExpression BuildInExpression(ParameterExpression paramExpression, FilterParam filter, PropertyInfo prop)
        {
            var strValues2 = filter.FilterValue.Split(",").ToList();
            var member = Expression.Property(paramExpression, prop.Name);
            var propertyType = ((PropertyInfo)member.Member).PropertyType;
            var listType = typeof(List<>).MakeGenericType(propertyType);
            var listInstance = Activator.CreateInstance(listType);

            MethodInfo addMethod = listType.GetMethod("Add")!;
            foreach (var value in strValues2)
                addMethod.Invoke(listInstance, new[] { ReflectionAccessor.ChangeType(value!, propertyType) });
            
            var listConstExpr2 = Expression.Constant(listInstance, listType);

            return Expression.Call(typeof(Enumerable), "Contains", new[] { propertyType }, listConstExpr2, member);
        }

        public static Expression BuildNotInExpression(ParameterExpression paramExpression, FilterParam filter, PropertyInfo prop) 
            => Expression.Not(BuildInExpression(paramExpression, filter, prop));


        private static Expression BuildIsNullExpression(ParameterExpression paramExpression, PropertyInfo prop)
        {
            var nType = prop.PropertyType;
            var propExpression = Expression.PropertyOrField(paramExpression, prop.Name);
            var convetedPropExpression = Expression.Convert(propExpression, nType);
            var nullExpression = Expression.Constant(null, typeof(object));

            return Expression.Equal(convetedPropExpression, nullExpression);
        }

        private static Expression BuildIsNotNullExpression(ParameterExpression paramExpression, PropertyInfo prop) => Expression.Not(BuildIsNullExpression(paramExpression, prop));

        private static Expression BuildOrExpression(Expression? left, Expression right) => (left == null) ? right : Expression.OrElse(left, right);

        private static Expression BuildAndExpression(Expression? left, Expression right) => (left == null) ? right : Expression.AndAlso(left, right);

        #endregion
    }

    public class SwapVisitor : ExpressionVisitor
    {
        private readonly Expression From;
        private readonly Expression To;

        public SwapVisitor(Expression from, Expression to)
        {
            From = from;
            To = to;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == From? To : base.Visit(node);
        }

        public static Expression? Swap(Expression body, Expression from, Expression to)
        {
            return new SwapVisitor(from, to).Visit(body);
        }
    }
}
