using System.Linq.Expressions;

namespace MedReability.Infrastructure.Persistence;

public static class ClinicEntityQueryExtensions
{
    public static IQueryable<TEntity> WithClinicEntity<TEntity>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, Guid>> clinicIdSelector,
        Guid clinicId)
        where TEntity : class
    {
        var parameter = clinicIdSelector.Parameters.Single();
        var body = Expression.Equal(
            clinicIdSelector.Body,
            Expression.Constant(clinicId));

        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        return query.Where(predicate);
    }

    public static IQueryable<TEntity> NotDeleted<TEntity>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> isDeletedSelector)
        where TEntity : class
    {
        var parameter = isDeletedSelector.Parameters.Single();
        var body = Expression.Not(isDeletedSelector.Body);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        return query.Where(predicate);
    }
}
