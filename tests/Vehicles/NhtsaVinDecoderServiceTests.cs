using System.Net;
using System.Text;
using AutoserviceVehicle.BLL.Services;
using Xunit;

namespace AutoserviceVehicle.Tests;

public sealed class NhtsaVinDecoderServiceTests
{
    [Fact]
    public async Task MapsNhtsaResponseToApplicationContract()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """
            {"Results":[{"Make":"VOLKSWAGEN","Model":"Golf","ModelYear":"2010","Manufacturer":"VOLKSWAGEN AG","VehicleType":"PASSENGER CAR","BodyClass":"Hatchback/Liftback/Notchback","Doors":"5","FuelTypePrimary":"Gasoline","DisplacementL":"1.6","EngineCylinders":"4","TransmissionStyle":"Automatic","TransmissionSpeeds":"6","DriveType":"FWD/Front-Wheel Drive","PlantCountry":"GERMANY","ErrorCode":"0"}]}
            """);
        var service = CreateService(handler);

        var result = await service.DecodeAsync("wvwzzz1kzaw000001");

        Assert.Equal("WVWZZZ1KZAW000001", result.Vin);
        Assert.Equal("VOLKSWAGEN", result.Make);
        Assert.Equal(2010, result.ModelYear);
        Assert.Equal(1.6m, result.EngineDisplacementLiters);
        Assert.True(result.IsComplete);
        Assert.Empty(result.Warnings);
        Assert.Contains("DecodeVinValuesExtended/WVWZZZ1KZAW000001", handler.LastRequestUri!.ToString());
    }

    [Fact]
    public async Task ProviderFailureBecomesServiceUnavailableError()
    {
        var service = CreateService(new StubHandler(HttpStatusCode.ServiceUnavailable, "{}"));
        await Assert.ThrowsAsync<VinProviderUnavailableException>(() => service.DecodeAsync("WVWZZZ1KZAW000001"));
    }

    [Fact]
    public async Task IncompleteResultBecomesNotFound()
    {
        var service = CreateService(new StubHandler(HttpStatusCode.OK, "{\"Results\":[{\"Make\":\"\",\"Model\":\"\",\"ErrorCode\":\"1\"}]}"));
        await Assert.ThrowsAsync<VinNotFoundException>(() => service.DecodeAsync("WVWZZZ1KZAW000001"));
    }

    private static NhtsaVinDecoderService CreateService(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://vpic.nhtsa.dot.gov/api/") });

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
