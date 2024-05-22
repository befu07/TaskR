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

    public virtual DbSet<TaskItem> TaskItems { get; set; }

    public virtual DbSet<ToDoList> ToDoLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=AppDb");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppRole__3214EC07A6AC8E55");

            entity.ToTable("AppRole");

            entity.HasIndex(e => e.RoleName, "UQ__AppRole__8A2B6160AAC85FAD").IsUnique();

            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppUser__3214EC070315A8E4");

            entity.ToTable("AppUser");

            entity.HasIndex(e => e.Email, "UQ__AppUser__A9D105340913E6D1").IsUnique();

            entity.Property(e => e.AppRoleId).HasDefaultValue(2);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
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
                .HasConstraintName("FK__AppUser__AppRole__3C69FB99");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC0721DFCC12");

            entity.Property(e => e.HexColor)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.AppUser).WithMany(p => p.Tags)
                .HasForeignKey(d => d.AppUserId)
                .HasConstraintName("Fk_Tags_AppUserId");
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskItem__3214EC0749D14605");

            entity.ToTable("TaskItem");

            entity.Property(e => e.CompletedOn).HasColumnType("datetime");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.ToDoList).WithMany(p => p.TaskItems)
                .HasForeignKey(d => d.ToDoListId)
                .HasConstraintName("FK__TaskItem__ToDoLi__4222D4EF");

            entity.HasMany(d => d.Tags).WithMany(p => p.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagsId")
                        .HasConstraintName("FK__TaskTags__TagsId__48CFD27E"),
                    l => l.HasOne<TaskItem>().WithMany()
                        .HasForeignKey("TaskId")
                        .HasConstraintName("FK__TaskTags__TaskId__47DBAE45"),
                    j =>
                    {
                        j.HasKey("TaskId", "TagsId").HasName("PK__TaskTags__A12A5F0EE75E9BA2");
                        j.ToTable("TaskTags");
                    });
        });

        modelBuilder.Entity<ToDoList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ToDoList__3214EC07507054A8");

            entity.ToTable("ToDoList");

            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.AppUser).WithMany(p => p.ToDoLists)
                .HasForeignKey(d => d.AppUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Fk_ToDoList_AppUserId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
