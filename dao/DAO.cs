using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ResourceMonitorAPI.utils;

namespace ResourceMonitorAPI.dao {
    abstract class DAO<T> where T : class {
        private Type type;
        private String tableName;

        public DAO(Type type, String tableName) {
            this.type = type;
            this.tableName = tableName;
        }

        public List<T> get() {
            try {
                using (var context = new DatabaseContext()) {
                    var table = (DbSet<T>)typeof(DatabaseContext).GetProperty(tableName).GetValue(context);
                    return table.ToList<T>();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}