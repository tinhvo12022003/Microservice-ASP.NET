using System.Linq.Expressions;
using UserMicroservice.Data;
using Microsoft.EntityFrameworkCore;
using Pagination.EntityFrameworkCore.Extensions;

namespace UserMicroservice.Repository;

public class GenericRepository<T> where T : class
{
    protected readonly UserdbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(UserdbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task Add(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task Update(T entity)
    {
        _dbSet.Update(entity);
    }


    public async Task<Pagination<T>> GetFilter(
        int page,
        int limit,
        Expression<Func<T, bool>>? filter = null,
        string? searchText = null,
        string? sortColumn = null,
        bool orderByDescending = true)
    {
        IQueryable<T> query = _dbSet;

        // 1. Áp dụng filter cứng (nếu có)
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // 2. Xử lý SearchText trên nhiều loại cột (String, Int, Decimal,...)
        if (!string.IsNullOrEmpty(searchText))
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? orExpression = null;

            // Lấy các thuộc tính là String hoặc Primitive (Số, Boolean, DateTime...)
            var searchableProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) ||
                            p.PropertyType.IsPrimitive ||
                            new[] { typeof(decimal), typeof(double), typeof(float) }.Contains(p.PropertyType));

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toStringMethod = typeof(object).GetMethod("ToString");
            var searchConstant = Expression.Constant(searchText);

            foreach (var prop in searchableProperties)
            {
                Expression propertyAccess = Expression.Property(parameter, prop);
                Expression comparisonExpression;

                if (prop.PropertyType == typeof(string))
                {
                    // Nếu là string: x.Prop.Contains(searchText)
                    // Thêm kiểm tra null để tránh lỗi runtime: x.Prop != null && x.Prop.Contains(...)
                    var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                    var containsCall = Expression.Call(propertyAccess, containsMethod!, searchConstant);
                    comparisonExpression = Expression.AndAlso(notNull, containsCall);
                }
                else
                {
                    // Nếu là số/kiểu khác: EF.Functions.Like(EF.Property<string>(x, "Name"), $"%{searchText}%")
                    // Hoặc đơn giản nhất cho đa số DB: x.Prop.ToString().Contains(searchText)
                    var toStringCall = Expression.Call(propertyAccess, toStringMethod!);
                    comparisonExpression = Expression.Call(toStringCall, containsMethod!, searchConstant);
                }

                orExpression = orExpression == null
                    ? comparisonExpression
                    : Expression.OrElse(orExpression, comparisonExpression);
            }

            if (orExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(orExpression, parameter);
                query = query.Where(lambda);
            }
        }

        // 3. Sắp xếp (Sorting)
        if (!string.IsNullOrEmpty(sortColumn))
        {
            query = orderByDescending
                ? query.OrderByDescending(e => EF.Property<object>(e!, sortColumn))
                : query.OrderBy(e => EF.Property<object>(e!, sortColumn));
        }
        else
        {
            // Try to sort by 'CreatedAt' shadow property or 'Id' if available
            var entityType = _context.Model.FindEntityType(typeof(T));
            if (entityType != null)
            {
                if (entityType.FindProperty("CreatedAt") != null)
                {
                    query = query.OrderByDescending(e => EF.Property<object>(e!, "CreatedAt"));
                }
                else if (entityType.FindProperty("Id") != null)
                {
                    query = query.OrderByDescending(e => EF.Property<object>(e!, "Id"));
                }
            }
        }


        var result = await query.AsPaginationAsync(page, limit);

        return result;
    }
}