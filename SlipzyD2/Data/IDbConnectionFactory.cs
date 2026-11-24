
using System.Data;

namespace SlipzyD2.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
