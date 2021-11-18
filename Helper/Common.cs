using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WxAPI.Helper
{
    public static class Common
    {
        public static List<Dictionary<string, object>> DataTableToList(this DataTable dt, bool isAddFirst = false)
        {
            if (isAddFirst) dt.Rows.InsertAt(dt.NewRow(), 0);
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> rowdic = new System.Collections.Generic.Dictionary<string, object>();
                int index = dt.Rows.IndexOf(row);


                foreach (DataColumn dc in dt.Columns)
                {
                    object value = row[dc];
                    if (dc.DataType == typeof(byte[]))
                    {
                        if (!string.IsNullOrEmpty(row[dc].ToString()))
                        {
                            rowdic.Add(dc.ColumnName, Convert.ToBase64String(row[dc] as byte[]));
                        }
                        else
                        {
                            rowdic.Add(dc.ColumnName, null);
                        }
                    }
                    else// if(row[dc] != DBNull.Value)
                    {
                        rowdic.Add(dc.ColumnName, row[dc]);
                    }
                }
                list.Add(rowdic);
            }
            return list;

        }
    }
}
