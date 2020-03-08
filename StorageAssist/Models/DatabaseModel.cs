using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StorageAssist.Models;

namespace StorageAssist.Models
{

    public class AppUserContext : IdentityDbContext<ApplicationUser>
    {
        public AppUserContext(DbContextOptions<AppUserContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<CommonResource> CommonResources { get; set; }
        public DbSet<UserCommonResource> UserCommonResources { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //many-to-many relationship between User-CommonResource
            modelBuilder.Entity<UserCommonResource>()
                .HasKey(uc => new { uc.UserId, uc.CommonResourceId });
            modelBuilder.Entity<UserCommonResource>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCommonResource)
                .HasForeignKey(uc => uc.UserId);
            modelBuilder.Entity<UserCommonResource>()
                .HasOne(uc => uc.CommonResource)
                .WithMany(c => c.UserCommonResource)
                .HasForeignKey(uc => uc.CommonResourceId);

            //UserCommonResource (join entity) unique key
            modelBuilder.Entity<UserCommonResource>()
                .HasAlternateKey(uc => uc.UserCommonResourceId);

            //cascade delete for Storage
            modelBuilder.Entity<Storage>()
                .HasMany(s => s.Products).WithOne(p => p.Storage)
                .HasForeignKey(s => s.StorageId)
                .OnDelete(DeleteBehavior.Cascade);

            //Product buyDate defaults to current day and time
            modelBuilder.Entity<Product>()
                .Property(p => p.BuyDate)
                // ReSharper disable once StringLiteralTypo
                .HasDefaultValueSql("getdate()");

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<StorageAssist.Models.ProductOpinion> ProductOpinion { get; set; }
    }

    public enum StorageType
    {
        Fridge, Freezer, Other
    }

    public enum ProductType
    {
        Meat, Vegetables, Fruits, Dairy, Candy, Baking, Other
    }

    public enum QuantityType
    {
        Count, Weight
    }

    [Table("Users")]
    public class ApplicationUser : IdentityUser
    {
        //list of join entities (many-to-many)
        public ApplicationUser()
        {
            UserCommonResource = new List<UserCommonResource>();
            ProductOpinion = new List<ProductOpinion>();
        }
        public List<UserCommonResource> UserCommonResource { get; set; }
        public List<ProductOpinion> ProductOpinion { get; set; }
    }

    [Table("ProductsOpinion")]
    public class ProductOpinion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProductOpinionId { get; set; }

        [Required(ErrorMessage = "Product name must be specified")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be 2-100 characters long")]
        [DisplayName("Product name")]
        public string ProductName { get; set; }

        [DisplayName("Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative (or enormously big)")]
        public double Price { get; set; }

        [DisplayName("Price opinion")]
        [Range(0,10, ErrorMessage = "Price opinion must be in 0 - 10 range")]
        public double PriceOpinion { get; set; }

        [DisplayName("Quality")]
        [Range(0, 10, ErrorMessage = "Quality must be in 0 - 10 range")]
        public double Quality { get; set; }

        [DisplayName("Value")]
        [Range(0, 10, ErrorMessage = "Value must be in 0 - 10 range")]
        public double Value { get; set; } 

        [DisplayName("Quality/Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative (or enormously big)")]
        public double PriceQualityRatio { get; set; } // computed by controller

        [DisplayName("Description")]
        [StringLength(5000, ErrorMessage = "Description too long")]
        public string Description { get; set; }
    }

    [Table("CommonResources")]
    public class CommonResource
    {
        public CommonResource()
        {
            UserCommonResource = new List<UserCommonResource>();
            Storages = new List<Storage>();
            Notes = new List<Note>();
        }
        //key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CommonResourceId { get; set; }
        [Required]
        public string CommonResourceName { get; set; }
        //list of join entities (many-to-many)
        public List<UserCommonResource> UserCommonResource { get; set; }
        public List<Storage> Storages { get; set; }
        public List<Note> Notes { get; set; }
        //Use to (dis)allow delete
        [Required]
        public string OwnerId { get; set; }
    }

    [Table("UserCommonResource")]
    public class UserCommonResource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserCommonResourceId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string CommonResourceId { get; set; }
        public CommonResource CommonResource { get; set; }
    }


    [Table("Notes")]
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string NoteId { get; set; }
        public string CommonResourceId { get; set; }
        public CommonResource CommonResource { get; set; }
        //use to (dis)allow delete
        [Required]
        public string OwnerId { get; set; }

        [Required(ErrorMessage = "Note name cannot be empty")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Note name must contain 2-100 characters")]
        [DisplayName("Note name")]
        public string NoteName { get; set; }

        public string NoteType { get; set; } //unused

        [DisplayName("Note text")]
        public string NoteText { get; set; }
    }

    /// <summary>
    /// Contains list of products, type of storage, commonResource to which it belongs.
    /// </summary>
    [Table("Storages")]
    public class Storage
    {
        public Storage()
        {
            Products = new List<Product>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string StorageId { get; set; }

        //Foreign key, common resource containing storage
        public string CommonResourceId { get; set; }
        public CommonResource CommonResource { get; set; }
        //use to (dis)allow delete
        [Required]
        public string OwnerId { get; set; }
        //name to present in ui
        [Required]
        public string StorageName { get; set; }
        public StorageType StorageType { get; set; }
        public List<Product> Products { get; set; }
    }

    /// <summary>
    /// Contains information about storage, quantity, type, expiration date, buy date, etc.
    /// </summary>
    [Table("Products")]
    public class Product
    {
        //Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProductId { get; set; }

        //Foreign key, storage containing product
        public string StorageId { get; set; }
        public Storage Storage { get; set; }

        //what is inside, to determinate default expiration date
        [DisplayName("Product type")]
        [DefaultValue(ProductType.Other)]
        public ProductType Type { get; set; }

        [Required(ErrorMessage = "Product name must be provided")]
        [StringLength(20, ErrorMessage = "Product name must contain 2-20 characters", MinimumLength = 2)]
        [DisplayName("Product name")]
        public string ProductName { get; set; }

        //determinate which Quantity metric should be applied
        [Required]
        [DisplayName("Quantity type")]
        public QuantityType QuantityType { get; set; }

        [Required(ErrorMessage = "Quantity cannot be empty")]
        [Range(0, double.MaxValue, ErrorMessage = "Invalid number. Number must be positive")]
        [DisplayName("Quantity")]
        public double Quantity { get; set; }

        public DateTime BuyDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        public string Comment { get; set; }

    }
}