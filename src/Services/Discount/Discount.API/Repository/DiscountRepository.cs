using System.Threading.Tasks;
using Dapper;
using Discount.API.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.API.Repository
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly string _connectionString;
        private readonly Coupon noDiscountCoupon = 
                new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc"};

        public DiscountRepository(IConfiguration configuration)
        {
            _connectionString = configuration["DatabaseSettings:ConnectionString"];
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            string command = "Select * from Coupon Where ProductName = @ProductName";

            using var connection = new NpgsqlConnection(_connectionString);
            var coupon = await 
                connection.QueryFirstOrDefaultAsync<Coupon>(command, new {ProductName = productName});
            
            return coupon ?? noDiscountCoupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            string command = 
                "Insert into Coupon (ProductName, Description, Amount) Values (@ProductName, @Description, @Amount)";

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.ExecuteAsync(command, new {
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = coupon.Amount
            });
            
            return result > 0;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            string command = 
                "Update Coupon Set ProductName = @ProductName, Description = @Description, Amount = @Amount Where Id = @Id";

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.ExecuteAsync(command, new {
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = coupon.Amount,
                Id = coupon.Id
            });
            
            return result > 0;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            string command = "Delete from Coupon Where ProductName = @ProductName";

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await 
                connection.ExecuteAsync(command, new {ProductName = productName});
            
            return result > 0;
        }
    }
}