using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Infrastructure.Persistence.Configurations;

internal static class TenantColumn
{
    public static void Map<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, string>> property)
        where TEntity : class
    {
        builder.Property(property)
            .IsRequired()
            .HasMaxLength(TenantIds.MaxLength)
            .HasDefaultValue(TenantIds.Default);

        var index = Expression.Lambda<Func<TEntity, object?>>(
            Expression.Convert(property.Body, typeof(object)),
            property.Parameters);
        builder.HasIndex(index);
    }
}
