using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    internal static class AggregateGraph
    {
        public static IQueryable<TAggregate> WithChildren<TAggregate>(this IQueryable<TAggregate> source, DbContext dbContext)
            where TAggregate : class
        {
            var root = dbContext.Model.FindEntityType(typeof(TAggregate))
                       ?? throw new InvalidOperationException($"{typeof(TAggregate).Name} is not mapped.");

            return Paths(root, null).Aggregate(source, (query, path) => query.Include(path));
        }

        private static IEnumerable<string> Paths(IEntityType type, string? prefix)
        {
            foreach (var navigation in type.GetNavigations().Where(navigation => !navigation.IsOnDependent && !navigation.TargetEntityType.IsOwned()))
            {
                var path = prefix is null ? navigation.Name : $"{prefix}.{navigation.Name}";
                var nested = Paths(navigation.TargetEntityType, path).ToList();

                if (nested.Count == 0)
                    yield return path;

                foreach (var child in nested)
                    yield return child;
            }
        }
    }
}
