using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbFactory dbFactory;
        private SalesVentanaConnection _dbConnection;

        public UnitOfWork(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public SalesVentanaConnection DbConnection
        {
            get { return _dbConnection ?? (_dbConnection = dbFactory.Initialize()); }
        }

        public void Terminate()
        {
            dbFactory.Terminate();
        }
    }
}
