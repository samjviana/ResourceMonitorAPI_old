using ResourceMonitorAPI.dao;
using ResourceMonitorAPI.models;

namespace ResourceMonitorApi.dao {
    class GPUDAO : DAO<GPU> {
        public GPUDAO() : base(typeof(GPU), "gpus") {

        }
    }
}