using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace eCommerceWebAPI.ModelFromDB
{
    public partial class ECOMMERCE : DbContext
    {
        public ECOMMERCE() { }

        public ECOMMERCE(DbContextOptions<ECOMMERCE> options)
            : base(options) { }

        public virtual DbSet<Cart> Carts { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<Color> Colors { get; set; } = null!;
        public virtual DbSet<Feedback> Feedbacks { get; set; } = null!;
        public virtual DbSet<Gender> Genders { get; set; } = null!;
        public virtual DbSet<Messenger> Messengers { get; set; } = null!;
        public virtual DbSet<Picture> Pictures { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<Promotion> Promotions { get; set; } = null!;
        public virtual DbSet<Receipt> Receipts { get; set; } = null!;
        public virtual DbSet<ReceiptVariant> ReceiptVariants { get; set; } = null!;
        public virtual DbSet<Size> Sizes { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Variant> Variants { get; set; } = null!;
        public virtual DbSet<Location> Locations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=MSI;Initial Catalog=ECOMMERCE;Persist Security Info=True;User ID=sa;Password=123456;Encrypt=false");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(d => d.DefaultLocation)
                    .WithMany()
                    .HasForeignKey(d => d.DefaultLocationId)
                    .HasConstraintName("FK_User_Location");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Location_User");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Carts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Cart_User");

                entity.HasOne(d => d.Variant)
                    .WithMany(p => p.Carts)
                    .HasForeignKey(d => d.VariantId)
                    .HasConstraintName("FK_Cart_Variant");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Chat_User");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasOne(d => d.Receipt)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.ReceiptId)
                    .HasConstraintName("FK_Feedback_Receipt");

                entity.HasOne(d => d.Variant)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.VariantId)
                    .HasConstraintName("FK_Feedback_Variant");
            });

            modelBuilder.Entity<Messenger>(entity =>
            {
                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Messengers)
                    .HasForeignKey(d => d.ChatId)
                    .HasConstraintName("FK_Messenger_Chat");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Messengers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Messenger_User");
            });

            modelBuilder.Entity<Picture>(entity =>
            {
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Pictures)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Picture_Product");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Product_Category");

                entity.HasOne(d => d.Gender)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.GenderId)
                    .HasConstraintName("FK_Product_Gender");

                entity.HasMany(d => d.Promotions)
                    .WithMany(p => p.Products)
                    .UsingEntity<Dictionary<string, object>>(
                        "ProductPromotion",
                        l => l.HasOne<Promotion>().WithMany().HasForeignKey("PromotionId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_ProductPromotion_Promotion"),
                        r => r.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_ProductPromotion_Product"),
                        j =>
                        {
                            j.HasKey("ProductId", "PromotionId").HasName("PK_ProductPromotion");
                            j.ToTable("Product_Promotion");
                        });

                entity.HasMany(d => d.Users)
                    .WithMany(p => p.Products)
                    .UsingEntity<Dictionary<string, object>>(
                        "Favorite",
                        l => l.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Favorite_User"),
                        r => r.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Favorite_Product"),
                        j =>
                        {
                            j.HasKey("ProductId", "UserId").HasName("PK_Favorite");
                            j.ToTable("Favorite");
                        });
            });

            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Receipts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Receipt_User");
            });

            modelBuilder.Entity<ReceiptVariant>(entity =>
            {
                entity.HasKey(e => new { e.VariantId, e.ReceiptId }).HasName("PK_ReceiptVariant");

                entity.HasOne(d => d.Receipt)
                    .WithMany(p => p.ReceiptVariants)
                    .HasForeignKey(d => d.ReceiptId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceiptVariant_Receipt");

                entity.HasOne(d => d.Variant)
                    .WithMany(p => p.ReceiptVariants)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceiptVariant_Variant");
            });

            modelBuilder.Entity<Variant>(entity =>
            {
                entity.HasOne(d => d.Color)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(d => d.ColorId)
                    .HasConstraintName("FK_Variant_Color");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Variant_Product");

                entity.HasOne(d => d.Size)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(d => d.SizeId)
                    .HasConstraintName("FK_Variant_Size");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
