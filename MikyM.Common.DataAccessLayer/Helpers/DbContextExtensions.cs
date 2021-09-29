using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace MikyM.Common.DataAccessLayer.Helpers
{
    public static class DbContextExtensions
    {
        public static TEntity FindTracked<TEntity>(this DbContext context, params object[] keyValues)
                where TEntity : class
        {
                var entityType = context.Model.FindEntityType(typeof(TEntity));
                var key = entityType.FindPrimaryKey();
                var stateManager = context.GetDependencies().StateManager;
                var entry = stateManager.TryGetEntry(key, keyValues);
                return entry?.Entity as TEntity;
        }
    }
}
