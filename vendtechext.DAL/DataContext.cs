using Microsoft.EntityFrameworkCore;

namespace vendtechext.DAL.Models;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Integrator> Integrators { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=92.205.181.48;Database=VENDTECHEXT_DEV;User Id=vendtech_main;Password=85236580@Ve;MultipleActiveResultSets=True;TrustServerCertificate=true;");
        }
    }
}
