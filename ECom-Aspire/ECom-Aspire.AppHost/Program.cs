var builder = DistributedApplication.CreateBuilder(args);

// API services
var apiAuth = builder.AddProject<Projects.Ecom_AuthApi>("authapi");
var apiProduct = builder.AddProject<Projects.Ecom_ProductApi>("productapi");
var orderApi = builder.AddProject<Projects.Ecom_OrderApi>("orderapi");

// Web frontend
builder.AddProject<Projects.Ecom_Client>("webfrontend")
    .WithExternalHttpEndpoints()
    .WaitFor(apiAuth)
    .WaitFor(apiProduct)
    .WaitFor(orderApi);

// Worker services (just need to be running, not referenced)
builder.AddProject<Projects.Ecom_InventoryWorkerService>("inventoryservice");
builder.AddProject<Projects.Ecom_ProductWorkerService>("productservice");
builder.AddProject<Projects.Ecom_OderWorkerService>("orderservice");
builder.AddProject<Projects.Ecom_NotificationWorkerService>("notificationservice");
builder.AddProject<Projects.Ecom_OrderInventoryService>("orderinventoryservice");
builder.AddProject<Projects.Ecom_PaymentWorkerService>("paymentworkerservice");

builder.Build().Run();

