using BookShop.API.Models.Catalog;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookShop.API.Infrastructure;

/// <summary>
/// Provides extension methods for building MongoDB <see cref="UpdateDefinition{T}"/> lists for partial updates.
/// </summary>
public static class UpdateDefinitionExtensions
{
    /// <summary>
    /// Conditionally adds an <see cref="UpdateDefinition{Book}"/> to the list if the specified value is not null 
    /// and optionally satisfies a given predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value to update.</typeparam>
    /// <param name="updates">The list of <see cref="UpdateDefinition{Book}"/> to which the new update will be added.</param>
    /// <param name="value">The value to set for the field. If <c>null</c>, the update is not added.</param>
    /// <param name="field">An expression specifying the field of <see cref="Book"/> to update.</param>
    /// <param name="predictate">An optional predicate function to determine if the value should be added. If provided and returns <c>false</c>, the update is skipped.</param>
    public static void AddIfNotNull<T>(this List<UpdateDefinition<Book>> updates, T? value, Expression<Func<Book, T>> field, Func<T, bool>? predictate = null)
    {
        if(value == null)
        {
            return;
        }

        if(predictate != null && !predictate(value))
        {
            return;
        }

        updates.Add(Builders<Book>.Update.Set(field, value));
    }
}
