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

    #endregion of Role Management

}
