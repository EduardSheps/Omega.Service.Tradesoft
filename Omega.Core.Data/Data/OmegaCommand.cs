using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public partial class OmegaCommand
    {
        internal const string TIMEOUT_EXPIRED_MESSAGE = "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.";

        private readonly ITransaction _transaction;

        private SqlCommand _scmd;

        internal SqlCommand SqlCommand => _scmd;

        public int CommandTimeout
        {
            get
            {
                return _scmd.CommandTimeout;
            }
            set
            {
                _scmd.CommandTimeout = value;
            }
        }

        public ITransaction Transaction => _transaction;

        public SqlParameterCollection Parameters => _scmd.Parameters;

        public CommandType CommandType
        {
            get
            {
                return _scmd.CommandType;
            }
            set
            {
                _scmd.CommandType = value;
            }
        }

        public string CommandText
        {
            get
            {
                return _scmd.CommandText;
            }
            set
            {
                _scmd.CommandText = value;
            }
        }

#if (DEBUG || STAND)
        internal string OutputCommandText
        {
            get
            {
                string methodName = "unknown(...)";
                StackFrame[] stackFrames = new StackTrace().GetFrames();
                if (stackFrames == null)
                {
                    return string.Concat(CommandWithParameterInLine(this._scmd), " from ", methodName);
                }
                /* method.DeclaringType может быть равен null для Ling*/
                var methodBase =
                    stackFrames.Select(stackFrame => stackFrame.GetMethod())
                        .FirstOrDefault(method => method.DeclaringType != null && method.DeclaringType.FullName.IndexOf("DAL", StringComparison.CurrentCultureIgnoreCase) >= 0);
                if (methodBase != null)
                {
                    methodName = string.Format("{0}::{1}(...)", methodBase.DeclaringType.FullName, methodBase.Name);

                }
                return string.Concat(CommandWithParameterInLine(this._scmd), " from ", methodName);
            }
        }

        private static string CommandWithParameterInLine(SqlCommand scmd)
        {
            StringBuilder sb = new StringBuilder();
            if (scmd.CommandType == CommandType.Text) sb.Append("query");
            else sb.AppendFormat("sp: {0}", scmd.CommandText);

            sb.Append("(");
            if (scmd.Parameters.Count > 0)
            {
                var parameters = new string[scmd.Parameters.Count];
                for (int index = 0; index < scmd.Parameters.Count; index++)
                {
                    SqlParameter param = scmd.Parameters[index];
                    parameters[index] = string.Concat(
                        param.ParameterName,
                        ": ",
                        param.Value,
                        param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput || param.Direction == ParameterDirection.ReturnValue
                            ? " [out]"
                            : string.Empty);
                }
                sb.Append(string.Join(", ", parameters));
            }
            sb.Append(")");

            return sb.ToString();
        }
#endif

        private OmegaCommand(string commandText)
        {
            _scmd = new SqlCommand(commandText);
        }

        internal OmegaCommand(string commandText, SqlConnection connection)
            : this(commandText)
        {
            _scmd.Connection = connection;
        }

        internal OmegaCommand(string commandText, ITransaction transaction)
            : this(commandText)
        {
            _transaction = transaction;

            if (transaction == null)
            {
                return;
            }
            if (transaction.Transaction == null || transaction.IsCompleted) throw new Exception("Транзакция откачена!");
            this._scmd.Connection = transaction.Transaction.Connection;
            this._scmd.Transaction = transaction.Transaction;
        }

        ~OmegaCommand()
        {
            Dispose(false);
        }

        /// <summary>
        /// Выполнить комманду с чтением данных в <see cref="SqlDataReader"/>
        /// </summary>
        /// <param name="timeout">Время ожидания данных с сервера</param>
        /// <returns>Данные <see cref="SqlDataReader"/></returns>
        public SqlDataReader ExecuteReader(int timeout = OmegaDataLayer.MIN_TIMEOUT_VALUE)
        {
            if (_transaction == null && _scmd.Connection.State != ConnectionState.Open)
            {
                _scmd.Connection.Open();
            }
            _scmd.CommandTimeout = timeout;
            SqlDataReader sdr;

#if (DEBUG)
            /* CALC EXECUTE TIME */
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            try
            {
                sdr = _transaction == null ? _scmd.ExecuteReader(CommandBehavior.CloseConnection) : _scmd.ExecuteReader();
            }
            catch (SqlException ex)
            {
                if (SqlExceptionHelper.IsTimeoutException(ex))
                {
                    throw new DataAccessTimeOutException(TIMEOUT_EXPIRED_MESSAGE, GetTextDecription(_scmd), ex);
                }
                throw new DataAccessExecuteException(ex.Message, GetTextDecription(_scmd), ex);
            }
#if (DEBUG)
            finally
            {
                stopwatch.Stop();
                Trace.WriteLine(string.Format("Execute {0} in {1} ms", OutputCommandText, stopwatch.ElapsedMilliseconds), "Dal");
            }
#endif
            return sdr;
        }

        /// <summary>
        /// Выполнить команду без чтения данных
        /// </summary>
        /// <param name="timeout">Время ожидания данных с сервера</param>
        /// <returns>Результат выполнения операции</returns>
        public int ExecuteNonQuery(int timeout = OmegaDataLayer.MIN_TIMEOUT_VALUE)
        {

            if (_transaction == null && _scmd.Connection.State != ConnectionState.Open) _scmd.Connection.Open();

            try
            {
                _scmd.CommandTimeout = timeout;
                int returnValue = _scmd.ExecuteNonQuery();
                return returnValue;
            }
            catch (SqlException ex)
            {
                if (SqlExceptionHelper.IsTimeoutException(ex)) throw new DataAccessTimeOutException(TIMEOUT_EXPIRED_MESSAGE, GetTextDecription(_scmd), ex);
                throw new DataAccessExecuteException(ex.Message, GetTextDecription(_scmd), ex);
            }
            finally
            {
                if (_transaction == null)
                {
                    _scmd.Connection.Close();
                }

            }
        }

        /// <summary>
        /// ? 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public object ExecuteScalar(int timeout = OmegaDataLayer.MIN_TIMEOUT_VALUE)
        {
            if (_transaction == null && _scmd.Connection.State != ConnectionState.Open)
            {
                _scmd.Connection.Open();
            }

#if (DEBUG)
            /* CALC EXECUTE TIME */
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            try
            {
                _scmd.CommandTimeout = timeout;
                object returnValue = _scmd.ExecuteScalar();
                return returnValue;
            }
            catch (SqlException ex)
            {
                if (SqlExceptionHelper.IsTimeoutException(ex))
                    throw new DataAccessTimeOutException(TIMEOUT_EXPIRED_MESSAGE, GetTextDecription(_scmd), ex);
                throw new DataAccessExecuteException(ex.Message, GetTextDecription(_scmd), ex);
            }
            finally
            {
                if (_transaction == null)
                {
                    _scmd.Connection.Close();
                }
#if (DEBUG)
                stopwatch.Stop();
                Trace.WriteLine(string.Format("Execute {0} in {1} ms", OutputCommandText, stopwatch.ElapsedMilliseconds), "Dal");
#endif
            }
        }

        /// <summary>
        /// Получить датасет
        /// </summary>
        /// <param name="timeout">Таймаут</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(int timeout = OmegaDataLayer.MIN_TIMEOUT_VALUE)
        {
            if (_transaction == null && _scmd.Connection.State != ConnectionState.Open)
            {
                _scmd.Connection.Open();
            }

#if (DEBUG)
            /* CALC EXECUTE TIME */
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            try
            {
                _scmd.CommandTimeout = timeout;
                using (var adapter = new SqlDataAdapter(_scmd))
                {
                    var result = new DataSet();
                    adapter.Fill(result);
                    return result;
                }
            }
            catch (SqlException ex)
            {
                if (SqlExceptionHelper.IsTimeoutException(ex))
                    throw new DataAccessTimeOutException(TIMEOUT_EXPIRED_MESSAGE, GetTextDecription(_scmd), ex);
                throw new DataAccessExecuteException(ex.Message, GetTextDecription(_scmd), ex);
            }
            finally
            {
                if (_transaction == null)
                {
                    _scmd.Connection.Close();
                }
#if (DEBUG)
                stopwatch.Stop();
                Trace.WriteLine(string.Format("Execute {0} in {1} ms", OutputCommandText, stopwatch.ElapsedMilliseconds), "Dal");
#endif
            }
        }

        /// <summary>
        /// Получить датасет
        /// </summary>
        /// <param name="timeout">Таймаут</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(int timeout = OmegaDataLayer.MIN_TIMEOUT_VALUE)
        {
            var result = ExecuteDataSet(timeout);
            return result.Tables.Count > 0 ? result.Tables[0] : null;
        }

        [Obsolete("Используйте коллекцию Parameters")]
        public SqlParameter AddParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, object value)
        {
            return _scmd.Parameters.Add(new SqlParameter(parameterName, dbType, size, direction, isNullable, 0, 0, string.Empty, DataRowVersion.Current, value));
        }

        [Obsolete("Используйте коллекцию Parameters")]
        public SqlParameter AddParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable)
        {
            return AddParameter(parameterName, dbType, size, direction, isNullable, DBNull.Value);
        }


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (this._scmd == null)
            {
                return;
            }
            if (this._transaction == null)
            {
                if (this._scmd.Connection != null)
                {
                    if (this._scmd.Connection.State == ConnectionState.Open)
                    {
                        this._scmd.Connection.Close();
                    }
                    this._scmd.Connection.Dispose();
                }
            }
            this._scmd.Dispose();
            this._scmd = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members


        /// <summary>
        /// Возвращает описание SQL процедуры
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static string GetTextDecription(SqlCommand command)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(string.Concat("CommandType: ", command.CommandType));
            sb.AppendLine(string.Concat("CommandTimeout: ", command.CommandTimeout));
            if (command.Transaction != null)
            {
                sb.AppendLine(string.Concat("Transaction IsolationLevel: ", command.Transaction.IsolationLevel));
            }

            if (command.CommandType == CommandType.StoredProcedure)
            {
                sb.AppendLine(string.Concat("Command Text: ", command.CommandText));
            }
            else
            {
                sb.AppendLine("Command Text:");
                sb.AppendLine(command.CommandText);
                sb.AppendLine();
            }

            if (command.Parameters.Count <= 0)
            {
                return sb.ToString();
            }
            const string tab = "  ";
            sb.AppendLine();
            sb.AppendLine("Command Parameters:");
            foreach (SqlParameter parameters in command.Parameters)
            {
                sb.AppendLine(string.Concat(tab, "Name: ", parameters.ParameterName, " Value: ", parameters.Value));
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
