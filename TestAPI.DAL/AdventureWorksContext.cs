using System.Data.Entity;
using TestAPI.DAL.Models;

namespace TestAPI.DAL
{
    public class AdventureWorksContext : DbContext
    {
        public AdventureWorksContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
