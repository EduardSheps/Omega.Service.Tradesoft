using Omega.Core.Data.Application;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    /// <summary>
    /// Описывает процедуры и функции выполнения запросов и процедур на базе данных
    /// </summary>
    public class OmegaDataLayer : IOmegaDataLayer
    {

        /// <summary>
        /// Делегат создания команды. Требуется для формирования команды в другом классе.
        /// </summary>
        /// <param name="cmdText">Текст команды.</param>
        /// <param name="cmdType">Тип команды.</param>
        /// <param name="transaction">Транзакция.</param>
        /// <returns>Экземпляр EmexCommand.</returns>
        public delegate OmegaCommand CreateCommandDelegate(string cmdText, CommandType cmdType, ITransaction transaction);

        /// <summary>
        /// Минимальное время задержки ожидания запроса в секундах
        /// </summary>
        public const int MIN_TIMEOUT_VALUE = 20;
        /// <summary>
        /// Максимальное время задержки ожидания запроса в секундах
        /// </summary>
        public const int MAX_TIMEOUT_VALUE = 600;

        protected readonly string _connectionString;

        /// <summary>
        /// Конструктор
        /// </summary>
        protected OmegaDataLayer()
        {
            var name = System.Configuration.ConfigurationManager.AppSettings["ConnectionPrefix"].ToString();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[name].ToString();
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connectionString">Строка соединения (оставляем для совместимости)</param>
        protected OmegaDataLayer(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connectionInfo">Информация о подключении</param>
        protected OmegaDataLayer(ConnectionInfo connectionInfo)
        {
            _connectionString = DataLayerApplicationBase.Current.Connection.ConnectionStringByName(connectionInfo.ConnectionName);
        }

        /// <summary>
        /// Создаёт новый объект подключения к базе данных
        /// </summary>
        /// <returns></returns>
        protected SqlConnection CreateConnection()
        {
            var sCnn = new SqlConnection(_connectionString);
            //ServiceSecurityContext.Current.PrimaryIdentity.Name
            return sCnn;
        }

        /// <summary>
        /// Создаёт новую транзакцию <see cref="OmegaTransaction"/>
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <param name="isolationLevel">Уровень изоляции транзакции</param>
        /// <returns>Новая транзакция <see cref="OmegaTransaction"/></returns>
        public static ITransaction BeginTransaction(String connectionString, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return new OmegaTransaction(connection.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Создаёт новую транзакцию <see cref="OmegaTransaction"/>
        /// </summary>
        /// <param name="isolationLevel">Уровень изоляции транзакции <see cref="IsolationLevel"/></param>
        /// <returns>Новая транзакция <see cref="OmegaTransaction"/></returns>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var connection = CreateConnection();
            connection.Open();
            return new OmegaTransaction(connection.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Создаёт новую транзакцию <see cref="OmegaTransaction"/> на основе существующей
        /// Если транзакция указана, то просто увеличивает номер транзакции
        /// </summary>
        /// <param name="transaction">Транзакция <see cref="OmegaTransaction"/></param>
        /// <param name="isolationLevel">Уровень изоляции транзакции <see cref="IsolationLevel"/>, если будет создана новая транзакция</param>
        /// <returns>Транзакция <see cref="OmegaTransaction"/></returns>
        public ITransaction BeginTransaction(ITransaction transaction, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (transaction == null)
            {
                return this.BeginTransaction(isolationLevel);
            }
            if (transaction.IsCompleted)
            {
                throw new AlreadyCompletedTransactionException { Source = "Omega.Data.OmegaDataLayer.BeginTransaction" };
            }
            return new OmegaTransaction(transaction);
        }

        /// <summary>
        /// Создаёт новый запрос к базе данных
        /// </summary>
        /// <param name="commandText">Текст запроса</param>
        /// <param name="commandType">Тип комманды запроса <see cref="CommandType"/></param>
        /// <param name="transaction">Транзакция, под управлением которой будет выполняться запрос</param>
        /// <returns>Новый запрос к базе данных<see cref="OmegaCommand"/></returns>
        protected OmegaCommand CreateCommand(string commandText, CommandType commandType, ITransaction transaction)
        {
            var newCommand = transaction != null ? new OmegaCommand(commandText, transaction) : new OmegaCommand(commandText, CreateConnection());
            newCommand.CommandType = commandType;
            newCommand.CommandTimeout = MAX_TIMEOUT_VALUE;
            return newCommand;
        }

        /// <summary>
        /// Создаёт новый текстовый запрос к базе данных
        /// </summary>
        /// <param name="commandText">Текст запроса</param>
        /// <param name="transaction">Транзакция, под управлением которой будет выполняться запрос</param>
        /// <returns>Новый запрос к базе данных</returns>
        protected OmegaCommand CreateCommand(string commandText, ITransaction transaction)
        {
            return CreateCommand(commandText, CommandType.Text, transaction);
        }

        /// <summary>
        /// Создаёт новый текстовый запрос к базе данных
        /// </summary>
        /// <param name="commandText">Текст запроса</param>
        /// <returns>Новый запрос к базе данных</returns>
        protected OmegaCommand CreateCommand(string commandText)
        {
            return CreateCommand(commandText, CommandType.Text, null);
        }

        /// <summary>
        /// Создаёт новый текстовый запрос к базе данных с пустой транзакцией
        /// </summary>
        /// <param name="commandText">Текст запроса</param>
        /// <param name="commandType">Тип комманды запроса <see cref="CommandType"/></param>
        /// <returns>Новый запрос к базе данных</returns>
        protected OmegaCommand CreateCommand(string commandText, CommandType commandType)
        {
            return CreateCommand(commandText, commandType, null);
        }

        /// <summary>
        /// Выполняет <see cref="SqlCommand"/> и возвращает результат выполнения операции как <see cref="SqlDataReader"/>
        /// </summary>
        /// <param name="command">Выполняемая команда <see cref="SqlCommand"/></param>
        /// <returns>Результат выполнения операции <see cref="SqlDataReader"/></returns>
        protected static SqlDataReader GetReader(SqlCommand command)
        {
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }
            try
            {
                return command.ExecuteReader();

            }
            catch (SqlException ex)
            {
                if (SqlExceptionHelper.IsTimeoutException(ex))
                {
                    throw new DataAccessTimeOutException(OmegaCommand.TIMEOUT_EXPIRED_MESSAGE, OmegaCommand.GetTextDecription(command), ex);
                }
                throw new DataAccessExecuteException(ex.Message, OmegaCommand.GetTextDecription(command), ex);
            }
        }

        ///// <summary>
        ///// Выполняет <see cref="SqlCommand"/> и возвращает результат выполнения операции как <see cref="EmexSelectDataAdapter"/>
        ///// </summary>
        ///// <param name="commandText">Текст запроса</param>
        ///// <param name="transaction">Транзакция, под управлением которой будет выполняться запрос</param>
        ///// <param name="tableMappings">Список наименований возвращаемых таблиц</param>
        ///// <returns>Результат выполнения операции <see cref="EmexSelectDataAdapter"/></returns>
        //protected EmexSelectDataAdapter CreateSelectDataAdapter(string commandText, EmexTransaction transaction, params string[] tableMappings)
        //{
        //    var dataAdapter = transaction != null ? new EmexSelectDataAdapter(commandText, transaction) : new EmexSelectDataAdapter(commandText, CreateConnection());

        //    foreach (string mapping in tableMappings)
        //    {
        //        dataAdapter.TableMappings.Add(dataAdapter.TableMappings.Count > 0 ? string.Format("Table{0}", dataAdapter.TableMappings.Count) : "Table", mapping);
        //    }

        //    return dataAdapter;
        //}

        /// <summary>
        /// Выполняет <see cref="SqlCommand"/> и возвращает результат выполнения операции как <see cref="OmegaBulkCopy"/>
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="transaction">Транзакция, под управлением которой будет выполняться запрос</param>
        /// <returns>Результат выполнения операции <see cref="OmegaBulkCopy"/></returns>
        protected OmegaBulkCopy CreateBulkCopy(string tableName, OmegaTransaction transaction)
        {
            var bulkCopy = transaction != null ? new OmegaBulkCopy(transaction) : new OmegaBulkCopy(CreateConnection());

            bulkCopy.DestinationTableName = tableName;

            return bulkCopy;
        }

        /// <summary>
        /// Заполняет <see cref="DataTable"/> значениями запроса <see cref="SqlCommand"/>
        /// </summary>
        /// <param name="table">Таблица, в которую необходимо записать значения</param>
        /// <param name="command">Запрос</param>
        /// <param name="startRecord">С какой строки начинать выборку данных</param>
        /// <param name="maxRecord">Максимальное кол-во строк</param>
        protected static void Fill(DataTable table, SqlCommand command, int startRecord, int maxRecord)
        {
            var sda = new SqlDataAdapter(command);

            var connectionState = command.Connection.State;
            if (connectionState != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            try
            {
                sda.Fill(startRecord, maxRecord, table);
            }
            catch (SqlException ex)
            {
                if (SqlExceptionHelper.IsTimeoutException(ex))
                {
                    throw new DataAccessTimeOutException(OmegaCommand.TIMEOUT_EXPIRED_MESSAGE, OmegaCommand.GetTextDecription(command), ex);
                }
                throw new DataAccessExecuteException(ex.Message, OmegaCommand.GetTextDecription(command), ex);
            }
            finally
            {
                if (connectionState != ConnectionState.Open)
                {
                    command.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Проверяет наличие значения в параметре <see cref="SqlParameter"/>
        /// </summary>
        /// <param name="parameter">Параметер</param>
        /// <returns>true - есть значение</returns>
        public static bool IsHaveValue(SqlParameter parameter)
        {
            return parameter != null && parameter.Value != DBNull.Value;
        }

        /// <summary>
        /// Описывает функцию, создающую и заполняющую данные объекта на основе строки данных из <see cref="SqlDataReader"/>
        /// </summary>
        /// <typeparam name="TDType">Тип создаваемого объекта</typeparam>
        /// <param name="sdr">Объект последовательного чтения строк <see cref="SqlDataReader"/></param>
        /// <returns>Созданный новый объект</returns>
        public delegate TDType FillDataType<out TDType>(SqlDataReader sdr);

        /// <summary>
        /// Описывает функцию, создающую и заполняющую данные объекта на основе строки данных из <see cref="SqlDataReader"/>
        /// </summary>
        /// <typeparam name="TDType">Тип создаваемого объекта</typeparam>
        /// <param name="sdr">Объект последовательного чтения строк <see cref="SqlDataReader"/></param>
        /// <param name="instance"> </param>
        /// <returns>Созданный новый объект</returns>
        public delegate void FillInstance<in TDType>(SqlDataReader sdr, TDType instance);

        /// <summary>
        /// Описывает функцию, создающую и заполняющую строку словаря на основе строки данных из <see cref="SqlDataReader"/>
        /// </summary>
        /// <typeparam name="TKey">Тип ключа</typeparam>
        /// <typeparam name="TValue">Тип значения</typeparam>
        /// <param name="sdr">Объект последовательного чтения строк <see cref="SqlDataReader"/></param>
        /// <returns>Созданный новый объект</returns>
        public delegate KeyValuePair<TKey, TValue> FillDataType<TKey, TValue>(SqlDataReader sdr);

        /// <summary>
        /// Возвращает список считанных из <see cref="SqlDataReader"/> объектов DType
        /// </summary>
        /// <typeparam name="TDType">Тип создаваемого объекта</typeparam>
        /// <param name="selectCommand">Выполняемый запрос</param>
        /// <param name="fillMethod">Метод чтения данных</param>
        /// <param name="timeout"></param>
        /// <returns>Список считанных объектов</returns>
        public static List<TDType> GetDataList<TDType>(OmegaCommand selectCommand, FillDataType<TDType> fillMethod, int timeout = MIN_TIMEOUT_VALUE)
        {
            using (SqlDataReader sdr = selectCommand.ExecuteReader(timeout))
            {
                var lst = new List<TDType>();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        lst.Add(fillMethod(sdr));
                    }
                }
                sdr.Close();
                return lst;
            }
        }

        /// <summary>
        /// Возвращает словарь считанных объектов из <see cref="SqlDataReader"/>
        /// </summary>
        /// <typeparam name="TKey">Тип ключа</typeparam>
        /// <typeparam name="TValue">Тип значения</typeparam>
        /// <param name="selectCommand">Выполняемый запрос</param>
        /// <param name="fillMethod">Метод чтения данных</param>
        /// <param name="timeout"></param>
        /// <returns>Список считанных объектов</returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(
            OmegaCommand selectCommand,
            FillDataType<KeyValuePair<TKey, TValue>> fillMethod,
            int timeout = MIN_TIMEOUT_VALUE)
        {
            using (var sdr = selectCommand.ExecuteReader(timeout))
            {
                var lst = new Dictionary<TKey, TValue>();
                if (sdr.HasRows)
                {
                    while (sdr.Read())
                    {
                        var item = fillMethod(sdr);
                        lst.Add(item.Key, item.Value);
                    }
                }
                sdr.Close();
                return lst;
            }
        }

        /// <summary>
        /// Возвращает единственную первую строку считанную из запроса или переданное значение по умолчанию.
        /// </summary>
        /// <typeparam name="TDType">Тип создаваемого объекта.</typeparam>
        /// <param name="selectCommand">Выполняемый запрос.</param>
        /// <param name="fillMethod">Метод чтения данных.</param>
        /// <param name="defaultValue">Значение по умолчанию.</param>
        /// <param name="timeout"></param>
        /// <returns>Возвращает счиатнный объект, если нет ни одной строки то <code>default(DType)</code> </returns>
        public static TDType GetDataItem<TDType>(OmegaCommand selectCommand, FillDataType<TDType> fillMethod, TDType defaultValue = default(TDType), int timeout = MIN_TIMEOUT_VALUE)
        {
            using (SqlDataReader sdr = selectCommand.ExecuteReader(timeout))
            {
                return sdr.Read() ? fillMethod(sdr) : defaultValue;
            }
        }

        /// <summary>
        /// Возвращает единственную первую строку считанную из запроса или переданное значение по умолчанию.
        /// </summary>
        /// <typeparam name="TDType">Тип создаваемого объекта.</typeparam>
        /// <param name="selectCommand">Выполняемый запрос.</param>
        /// <param name="fillMethod">Метод чтения данных.</param>
        /// <param name="timeout"></param>
        /// <returns>Возвращает счиатнный объект, если нет ни одной строки то <code>default(DType)</code> </returns>
        public static void FillDataItem<TDType>(OmegaCommand selectCommand, FillInstance<TDType> fillMethod, TDType instance, int timeout = MIN_TIMEOUT_VALUE)
        {
            using (SqlDataReader sdr = selectCommand.ExecuteReader(timeout))
            {
                if (sdr.Read())
                {
                    fillMethod(sdr, instance);
                }
            }
        }



        /// <summary>
        /// Возвращает список считанных из <see cref="SqlDataReader"/> объектов DType
        /// </summary>
        /// <typeparam name="TDType">Тип создаваемого объекта</typeparam>
        /// <param name="reader">Объект последовательного чтения строк <see cref="SqlDataReader"/></param>
        /// <param name="fillMethod">Метод чтения данных</param>
        /// <returns>Считанные объекты</returns>
        [Obsolete(message: "Функция некорректная", error: true)]
        public static TDType[] GetDataList<TDType>(SqlDataReader reader, FillDataType<TDType> fillMethod)
        {
            var lst = new List<TDType>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    lst.Add(fillMethod(reader));
                }
            }

            if (lst.Count <= 0)
            {
                return new TDType[0];
            }
            var returnValue = new TDType[lst.Count];
            lst.CopyTo(returnValue);
            return returnValue;
        }



        /// <summary>
        /// Получить набор таблиц
        /// </summary>
        /// <param name="selectCommand">Команда выборки</param>
        /// <returns></returns>
        public static DataSet GetDataSet(OmegaCommand selectCommand)
        {
            return selectCommand.ExecuteDataSet();
        }

        /// <summary>
        /// Получить таблицу
        /// </summary>
        /// <param name="selectCommand">Команда выборки</param>
        /// <returns></returns>
        public static DataTable GetDataTable(OmegaCommand selectCommand, int timeout = OmegaDataLayer.MIN_TIMEOUT_VALUE)
        {
            return selectCommand.ExecuteDataTable(timeout);
        }

        /// <summary>
        /// Осуществляет массовую вставку в таблицу.
        /// </summary>
        /// <typeparam name="T">Тип сущности, поля которой будут вставляться в аналогичные столбцы таблицы.</typeparam>
        /// <param name="destinationTableName">Таблица, в которую необходимо осуществить массовую вставку.</param>
        /// <param name="sourceToCopy">Источник данных, из которого должна осуществляться массовая вставка.</param>
        /// <param name="transaction">Транзакция, в рамках которой проходит операция.</param>
        protected void BulkCopy<T>(string destinationTableName, IEnumerable<T> sourceToCopy, ITransaction transaction) where T : class
        {
            if (string.IsNullOrWhiteSpace(destinationTableName))
            {
                throw new ArgumentException(@"Название таблицы, в которую происходит вставка не может быть пустым", "destinationTableName");
            }

            if (sourceToCopy == null)
            {
                throw new ArgumentNullException("sourceToCopy");
            }

            var useTransaction = transaction != null;
            var connection = useTransaction ? transaction.Connection : CreateConnection();
            try
            {
                using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, useTransaction ? transaction.Transaction : null))
                {
                    sqlBulkCopy.DestinationTableName = destinationTableName;
                    sqlBulkCopy.BulkCopyTimeout = MAX_TIMEOUT_VALUE;

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    sqlBulkCopy.WriteToServer(new BulkCopyGenericDataReader<T>(sourceToCopy));
                }
            }
            catch (Exception ex)
            {
                throw new DataAccessExecuteException(ex.Message, string.Format("BulkCopy\nDestinationTableName: {0}", destinationTableName), ex);
            }
            finally
            {
                if (!useTransaction && connection != null)
                {
                    connection.Dispose();
                }
            }
        }
    }
}
