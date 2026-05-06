using Microsoft.EntityFrameworkCore;
using UserMicroservice.Models;

namespace UserMicroservice.Data;

public class UserdbContext : DbContext
{
    public UserdbContext(DbContextOptions<UserdbContext> options) : base(options)
    {
    }

    public DbSet<Models.UserModel> Users { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseModel>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.BaseType == typeof(BaseModel) ||
                (entityType.ClrType.BaseType != null && entityType.ClrType.BaseType.IsGenericType && entityType.ClrType.BaseType.GetGenericTypeDefinition() == typeof(BaseModel)))
            {

                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .HasColumnType(typeName: "DATETIME2")
                    .HasColumnName(name: "CreatedAt")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("UpdatedAt")
                    .HasColumnType("DATETIME2")
                    .HasColumnName("UpdatedAt")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");


                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("CreatedBy")
                    .HasColumnType("NVARCHAR(100)")
                    .HasDefaultValue("SYSTEM")
                    .HasColumnName("CreatedBy");

                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("UpdatedBy")
                    .HasColumnType(typeName: "NVARCHAR(100)")
                    .HasColumnName(name: "UpdatedBy");

                modelBuilder.Entity(entityType.ClrType)
                    .Property<bool>("Status")
                    .HasColumnType("BIT")
                    .HasDefaultValue(true);
            }

        }


        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.ToTable(name: "Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType(typeName: "UNIQUEIDENTIFIER").HasColumnName(name: "Id").ValueGeneratedOnAdd();
            entity.Property(e => e.Fname).HasColumnType(typeName: "NVARCHAR(100)").HasColumnName(name: "Fname");
            entity.Property(e => e.Lname).HasColumnType(typeName: "NVARCHAR(100)").HasColumnName(name: "Lname");
            entity.Property(e => e.Email).HasColumnType(typeName: "NVARCHAR(255)").HasColumnName(name: "Email");
            entity.Property(e => e.PasswordHash).HasColumnType(typeName: "NVARCHAR(255)").HasColumnName(name: "PasswordHash");
            entity.Property(e => e.Gender).HasColumnType(typeName: "BIT").HasColumnName(name: "Gender");
            entity.Property(e => e.BirthDate).HasColumnType(typeName: "DATE").HasColumnName(name: "BirthDate").HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v));
            entity.Property(e => e.Phone).HasColumnType(typeName: "NVARCHAR(20)").HasColumnName(name: "Phone");
            entity.Property(e => e.Role).HasColumnType(typeName: "INT").HasColumnName(name: "Role");
        });

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserModel>().HasQueryFilter(x => x.Status == true);

        modelBuilder.Entity<UserModel>().HasIndex(u => u.Fname).IsUnique();

    }
}