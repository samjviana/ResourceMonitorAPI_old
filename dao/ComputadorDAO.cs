using ResourceMonitorAPI.models;

namespace ResourceMonitorAPI.dao {
    class ComputadorDAO : DAO<Computador> {
        public ComputadorDAO() : base(typeof(Computador), "computadores") {
            
        }
    }
}