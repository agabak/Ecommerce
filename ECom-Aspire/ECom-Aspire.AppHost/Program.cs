var builder = DistributedApplication.CreateBuilder(args);

// API services
var apiAuth = builder.AddProject<Projects.Ecom_AuthApi>("apiauth");
var apiProduct = builder.AddProject<Projects.Ecom_ProductApi>("apiproduct");
var orderApi = builder.AddProject<Projects.Ecom_OrderApi>("apiorder");

// Web frontend
builder.AddProject<Projects.Ecom_Client>("webfrontend")
    .WithExternalHttpEndpoints()
    .WaitFor(apiAuth)
    .WaitFor(apiProduct)
    .WaitFor(orderApi);

// Worker services (just need to be running, not referenced)
builder.AddProject<Projects.Ecom_InventoryWorkerService>("apiinventory");
builder.AddProject<Projects.Ecom_ProductWorkerService>("apiproductworker");
builder.AddProject<Projects.Ecom_OderWorkerService>("apiorderworker");
builder.AddProject<Projects.Ecom_NotificationWorkerService>("apinotificationworker");

builder.Build().Run();

