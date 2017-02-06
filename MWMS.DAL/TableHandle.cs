using DBHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MWMS.DAL
{
    /// <summary>
    /// 表操作类
    /// </summary>
    public class TableHandle
    {
        public string TableName { get; set; }
        public TableHandle()
        {

        }
        public TableHandle(string tableName)
        {
            TableName = tableName;
        }
        public Dictionary<string,object> GetModel(double id,string fields)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("id", id);
            return GetModel(fields, "id=@id", p, "");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p)
        {
            return GetModel(fields, where, p, "");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p, string desc)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> model = new Dictionary<string, object>();
            string[] _fields = fields.Split(',');
            SqlParameter[] _p = new SqlParameter[p.Count];
            int i1 = 0;
            foreach (var value in p)
            {
                _p[i1] = new SqlParameter(value.Key,value.Value);
                    i1++;
            }
            SqlDataReader rs = SqlServer.ExecuteReader("select " + fields + " from [" + TableName + "] where "+ where+" "+desc, _p);
            bool flag = false;
            if (rs.Read())
            {
                for (int i = 0; i < _fields.Length; i++)
                {
                    model[_fields[i]] = rs[_fields[i]];
                }
                flag = true;
            }
            rs.Close();
            if (!flag) return null;
            return model;
        }
        /// <summary>
        /// 保存模型到表中
        /// </summary>
        /// <param name="model">模型</param>
        public void Save(Dictionary<string, object> model)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            bool updateFlag = model.ContainsKey("id");
            if (updateFlag) { 
                Update(model);
            }else
            {
                Append(model);
            }
        }
        void Append(Dictionary<string, object> model)
        {
            StringBuilder fieldstr = new StringBuilder();
            StringBuilder fieldstr2 = new StringBuilder();
            foreach (var field in model)
            {
                if (fieldstr.Length == 0)
                {
                    fieldstr.Append(",");
                    fieldstr2.Append(",");
                }
                fieldstr.Append(field.Key);
                fieldstr2.Append("@" + field.Key);
            }
            SqlServer.ExecuteNonQuery("update [" + TableName + "] set " + fieldstr.ToString() + " where id=@id", model);
        }
        void Update(Dictionary<string, object> model)
        {
            StringBuilder fieldstr = new StringBuilder();
            foreach (var field in model)
            {
                    if (fieldstr.Length == 0) fieldstr.Append(",");
                    fieldstr.Append(field.Key + "=@" + field.Key);
            }
            SqlServer.ExecuteNonQuery("update [" + TableName + "] set " + fieldstr.ToString() + " where id=@id", model);
        }
    }
}
