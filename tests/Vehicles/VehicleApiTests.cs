using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using AutoserviceVehicle.BLL.DTO;
using AutoserviceVehicle.BLL.Services;
using AutoserviceVehicle.DAL.DB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AutoserviceVehicle.Tests;

public sealed class VehicleApiTests : IDisposable
{
    private readonly VehicleApplication _app = new();
    private static SaveVehicleDto Car(string vin = "WVWZZZ1KZAW000001") => new()
    {
        Vin = vin, Make = "Volkswagen", Model = "Golf", Year = 2010, MileageKm = 150000
    };

    [Fact]
    public async Task OwnerCanCreateReadUpdateAndDeleteAcrossRequests()
    {
        using var client = _app.Client("owner");
        var request = Car("wvwzzz1kzaw000001");
        request.Make = " Volkswagen ";
        var response = await client.PostAsJsonAsync("/api/vehicles", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var vehicle = (await response.Content.ReadFromJsonAsync<VehicleDto>())!;
        Assert.Equal("WVWZZZ1KZAW000001", vehicle.Vin);
        Assert.Equal("Volkswagen", vehicle.Make);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<List<VehicleDto>>("/api/vehicles"))!);

        request.MileageKm = 151000;
        var update = await client.PutAsJsonAsync($"/api/vehicles/{vehicle.Id}", request);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(151000, (await client.GetFromJsonAsync<VehicleDto>($"/api/vehicles/{vehicle.Id}"))!.MileageKm);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/vehicles/{vehicle.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/vehicles/{vehicle.Id}")).StatusCode);
    }

