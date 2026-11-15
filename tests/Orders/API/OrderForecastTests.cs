using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoserviceOrders.API.Controllers;
using AutoserviceOrders.BLL.Services;
using AutoserviceOrders.DAL.Models;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using AutoserviceOrders.DAL.UnitOfWork;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RepairPredictionGrpc;
using VehicleGrpc;
using Xunit;

namespace AutoserviceOrders.Tests.API;

public sealed class OrderForecastTests : IAsyncLifetime
{
    private const string OrderRoute = "/api/Orders/Order/7";
    private readonly Mock<RepairPrediction.RepairPredictionClient> prediction = new(MockBehavior.Strict);
    private readonly Order order = new()
    {
        OrderId = 7, UserId = "owner", VehicleId = Guid.NewGuid(), MechanicId = "mechanic",
        OrderDate = new DateTime(2026, 10, 1, 10, 0, 0),
        PlannedCompletionAt = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc)
    };
    private WebApplication app = null!;
    private HttpClient http = null!;

    public async Task InitializeAsync()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var orders = new Mock<IOrderRepository>();
        orders.Setup(repo => repo.GetByIdAsync(7)).ReturnsAsync(order);
        orders.Setup(repo => repo.GetByIdAsync(8)).ReturnsAsync((Order)null!);
        var items = new Mock<IOrderItemRepository>();
        items.Setup(repo => repo.GetItemsByOrderIdAsync(7))
            .ReturnsAsync(new[] { new OrderItem { ProductId = 1, Quantity = 1 } });
        unitOfWork.Setup(uow => uow.GetRepository<IOrderRepository>()).Returns(orders.Object);
        unitOfWork.Setup(uow => uow.GetRepository<IOrderItemRepository>()).Returns(items.Object);

        var vehicles = new Mock<VehicleRegistry.VehicleRegistryClient>();
        vehicles.Setup(client => client.GetOwnedVehicleAsync(It.IsAny<GetOwnedVehicleRequest>(),
                null, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(Call(new VehicleBookingReply { Year = 2018, MileageKm = 80000 }));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddControllers().AddApplicationPart(typeof(OrderForecastController).Assembly);
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton(unitOfWork.Object);
        builder.Services.AddSingleton(vehicles.Object);
        builder.Services.AddSingleton(prediction.Object);
        builder.Services.AddScoped<RepairPredictionService>();
        app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        http = app.GetTestClient();
    }

    [Theory]
    [InlineData("estimated-duration", null, HttpStatusCode.Unauthorized)]
    [InlineData("delay-risk", null, HttpStatusCode.Unauthorized)]
    [InlineData("estimated-duration", "other", HttpStatusCode.NotFound)]
    [InlineData("delay-risk", "owner", HttpStatusCode.Forbidden)]
    public async Task RestrictsAccess(string endpoint, string? user, HttpStatusCode expected)
    {
        using var response = await GetAsync($"{OrderRoute}/{endpoint}", user);
        Assert.Equal(expected, response.StatusCode);
        prediction.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("estimated-duration", "owner")]
    [InlineData("delay-risk", "admin")]
    public async Task MissingOrderReturnsNotFound(string endpoint, string user)
    {
        using var response = await GetAsync($"/api/Orders/Order/8/{endpoint}", user);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        prediction.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("estimated-duration", "owner", "estimatedDurationMinutes", 94.5)]
    [InlineData("delay-risk", "admin", "delayProbability", 0)]
    public async Task ReturnsOnlyRequestedForecast(string endpoint, string user, string field, double value)
    {
        prediction.Setup(client => client.PredictRepairAsync(It.IsAny<PredictRepairRequest>(),
                null, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(Call(new PredictRepairReply { EstimatedDurationMinutes = 94.5, DelayProbability = 0 }));

        using var response = await GetAsync($"{OrderRoute}/{endpoint}", user);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var property = Assert.Single(json.EnumerateObject());
        Assert.Equal(field, property.Name);
        Assert.Equal(value, property.Value.GetDouble());
    }

    [Fact]
    public async Task MissingPlanReturnsNoRiskWithoutCallingPrediction()
    {
        order.PlannedCompletionAt = null;
        using var response = await GetAsync($"{OrderRoute}/delay-risk", "admin");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        prediction.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingVehicleReturnsNoDurationWithoutCallingPrediction()
    {
        order.VehicleId = null;
        using var response = await GetAsync($"{OrderRoute}/estimated-duration", "owner");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        prediction.VerifyNoOtherCalls();
    }

    private async Task<HttpResponseMessage> GetAsync(string path, string? user)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (user is not null)
            request.Headers.Add("X-Test-User", user);
        return await http.SendAsync(request);
    }

    private static AsyncUnaryCall<T> Call<T>(T value) =>
        new(Task.FromResult(value), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });

    public async Task DisposeAsync()
    {
        http.Dispose();
        await app.DisposeAsync();
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var user = Request.Headers["X-Test-User"].ToString();
            if (user.Length == 0)
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user),
                new Claim(ClaimTypes.Role, user == "admin" ? "Admin" : "User") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
