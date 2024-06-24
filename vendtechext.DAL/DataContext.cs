using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using vendtechext.DAL.Models;

namespace vendtechext.DAL;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<StagingDevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountVerificationOtp> AccountVerificationOtps { get; set; }

    public virtual DbSet<Agency> Agencies { get; set; }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<B2bUserAccess> B2bUserAccesses { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<ChequeBank> ChequeBanks { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Cmspage> Cmspages { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<ContactU> ContactUs { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Deposit> Deposits { get; set; }

    public virtual DbSet<DepositLog> DepositLogs { get; set; }

    public virtual DbSet<DepositOtp> DepositOtps { get; set; }

    public virtual DbSet<EmailConfirmationRequest> EmailConfirmationRequests { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<ForgotPasswordRequest> ForgotPasswordRequests { get; set; }

    public virtual DbSet<Meter> Meters { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Nation> Nations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<PendingDeposit> PendingDeposits { get; set; }

    public virtual DbSet<Platform> Platforms { get; set; }

    public virtual DbSet<PlatformApi> PlatformApis { get; set; }

    public virtual DbSet<PlatformApiConnection> PlatformApiConnections { get; set; }

    public virtual DbSet<PlatformApiLog> PlatformApiLogs { get; set; }

    public virtual DbSet<PlatformPacParam> PlatformPacParams { get; set; }

    public virtual DbSet<PlatformTransaction> PlatformTransactions { get; set; }

    public virtual DbSet<Po> Pos { get; set; }

    public virtual DbSet<PosassignedPlatform> PosassignedPlatforms { get; set; }

    public virtual DbSet<ReferralCode> ReferralCodes { get; set; }

    public virtual DbSet<SmsLog> SmsLogs { get; set; }

    public virtual DbSet<StanTable> StanTables { get; set; }

    public virtual DbSet<TempCountry> TempCountries { get; set; }

    public virtual DbSet<TokensManager> TokensManagers { get; set; }

    public virtual DbSet<TransactionDetail> TransactionDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAssignedModule> UserAssignedModules { get; set; }

    public virtual DbSet<UserAssignedPlatform> UserAssignedPlatforms { get; set; }

    public virtual DbSet<UserAssignedWidget> UserAssignedWidgets { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSchedule> UserSchedules { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<Widget> Widgets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=92.205.181.48;Database=STAGING_DEV;User Id=vendtech_main;Password=85236580@Ve;MultipleActiveResultSets=True;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountVerificationOtp>(entity =>
        {
            entity.ToTable("AccountVerificationOTP");

            entity.Property(e => e.AccountVerificationOtpid).HasColumnName("AccountVerificationOTPId");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Otp)
                .HasMaxLength(50)
                .HasColumnName("OTP");

            entity.HasOne(d => d.User).WithMany(p => p.AccountVerificationOtps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountVerificationOTP_Users");
        });

        modelBuilder.Entity<Agency>(entity =>
        {
            entity.ToTable("Agency");

            entity.Property(e => e.AgencyName).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Commission).WithMany(p => p.Agencies)
                .HasForeignKey(d => d.CommissionId)
                .HasConstraintName("FK_Agency_Commissions");

            entity.HasOne(d => d.RepresentativeNavigation).WithMany(p => p.Agencies)
                .HasForeignKey(d => d.Representative)
                .HasConstraintName("FK_Agency_Users");
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<B2bUserAccess>(entity =>
        {
            entity.ToTable("B2bUserAccess");

            entity.Property(e => e.Apikey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("APIKey");
            entity.Property(e => e.Apitoken)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("APIToken");
            entity.Property(e => e.ClientToken)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Clientkey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.B2bUserAccesses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_B2bUserAccess_Users");
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.Property(e => e.BankAccountId).ValueGeneratedNever();
            entity.Property(e => e.AccountName).HasMaxLength(500);
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(500);
            entity.Property(e => e.Bban).HasColumnName("BBAN");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ChequeBank>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BankCode).HasMaxLength(200);
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.Createdon).HasColumnType("datetime");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("City");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);

            entity.HasOne(d => d.Country).WithMany(p => p.Cities)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_City_Country");
        });

        modelBuilder.Entity<Cmspage>(entity =>
        {
            entity.HasKey(e => e.PageId);

            entity.ToTable("CMSPages");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.MetaKeywords).IsUnicode(false);
            entity.Property(e => e.MetaTitle)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PageName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.PageTitle)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Percentage).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ContactU>(entity =>
        {
            entity.HasKey(e => e.ContactUsId);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.ContactUs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactUs_Users");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");

            entity.Property(e => e.CountryId).ValueGeneratedNever();
            entity.Property(e => e.CountryCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CountryName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CurrencyName).HasMaxLength(75);
            entity.Property(e => e.CurrencySymbol).HasMaxLength(75);
            entity.Property(e => e.DomainUrl)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(3);
            entity.Property(e => e.CountryCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CountryName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(75);
        });

        modelBuilder.Entity<Deposit>(entity =>
        {
            entity.Property(e => e.AgencyCommission).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BalanceBefore)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CheckNumberOrSlipId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsAudit).HasColumnName("isAudit");
            entity.Property(e => e.NewBalance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NextReminderDate).HasColumnType("datetime");
            entity.Property(e => e.PercentageAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Posid).HasColumnName("POSId");
            entity.Property(e => e.TransactionId).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ValueDate)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ValueDateStamp).HasColumnType("datetime");

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.BankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_BankAccounts");

            entity.HasOne(d => d.PaymentTypeNavigation).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.PaymentType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_PaymentType");

            entity.HasOne(d => d.Pos).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.Posid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_POS");

            entity.HasOne(d => d.User).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_Users");
        });

        modelBuilder.Entity<DepositLog>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Deposit).WithMany(p => p.DepositLogs)
                .HasForeignKey(d => d.DepositId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DepositLogs_Deposits");

            entity.HasOne(d => d.User).WithMany(p => p.DepositLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DepositLogs_Users");
        });

        modelBuilder.Entity<DepositOtp>(entity =>
        {
            entity.ToTable("DepositOTP");

            entity.Property(e => e.DepositOtpid).HasColumnName("DepositOTPId");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Otp)
                .HasMaxLength(50)
                .HasColumnName("OTP");
        });

        modelBuilder.Entity<EmailConfirmationRequest>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.EmailConfirmationRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmailConfirmationRequests_Users");
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId);

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Desription)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.EmailSubject).HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasColumnName("sortOrder");
            entity.Property(e => e.TargetUser)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TemplateName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TemplateStatus).HasDefaultValue(true);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("ErrorLog");

            entity.Property(e => e.ErrorLogId).HasColumnName("ErrorLogID");
            entity.Property(e => e.FormData).IsUnicode(false);
            entity.Property(e => e.InnerException).IsUnicode(false);
            entity.Property(e => e.LoggedAt).HasColumnType("datetime");
            entity.Property(e => e.LoggedInDetails).IsUnicode(false);
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.QueryData).IsUnicode(false);
            entity.Property(e => e.RouteData).IsUnicode(false);
            entity.Property(e => e.StackTrace).IsUnicode(false);
        });

        modelBuilder.Entity<ForgotPasswordRequest>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .HasColumnName("OTP");

            entity.HasOne(d => d.User).WithMany(p => p.ForgotPasswordRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ForgotPasswordRequests_Users");
        });

        modelBuilder.Entity<Meter>(entity =>
        {
            entity.Property(e => e.Allias).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsSaved).HasDefaultValue(true);
            entity.Property(e => e.MeterMake).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Number).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Meters)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Meters_Users");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.Property(e => e.ControllerName).HasMaxLength(500);
            entity.Property(e => e.ModuleName).HasMaxLength(500);

            entity.HasOne(d => d.SubMenuOfNavigation).WithMany(p => p.InverseSubMenuOfNavigation)
                .HasForeignKey(d => d.SubMenuOf)
                .HasConstraintName("FK_Modules_Modules");
        });

        modelBuilder.Entity<Nation>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsdCode).HasColumnName("isd_code");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Sortname)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("sortname");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.SentOn).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.ToTable("PaymentType");

            entity.Property(e => e.Name).IsUnicode(false);
        });

        modelBuilder.Entity<PendingDeposit>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CheckNumberOrSlipId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsAudit).HasColumnName("isAudit");
            entity.Property(e => e.NewBalance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NextReminderDate).HasColumnType("datetime");
            entity.Property(e => e.PercentageAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Posid).HasColumnName("POSId");
            entity.Property(e => e.TransactionId).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ValueDate)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ValueDateStamp).HasColumnType("datetime");

            entity.HasOne(d => d.PaymentTypeNavigation).WithMany(p => p.PendingDeposits)
                .HasForeignKey(d => d.PaymentType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingDeposits_PaymentType");

            entity.HasOne(d => d.Pos).WithMany(p => p.PendingDeposits)
                .HasForeignKey(d => d.Posid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingDeposits_POS");

            entity.HasOne(d => d.User).WithMany(p => p.PendingDeposits)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingDeposits_Users");
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.ToTable("Platform");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DisabledPlatformMessage).IsUnicode(false);
            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.Logo)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.MinimumAmount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PlatformType).HasDefaultValue(-1);
            entity.Property(e => e.ShortName)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(1000);

            entity.HasOne(d => d.PlatformApiConnBackup).WithMany(p => p.PlatformPlatformApiConnBackups)
                .HasForeignKey(d => d.PlatformApiConnBackupId)
                .HasConstraintName("FK_Platform_PlatformApiConnections_Backup");

            entity.HasOne(d => d.PlatformApiConn).WithMany(p => p.PlatformPlatformApiConns)
                .HasForeignKey(d => d.PlatformApiConnId)
                .HasConstraintName("FK_Platform_PlatformApiConnections");
        });

        modelBuilder.Entity<PlatformApi>(entity =>
        {
            entity.HasIndex(e => e.ApiType, "Index_PlatformApis_1");

            entity.Property(e => e.Balance).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Name).HasMaxLength(75);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CurrencyNavigation).WithMany(p => p.PlatformApis)
                .HasForeignKey(d => d.Currency)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformApis_Currencies");
        });

        modelBuilder.Entity<PlatformApiConnection>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.PlatformApi).WithMany(p => p.PlatformApiConnections)
                .HasForeignKey(d => d.PlatformApiId)
                .HasConstraintName("FK_PlatformApiConnections_PlatformApis");

            entity.HasOne(d => d.Platform).WithMany(p => p.PlatformApiConnections)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformApiConnections_Platform");
        });

        modelBuilder.Entity<PlatformApiLog>(entity =>
        {
            entity.HasIndex(e => e.TransactionId, "Index_PlatformApiLogs_1");

            entity.Property(e => e.LogDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PlatformPacParam>(entity =>
        {
            entity.Property(e => e.Config).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.PlatformApiConnection).WithMany(p => p.PlatformPacParams)
                .HasForeignKey(d => d.PlatformApiConnectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformPacParams_PlatformApiConnections");

            entity.HasOne(d => d.Platform).WithMany(p => p.PlatformPacParams)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformPacParams_Platform");
        });

        modelBuilder.Entity<PlatformTransaction>(entity =>
        {
            entity.HasIndex(e => e.Status, "Index_PlatformTransactions_1").IsDescending();

            entity.HasIndex(e => e.Beneficiary, "Index_PlatformTransactions_2");

            entity.HasIndex(e => e.UserReference, "Index_PlatformTransactions_3");

            entity.HasIndex(e => e.ApiTransactionId, "Index_PlatformTransactions_4");

            entity.HasIndex(e => e.OperatorReference, "Index_PlatformTransactions_5");

            entity.HasIndex(e => e.UserId, "Index_PlatformTransactions_6");

            entity.HasIndex(e => e.PlatformId, "Index_PlatformTransactions_7");

            entity.HasIndex(e => e.ApiConnectionId, "Index_PlatformTransactions_8");

            entity.Property(e => e.Amount).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.AmountPlatform).HasColumnType("decimal(16, 2)");
            entity.Property(e => e.ApiTransactionId).HasMaxLength(50);
            entity.Property(e => e.Beneficiary).HasMaxLength(150);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.OperatorReference).HasMaxLength(150);
            entity.Property(e => e.PinNumber).HasMaxLength(1000);
            entity.Property(e => e.PinSerial).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserReference).HasMaxLength(32);

            entity.HasOne(d => d.ApiConnection).WithMany(p => p.PlatformTransactions)
                .HasForeignKey(d => d.ApiConnectionId)
                .HasConstraintName("FK_PlatformTransactions_PlatformApiConnections");

            entity.HasOne(d => d.CurrencyNavigation).WithMany(p => p.PlatformTransactions)
                .HasForeignKey(d => d.Currency)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformTransactions_Currencies");

            entity.HasOne(d => d.Platform).WithMany(p => p.PlatformTransactions)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformTransactions_Platform");

            entity.HasOne(d => d.Pos).WithMany(p => p.PlatformTransactions)
                .HasForeignKey(d => d.PosId)
                .HasConstraintName("FK_PlatformTransactions_POS");

            entity.HasOne(d => d.User).WithMany(p => p.PlatformTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatformTransactions_Users");
        });

        modelBuilder.Entity<Po>(entity =>
        {
            entity.HasKey(e => e.Posid);

            entity.ToTable("POS");

            entity.HasIndex(e => e.PassCode, "DF_PassCode").IsUnique();

            entity.Property(e => e.Posid).HasColumnName("POSId");
            entity.Property(e => e.Balance)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.Enabled).HasDefaultValue(false);
            entity.Property(e => e.PassCode).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PosBarcode).HasDefaultValue(false);
            entity.Property(e => e.PosPrint).HasDefaultValue(false);
            entity.Property(e => e.PosSms).HasDefaultValue(false);
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.SmsnotificationDeposit).HasColumnName("SMSNotificationDeposit");
            entity.Property(e => e.SmsnotificationSales).HasColumnName("SMSNotificationSales");
            entity.Property(e => e.WebBarcode).HasDefaultValue(false);
            entity.Property(e => e.WebPrint).HasDefaultValue(false);
            entity.Property(e => e.WebSms).HasDefaultValue(false);

            entity.HasOne(d => d.CommissionPercentageNavigation).WithMany(p => p.Pos)
                .HasForeignKey(d => d.CommissionPercentage)
                .HasConstraintName("FK_POS_Commissions");

            entity.HasOne(d => d.Vendor).WithMany(p => p.Pos)
                .HasForeignKey(d => d.VendorId)
                .HasConstraintName("FK_POS_Vendors");
        });

        modelBuilder.Entity<PosassignedPlatform>(entity =>
        {
            entity.HasKey(e => e.AssignPosplatformId);

            entity.ToTable("POSAssignedPlatforms");

            entity.Property(e => e.AssignPosplatformId).HasColumnName("AssignPOSPlatformId");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Posid).HasColumnName("POSId");

            entity.HasOne(d => d.Platform).WithMany(p => p.PosassignedPlatforms)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSAssignedPlatforms_Platform");

            entity.HasOne(d => d.Pos).WithMany(p => p.PosassignedPlatforms)
                .HasForeignKey(d => d.Posid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSAssignedPlatforms_POS");
        });

        modelBuilder.Entity<ReferralCode>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FkUserId).HasColumnName("FK_UserId");

            entity.HasOne(d => d.FkUser).WithMany(p => p.ReferralCodes)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReferralCodes_Users");
        });

        modelBuilder.Entity<SmsLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SMS_LOG__3214EC27A3815927");

            entity.ToTable("SMS_LOG");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AgentId).HasColumnName("AgentID");
            entity.Property(e => e.DateTime)
                .HasColumnType("datetime")
                .HasColumnName("Date_Time");
            entity.Property(e => e.MeterNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Meter_Number");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Phone_number");
            entity.Property(e => e.Posid).HasColumnName("POSID");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TransactionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<StanTable>(entity =>
        {
            entity.ToTable("StanTable");
        });

        modelBuilder.Entity<TempCountry>(entity =>
        {
            entity.HasKey(e => e.CountryId);

            entity.ToTable("TempCountry");

            entity.Property(e => e.CountryId).ValueGeneratedNever();
            entity.Property(e => e.CountryCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CountryName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CurrencyName).HasMaxLength(75);
            entity.Property(e => e.CurrencySymbol).HasMaxLength(75);
            entity.Property(e => e.DomainUrl)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TokensManager>(entity =>
        {
            entity.ToTable("TokensManager");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpiresOn).HasColumnType("datetime");
            entity.Property(e => e.PosNumber).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.TokensManagers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TokensManager_Users");
        });

        modelBuilder.Entity<TransactionDetail>(entity =>
        {
            entity.HasKey(e => e.TransactionDetailsId);

            entity.HasIndex(e => e.CreatedAt, "IX_CreatedAt");

            entity.HasIndex(e => e.MeterToken1, "IX_MeterToken1");

            entity.HasIndex(e => e.TransactionDetailsId, "IX_TransactionDetailsId");

            entity.HasIndex(e => e.TransactionId, "IX_TransactionId");

            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CostOfUnits)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CurrentDealerBalance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CurrentVendorBalance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Customer)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.DateAndTimeFinalised).IsUnicode(false);
            entity.Property(e => e.DateAndTimeLinked).IsUnicode(false);
            entity.Property(e => e.DateAndTimeSold).IsUnicode(false);
            entity.Property(e => e.DebitRecovery)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MeterNumber1)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MeterToken1)
                .HasMaxLength(22)
                .IsUnicode(false);
            entity.Property(e => e.MeterToken2)
                .HasMaxLength(22)
                .IsUnicode(false);
            entity.Property(e => e.MeterToken3)
                .HasMaxLength(22)
                .IsUnicode(false);
            entity.Property(e => e.Posid).HasColumnName("POSId");
            entity.Property(e => e.QueryStatusCount).HasDefaultValue(0);
            entity.Property(e => e.ReceiptNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Request).IsUnicode(false);
            entity.Property(e => e.RequestDate).HasColumnType("datetime");
            entity.Property(e => e.Response).IsUnicode(false);
            entity.Property(e => e.RtsuniqueId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RTSUniqueID");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ServiceCharge)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StatusResponse).IsUnicode(false);
            entity.Property(e => e.Tariff)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TaxCharge)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenderedAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionId).HasMaxLength(50);
            entity.Property(e => e.Units)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VendStatus)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.VendStatusDescription)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.VoucherSerialNumber)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Vprovider)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("VProvider");

            entity.HasOne(d => d.PlatForm).WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.PlatFormId)
                .HasConstraintName("FK_TransactionDetails_Platform");

            entity.HasOne(d => d.Pos).WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.Posid)
                .HasConstraintName("FK_TransactionDetails_POS");

            entity.HasOne(d => d.User).WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionDetails_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.AppLastUsed).HasColumnType("datetime");
            entity.Property(e => e.AutoApprove).HasDefaultValue(false);
            entity.Property(e => e.CountryCode).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(300);
            entity.Property(e => e.FkvendorId).HasColumnName("FKVendorId");
            entity.Property(e => e.MobileAppVersion)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(300);
            entity.Property(e => e.PassCode).HasMaxLength(5);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.ProfilePic).HasMaxLength(500);
            entity.Property(e => e.SurName).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(500);

            entity.HasOne(d => d.Agent).WithMany(p => p.Users)
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("FK_Users_Agency");

            entity.HasOne(d => d.City).WithMany(p => p.Users)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Users_City");

            entity.HasOne(d => d.Country).WithMany(p => p.Users)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("FK_Users_Country");

            entity.HasOne(d => d.Fkvendor).WithMany(p => p.InverseFkvendor)
                .HasForeignKey(d => d.FkvendorId)
                .HasConstraintName("FK_Users_Users1");

            entity.HasOne(d => d.UserTypeNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_UserRoles");

            entity.HasOne(d => d.VendorCommissionPercentageNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.VendorCommissionPercentage)
                .HasConstraintName("FK_Users_Commissions");
        });

        modelBuilder.Entity<UserAssignedModule>(entity =>
        {
            entity.HasKey(e => e.AssignUserModuleId);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsAddedFromAgency).HasDefaultValue(false);

            entity.HasOne(d => d.Module).WithMany(p => p.UserAssignedModules)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAssignedModules_Modules");

            entity.HasOne(d => d.User).WithMany(p => p.UserAssignedModules)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAssignedModules_Users");
        });

        modelBuilder.Entity<UserAssignedPlatform>(entity =>
        {
            entity.HasKey(e => e.AssignUserPlatformId);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Platform).WithMany(p => p.UserAssignedPlatforms)
                .HasForeignKey(d => d.PlatformId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAssignedPlatforms_Platform");

            entity.HasOne(d => d.User).WithMany(p => p.UserAssignedPlatforms)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAssignedPlatforms_Users");
        });

        modelBuilder.Entity<UserAssignedWidget>(entity =>
        {
            entity.HasKey(e => e.AssignWidgetId);

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsAddedFromAgency).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserAssignedWidgets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAssignedWidgets_User");

            entity.HasOne(d => d.Widget).WithMany(p => p.UserAssignedWidgets)
                .HasForeignKey(d => d.WidgetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAssignedWidgets_Widget");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);
        });

        modelBuilder.Entity<UserSchedule>(entity =>
        {
            entity.ToTable("UserSchedule");

            entity.Property(e => e.Balance)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.Property(e => e.CommissionPercentage).HasDefaultValue(2);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Vendor1).HasColumnName("Vendor");

            entity.HasOne(d => d.Agency).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vendors_Agency");
        });

        modelBuilder.Entity<Widget>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(1000);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
