namespace TestProject.Models.FirstModel
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
                .Property(e => e.id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Payment>()
                .Property(e => e.payer_id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Payment>()
                .Property(e => e.code)
                .IsUnicode(false);

            modelBuilder.Entity<Payment>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.id)
                .HasPrecision(18, 0);

            modelBuilder.Entity<User>()
                .Property(e => e.full_name)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.payer_id)
                .WillCascadeOnDelete(false);
        }
    }
}
