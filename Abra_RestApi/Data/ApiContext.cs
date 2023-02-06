using Abra_RestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Abra_RestApi.Data
{
    public class ApiContext:DbContext
    {
        public DbSet<User> users { get; set; }
        public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }
    }
}
