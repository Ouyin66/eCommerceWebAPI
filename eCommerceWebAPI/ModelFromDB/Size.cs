using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Size")]
    public partial class Size
    {
        public Size()
        {
            Variants = new HashSet<Variant>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("size")]
        [StringLength(50)]
        public string? Name { get; set; }

        [JsonIgnore]
        [InverseProperty(nameof(Variant.Size))]
        public virtual ICollection<Variant> Variants { get; set; }
    }
}
