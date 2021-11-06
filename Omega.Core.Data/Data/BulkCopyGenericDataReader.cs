using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    /// <summary>
    /// Класс, который реализуют интерфейс IDataReader, используемый для реализации операции BulkInsert.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BulkCopyGenericDataReader<T> : IDataReader where T : class
    {
        // Реализация полностью взята отсюда:
        // http://www.csvreader.com/posts/generic_list_datareader.php
        // Альтернативный вариант исполнения :
        //  http://technico.qnownow.com/2012/03/27/custom-data-reader-to-bulk-copy-data-from-object-collection-to-sql-server/

        private readonly IEnumerator<T> list = null;
        private readonly List<PropertyInfo> properties = new List<PropertyInfo>();

        public BulkCopyGenericDataReader(IEnumerable<T> list)
        {
            this.list = list.GetEnumerator();
            foreach (PropertyInfo property in typeof(T).GetProperties(
                BindingFlags.GetProperty |
                BindingFlags.Instance |
                BindingFlags.Public).Where(property => CheckPropertyType(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType)))
            {
                this.properties.Add(property);
            }
        }

        private static bool CheckPropertyType(Type propertyType)
        {
            return propertyType.IsPrimitive ||
                   propertyType == typeof(string) ||
                   propertyType == typeof(DateTime);
        }

        #region IDataReader Members

        public void Close()
        {
            list.Dispose();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return list.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return properties.Count; }
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            return properties[i].PropertyType;
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            return properties[i].Name;
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            return properties[i].GetValue(list.Current, null);
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
