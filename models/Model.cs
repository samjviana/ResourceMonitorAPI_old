using System;
using System.ComponentModel.DataAnnotations;

namespace ResourceMonitorAPI.models {
    public abstract class Model {
        [Required]
        public DateTime dataCriacao { get; set; }
        [Required]
        public DateTime dataUpdate { get; set; }
    }
}