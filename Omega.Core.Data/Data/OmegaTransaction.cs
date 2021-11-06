using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    /// <summary>
    /// Предоставляет методы для работы с транзакцией базы данных 
    /// </summary>
    public class OmegaTransaction : ITransaction
    {
        /// <summary>
        /// Родительская транзакция (верхнего уровня)
        /// </summary>
        private readonly ITransaction _parentTransaction;

        /// <summary>
        /// Создать новую транзакцию
        /// </summary>
        /// <param name="transaction">транзакция, начатая в SqlConnection</param>
        public OmegaTransaction(SqlTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// Транзакция
        /// </summary>
        public SqlTransaction Transaction { get; set; }

        /// <summary>
        /// Транзакция
        /// </summary>
        public ITransaction ParentTransaction
        {
            get { return _parentTransaction; }
        }

        /// <summary>
        /// Возвращает подключение транзакции 
        /// </summary>
        public SqlConnection Connection
        {
            get { return Transaction.Connection; }
        }

        /// <summary>
        /// Создать вложенную транзакцию
        /// </summary>
        /// <param name="parentTransaction">родительская транзакция</param>
        public OmegaTransaction(ITransaction parentTransaction)
        {
            _parentTransaction = parentTransaction;
            Transaction = parentTransaction.Transaction;
        }

        /// <summary>
        /// Возвращает уровень вложенности транзакций
        /// </summary>
        public int TransactionLevel
        {
            get
            {
                var tranCount = 0; //Счетчик транзакций. Изначально 0
                var parentTransaction = _parentTransaction; //буфер для обхода родительских транзакций
                while (parentTransaction != null)
                {
                    tranCount++;
                    parentTransaction = parentTransaction.ParentTransaction;
                }
                return tranCount;
            }
        }

        /// <summary>
        /// флаг того, что транзакция была подтверждена
        /// </summary>
        private bool _isCommited;

        /// <summary>
        /// Подтверждаем транзакцию
        /// </summary>
        public void Commit()
        {
            if (IsCompleted)
                throw new AlreadyCompletedTransactionException();
            if (_parentTransaction == null) //если это не вложенная транзакция, то делаем коммит. Иначе ничего делать не нужно
            {
                var connection = Connection;
                Transaction.Commit();
                CloseConnection(connection);
            }
            _isCommited = true;
        }

        /// <summary>
        /// Подтверждаем транзакцию (используется SqlCommand)
        /// </summary>
        /// <param name="timeout">таймаут на выполнение в секундах</param>
        public void CommitCommand(int timeout)
        {
            if (IsCompleted)
                throw new AlreadyCompletedTransactionException();
            if (_parentTransaction == null) //если это не вложенная транзакция, то делаем коммит. Иначе ничего делать не нужно
            {
                var connection = Connection;

                // при завершении некоторых транзакций они отлетают по таймауту
                // чтобы этого избежать используется такой способ
                var cmd = new SqlCommand("COMMIT")
                {
                    Connection = connection,
                    Transaction = this.Transaction,
                    CommandTimeout = timeout
                };
                cmd.ExecuteNonQuery();

                CloseConnection(connection);
            }
            _isCommited = true;
        }

        /// <summary>
        /// Отменяем транзацкию
        /// </summary>
        public void Rollback()
        {
            if (IsCompleted || Transaction.Connection == null)
                return;
            var connection = Connection;
            Transaction.Rollback();
            CloseConnection(connection);
        }

        /// <summary>
        /// Закрыть коннекшн
        /// </summary>
        private void CloseConnection(SqlConnection connection)
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                //Обнуляем объект транзакции
                var parentTransaction = _parentTransaction; //буфер для обхода родительских транзакций
                while (parentTransaction != null)
                {
                    parentTransaction.Transaction = null;
                    parentTransaction = parentTransaction.ParentTransaction;
                }
                Transaction = null;
            }
        }

        /// <summary>
        /// Указывает на то что транзакция завершена
        /// </summary>
        public bool IsCompleted
        {
            get { return Transaction == null; }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_isCommited)
                    Rollback();
            }
        }

        ~OmegaTransaction()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
