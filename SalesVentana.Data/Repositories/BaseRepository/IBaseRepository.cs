using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesVentana.BO;
using System.Linq.Expressions;

namespace SalesVentana.Data
{
    public interface IBaseRepository<T> where T : class, IBasicObjectBase, new()
    {
        //IQueryable<T> AllIncluding(params Expression<Func<T, object>>[]includeProperties);
        IEnumerable<T> GetAll<M>(string table) where M : class, new();
        T GetSingle<M>(string table, string column, string value) where M : class, new();
        IEnumerable<T> FindBy<M>(string table, string column, string value) where M : class, new();
        void Add(string table, IDictionary<string, object> columnValue);
        void Delete(string table);
        void Edit(T entity);

    }
}
