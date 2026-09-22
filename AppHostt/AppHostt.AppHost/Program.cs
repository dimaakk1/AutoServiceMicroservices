using Aspire.Hosting;

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
    .AddProject<Projects.AutoServiceCatalog_API>("catalog-service")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WaitFor(catalogDb)
    .WaitFor(redis);

var vehicleService = builder
    .AddProject<Projects.AutoserviceVehicle_API>("autoservicevehicle")
    .WithReference(vehiclesDb)
    .WaitFor(vehiclesDb);

var ordersService = builder
    .AddProject<Projects.AutoserviceOrders_API>("orders-service")
    .WithReference(ordersDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(vehicleService)
    .WithEnvironment("Services__VehicleServiceUrl", vehicleService.GetEndpoint("https"))
    .WaitFor(ordersDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(vehicleService);

var reviewsService = builder
    .AddProject<Projects.WebApi>("reviews-service")
    .WithReference(mongo)
    .WithReference(redis)
    .WaitFor(mongo)
    .WaitFor(redis);

var usersService = builder
    .AddProject<Projects.AutoServiceUsers_API>("users-service")
    .WithReference(usersDb)
    .WithEnvironment("PublicUrls__ApiBaseUrl", "http://localhost:5000")
    .WithEnvironment("PublicUrls__FrontendBaseUrl", "http://localhost:5173")
    .WaitFor(usersDb);

var aiService = builder
    .AddProject<Projects.AutoserviceAI_API>("autoserviceai")
    .WithReference(catalogService)
    .WaitFor(catalogService);

var telegramService = builder
    .AddProject<Projects.AutoServiceTelegram_API>("autoservicetelegram")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);




/* ================= GATEWAY ================= */

var apiGateway = builder
    .AddProject<Projects.ApiGateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(usersService)
    .WithReference(aiService)
    .WithReference(vehicleService)
    .WithExternalHttpEndpoints()
    .WaitFor(aiService)
    .WaitFor(sql)
    .WaitFor(mongo)
    .WaitFor(redis);

/* ================= AGGREGATION ================= */

var aggregationApi = builder
    .AddProject<Projects.AggregatorService>("aggregation-service")
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(redis)
    .WaitFor(redis);








builder.Build().Run();
