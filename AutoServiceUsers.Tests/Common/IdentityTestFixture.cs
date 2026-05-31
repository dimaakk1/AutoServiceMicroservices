using AutoServiceUsers.DAL.DB;
using AutoServiceUsers.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoServiceUsers.Tests.Common;

internal sealed class IdentityTestFixture : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public ApplicationDbContext Context { get; }
    public UserManager<ApplicationUser> UserManager { get; }

    private IdentityTestFixture(ServiceProvider provider, IServiceScope scope, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _provider = provider;
        _scope = scope;
        Context = context;
        UserManager = userManager;
    }

    public static async Task<IdentityTestFixture> CreateAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return new IdentityTestFixture(provider, scope, context, userManager);
    }

    public async Task<ApplicationUser> SeedUserAsync(
        string username = "testuser",
        string email = "test@example.com",
        string password = "Password123!",
        bool emailConfirmed = true,
        IEnumerable<RefreshToken>? refreshTokens = null)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = emailConfirmed
        };

        var createResult = await UserManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        if (refreshTokens != null)
        {
            var dbUser = await Context.Users
                .Include(u => u.RefreshTokens)
                .FirstAsync(u => u.Id == user.Id);

            foreach (var token in refreshTokens)
            {
                token.UserId = user.Id;
                dbUser.RefreshTokens.Add(token);
            }

            await Context.SaveChangesAsync();
        }

        return user;
    }

    public async ValueTask DisposeAsync()
    {
        _scope.Dispose();
        if (_provider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _provider.Dispose();
        }
    }
}
