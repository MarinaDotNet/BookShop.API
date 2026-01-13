using BookShop.API.Models.Auth;

namespace BookShop.API.Repositories;

/// <summary>
/// The repository interface for managing <see cref="User"/> entities.
/// Defines data access operations related to users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new <see cref="User"/> to the database and persists the change asynchronously.
    /// </summary>
    /// <param name="user">The <see cref="User"/> entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A Task that returns the added <see cref="User"/> entity.
    /// </returns>
    Task<User> AddUserAsync(User user, CancellationToken cancellationToken);
}
