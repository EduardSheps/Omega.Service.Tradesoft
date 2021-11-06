using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public class OmegaConnection
    {
        private readonly Dictionary<string, string> _connectionDictionary;

        public OmegaConnection(string defaultApplicationName)
        {
            _connectionDictionary = ReadConnectionStringFromConfigurationDictionary(defaultApplicationName);
        }

        public void CheckConnection()
        {
            /* Проверка работоспособности соединения и организация первого соединения в пуле */
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString)
            { ConnectTimeout = 100 };
            using (SqlConnection connection = new SqlConnection(connectionStringBuilder.ToString()))
            {
                try
                {
                    connection.Open();

                    string context = GetServiceExecutuionContext();
                    CheckConnectionByDate(connection);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
        }

        private static string ConnectionPrefix => ConfigurationManager.AppSettings["ConnectionPrefix"];

        private static string GetServiceExecutuionContext()
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;

            var ctx = appSettings["ServiceExecutionContext"];
            return ctx != null ? ctx.Value : string.Empty;
        }

        private static Dictionary<string, string> ReadConnectionStringFromConfigurationDictionary(string defaultApplicationName)
        {

            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettingsCollection connectionStrings = config.ConnectionStrings.ConnectionStrings;

            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (ConnectionStringSettings connectionStringSetting in connectionStrings)
            {
                if (connectionStringSetting.Name.StartsWith(ConnectionPrefix, StringComparison.CurrentCultureIgnoreCase))
                {
                    SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionStringSetting.ConnectionString);
                    if (string.IsNullOrWhiteSpace(sqlConnectionStringBuilder.ApplicationName))
                        sqlConnectionStringBuilder.ApplicationName = defaultApplicationName;
                    if (!sqlConnectionStringBuilder.Pooling)
                    {
                        sqlConnectionStringBuilder.MinPoolSize = 1;
                        sqlConnectionStringBuilder.MinPoolSize = 10;
                        sqlConnectionStringBuilder.Pooling = true;
                    }
                    result.Add(connectionStringSetting.Name, sqlConnectionStringBuilder.ToString());
                }
            }
            return result;
        }


        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return ConnectionStringByName(string.Empty);
            }
        }

        /// <summary>
        /// Строка подключения к базе данных по имени соединеня
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ConnectionStringByName(string name)
        {
            return _connectionDictionary[string.Concat(ConnectionPrefix, name)];
        }

        /// <summary>
        ///
        /// </summary>
        public string DataSource
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
                return sqlConnectionStringBuilder.DataSource;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string InitialCatalog
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
                return sqlConnectionStringBuilder.InitialCatalog;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string ApplicationName
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
                return sqlConnectionStringBuilder.ApplicationName;
            }
        }

        /// <summary>
        /// Convert from OmegaConnection to string
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static explicit operator String(OmegaConnection connection)
        {
            return connection.ConnectionString;
        }

        /// <summary>
        /// Convert from OmegaConnection to SqlConnectionStringBuilder
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static explicit operator SqlConnectionStringBuilder(OmegaConnection connection)
        {
            return new SqlConnectionStringBuilder(connection.ConnectionString);
        }


        private static void CheckConnectionByDate(SqlConnection connection)
        {
            new OmegaCommand("SELECT getdate()", connection).ExecuteNonQuery();
        }
    }
}
