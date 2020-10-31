using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("armazenamento")]
    public class Armazenamento : Model {
        [Key]
        public int id { get; set; }
        [Required]
        public int number { get; set; }
        [Required]
        public string name { get; set; }
        public double size { get; set; }
        public string disks { get; set; }
    }
}