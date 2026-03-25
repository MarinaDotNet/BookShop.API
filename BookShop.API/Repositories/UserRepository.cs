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
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a deleted user whose normalized email matches the specified value.
    /// </summary>
    /// <remarks>This method performs a case-insensitive search using the normalized email value. The returned
    /// user is not tracked by the context.</remarks>
    /// <param name="normalizedEmail">The normalized email address to search for. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user whose normalized email
    /// matches the specified value, or null if no such user exists.</returns>
    public async Task<User?> GetDeletedUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.IsDeleted, cancellationToken);
    }

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
    public async Task<User?> GetDeletedUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted, cancellationToken);
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

    /// <summary>
    /// Asynchronously retrieves a refresh token entity from the data store based on its hashed value. This method is used to find 
    /// a refresh token that matches the provided hash, which is typically generated from the original token value for security purposes. The returned refresh token can be used to validate token renewal requests or to manage active sessions for users.
    /// </summary>
    /// <param name="hash">
    /// The hashed value of the refresh token to be retrieved. This should be a non-null, non-empty string that represents the 
    /// hash of the original refresh token. The method will search for a refresh token entity in the data store that matches this 
    /// hash.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation. This allows the caller to gracefully handle 
    /// cancellation requests, such as when a user logs out or when the application is shutting down.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the refresh token entity if found, or null if 
    /// not found.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided hash is null, empty, or consists only of whitespace characters.
    /// </exception>
    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string hash, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash, nameof(hash));

        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves any pending changes in the context to the data store. This method is typically used to persist changes 
    /// made to user and role entities after performing operations such as adding or updating records. The method accepts a 
    /// cancellation token that can be used to cancel the save operation if needed, allowing for graceful handling of cancellation 
    /// requests in scenarios such as application shutdown or user logout.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
     _context.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// Asynchronously revokes all active refresh tokens associated with a specific user by setting their revoked timestamp. 
    /// This method is typically used when a user logs out or when there is a need to invalidate all existing sessions for a user. 
    /// The method retrieves all refresh tokens for the specified user that have not yet been revoked, updates their revoked 
    /// timestamp to the provided value, and saves the changes to the data store. If there are no active tokens to revoke, 
    /// the method simply returns without making any changes.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user whose refresh tokens are to be revoked. This should be a positive integer 
    /// corresponding to an existing user in the data store.
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
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the provided userId is less than or equal to zero, indicating an invalid user identifier.
    /// </exception>
     public async Task RevokeAllRefreshTokensForUserAsync(int userId, DateTime revokedAt, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId, nameof(userId));
        
        var tokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToListAsync(cancellationToken);
        
        if(tokens.Count == 0)
        {
            return; // No active tokens to revoke
        }
        
        foreach (var token in tokens)
        {
            token.RevokedAt = revokedAt;
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
    #endregion of Role Management

}
