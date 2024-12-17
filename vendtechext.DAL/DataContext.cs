using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace vendtechext.DAL.Models;

public partial class DataContext : IdentityDbContext<AppUser>
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }


    private readonly IHttpContextAccessor _httpContextAccessor;
    public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DbSet<Integrator> Integrators { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<Log> Logs { get; set; }
    public virtual DbSet<Wallet> Wallets { get; set; }
    public virtual DbSet<Deposit> Deposits { get; set; }
    public virtual DbSet<AppSetting> AppSettings { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=92.205.181.48;Database=VENDTECHEXT_DEV;User Id=vendtech_main;Password=85236580@Ve;MultipleActiveResultSets=True;TrustServerCertificate=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity => { entity.ToTable(name: "Users"); });
        modelBuilder.Entity<IdentityRole>(entity => { entity.ToTable(name: "Roles"); });
        modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
        modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        foreach (var entry in ChangeTracker.Entries<AuditTrail>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Deleted = false;
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedAt = DateTime.Now;
            }
            else
            {
                entry.Entity.UpdatedAt = DateTime.Now;
                entry.Entity.UpdatedBy = userId;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        foreach (var entry in ChangeTracker.Entries<AuditTrail>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Deleted = false;
                entry.Entity.CreatedAt = DateTime.Now;
                entry.Entity.CreatedBy = userId;
            }
            else
            {
                entry.Entity.UpdatedAt = DateTime.Now;
                entry.Entity.UpdatedBy = userId;
            }
        }
        return base.SaveChanges();
    }
}
