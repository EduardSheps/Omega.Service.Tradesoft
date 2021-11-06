using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// SQL Транзакция
        /// </summary>
        SqlTransaction Transaction { get; set; }

        /// <summary>
        /// Транзакция
        /// </summary>
        ITransaction ParentTransaction { get; }

        /// <summary>
        /// Возвращает подключение транзакции 
        /// </summary>
        SqlConnection Connection { get; }

        /// <summary>
        /// Возвращает уровень вложенности транзакций
        /// </summary>
        int TransactionLevel { get; }

        /// <summary>
        /// Указывает на то что транзакция завершена
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Подтверждаем транзакцию
        /// </summary>
        void Commit();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">таймаут на выполнение в секундах</param>
        void CommitCommand(int timeout);

        /// <summary>
        /// Отменяем транзацкию
        /// </summary>
        void Rollback();
    }
}
