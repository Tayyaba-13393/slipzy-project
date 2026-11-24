
using Dapper;
using SlipzyD2.Data;
using SlipzyD2.Models;
using SlipzyD2.Repositories.Interfaces;

namespace SlipzyD2.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory _factory;

        public CategoryRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            const string sql = @"
                SELECT CategoryId, CategoryName
                FROM dbo.Categories
                ORDER BY CategoryName;";

            using var connection = _factory.CreateConnection();

            return await connection.QueryAsync<Category>(sql);
        }
    }
}
