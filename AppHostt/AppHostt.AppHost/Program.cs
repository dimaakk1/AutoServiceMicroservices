using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

/* ================= SQL SERVER ================= */

var sql = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

/* ================= DATABASES ================= */

var usersDb = sql.AddDatabase("UsersDb");

var catalogDb = sql.AddDatabase("CatalogDb");

var ordersDb = sql.AddDatabase("OrdersDb");

/* ================= MONGODB ================= */

var mongo = builder.AddMongoDB("mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongo");

/* ================= REDIS ================= */

var redis = builder.AddRedis("redis")
    .WithDataVolume("redis-data");

/* ================= SERVICES ================= */

var catalogService = builder
    .AddProject<Projects.AutoServiceCatalog_API>("catalog-service")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WaitFor(catalogDb)
    .WaitFor(redis);

var ordersService = builder
    .AddProject<Projects.AutoserviceOrders_API>("orders-service")
    .WithReference(ordersDb)
    .WithReference(redis)
    .WaitFor(ordersDb)
    .WaitFor(redis);

var reviewsService = builder
    .AddProject<Projects.WebApi>("reviews-service")
    .WithReference(mongo)
    .WithReference(redis)
    .WaitFor(mongo)
    .WaitFor(redis);

var usersService = builder
    .AddProject<Projects.AutoServiceUsers_API>("users-service")
    .WithReference(usersDb)
    .WaitFor(usersDb);

/* ================= GATEWAY ================= */

var apiGateway = builder
    .AddProject<Projects.ApiGateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(usersService)
    .WithExternalHttpEndpoints()
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