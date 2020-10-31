using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("ram")]
    public class RAM : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public double total { get; set; }
        public int modules { get; set; }
    }
}