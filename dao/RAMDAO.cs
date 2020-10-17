using ResourceMonitorAPI.models;

namespace ResourceMonitorAPI.dao {
    class RAMDAO : DAO<RAM> {
        public RAMDAO() : base(typeof(RAM), "rams") {
            
        }
    }
}