using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebCrawler.Models
{
    public partial class CrawlerContext : DbContext
    {
        public CrawlerContext()
        {
        }

        public CrawlerContext(DbContextOptions<CrawlerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Analysis> Analyses { get; set; } = null!;
        public virtual DbSet<Crawler> Crawlers { get; set; } = null!;
        public virtual DbSet<Type> Types { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=127.0.0.1;Database=Crawler;Trusted_Connection=True;User ID=sa;Password=1234");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.ToTable("Analysis");

                entity.Property(e => e.AnalysisId)
                    .ValueGeneratedNever()
                    .HasColumnName("AnalysisID");

                entity.Property(e => e.Comtent)
                    .HasColumnType("text")
                    .HasColumnName("comtent");

                entity.Property(e => e.CrawlerId).HasColumnName("CrawlerID");

                entity.Property(e => e.State)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");
            });

            modelBuilder.Entity<Crawler>(entity =>
            {
                entity.ToTable("Crawler");

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .HasColumnType("text")
                    .HasColumnName("URL");

                entity.Property(e => e.WebCrawler)
                    .HasColumnType("text")
                    .HasColumnName("Web_Crawler");
            });

            modelBuilder.Entity<Type>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Type");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.TypeName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Account)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
