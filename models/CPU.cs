using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("cpu")]
    public class CPU : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public int numero { get; set; }
        [Required]
        public string nome { get; set; }
        public double temperatura { get; set; }
        public double clock { get; set; }
        public double potencia { get; set; }
        public int nucleos { get; set; }
    }
}