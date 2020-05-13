using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SqlBatis.Test
{
    public class DataReader : IDataReader
    {
        public DataReader(DataTable table)
        {
            this.table = table;
        }
        private int index = -1;
        private DataTable table = new DataTable();
        public object this[int i]
        {
            get
            {
                var row = table.Rows[index];
                return row[i];
            }
        }

        public object this[string name]
        {
            get
            {
                var row = table.Rows[index];
                return row[name];
            }
        }

        public int Depth => table.Rows.Count;

        public bool IsClosed => false;

        public int RecordsAffected => table.Rows.Count;

        public int FieldCount => table.Columns.Count;

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i)
        {
            var val = table.Rows[i][i];
            return Convert.ToBoolean(val);
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
            return 0;
        }

        public IDataReader GetData(int i)
        {
            throw null;
        }

        public string GetDataTypeName(int i)
        {
            return table.Columns[i].DataType.Name;
        }

        public DateTime GetDateTime(int i)
        {
            var val = table.Rows[index][i];
            return Convert.ToDateTime(val);
        }

        public decimal GetDecimal(int i)
        {
            var val = table.Rows[i][i];
            return Convert.ToDecimal(val);
        }

        public double GetDouble(int i)
        {
            var val = table.Rows[i][i];
            return Convert.ToDouble(val);
        }

        public Type GetFieldType(int i)
        {
            return table.Columns[i].DataType;
        }

        public float GetFloat(int i)
        {
            var val = table.Rows[i][i];
            return Convert.ToSingle(val);
        }

        public Guid GetGuid(int i)
        {
            return Guid.NewGuid();
        }

        public short GetInt16(int i)
        {
            var val = table.Rows[i][i];
            return Convert.ToInt16(val);
        }

        public int GetInt32(int i)
        {
            var val = table.Rows[index][i];
            return Convert.ToInt32(val);
        }

        public long GetInt64(int i)
        {
            var val = table.Rows[index][i];
            return Convert.ToInt64(val);
        }

        public string GetName(int i)
        {
            var columns = table.Columns.Cast<DataColumn>().Select(s => s.ColumnName).ToArray();
            return columns[i];
        }

        public int GetOrdinal(string name)
        {
            var columns = table.Columns.Cast<DataColumn>().Select(s => s.ColumnName).ToArray();
            return Array.IndexOf(columns, name);
        }

        public DataTable GetSchemaTable()
        {
            return table;
        }

        public string GetString(int i)
        {
            return table.Rows[index][i]?.ToString();
        }

        public object GetValue(int i)
        {
            return table.Rows[i][i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return table.Rows[i][i] == null;
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            index++;
            if (index < table.Rows.Count)
            {
                return true;
            }
            return false;
        }
    }
}
