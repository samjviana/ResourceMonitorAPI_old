using ResourceMonitorAPI.models;

namespace ResourceMonitorAPI.dao {
    class CPUDAO : DAO<CPU> {
        public CPUDAO() : base(typeof(CPU), "cpus") {
            
        }
    }
}