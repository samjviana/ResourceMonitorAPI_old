using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceMonitorAPI.models {
    [Table("computador")]
    public class Computador : Model {
        [Key]
        [Index(IsUnique = true, Order = 0)]
        public int id { get; set; }
        [Required]
        [Index(IsUnique = true, Order = 1)]
        [StringLength(16)]
        public string nome { get; set; }
        [Required]
        public ICollection<Armazenamento> armazenamentos { get; set; }
        [Required]
        public RAM ram { get; set; }
        [Required]
        public ICollection<CPU> cpus { get; set; }
        [Required]
        public ICollection<GPU> gpus { get; set; }
        public bool estado { get; set; }
    }
}