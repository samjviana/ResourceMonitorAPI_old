using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("gpu")]
    public class GPU : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public int number { get; set; }
        [Required]
        public string name { get; set; }
        public double temperature { get; set; }
        public double coreclock { get; set; }
        public double memoryclock { get; set; }
    }
}