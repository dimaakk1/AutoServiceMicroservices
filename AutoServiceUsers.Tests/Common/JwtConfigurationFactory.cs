using Microsoft.Extensions.Configuration;

namespace AutoServiceUsers.Tests.Common;

internal static class JwtConfigurationFactory
{
    public static IConfiguration Create() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "UnitTestSecretKey_AtLeast32Characters!",
                ["Jwt:Issuer"] = "autoservice-users-tests",
                ["Jwt:Audience"] = "autoservice-users-tests"
            })
            .Build();
}
