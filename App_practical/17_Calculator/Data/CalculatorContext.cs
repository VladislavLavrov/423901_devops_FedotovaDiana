using Calculator.Models;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data
{
    public class CalculatorContext : DbContext
    {
        public CalculatorContext(DbContextOptions<CalculatorContext> options) : base(options)
        {
        }

        public DbSet<DataInputVariant> DataInputVariants { get; set; }
    }
}

