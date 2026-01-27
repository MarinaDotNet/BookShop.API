using BookShop.API.Models.Auth;

namespace BookShop.API.Repositories;

/// <summary>
/// The repository interface for managing <see cref="User"/> entities.
/// Defines data access operations related to users.
/// </summary>
public interface IUserRepository
{
    #region of User Management
    /// <summary>
    /// Adds a new <see cref="User"/> to the database and persists the change asynchronously.
    /// </summary>
    /// <param name="user">The <see cref="User"/> entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A Task that returns the added <see cref="User"/> entity.
    /// </returns>
    Task<User> AddUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a <see cref="User"/> by its unique identifier asynchronously.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the <see cref="User"/> to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <returns>
    /// A Task that returns the <see cref="User"/> entity if found.
    /// </returns>
    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing <see cref="User"/> in the database and persists the change asynchronously.
    /// </summary>
    /// <param name="user">
    /// The <see cref="User"/> entity to update.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <returns>
    /// A Task that returns the updated <see cref="User"/> entity.
    /// </returns>
    Task<User?> UpdateUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a user whose normalized email address matches the specified value.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address to search for. This value should be in uppercase and free of leading or trailing
    /// whitespace.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user whose normalized email
    /// matches the specified value, or null if no such user is found.</returns>
    Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a user whose normalized username matches the specified value.
    /// </summary>
    /// <param name="normalizedUsername">The normalized username to search for. This value should be in a consistent, case-insensitive format as defined
    /// by the application's normalization logic. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user whose normalized username
    /// matches the specified value, or null if no such user exists.</returns>
    Task<User?> GetUserByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken);
    #endregion of User Management

    #region of Role Management
    /// <summary>
    /// Asynchronously retrieves a role by its unique name.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role with the specified name, or
    /// null if no such role exists.</returns>
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously adds a new user role to the data store.
    /// </summary>
    /// <param name="entity">The user role to add. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added user role, or null if the
    /// operation fails.</returns>
    Task<UserRole?> AddUserRoleAsync(UserRole entity, CancellationToken cancellationToken);
    #endregion
}
