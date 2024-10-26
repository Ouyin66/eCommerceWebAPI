using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Product")]
    [Index(nameof(NamePro), Name = "UQ__Product__3AE73A3F6169C5E9", IsUnique = true)]
    public partial class Product
    {
        public Product()
        {
            Pictures = new HashSet<Picture>();
            Variants = new HashSet<Variant>();
            Promotions = new HashSet<Promotion>();
            Users = new HashSet<User>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("genderID")]
        public int? GenderId { get; set; }
        [Column("categoryID")]
        public int? CategoryId { get; set; }
        [Column("namePro")]
        [StringLength(100)]
        public string? NamePro { get; set; }
        [Column("describe")]
        public string? Describe { get; set; }
        [Column("price", TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }
        [Column("discount", TypeName = "decimal(18, 2)")]
        public decimal? Discount { get; set; }
        [Column("amount")]
        public int? Amount { get; set; }
        [Column("state")]
        public byte? State { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey(nameof(CategoryId))]
        [JsonIgnore]
        [InverseProperty("Products")]
        public virtual Category? Category { get; set; }
        [ForeignKey(nameof(GenderId))]

        [InverseProperty("Products")]
        [JsonIgnore]
        public virtual Gender? Gender { get; set; }
        [InverseProperty(nameof(Picture.Product))]
        public virtual ICollection<Picture> Pictures { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Variant.Product))]
        public virtual ICollection<Variant> Variants { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        [InverseProperty(nameof(Promotion.Products))]
        public virtual ICollection<Promotion> Promotions { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        [InverseProperty(nameof(User.Products))]
        public virtual ICollection<User> Users { get; set; }
    }
}
