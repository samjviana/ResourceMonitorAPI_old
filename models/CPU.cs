using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("cpu")]
    public class CPU : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public int number { get; set; }
        [Required]
        public string name { get; set; }
        public double temperature { get; set; }
        public double clock { get; set; }
        public double power { get; set; }
        public int cores { get; set; }
    }
}