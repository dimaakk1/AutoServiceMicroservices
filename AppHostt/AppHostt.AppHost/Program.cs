using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var sql = builder.AddSqlServer("sqlserver")
    .WithDataVolume();



var mongo = builder.AddMongoDB("mongo")
    .WithImage("mongo:7")
    .WithDataVolume("mongo");

var redis = builder.AddRedis("redis")
    .WithDataVolume("redis-data");




var catalogService = builder.AddProject<Projects.AutoServiceCatalog_API>("catalog-service")
    .WithReference(sql)
    .WithReference(redis)
    .WaitFor(sql)
    .WaitFor(redis);


var ordersService = builder.AddProject<Projects.AutoserviceOrders_API>("orders-service")
    .WithReference(sql)
    .WithReference(redis)
    .WaitFor(sql)
    .WaitFor(redis);


var reviewsService = builder.AddProject<Projects.WebApi>("reviews-service")
    .WithReference(mongo)
    .WithReference(redis)
    .WaitFor(mongo)
    .WaitFor(redis);


var apiGateway = builder.AddProject<Projects.ApiGateway>("gateway")
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithExternalHttpEndpoints()
    .WaitFor(mongo)
    .WaitFor(sql)
    .WaitFor(redis);


var aggregationApi = builder.AddProject<Projects.AggregatorService>("aggregation-service")
    .WithReference(ordersService)
    .WithReference(reviewsService)
    .WithReference(redis)
    .WaitFor(sql)
    .WaitFor(mongo)
    .WaitFor(redis);




builder.Build().Run();