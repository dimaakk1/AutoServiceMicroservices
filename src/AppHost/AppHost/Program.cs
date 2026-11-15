using Aspire.Hosting;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

/* ================= SQL SERVER ================= */

var sql = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

/* ================= DATABASES ================= */

var usersDb = sql.AddDatabase("UsersDb");

var catalogDb = sql.AddDatabase("CatalogDb");

var ordersDb = sql.AddDatabase("OrdersDb");
var vehiclesDb = sql.AddDatabase("VehiclesDb");

/* ================= MONGODB ================= */

var mongo = builder.AddMongoDB("mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongo");

/* ================= REDIS ================= */

var redis = builder.AddRedis("redis")
    .WithDataVolume("redis-data");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume("rabbitmq-data");

/* ================= SERVICES ================= */

var notificationService = builder
    .AddProject<Projects.AutoserviceNotification>("autoservicenotification")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var catalogService = builder
    .AddProject<Projects.AutoServiceCatalog_API>("catalog-service", launchProfileName: "https")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WaitFor(catalogDb)
    .WaitFor(redis);

var vehicleService = builder
    .AddProject<Projects.AutoserviceVehicle_API>("autoservicevehicle", launchProfileName: "https")
    .WithReference(vehiclesDb)
    .WaitFor(vehiclesDb);

var ordersService = builder
    .AddProject<Projects.AutoserviceOrders_API>("orders-service", launchProfileName: "https")
    .WithReference(ordersDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(vehicleService)
    .WithReference(catalogService)
    .WithEnvironment("Services__VehicleServiceUrl", vehicleService.GetEndpoint("https"))
    .WithEnvironment("Services__CatalogServiceUrl", catalogService.GetEndpoint("https"))
    .WaitFor(ordersDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(catalogService)
    .WaitFor(vehicleService);

var reviewsService = builder
    .AddProject<Projects.WebApi>("reviews-service", launchProfileName: "https")
    .WithReference(mongo)
    .WithReference(redis)
    .WaitFor(mongo)
    .WaitFor(redis);

var usersService = builder
    .AddProject<Projects.AutoServiceUsers_API>("users-service", launchProfileName: "https")
    .WithReference(usersDb)
    .WithEnvironment("PublicUrls__ApiBaseUrl", "http://localhost:5000")
    .WithEnvironment("PublicUrls__FrontendBaseUrl", "http://localhost:5173")
    .WaitFor(usersDb);

var aiService = builder
    .AddProject<Projects.AutoserviceAI_API>("autoserviceai", launchProfileName: "https")
    .WithReference(catalogService)
    .WaitFor(catalogService);

if (builder.Environment.IsDevelopment())
{
    var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", ".."));
    var predictionDirectory = Path.Combine(repoRoot, "src", "Services", "RepairPrediction");
    var modelDirectory = Path.Combine(repoRoot, "artifacts", "ml");
    var venvPython = OperatingSystem.IsWindows() ? Path.Combine("Scripts", "python.exe") : Path.Combine("bin", "python");
    var configuredPython = builder.Configuration["RepairPrediction:PythonExecutable"];
    var defaultPython = new[]
    {
        Path.Combine(predictionDirectory, ".venv", venvPython),
        Path.Combine(modelDirectory, "training-venv", venvPython),
        Path.Combine(modelDirectory, ".venv", venvPython)
    }.FirstOrDefault(File.Exists);
    var predictionPython = string.IsNullOrWhiteSpace(configuredPython) ? defaultPython : configuredPython;

    if (predictionPython is not null
        && File.Exists(Path.Combine(modelDirectory, "duration", "duration.joblib"))
        && File.Exists(Path.Combine(modelDirectory, "delay", "delay.joblib")))
    {
        var repairPrediction = builder.AddExecutable("repair-prediction", predictionPython, predictionDirectory,
                "-u", "prediction_server.py", "--model-dir", modelDirectory)
            .WithHttpEndpoint(name: "grpc", env: "REPAIR_PREDICTION_PORT");

        // No WaitFor: prediction is optional and must not hold up orders or booking.
        ordersService.WithEnvironment("Services__RepairPredictionUrl", repairPrediction.GetEndpoint("grpc"));
    }
    else
    {
        Console.WriteLine("RepairPrediction skipped: set up the Python environment and both model files (see RepairPrediction/README.md).");
    }

    builder.AddProject<Projects.MlSeed>("ml-seed", launchProfileName: null)
        .WithReference(ordersDb)
        .WithReference(catalogDb)
        .WithReference(usersDb)
        .WithReference(vehiclesDb)
        .WithArgs("--apphost", "--apply")
        .WaitFor(ordersService)
        .WaitFor(catalogService)
        .WaitFor(usersService)
        .WaitFor(vehicleService)
        .WithExplicitStart();

    builder.AddProject<Projects.MlSeed>("ml-export", launchProfileName: null)
        .WithReference(ordersDb)
        .WithArgs("--apphost", "--export")
        .WithEnvironment("ML_EXPORT_OUTPUT", Path.GetFullPath(Path.Combine(builder.AppHostDirectory,
            "..", "..", "..", "artifacts", "ml", "repair-orders.csv")))
        .WaitFor(ordersService)
        .WithExplicitStart();
}

var telegramService = builder
    .AddProject<Projects.AutoServiceTelegram_API>("autoservicetelegram")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);




var aggregationApi = builder
    .AddProject<Projects.AggregatorService>("aggregation-service", launchProfileName: "https")
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(usersService)
    .WithReference(redis)
    .WithEnvironment("Services__OrdersServiceUrl", ordersService.GetEndpoint("https"))
    .WithEnvironment("Services__ReviewsServiceUrl", reviewsService.GetEndpoint("https"))
    .WithEnvironment("Services__UsersServiceUrl", usersService.GetEndpoint("https"))
    .WaitFor(ordersService)
    .WaitFor(reviewsService)
    .WaitFor(usersService)
    .WaitFor(redis);

/* ================= GATEWAY ================= */

var apiGateway = builder
    .AddProject<Projects.ApiGateway>("gateway", launchProfileName: "https")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(usersService)
    .WithReference(aiService)
    .WithReference(vehicleService)
    .WithReference(aggregationApi)
    .WithEnvironment("ReverseProxy__Clusters__auth-cluster__Destinations__auth-service__Address", usersService.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__orders-cluster__Destinations__orders-service__Address", ordersService.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__catalog-cluster__Destinations__catalog-service__Address", catalogService.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__reviews-cluster__Destinations__reviews-service__Address", reviewsService.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__aggregation-cluster__Destinations__aggregation-service__Address", aggregationApi.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__ai-cluster__Destinations__ai-service__Address", aiService.GetEndpoint("https"))
    .WithEnvironment("ReverseProxy__Clusters__vehicles-cluster__Destinations__vehicle-service__Address", vehicleService.GetEndpoint("https"))
    .WithExternalHttpEndpoints()
    .WaitFor(aggregationApi)
    .WaitFor(aiService)
    .WaitFor(sql)
    .WaitFor(mongo)
    .WaitFor(redis);









builder.Build().Run();
