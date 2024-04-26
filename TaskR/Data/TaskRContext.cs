using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TaskR.Data;

public partial class TaskRContext : DbContext
{
    public TaskRContext()
    {
    }

    public TaskRContext(DbContextOptions<TaskRContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<ToDoList> ToDoLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=AppDb");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppRole__3214EC07342A3313");

            entity.ToTable("AppRole");

            entity.HasIndex(e => e.RoleName, "UQ__AppRole__8A2B6160BA23A9AF").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppUser__3214EC07F124E29F");

            entity.ToTable("AppUser");

            entity.Property(e => e.AppRoleId).HasDefaultValue(1);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(32)
                .IsFixedLength();
            entity.Property(e => e.RegisteredOn).HasColumnType("datetime");
            entity.Property(e => e.Salt)
                .HasMaxLength(32)
                .IsFixedLength();
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.AppRole).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.AppRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AppUser__AppRole__4D94879B");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC07C9405DCA");

            entity.Property(e => e.HexColor)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Task__3214EC0732CB8653");

            entity.ToTable("Task");

            entity.Property(e => e.CompletedOn).HasColumnType("datetime");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Descripton)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PricePerPerson).HasColumnType("decimal(9, 2)");

            entity.HasOne(d => d.ToDoList).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ToDoListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__ToDoListId__52593CB8");

            entity.HasMany(d => d.Tags).WithMany(p => p.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaskTags__TagsId__5812160E"),
                    l => l.HasOne<Task>().WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaskTags__TaskId__571DF1D5"),
                    j =>
                    {
                        j.HasKey("TaskId", "TagsId").HasName("PK__TaskTags__A12A5F0E585689FE");
                        j.ToTable("TaskTags");
                    });
        });

        modelBuilder.Entity<ToDoList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ToDoList__3214EC07EACC0BB9");

            entity.ToTable("ToDoList");

            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
