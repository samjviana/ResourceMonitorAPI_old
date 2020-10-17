using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("armazenamento")]
    public class Armazenamento : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public int numero { get; set; }
        [Required]
        public string nome { get; set; }
        public int capacidade { get; set; }
        public string discos { get; set; }
    }
}