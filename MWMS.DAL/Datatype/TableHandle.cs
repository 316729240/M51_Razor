using DBHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Helper;
using System.Web;

namespace MWMS.DAL.Datatype
{
    public class TableHandle : TableStructure
    {
        public TableHandle(string tableName) : base(tableName)
        {

        }
        public TableHandle(double datatypeId) : base(datatypeId)
        {

        }
        public Dictionary<string, object> GetModel(double id)
        {
            return GetModel(id, "*");
        }
        public Dictionary<string, object> GetModel(double id, string fields)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("id", id);
            return GetModel(fields, "A.id=@id", p, "");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p, string desc)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> model = new Dictionary<string, object>();
            string[] _fields = fields.Split(',');
            string fieldList = "";
            for (int i = 0; i < _fields.Length; i++)
            {
                int count = Fields.Where(p1 => p1.isPublicField && p1.name == _fields[i]).Count();
                fieldList += ((count > 0) ? "A." : "B.") + _fields[i];
            }
            SqlParameter[] _p = GetParameter(p);
            SqlDataReader rs = SqlServer.ExecuteReader("select " + fields + " from [mainTable] A inner join [" + TableName + "] B on A.id=B.id where " + where + " " + desc, _p);
            bool flag = false;
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    model[rs.GetName(i)] = rs[rs.GetName(i)];
                }
                flag = true;
            }
            rs.Close();
            if (!flag) return null;
            return model;
        }
        SqlParameter[] GetParameter(Dictionary<string, object> p)
        {
            SqlParameter[] _p = new SqlParameter[p.Count];
            int i1 = 0;
            foreach (var value in p)
            {
                _p[i1] = new SqlParameter(value.Key, value.Value);
                i1++;
            }
            return _p;
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="model">数据模型</param>
        /// <returns></returns>
        public double Save(Dictionary<string, object> model)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> mainFields = new Dictionary<string, object>();
            Dictionary<string, object> dataFields = new Dictionary<string, object>();
            foreach (var field in model)
            {
                Field f = Fields.Where(p1 => p1.name == field.Key).First<Field>();
                if (f == null)
                    throw new Exception(f.name + "字段不存在");
                object value = GetValue(f, field.Value.ToString());
                if (value != null)
                {
                    if (f.isPublicField)
                    {
                        mainFields[field.Key] = value;
                    }
                    else
                    {
                        dataFields[field.Key] = value;
                    }
                }
            }
            if (mainFields.ContainsKey("id")) dataFields["id"] = mainFields["id"];
            StringBuilder fieldstr = new StringBuilder();
            MWMS.DAL.TableHandle t = new MWMS.DAL.TableHandle("maintable");
            MWMS.DAL.TableHandle t1 = new MWMS.DAL.TableHandle(TableName);
            double id = 0;
            if (mainFields.ContainsKey("id")) {
                t.Update(mainFields);
                id = t1.Update(dataFields);
            } else
            {
                id = double.Parse(Helper.Tools.GetId());
                mainFields["id"] = id;
                dataFields["id"] = id;
                t.Append(mainFields);
                id = t1.Append(dataFields);
            }
            return id;
        }
        object GetValue(Field f, string data)
        {
            object value = data;
            switch (f.type)
            {
                case "Number":
                    try
                    {
                        value = int.Parse(data);
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                case "Double":
                    try
                    {
                        value = double.Parse(data);
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                case "DateTime":
                    try
                    {
                        value = DateTime.Parse(data);
                    }
                    catch
                    {
                        return null;
                    }
                    break;
                case "Files":
                    FieldType.Files file = FieldType.Files.Parse(data);
                    if (file != null) value = file.ToJson();
                    else { value = ""; }
                    break;
            }
            return value;
        }
    }
}
