using MediGuru.DataExtractionTool.DatabaseModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool;

public sealed class MediGuruDbContext : IdentityDbContext<User, UserRole, string>
{
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<Discipline> Disciplines { get; set; }
    
    public DbSet<MedicalAidScheme> MedicalAidSchemes { get; set; }
    
    public DbSet<MedicalAidSchemeProcedure> MedicalAidSchemeProcedures { get; set; }
    
    public DbSet<Procedure> Procedures { get; set; }
    
    public DbSet<Provider> Providers { get; set; }
    
    public DbSet<ProviderProcedure> ProviderProcedures { get; set; }
    
    public DbSet<ProviderProcedureDataSourceType> ProviderProcedureDataSourceTypes { get; set; }
    
    public DbSet<ProviderProcedureType> ProviderProcedureTypes { get; set; }
    
    public DbSet<SearchData> SearchDatas { get; set; }
    
    public DbSet<SearchDataPoint> SearchDataPoints { get; set; }
    
    public DbSet<TaskExecutionHistory> TaskExecutionHistories { get; set; }

    public MediGuruDbContext(DbContextOptions<MediGuruDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>(table =>
        {
            table.ToTable(nameof(Category));
            table.HasIndex(x => x.Description).IsUnique();
        });
        modelBuilder.Entity<Discipline>(table =>
        {
            table.ToTable(nameof(Discipline));
            table.HasIndex(x => new { x.Code, x.Description, x.SubCode }).IsUnique();
        });
        modelBuilder.Entity<MedicalAidScheme>().ToTable(nameof(MedicalAidScheme));
        
        modelBuilder.Entity<User>(table =>
        {
            table.ToTable(nameof(User));
            table.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<UserRole>(table =>
        {
            table.ToTable(nameof(UserRole));
            table.Property(x => x.Id).ValueGeneratedOnAdd();
        });
    }
}