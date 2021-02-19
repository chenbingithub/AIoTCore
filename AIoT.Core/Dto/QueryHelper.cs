using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Dto;
using AIoT.Core.Entities;
using AIoT.Core.Enums;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AIoT.Core.Dto
{

    /// <summary>
    /// 查询扩展方法
    /// </summary>
    public static class QueryHelper
    {
        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="cancellationToken"></param>
        public static Task<TDto> FirstOrDefaultAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            IQuery query, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var data = source.Where(query).ProjectTo<TDto>();

            if (query is ISortInfo sort)
            {
                data = data.OrderBy(sort);
            }

            return data.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="cancellationToken"></param>
        public static Task<TDto> FirstOrDefaultAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            Expression<Func<TEntity, bool>> query, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var data = source.Where(query).ProjectTo<TDto>();
            return data.FirstOrDefaultAsync(cancellationToken);
        }


        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="sort">排序</param>
        /// <param name="defaultSort">默认排序</param>
        /// <param name="cancellationToken"></param>
        public static Task<List<TDto>> ToListAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            IQuery query, ISortInfo sort=null, string defaultSort = null, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var data = source.Where(query).ProjectTo<TDto>().OrderBy(sort, defaultSort);
            return data.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="sort">排序</param>
        /// <param name="defaultSort">默认排序</param>
        /// <param name="cancellationToken"></param>
        public static Task<List<TDto>> ToListAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            Expression<Func<TEntity, bool>> query, ISortInfo sort=null, string defaultSort = null, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var data = source.Where(query).ProjectTo<TDto>().OrderBy(sort, defaultSort);
            return data.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="sort">排序</param>
        /// <param name="defaultSort">默认排序</param>
        /// <param name="cancellationToken"></param>
        public static Task<List<TDto>> ToListAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            ISortInfo sort=null, string defaultSort = null, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var data = source.ProjectTo<TDto>().OrderBy(sort, defaultSort);
            return data.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="cancellationToken"></param>
        public static Task<List<TDto>> ToListAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            IQuery query, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var data = source.Where(query).ProjectTo<TDto>();
            return data.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="defaultSort">默认排序</param>
        /// <param name="sortByEntity">是否按实体字段排序</param>
        /// <param name="cancellationToken"></param>
        public static Task<PageResult<TDto>> ToPageResultAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            IPageQuery query, string defaultSort = null, bool sortByEntity = false, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            return ToPageResultAsync<TEntity, TDto>(source, query, query, defaultSort, sortByEntity, cancellationToken);
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <typeparam name="TDto">返回的类型</typeparam>
        /// <param name="source"></param>
        /// <param name="query">查询条件</param>
        /// <param name="page">分页信息</param>
        /// <param name="defaultSort">默认排序</param>
        /// <param name="sortByEntity">是否按实体字段排序</param>
        /// <param name="cancellationToken"></param>
        public static async Task<PageResult<TDto>> ToPageResultAsync<TEntity, TDto>(this IQueryable<TEntity> source, 
            IQuery query, IPageSortInfo page, string defaultSort = null, bool sortByEntity = false, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            page = page ?? new PageSortInfo();
            var pageIndex = Math.Max(1, page.PageIndex);
            var pageSize = Math.Max(1, page.PageSize);

            var result = new PageResult<TDto> { PageIndex = pageIndex, PageSize = pageSize };

            if (sortByEntity)
            {
                var data = source.Where(query);
                result.TotalCount = await data.CountAsync(cancellationToken);
                if (result.TotalCount > 0)
                {
                    var mapData = data.OrderBy(page, defaultSort);
                    result.Data = await mapData.Skip(pageSize * (pageIndex - 1))
                        .Take(pageSize).ProjectTo<TDto>().ToListAsync(cancellationToken);
                }
            }
            else
            {
                var data = source.Where(query).ProjectTo<TDto>();
                result.TotalCount = await data.CountAsync(cancellationToken);
                if (result.TotalCount > 0)
                {
                    var mapData = data.OrderBy(page, defaultSort);
                    result.Data = await mapData.Skip(pageSize * (pageIndex - 1))
                        .Take(pageSize).ToListAsync(cancellationToken);
                }
            }

            return result;
        }

        /// <summary>
        /// 返回分页的数据数据
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="source"></param>
        /// <param name="page">分页信息</param>
        /// <param name="defaultSort">默认排序</param>
        /// <param name="cancellationToken"></param>
        public static async Task<PageResult<TEntity>> ToPageResultAsync<TEntity>(this IQueryable<TEntity> source, 
            IPageSortInfo page, string defaultSort = null, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            page = page ?? new PageSortInfo();
            var pageIndex = Math.Max(1, page.PageIndex);
            var pageSize = Math.Max(1, page.PageSize);

            var result = new PageResult<TEntity>() { PageIndex = pageIndex, PageSize = pageSize };
            result.TotalCount = await source.CountAsync(cancellationToken);
            if (result.TotalCount > 0)
            {
                var mapData = source.OrderBy(page, defaultSort);
                result.Data = await mapData.Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize).ToListAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        public static IQueryable<TEntity> Where<TEntity>(this IQueryable<TEntity> source,  IQuery query)
            where TEntity : class, IEntity
        {
            var filter = query?.GetFilter<TEntity>();
            if (filter != null)
                return source.Where(filter);
            return source.AsQueryable();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public static IQueryable<TEntity> PageBy<TEntity>(this IQueryable<TEntity> source,  IPageSortInfo page, string defaultSort = null)
            where TEntity : class, IEntity
        {
            page = page ?? new PageSortInfo();
            var pageIndex = Math.Max(0, page.PageIndex);
            var pageSize = Math.Max(1, page.PageSize);

            return source.OrderBy(page, defaultSort).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public static async Task<PageResult<TDto>> PageAsync<TEntity, TDto>(this IQueryable<TEntity> source,  IPageSortInfo page, string defaultSort = null, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            page = page ?? new PageSortInfo();
            var pageIndex = Math.Max(1, page.PageIndex);
            var pageSize = Math.Max(1, page.PageSize);

            var result = new PageResult<TDto>() { PageIndex = pageIndex, PageSize = pageSize };
            var data = source.AsQueryable();
            result.TotalCount = await data.CountAsync(cancellationToken);
            if (result.TotalCount > 0)
            {
                var mapData = data.OrderBy(page, defaultSort).ProjectTo<TDto>();
                result.Data = await mapData.Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize).ToListAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// 查询指定条件的数据
        /// </summary>
        /// <typeparam name="TEntity">查询的实体</typeparam>
        /// <param name="source"></param>
        /// <param name="sort">排序</param>
        /// <param name="defaultSort">默认排序</param>
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source,  ISortInfo sort, string defaultSort = null)
        {
            var sortFields = sort?.SortFields?.Where(p => !string.IsNullOrEmpty(p))
                .Select(p => p.StartsWith("+") ? (p.TrimStart('+') + " ASC") : p.StartsWith("-") ? (p.TrimStart('-') + " DESC") : p).ToArray();
            if (sortFields != null && sortFields.Length > 0)
            {
                return source.OrderBy(string.Join(" , ", sortFields));
            }
            else if (!string.IsNullOrEmpty(defaultSort))
            {
                return source.OrderBy(defaultSort);
            }
            // 默认不使用排序，提高性能
            //else
            //{
            //    source = source.OrderBy("Id DESC");
            //}

            return source.AsQueryable();
        }


        /// <summary>
        /// 获取查询表达式
        /// </summary>
        /// <typeparam name="TEntity">要查询的实体类型</typeparam>
        public static Expression<Func<TEntity, bool>> GetQueryExpression<TEntity>(this IQuery query)
            where TEntity : class
        {
            if (query == null) return null;

            var queryType = query.GetType();
            var entityParam = Expression.Parameter(typeof(TEntity), "o");

            Expression body = null;

            var groupQuery = GetQueryGroup(queryType);

            foreach (var group in groupQuery.Values)
            {
                Expression sub = null;

                foreach ((var property, var attr) in group)
                {
                    var value = property.GetValue(query);
                    if (value is string str)
                    {
                        str = str.Trim();
                        value = string.IsNullOrEmpty(str) ? null : str;
                    }

                    var experssion = CreateQueryExpression(entityParam, value, attr.PropertyPath, attr.Compare);
                    if (experssion != null)
                    {
                        sub = sub == null ? experssion : Expression.OrElse(sub, experssion);
                    }
                }

                if (sub != null)
                {
                    body = body == null ? sub : Expression.AndAlso(body, sub);
                }
            }

            return body != null ? Expression.Lambda<Func<TEntity, bool>>(body, entityParam) : null;
        }

        private static readonly ConcurrentDictionary<Type, Dictionary<string, List<ValueTuple<PropertyInfo, QueryAttribute>>>>
            _queryCache = new ConcurrentDictionary<Type, Dictionary<string, List<(PropertyInfo, QueryAttribute)>>>();

        private static Dictionary<string, List<ValueTuple<PropertyInfo, QueryAttribute>>> GetQueryGroup(Type type)
        {
            return _queryCache.GetOrAdd(type, queryType =>
            {
                var groupIndex = 0;
                var groupQuery = new Dictionary<string, List<ValueTuple<PropertyInfo, QueryAttribute>>>();
                var properties = queryType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic |
                                                         BindingFlags.Public);
                foreach (var property in properties)
                {
                    var queries = property.GetAttributes<QueryAttribute>();
                    if (queries.Count() == 0) continue;

                    foreach (var attr in queries)
                    {
                        if (attr.OrGroup == null)
                            attr.OrGroup = groupIndex.ToString();
                        if (attr.PropertyPath == null || attr.PropertyPath.Length == 0)
                            attr.PropertyPath = new[] { property.Name };

                        var group = groupQuery.GetOrAdd(attr.OrGroup, p => new List<(PropertyInfo, QueryAttribute)>());
                        group.Add((property, attr));
                    }
                    groupIndex++;
                }
                return groupQuery;
            });
        }

        /// <summary>
        /// 获取应射的排序字段
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="mapType">映射的目标类型</param>
        public static IList<string> GetMapSortInfo(this ISortInfo sort, Type mapType)
        {
            if (sort?.SortFields == null || sort.SortFields.Count == 0) return new string[0];

            var sortList = new List<string>();
            var propertyMapType = typeof(MapFromAttribute);
            var properties = TypeDescriptor.GetProperties(mapType);

            foreach (var field in sort.SortFields)
            {
                // 考虑有使用 +/- 表示升序/降序的情况
                var split = field.Split(new[] { ' ' }, 2);
                var name = split.First().TrimStart('+', '-');
                var sorting = split.Length == 2
                    ? split[1]
                    : field.StartsWith("+") ? "Asc" : field.StartsWith("-") ? "Desc" : "Asc";

                var property = properties.Find(name, true);
                var attr = (MapFromAttribute)property?.Attributes[propertyMapType];
                if (attr?.PropertyPath != null && attr.PropertyPath.Length > 0)
                {
                    sortList.Add(string.Join(".", attr.PropertyPath) + " " + sorting);
                    continue;
                }

                sortList.Add(field);
            }

            return sortList;
        }

        /// <summary>
        /// 获取应射的排序字段
        /// </summary>
        /// <typeparam name="TMap">映射的目标类型</typeparam>
        /// <param name="sort"></param>
        public static IList<string> GetMapSortInfo<TMap>(this ISortInfo sort)
        {
            return GetMapSortInfo(sort, typeof(TMap));
        }

        #region Private Method

        private static Expression CreateQueryExpression(Expression entityParan, object value, string[] propertyPath,
            QueryCompare compare)
        {
            var member = CreatePropertyExpression(entityParan, propertyPath);

            switch (compare)
            {
                case QueryCompare.Equal:
                    return CreateEqualExpression(member, value);
                case QueryCompare.NotEqual:
                    return CreateNotEqualExpression(member, value);
                case QueryCompare.Like:
                    return CreateLikeExpression(member, value);
                case QueryCompare.NotLike:
                    return CreateNotLikeExpression(member, value);
                case QueryCompare.StartWidth:
                    return CreateStartsWithExpression(member, value);
                case QueryCompare.EndsWith:
                    return CreateEndsWithExpression(member, value);
                case QueryCompare.LessThan:
                    return CreateLessThanExpression(member, value);
                case QueryCompare.LessThanOrEqual:
                    return CreateLessThanOrEqualExpression(member, value);
                case QueryCompare.GreaterThan:
                    return CreateGreaterThanExpression(member, value);
                case QueryCompare.GreaterThanOrEqual:
                    return CreateGreaterThanOrEqualExpression(member, value);
                case QueryCompare.Between:
                    return CreateBetweenExpression(member, value);
                case QueryCompare.GreaterEqualAndLess:
                    return CreateGreaterEqualAndLessExpression(member, value);
                case QueryCompare.GreaterEqualAndLessEqual:
                    return CreateGreaterEqualAndLessEqualExpression(member, value);
                case QueryCompare.Include:
                    return CreateIncludeExpression(member, value);
                case QueryCompare.NotInclude:
                    return CreateNotIncludeExpression(member, value);
                case QueryCompare.IsNull:
                    return CreateIsNullExpression(member, value);
                case QueryCompare.HasFlag:
                    return CreateHasFlagExpression(member, value);
                default:
                    return null;
            }
        }

        private static MemberExpression CreatePropertyExpression(Expression param, string[] propertyPath)
        {
            var expression = propertyPath.Aggregate(param, Expression.Property) as MemberExpression;
            return expression;
        }


        private static Expression CreateEqualExpression(MemberExpression member, object value)
        {
            if (value == null) return null;

            var val = CreateQueryParamExpression(ChangeType(value, member.Type), member.Type);

            return Expression.Equal(member, val);
        }


        private static Expression CreateNotEqualExpression(MemberExpression member, object value)
        {
            if (value == null) return null;

            var val = CreateQueryParamExpression(ChangeType(value, member.Type), member.Type);

            return Expression.NotEqual(member, val);
        }

        private static Expression CreateLikeExpression(MemberExpression member, object value)
        {
            if (value == null) return null;
            if (member.Type != typeof(string))
                throw new ArgumentOutOfRangeException(nameof(member), $"Member '{member}' can not use 'Like' compare");

            var str = value.ToString();
            var val = CreateQueryParamExpression(str, member.Type);

            return Expression.Call(member, nameof(string.Contains), null, val);
        }

        private static Expression CreateNotLikeExpression(MemberExpression member, object value)
        {
            var like = CreateLikeExpression(member, value);
            if (like == null) return null;

            return Expression.Not(like);
        }

        private static Expression CreateStartsWithExpression(MemberExpression member, object value)
        {
            if (value == null) return null;
            if (member.Type != typeof(string))
                throw new ArgumentOutOfRangeException(nameof(member), $"Member '{member}' can not use 'Like' compare");

            var str = value.ToString();
            var val = CreateQueryParamExpression(str, member.Type);

            return Expression.Call(member, nameof(string.StartsWith), null, val);
        }

        private static Expression CreateEndsWithExpression(MemberExpression member, object value)
        {
            if (value == null) return null;
            if (member.Type != typeof(string))
                throw new ArgumentOutOfRangeException(nameof(member), $"Member '{member}' can not use 'Like' compare");

            var str = value.ToString();
            var val = CreateQueryParamExpression(str, member.Type);

            return Expression.Call(member, nameof(string.EndsWith), null, val);
        }

        private static Expression CreateLessThanExpression(MemberExpression member, object value)
        {
            if (value == null) return null;

            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var right = CreateQueryParamExpression(ChangeType(value, val.Type), val.Type);

            return Expression.LessThan(val, right);
        }

        private static Expression CreateLessThanOrEqualExpression(MemberExpression member, object value)
        {
            if (value == null) return null;

            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var right = CreateQueryParamExpression(ChangeType(value, val.Type), val.Type);

            return Expression.LessThanOrEqual(val, right);
        }

        private static Expression CreateGreaterThanExpression(MemberExpression member, object value)
        {
            if (value == null) return null;

            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var right = CreateQueryParamExpression(ChangeType(value, val.Type), val.Type);

            return Expression.GreaterThan(val, right);
        }

        private static Expression CreateGreaterThanOrEqualExpression(MemberExpression member, object value)
        {
            if (value == null) return null;

            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var right = CreateQueryParamExpression(ChangeType(value, val.Type), val.Type);

            return Expression.GreaterThanOrEqual(val, right);
        }

        private static Expression CreateBetweenExpression(MemberExpression member, object value)
        {
            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var list = GetListValue(val.Type, value);
            if (list == null) return null;
            if (list.Count < 2) return null;

            var left = list[0];
            var right = list[list.Count - 1];
            if (left == null || right == null) return null;


            Expression greaterEqual = null;
            Expression less = null;
            if (left != null)
            {
                var leftVal = CreateQueryParamExpression(left, val.Type);
                greaterEqual = Expression.GreaterThanOrEqual(val, leftVal);
            }
            if (right != null)
            {
                if (right is DateTime date && date.TimeOfDay == TimeSpan.Zero)
                {
                    right = date.AddDays(1);
                }
                var rightVal = CreateQueryParamExpression(right, val.Type);
                less = Expression.LessThan(val, rightVal);
            }

            if (greaterEqual == null)
                return less;
            else if (less == null)
                return greaterEqual;
            else
                return Expression.AndAlso(greaterEqual, less);
        }

        private static Expression CreateGreaterEqualAndLessExpression(MemberExpression member, object value)
        {
            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var list = GetListValue(val.Type, value);
            if (list == null) return null;
            if (list.Count < 2) return null;

            var left = list[0];
            var right = list[list.Count - 1];

            Expression greaterEqual = null;
            Expression less = null;
            if (left != null)
            {
                var leftVal = CreateQueryParamExpression(left, val.Type);
                greaterEqual = Expression.GreaterThanOrEqual(val, leftVal);
            }
            if (right != null)
            {
                if (right is DateTime date && date.TimeOfDay == TimeSpan.Zero)
                {
                    right = date.AddDays(1);
                }
                var rightVal = CreateQueryParamExpression(right, val.Type);
                less = Expression.LessThan(val, rightVal);
            }

            if (greaterEqual == null)
                return less;
            else if (less == null)
                return greaterEqual;
            else
                return Expression.AndAlso(greaterEqual, less);

        }


        private static Expression CreateGreaterEqualAndLessEqualExpression(MemberExpression member, object value)
        {
            var val = CheckConvertToEnumUnderlyingType(member.Type, member);
            var list = GetListValue(val.Type, value);
            if (list == null) return null;
            if (list.Count < 2) return null;

            var left = list[0];
            var right = list[list.Count - 1];

            Expression greaterEqual = null;
            Expression lessEual = null;
            if (left != null)
            {
                var leftVal = CreateQueryParamExpression(left, val.Type);
                greaterEqual = Expression.GreaterThanOrEqual(val, leftVal);
            }
            if (right != null)
            {
                if (right is DateTime date && date.TimeOfDay == TimeSpan.Zero)
                {
                    right = date.AddDays(1);
                }
                var rightVal = CreateQueryParamExpression(right, val.Type);
                lessEual = Expression.LessThanOrEqual(val, rightVal);
            }

            if (greaterEqual == null)
                return lessEual;
            else if (lessEual == null)
                return greaterEqual;
            else
                return Expression.AndAlso(greaterEqual, lessEual);

        }

        private static Expression CheckConvertToEnumUnderlyingType(Type memberType, Expression val)
        {
            var nonNullableType = memberType.GetNonNullableType();
            if (nonNullableType.IsEnum)
            {
                var underlyingType = nonNullableType.GetEnumUnderlyingType();
                if (memberType.IsNullableType())
                {
                    underlyingType = underlyingType.GetGenericArguments()[0];
                }

                return Expression.Convert(val, underlyingType);
            }
            return val;
        }

        private static Expression CreateIncludeExpression(MemberExpression member, object value)
        {
            var list = GetListValue(member.Type, value);
            if (list == null || list.Count == 0) return null;
            if (list.Count == 1)
            {
                return CreateEqualExpression(member, list[0]);
            }

            var listType = typeof(IEnumerable<>).MakeGenericType(member.Type);
            var vals = CreateQueryParamExpression(list, listType);

            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { member.Type }, vals, member);
        }

        private static Expression CreateNotIncludeExpression(MemberExpression member, object value)
        {
            var includeExpression = CreateIncludeExpression(member, value);
            if (includeExpression == null) return null;

            return Expression.Not(includeExpression);
        }

        private static Expression CreateIsNullExpression(MemberExpression member, object value)
        {
            if (member.Type.IsValueType && !member.Type.IsNullableType())
                throw new InvalidOperationException($"Member:{member} can not use '{QueryCompare.IsNull}' compare");

            var nullVal = CreateQueryParamExpression(null, member.Type);

            if (value == null || Equals(value, false))
                return Expression.Equal(member, nullVal);

            return Expression.NotEqual(member, nullVal);
        }


        private static Expression CreateHasFlagExpression(MemberExpression member, object value)
        {
            if (!member.Type.GetNonNullableType().IsEnum)
                throw new InvalidOperationException($"Member:{member} is not a Enum type");
            var list = GetListValue(member.Type.GetNonNullableType(), value);
            if (list == null || list.Count == 0) return null;

            var p = member;
            if (member.Type.IsNullableType())
                p = Expression.Property(member, "Value");

            Expression exp = null;
            foreach (var item in list)
            {
                var val = CreateQueryParamExpression(item, typeof(Enum));
                var method = typeof(Enum).GetMethod(nameof(Enum.HasFlag), new[] { typeof(Enum) });
                Expression temp = Expression.Call(p, method, val);
                exp = exp != null ? Expression.OrElse(exp, temp) : temp;
            }

            return exp;
        }

        private static IList GetListValue(Type memberType, object value)
        {
            if (value == null) return null;
            var data = value as IEnumerable;
            if (value is string str)
            {
                data = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()).ToArray();
            }

            if (data == null)
            {
                data = new[] { value };
            }

            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(memberType));

            foreach (var item in data)
            {
                try
                {
                    list.Add(ChangeType(item, memberType));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            }

            return list.Count == 0 ? null : list;
        }

        private static object ChangeType(object value, Type type)
        {
            if (value == null) return null;

            type = type.GetNonNullableType();
            if (type == value.GetType().GetNonNullableType()) return value;

            if (type.IsEnum)
            {
                if (value is string str1)
                    return Enum.Parse(type, str1);
                else
                    return Enum.ToObject(type, value);
            }
            if (value is string str2 && type == typeof(Guid))
                return new Guid(str2);

            return Convert.ChangeType(value, type);
        }

        private static Expression CreateQueryParamExpression(object value, Type type)
        {
            var queryType = _createValueTupleMethod.MakeGenericMethod(type);
            var queryParam = queryType.Invoke(null, new[] { value });
            var param = Expression.Constant(queryParam);

            return Expression.Field(param, "Value");
        }

        private static readonly MethodInfo _createValueTupleMethod =
            typeof(QueryHelper).GetMethod(nameof(CreateQueryParam), BindingFlags.Static | BindingFlags.NonPublic);
        private static QueryParam<T> CreateQueryParam<T>(T value)
        {
            return new QueryParam<T>(value);
        }

        public struct QueryParam<TValue>
        {
            public TValue Value;

            public QueryParam(TValue value)
            {
                Value = value;
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (obj is QueryParam<TValue>)
                    return this.Equals((QueryParam<TValue>)obj);
                return false;
            }

            public bool Equals(QueryParam<TValue> other)
            {
                return EqualityComparer<TValue>.Default.Equals(this.Value, other.Value);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return EqualityComparer<TValue>.Default.GetHashCode(this.Value);
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"QueryParam<{typeof(TValue).Name}>";
            }
        }

        #endregion
        public static Type GetNonNullableType(this Type type)
        {
            if (!type.IsNullableType())
            {
                return type;
            }

            return type.GetGenericArguments()[0];
        }
    }
    /// <summary>
    /// Auto Map 属性映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MapFromAttribute : Attribute
    {
        /// <summary>
        /// 对应属性路径
        /// </summary>
        public string[] PropertyPath { get; set; }

        /// <summary>
        /// 属性映射
        /// </summary>
        public MapFromAttribute(params string[] propertyPath)
        {
            propertyPath = propertyPath?.SelectMany(p => p.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            if (propertyPath?.Length > 0)
                PropertyPath = propertyPath;
        }
    }
}