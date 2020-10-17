using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("gpu")]
    public class GPU : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public int numero { get; set; }
        [Required]
        public string nome { get; set; }
        public double temperatura { get; set; }
        public double clockNucleo { get; set; }
        public double clockMemoria { get; set; }
    }
}