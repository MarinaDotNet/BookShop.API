using BookShop.API.Infrastructure.Persistence;
using BookShop.API.Models.Auth;

namespace BookShop.API.Repositories;

/// <summary>
/// The repository for managing <see cref="User"/> entities.
/// 
/// This is a concrete <see cref="IUserRepository"/> implementation that uses Entity Framework Core  
/// via <see cref="AuthDbContext"/> to perform data access operations.
/// </summary>
/// <param name="context">
/// The <see cref="AuthDbContext"/> instance used to access the database.
/// </param>
public class UserRepository(AuthDbContext context) : IUserRepository
{
    private readonly AuthDbContext _context = context;

    /// <summary>
    /// Adds a new <see cref="User"/> to the database and persists the change asynchronously.
    /// </summary>
    /// <param name="user">The <see cref="User"/> entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A Task that returns the added <see cref="User"/> entity.
    /// </returns>
    public async Task<User> AddUserAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
