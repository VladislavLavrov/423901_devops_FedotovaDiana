using Microsoft.EntityFrameworkCore;
using Calculator.Models;

namespace Calculator.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Таблица, которая будет создана в базе данных
        public DbSet<DataInputVariant> DataInputVariants { get; set; }
    }
}

