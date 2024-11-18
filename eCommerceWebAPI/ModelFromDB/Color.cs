using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Color")]
    public partial class Color
    {
        public Color()
        {
            Variants = new HashSet<Variant>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        [StringLength(50)]
        public string? Name { get; set; }
        [Column("image")]
        public string? Image { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Variant.Color))]
        public virtual ICollection<Variant> Variants { get; set; }
    }
}
