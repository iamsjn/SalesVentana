using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesVentana.BO;
using System.Data.SqlClient;

namespace SalesVentana.Data
{
    public class SalesVentanaConnection
    {
        public SalesVentanaConnection(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        private SqlConnection _connection;
        public SqlConnection SqlConnection
        {
            get { return _connection; }
            set { _connection = value; }
        }

    }
}
