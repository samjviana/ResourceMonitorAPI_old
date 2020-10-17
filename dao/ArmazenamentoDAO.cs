using ResourceMonitorAPI.dao;
using ResourceMonitorAPI.models;

namespace ResourceMonitorApi.dao {
    class ArmazenamentoDAO : DAO<Armazenamento> {
        public ArmazenamentoDAO() : base(typeof(Armazenamento), "armazenamentos") {
            
        }
    }
}