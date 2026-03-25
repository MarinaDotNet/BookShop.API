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

    /// <summary>
    /// Asynchronously retrieves the names of the roles associated with a specific user.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user whose role names are to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of role names associated with the specified user, or an empty collection if the user has no roles.
    /// </returns>
    Task<ICollection<UserRole>> GetUserRolesAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously saves a refresh token to the data store for later retrieval and validation.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token to save. This token should contain all necessary information for later validation, such as the token value, expiration time, and associated user identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a refresh token from the data store by its hashed value.
    /// </summary>
    /// <param name="hash">
    /// The hashed value of the refresh token to retrieve. This value should be the result of hashing the original refresh token 
    /// using the same hashing algorithm and salt used when saving the token.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the refresh token entity if found, or null 
    /// if not found.
    /// </returns>
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string hash, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously removes a refresh token from the data store, effectively invalidating it for future use.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously revokes all refresh tokens associated with a specific user, typically used when a user's credentials are 
    /// compromised or when they log out from all devices. This method should update the relevant refresh token records in the 
    /// data store to mark them as revoked, using the provided timestamp to indicate when the revocation occurred.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user whose refresh tokens are to be revoked.
    /// </param>
    /// <param name="revokedAt">
    /// The date and time when the refresh tokens are to be marked as revoked.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task RevokeAllRefreshTokensForUserAsync(int userId, DateTime revokedAt, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a deleted user whose normalized email matches the specified value.
    /// </summary>
    /// <remarks>This method performs a case-insensitive search using the normalized email value. The returned
    /// user is not tracked by the context.</remarks>
    /// <param name="normalizedEmail">The normalized email address to search for. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user whose normalized email
    /// matches the specified value, or null if no such user exists.</returns>
    Task<User?> GetDeletedUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a deleted user by their unique identifier. Ignores query filters.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user with the specified identifier,
    /// </returns>
    Task<User?> GetDeletedUserByIdAsync(int id, CancellationToken cancellationToken);
    #endregion
}
