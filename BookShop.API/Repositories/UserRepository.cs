using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Repositories;

/// <summary>
/// Provides user and role management operations for the authentication data store.
/// </summary>
/// <remarks>This repository offers asynchronous methods for adding and retrieving users and roles. All queries
/// are performed against the provided context, and returned entities are not tracked unless otherwise specified. Thread
/// safety depends on the usage of the underlying context; concurrent access to the same context instance is not
/// supported.</remarks>
/// <param name="context">The database context used to access user and role entities. Cannot be null.</param>
public class UserRepository(AuthDbContext context) : IUserRepository
{
    private readonly AuthDbContext _context = context;

    #region of User Management
    /// <summary>
    /// Asynchronously adds a new user to the data store.
    /// </summary>
    /// <param name="user">The user entity to add. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added user entity.</returns>
    public async Task<User> AddUserAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <summary>
    /// Asynchronously retrieves a user whose normalized email matches the specified value.
    /// </summary>
    /// <remarks>This method performs a case-insensitive search using the normalized email value. The returned
    /// user is not tracked by the context.</remarks>
    /// <param name="normalizedEmail">The normalized email address to search for. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user whose normalized email
    /// matches the specified value, or null if no such user exists.</returns>
    public async Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail, nameof(normalizedEmail));

        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a user whose normalized username matches the specified value.
    /// </summary>
    /// <param name="normalizedUsername">The normalized username to search for. This value is typically uppercase and must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user whose normalized username
    /// matches the specified value, or null if no such user exists.</returns>
    public async Task<User?> GetUserByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUsername, nameof(normalizedUsername));

        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user with the specified identifier,
    /// </returns>
    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Asynchronously updates an existing user in the data store.
    /// </summary>
    /// <param name="user">
    /// The user entity to update. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated user entity,
    /// </returns>
    public async Task<User?> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
    #endregion of User Management

    #region of Role Management

    /// <summary>
    /// Asynchronously retrieves a role entity that matches the specified role name.
    /// </summary>
    /// <remarks>The search is case-sensitive and returns the first matching role found. The returned entity
    /// is not tracked by the context.</remarks>
    /// <param name="roleName">The name of the role to search for. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Role"/> object if a role with the specified name exists; otherwise, <see langword="null"/>.</returns>
    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName, nameof(roleName));

        return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    }

    /// <summary>
    /// Asynchronously adds a new user role to the data store.
    /// </summary>
    /// <param name="entity">The user role entity to add. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added user role entity, or null
    /// if the operation was unsuccessful.</returns>
    public async Task<UserRole?> AddUserRoleAsync(UserRole entity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        await _context.UserRoles.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <summary>
    /// Asynchronously retrieves the roles associated with a specific user by their unique identifier.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user whose roles are to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of <see cref="UserRole"/> entities associated with the specified user. If the user has no roles, the collection will be empty.
    /// </returns>
    public async Task<ICollection<UserRole>> GetUserRolesAsync(int userId, CancellationToken cancellationToken)
    {
        if(userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be a positive integer.");
        }

        return await _context.UserRoles
        .Include(ur => ur.Role)
        .Where(ur => ur.UserId == userId)
        .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves a refresh token to the data store for a specific user. This method adds the provided refresh token entity to the context and persists the changes to the database. The refresh token is associated with a user and can be used for token renewal processes in authentication workflows.
    /// </summary>
    /// <param name="refreshToken">
    /// The <see cref="RefreshToken"/> entity to be saved. This entity should contain the necessary information such as the token value, expiration time, and associated user ID. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation. This allows the caller to gracefully handle cancellation requests, such as when a user logs out or when the application is shutting down.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the refresh token has been successfully saved to the data store. If the operation fails, an exception may be thrown.
    /// </returns>
    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken, nameof(refreshToken));

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    #endregion of Role Management

}
