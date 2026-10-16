#nullable enable
using AriaPrint.Repository;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AriaPrint.BL
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        private SqlConnection Connection => new SqlConnection(_connectionString);

        public async Task<AppUser?> GetByPhoneNumberAsync(long phoneNumber)
        {
            using var conn = Connection;
            return await conn.QueryFirstOrDefaultAsync<AppUser>(
                "SELECT * FROM UsersTbl WHERE PhoneNumber = @PhoneNumber", new { PhoneNumber = phoneNumber });
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            using var conn = Connection;
            return await conn.QueryFirstOrDefaultAsync<AppUser>(
                "SELECT * FROM UsersTbl WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(AppUser user)
        {
            using var conn = Connection;
            var sql = @"INSERT INTO UsersTbl (FirstName, LastName, PhoneNumber, PasswordHash, Role)
                    VALUES (@FirstName, @LastName, @PhoneNumber, @PasswordHash, @Role);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }
    }
}