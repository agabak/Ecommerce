using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ECom.Infrastructure.DataAccess.User.Repositories;

// ... other using statements

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register your services
        services.AddScoped<IUserRepository>(
            _ => new UserMySqlRepository(context.Configuration.GetConnectionString("mysqlConnection")!)
        );
        // ...etc.
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

// Example usage
 var repo = host.Services.GetRequiredService<IUserRepository>();

var scope = await repo.CreateUser(new Ecommerce.Common.Models.Users.CreateUserDto
{
    Username = "testuser3",
    Email = "agaba_k@hotmail3.com",
    Street = "123 Main St",
    City = "Test City",
    State = "Test State",
    ZipCode = "12345",
    Password ="password",
    Roles = new List<string> { "Customer" },
    Phone = "123-456-7890"

});
