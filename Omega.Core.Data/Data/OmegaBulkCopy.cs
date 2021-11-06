using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public class OmegaBulkCopy : IDisposable
    {
        private SqlBulkCopy _bulkCopy;
        private SqlConnection _connection;
        private OmegaTransaction _transaction;


        internal OmegaBulkCopy(SqlConnection connection)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            _bulkCopy = new SqlBulkCopy(_connection);
            _bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(_bulkCopy_SqlRowsCopied);
        }


        internal OmegaBulkCopy(OmegaTransaction tran) :
            this(tran != null ? tran.Connection : null)
        {
            _transaction = tran;
        }

        public event SqlRowsCopiedEventHandler SqlRowsCopied;

        void _bulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            if (SqlRowsCopied != null)
                SqlRowsCopied(this, e);
        }

        public SqlBulkCopyColumnMappingCollection ColumnMappings
        {
            get { return _bulkCopy.ColumnMappings; }
        }

        public string DestinationTableName
        {
            get { return _bulkCopy.DestinationTableName; }
            set { _bulkCopy.DestinationTableName = value; }
        }

        public int BulkCopyTimeout
        {
            get { return _bulkCopy.BulkCopyTimeout; }
            set { _bulkCopy.BulkCopyTimeout = value; }
        }

        public int BatchSize
        {
            get { return _bulkCopy.BatchSize; }
            set { _bulkCopy.BatchSize = value; }
        }

        public int NotifyAfter
        {
            get { return _bulkCopy.NotifyAfter; }
            set { _bulkCopy.NotifyAfter = value; }
        }

        public void WriteToServer(DataTable table)
        {
            _bulkCopy.WriteToServer(table);
        }

        public void WriteToServer(DataRow[] rows)
        {
            _bulkCopy.WriteToServer(rows);
        }

        public void WriteToServer(IDataReader reader)
        {
            _bulkCopy.WriteToServer(reader);
        }

        public void WriteToServer(DataTable table, DataRowState state)
        {
            _bulkCopy.WriteToServer(table, state);
        }

        #region IDisposable Members
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bulkCopy != null)
                {
                    if (_transaction == null)
                    {
                        if (_connection != null)
                        {
                            if (_connection.State == ConnectionState.Open)
                                _connection.Close();
                            _connection.Dispose();
                        }
                    }
                    _bulkCopy.SqlRowsCopied -= new SqlRowsCopiedEventHandler(_bulkCopy_SqlRowsCopied);
                    ((IDisposable)_bulkCopy).Dispose();
                    _bulkCopy = null;
                }
            }
        }
        ~OmegaBulkCopy()
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