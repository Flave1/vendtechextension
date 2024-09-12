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

    public virtual DbSet<BusinessUsers> BusinessUsers { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }

}
