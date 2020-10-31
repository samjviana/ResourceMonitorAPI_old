using System.Linq;
using ResourceMonitorAPI.models;
using ResourceMonitorAPI.utils;

namespace ResourceMonitorAPI.dao {
    class ComputadorDAO : DAO<Computador> {
        public ComputadorDAO() : base(typeof(Computador), "computadores") {
            
        }
        
        public Computador getByNome(string nome) {
            using (var context = new DatabaseContext()) {
                Computador computador = context.computadores.Include(
                    "storages"
                ).Include(
                    "cpus"
                ).Include(
                    "gpus"
                ).Include(
                    "ram"
                ).Where(
                    c => c.name.ToLower() == nome.ToLower()
                ).FirstOrDefault();
                return computador;
            }
        }
    }
}