    [Fact]
    public async Task OtherUserCannotReadEditOrDeleteVehicle()
    {
        using var owner = _app.Client("owner");
        using var other = _app.Client("other");
        var response = await owner.PostAsJsonAsync("/api/vehicles", Car());
        var vehicle = (await response.Content.ReadFromJsonAsync<VehicleDto>())!;
        var path = $"/api/vehicles/{vehicle.Id}";
        Assert.Empty((await other.GetFromJsonAsync<List<VehicleDto>>("/api/vehicles"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PutAsJsonAsync(path, Car())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task SubmittedUserIdDoesNotOverrideTokenOwner()
    {
        using var owner = _app.Client("owner");
        using var other = _app.Client("other");
        var response = await owner.PostAsJsonAsync("/api/vehicles", new
        {
            userId = "other", vin = "WVWZZZ1KZAW000001", make = "VW", model = "Golf", year = 2010
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single((await owner.GetFromJsonAsync<List<VehicleDto>>("/api/vehicles"))!);
        Assert.Empty((await other.GetFromJsonAsync<List<VehicleDto>>("/api/vehicles"))!);
    }

    [Fact]
    public async Task DuplicateVinIsRejectedPerOwnerIncludingOnUpdate()
    {
        using var owner = _app.Client("owner");
        using var other = _app.Client("other");
        Assert.Equal(HttpStatusCode.Created, (await owner.PostAsJsonAsync("/api/vehicles", Car())).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.PostAsJsonAsync("/api/vehicles", Car("wvwzzz1kzaw000001"))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await other.PostAsJsonAsync("/api/vehicles", Car())).StatusCode);
        var second = await owner.PostAsJsonAsync("/api/vehicles", Car("WVWZZZ1KZAW000002"));
        var vehicle = (await second.Content.ReadFromJsonAsync<VehicleDto>())!;
        Assert.Equal(HttpStatusCode.Conflict, (await owner.PutAsJsonAsync($"/api/vehicles/{vehicle.Id}", Car())).StatusCode);
    }

    [Theory]
    [InlineData("short", "VW", 2010, 100)]
    [InlineData("WVWZZZ1KZAW00000I", "VW", 2010, 100)]
    [InlineData("WVWZZZ1KZAW000001", " ", 2010, 100)]
    [InlineData("WVWZZZ1KZAW000001", "VW", 1800, 100)]
    [InlineData("WVWZZZ1KZAW000001", "VW", 2100, 100)]
    [InlineData("WVWZZZ1KZAW000001", "VW", 2010, -1)]
    public async Task InvalidVehicleIsRejected(string vin, string make, int year, int mileage)
    {
        using var client = _app.Client("owner");
        var request = Car(vin);
        request.Make = make;
        request.Year = year;
        request.MileageKm = mileage;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/vehicles", request)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<List<VehicleDto>>("/api/vehicles"))!);
    }

    [Theory]
    [InlineData("GET", "/api/vehicles")]
    [InlineData("GET", "/api/vehicles/11111111-1111-1111-1111-111111111111")]
    [InlineData("POST", "/api/vehicles")]
    [InlineData("PUT", "/api/vehicles/11111111-1111-1111-1111-111111111111")]
    [InlineData("DELETE", "/api/vehicles/11111111-1111-1111-1111-111111111111")]
    public async Task AllEndpointsRequireAuthentication(string method, string path)
    {
        using var client = _app.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(Car()) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task TokenWithoutOwnerIsForbidden()
    {
        using var client = _app.Client(null);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/vehicles")).StatusCode);
    }

    [Fact]
    public async Task VinDecoderIsPublicAndReturnsVehicleDetails()
    {
        using var client = _app.CreateClient();

        var response = await client.GetAsync("/api/vehicles/decode/WVWZZZ1KZAW000001");
        var decoded = await response.Content.ReadFromJsonAsync<VinDecodeDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(decoded);
        Assert.Equal("Volkswagen", decoded.Make);
        Assert.Equal("Golf", decoded.Model);
        Assert.Equal(2010, decoded.ModelYear);
    }

    [Fact]
    public async Task VinDecoderRejectsInvalidVinWithoutAuthentication()
    {
        using var client = _app.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/vehicles/decode/invalid")).StatusCode);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task InvalidOrExpiredTokenIsRejected(bool wrongSignature, bool expired)
    {
        using var client = _app.Client("owner", wrongSignature, expired);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/vehicles")).StatusCode);
    }

    public void Dispose() => _app.Dispose();
}

internal sealed class VehicleApplication : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public VehicleApplication() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:ApplyMigrations", "false");
        builder.UseSetting("Jwt:Key", "vehicle-tests-only-signing-key-1234567890");
        builder.UseSetting("ConnectionStrings:VehiclesDb", "Server=unused;Database=unused");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<VehicleDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<VehicleDbContext>>();
            services.RemoveAll<IVinDecoderService>();
            services.AddDbContext<VehicleDbContext>(options => options.UseSqlite(_connection));
            services.AddSingleton<IVinDecoderService, FakeVinDecoderService>();
        });
    }

    public HttpClient Client(string? userId, bool wrongSignature = false, bool expired = false)
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<VehicleDbContext>().Database.EnsureCreated();
        var validation = Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer").TokenValidationParameters;
        var key = wrongSignature ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("wrong-test-signing-key-1234567890123456")) : validation.IssuerSigningKey;
        var token = new JwtSecurityToken(validation.ValidIssuer, validation.ValidAudience,
            userId is null ? [] : [new Claim(ClaimTypes.NameIdentifier, userId)],
            expires: DateTime.UtcNow.AddMinutes(expired ? -10 : 10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

internal sealed class FakeVinDecoderService : IVinDecoderService
{
    public Task<VinDecodeDto> DecodeAsync(string vin, CancellationToken cancellationToken = default)
    {
        if (vin != "WVWZZZ1KZAW000001")
            throw new System.ComponentModel.DataAnnotations.ValidationException("Некоректний VIN.");

        return Task.FromResult(new VinDecodeDto(
            vin, "Volkswagen", "Golf", 2010, "Volkswagen AG", "PASSENGER CAR", "Hatchback/Liftback/Notchback",
            5, "Gasoline", 1.6m, 4, null, "Automatic", 6, "FWD", null, null, "Germany", "Wolfsburg",
            null, true, []));
    }
}
