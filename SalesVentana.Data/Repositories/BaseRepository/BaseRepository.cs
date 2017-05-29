using Autofac;
using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, IBasicObjectBase, new()
    {
        private SalesVentanaConnection _dbConnection;
        private string _sqlQuery;
        private SqlCommand _command;
        private IMapperBase<T> _mapper = null;

        #region Properties
        protected IDbFactory DbFactory
        {
            get;
            private set;
        }
        protected SalesVentanaConnection DbConnection
        {
            get { return _dbConnection ?? (_dbConnection = DbFactory.Initialize()); }
        }
        public BaseRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }
        #endregion

        public virtual IEnumerable<T> GetAll<M>(string table) where M : class, new()
        {
            ICollection<T> objCollection = new List<T>();
            InjectMapper<M>();
            _sqlQuery = string.Format("Select * From [{0}];", table);
            IDataReader reader = ExecuteReader();
            while (reader.Read()) objCollection.Add(_mapper.MapObject(reader));
            reader.Close();
            return objCollection;
        }

        public virtual IEnumerable<T> FindBy<M>(string table, string column, string value) where M : class, new()
        {
            ICollection<T> objCollection = new List<T>();
            InjectMapper<M>();
            _sqlQuery = string.Format(@"Select * From [{0}] Where {1} = {2};", table, column, value);
            IDataReader reader = ExecuteReader();
            while (reader.Read()) objCollection.Add(_mapper.MapObject(reader));
            reader.Close();
            return objCollection;
        }

        public virtual T GetSingle<M>(string table, string column, string value) where M : class, new()
        {
            InjectMapper<M>();
            T obj = null;
            _sqlQuery = string.Format(@"Select * From [{0}] Where {1} = {2};", table, column, value);
            IDataReader reader = ExecuteReader();
            obj = _mapper.MapObject(reader);
            reader.Close();

            return obj;
        }

        //public virtual IQueryable<T> AllIncluding(params Expression<Func<T, object>>[]includeProperties)
        //{
        //}
        //public T GetSingle(int id)
        //{
        //    return GetAll().FirstOrDefault(x => x.ID == id);
        //}
        //public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        //{
        //}

        public virtual void Add(string table, IDictionary<string, object> columnValue)
        {
            _sqlQuery = string.Format("Insert Into [{0}]({1}) Values({2});", table, string.Join(",", columnValue.Keys.ToArray()), string.Join(",", columnValue.Values.ToArray()));
            ExecuteNonQuery();
        }

        public virtual void Edit(T entity)
        {
        }

        public virtual void Delete(string table)
        {
            _sqlQuery = string.Format("Delete From [{0}]", table);
            ExecuteNonQuery();
        }

        private IDataReader ExecuteReader()
        {
            _command = new SqlCommand(_sqlQuery, DbConnection.SqlConnection);
            return _command.ExecuteReader();
        }

        private int ExecuteNonQuery()
        {
            _command = new SqlCommand(_sqlQuery, DbConnection.SqlConnection);
            return _command.ExecuteNonQuery();
        }

        private object ExecuteScalar()
        {
            _command = new SqlCommand(_sqlQuery, DbConnection.SqlConnection);
            return _command.ExecuteScalar();
        }

        private void InjectMapper<M>()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<M>().As<IMapperBase<T>>();
            IContainer container = builder.Build();
            _mapper = container.Resolve<IMapperBase<T>>();
        }
    }
}
