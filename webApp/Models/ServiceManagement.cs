namespace webApp.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ServiceManagement : DbContext
    {
        public ServiceManagement()
            : base("name=ServiceManagement")
        {
        }

        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServiceColumnDbMapping> ServiceColumnDbMappings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>()
                .Property(e => e.serviceName)
                .IsUnicode(false);

            modelBuilder.Entity<Service>()
                .Property(e => e.wordTemplate)
                .IsUnicode(false);

            modelBuilder.Entity<Service>()
                .HasMany(e => e.ServiceColumnDbMappings)
                .WithRequired(e => e.Service)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServiceColumnDbMapping>()
                .Property(e => e.csvColumnName)
                .IsUnicode(false);

            modelBuilder.Entity<ServiceColumnDbMapping>()
                .Property(e => e.dbColumnName)
                .IsUnicode(false);
        }
    }
}
