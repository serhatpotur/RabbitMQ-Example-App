using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RabbitMQExample.FileCreateWorkerService.Models
{
    public partial class RabbitMqCreateExcelDbContext : DbContext
    {
        public RabbitMqCreateExcelDbContext()
        {
        }

        public RabbitMqCreateExcelDbContext(DbContextOptions<RabbitMqCreateExcelDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<RabbitMqexcelTestTable> RabbitMqexcelTestTables { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RabbitMqexcelTestTable>(entity =>
            {
                entity.ToTable("RabbitMQExcelTestTable");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
