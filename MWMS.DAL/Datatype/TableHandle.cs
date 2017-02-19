using DBHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Helper.Extensions;
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
                int count = Fields.Where(p1 => p1.Value.isPublicField && p1.Value.name == _fields[i]).Count();
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
        /// 获取数据地址
        /// </summary>
        /// <param name="columnId">数据所属栏目</param>
        /// <returns></returns>
        void ReplaceUrl(double columnId, double dataId)
        {
            MWMS.DAL.TableHandle column = new MWMS.DAL.TableHandle("Class");
            Dictionary<string, object> columnModel = column.GetModel(columnId, "dirPath,dirName,rootId");
            Dictionary<string, object> channelModel = column.GetModel(columnModel["rootId"].ToDouble(), "dirName");
            StringBuilder url = new StringBuilder(BaseConfig.contentUrlTemplate);
            string text = BaseConfig.contentUrlTemplate.ToString().Trim();
            url.Append(BaseConfig.contentUrlTemplate.ToString().Trim());
            url.Replace("$id", "'+convert(varchar(20),convert(decimal(18,0),id))+'");
            url.Replace("$create.year", "'+convert(varchar(4),year(createdate))+'");
            url.Replace("$create.month", "'+right('00'+cast(month(createdate) as varchar),2)+'");
            url.Replace("$create.day", "'+right('00'+cast(day(createdate) as varchar),2)+'");
            url.Replace("$column.dirPath", columnModel["dirPath"].ToStr());
            url.Replace("$column.dirName", columnModel["dirName"].ToStr());
            url.Replace("$channel.dirName", channelModel["dirName"].ToStr());
            url.Replace(".$extension", "");
            string sql = "update mainTable set url='" + url + "' where id=@id";
            Helper.Sql.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("id", dataId) });
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
                try { 
                    Field f = Fields[field.Key];
                    object value = f.Convert(field.Value,Field.ConvertType.SqlData);
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
                }catch
                {

                }
            }
            if (mainFields.ContainsKey("id")) dataFields["id"] = mainFields["id"];
            
            StringBuilder fieldstr = new StringBuilder();
            MWMS.DAL.TableHandle t = new MWMS.DAL.TableHandle("maintable");
            MWMS.DAL.TableHandle t1 = new MWMS.DAL.TableHandle(TableName);
            double id = 0;
            if (mainFields.ContainsKey("id") && mainFields["id"].ToDouble()>0)
            {
                mainFields["createDate"] = DateTime.Now;
                mainFields["updateDate"] = DateTime.Now;
                t.Update(mainFields);
                id = t1.Update(dataFields);
            } else
            {
                id = double.Parse(Helper.Tools.GetId());
                mainFields["id"] = id;
                mainFields["updateDate"] = DateTime.Now;
                dataFields["id"] = id;
                t.Append(mainFields);
                id = t1.Append(dataFields);
            }
            if (mainFields.ContainsKey("classId")) ReplaceUrl(mainFields["classId"].ToDouble(), mainFields["id"].ToDouble());
            return id;
        }
    }
}
