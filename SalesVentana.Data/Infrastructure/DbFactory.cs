using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;

namespace SalesVentana.Data
{
    public class DbFactory : IDbFactory
    {
        private SalesVentanaConnection _dbConnection;

        public SalesVentanaConnection Initialize()
        {
            string connectionString = ConfigManager.ConnectionStrings.ConnectionStrings["SalesVentanaConnection"].ToString();

            if (_dbConnection == null)
                _dbConnection = new SalesVentanaConnection(connectionString);
            if (_dbConnection.SqlConnection.State == ConnectionState.Closed)
                _dbConnection.SqlConnection.Open();

            return _dbConnection;
        }

        public void Terminate()
        {
            if (_dbConnection != null)
            {
                if (_dbConnection.SqlConnection != null && _dbConnection.SqlConnection.State == ConnectionState.Open)
                    _dbConnection.SqlConnection.Close();

                _dbConnection.SqlConnection = null;
                _dbConnection = null;
            }
        }

        public Configuration ConfigManager
        {
            get
            {
                var filePath = @"G:\LOCAL\SalesVentana\Updated\SalesVentana\SalesVentana.Data\App.config";
                //var filePath = @"E:\SalesVentana\SalesVentana.Data\App.config";
                var map = new ExeConfigurationFileMap { ExeConfigFilename = filePath };
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
        }
    }
}
