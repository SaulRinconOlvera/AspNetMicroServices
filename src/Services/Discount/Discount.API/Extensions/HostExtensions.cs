using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        private static ILogger _logger;
        private static IConfiguration _configuration;

        public static IHost MigrateDatabase<TContext>(this IHost host, int retry = 0)
        {
            using(var scope = host.Services.CreateScope()){
                InitializeLocalVariables<TContext>(scope);
                CreateDatabase(retry);
            }

            return host;
        }

        private static void CreateDatabase(int retry)
        {
            while(retry >= 0)
            {
                try
                {
                    InitializeDatabase();
                    break;
                }
                catch (NpgsqlException ex)
                {
                    retry--;
                    
                    _logger.LogError(ex, "An error ocurred while migrating the postgresql database");
                    Thread.Sleep(2000);
                }
            }
        }

        private static void InitializeDatabase()
        {
            _logger.LogInformation("Initializing postgres database");
            
            using(var connection = new NpgsqlConnection(_configuration["DatabaseSettings:ConnectionString"]))
            {
                connection.Open();

                DropTableIfExists(connection);
                CreateTable(connection);
                SeedTable(connection);

                connection.Close();
            }
        }

        private static void SeedTable(NpgsqlConnection connection)
        {
            _logger.LogInformation("Seeding Coupon table");

            var command = "Insert into Coupon(ProductName, Description, Amount) Values('IPhone X','IPhone Discount', 150);";
            ExecuteCommand(connection, command);

            command = "Insert into Coupon(ProductName, Description, Amount) Values('Samsung 10','Samsung Discount', 100);";
            ExecuteCommand(connection, command);
        }

        private static void CreateTable(NpgsqlConnection connection)
        {
            _logger.LogInformation("Creating Coupon table");

            var command = @"Create Table Coupon(Id Serial Primary Key, ProductName VarChar(24) Not Null, Description Text, Amount Int);";
            ExecuteCommand(connection, command);
        }

        private static void DropTableIfExists(NpgsqlConnection connection)
        {
            _logger.LogInformation("Dropping table Coupon if exists");

            var command = "Drop Table If Exists Coupon";
            ExecuteCommand(connection, command);
        }

        private static void ExecuteCommand(NpgsqlConnection connection, string commandText)
        {
            _logger.LogInformation($"Executing command: {commandText}");
            using(var command = new NpgsqlCommand {Connection = connection }){
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        private static void InitializeLocalVariables<TContext>(IServiceScope scope)
        {
            var services = scope.ServiceProvider;

            _configuration = services.GetRequiredService<IConfiguration>();
            _logger = services.GetRequiredService<ILogger<TContext>>();
        }
    }
}