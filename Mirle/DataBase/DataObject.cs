using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Mirle.DataBase
{
    public sealed class DataObject<T> where T : ValueObject, new()
       
    {
        private readonly List<T> _data = new List<T>();

        public IEnumerable<T> Data => _data;

        public T this[int index] => _data[index];

        public int Count => _data.Count;

        public DataObject()
        {
        }

        public DataObject(DataTable dataTable)
        {
            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)
            {
                var row = dataTable.Rows[iRow];
                _data.Add((T)new T().ConvaertDataRow(row));
            }
        }
    }
}
