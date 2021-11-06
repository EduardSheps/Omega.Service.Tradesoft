using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data.Application
{
    /// <summary>
    /// Предоставляет абстрактный класс для реализации методов и свойства базового приложения, работающего с базой данных
    /// </summary>
    public abstract class DataLayerApplicationBase
        : ApplicationBase
    {
        /// <summary>
        /// Объект блокировки для операций подключения к базе данных.
        /// </summary>
        private readonly static object _dbLocker = new object();

        /// <summary>
        /// Текущее подключение к базе данных
        /// </summary>
        private OmegaConnection _connection;

        /// <summary>
        /// Описывает реализиацию текущего приложения
        /// </summary>
        public static DataLayerApplicationBase Current { get; private set; }

        /// <summary>
        /// Конструктор приложения
        /// </summary>
        protected DataLayerApplicationBase()
        {
            _connection = null;
            Current = this;
        }

        /// <summary>
        /// Возвращает имя приложения описанного в строке подключения
        /// </summary>
        public override string ApplicationName
        {
            get { return ((System.Data.SqlClient.SqlConnectionStringBuilder)Connection).ApplicationName; ; }
        }


        /// <summary>
        /// Описание строки подключения к базе данных
        /// </summary>
        public OmegaConnection Connection
        {
            get
            {
                if (_connection == null)
                    lock (_dbLocker)
                    {
                        if (_connection == null)
                        {
                            try
                            {
                                _connection = new OmegaConnection("DataLayerApplicationBase");
                                _connection.CheckConnection();
                            }
                            catch (SqlException sqlex)
                            {
                                //Log.LogStorage.RegisterException(sqlex, "DataLayerApplicationBase::Connection", _connection == null ? String.Empty : _connection.ConnectionString);
                                //Log.LogStorage.WaitForAllMessageIsSave();
                                Environment.Exit(-1);
                            }
                        }
                    }
                return _connection;
            }
        }
    }
}
