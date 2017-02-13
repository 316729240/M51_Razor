using MWMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using MWMS.DAL.Datatype;
namespace ManagerFramework
{
    public class DataManageHandle : ManageHandle
    {
        TableHandle table = null;
        public DataManageHandle()
        {

        }
        public DataManageHandle(double datatypeId)
        {
            table = new TableHandle(datatypeId);
        }
        public DataManageHandle(string tableName)
        {
            table = new TableHandle(tableName);

        }
        /// <summary>
        /// 数据信息读取
        /// </summary>
        /// <returns></returns>
        public virtual ReturnValue read()
        {
            ReturnValue returnValue = new ReturnValue();
            double id = s_request.getDouble("id");
            Dictionary<string, object> data = table.GetModel(id);
            data["url"] = TemplateEngine._replaceUrl(Config.webPath + data["url"].ToString() + "." + BaseConfig.extension);
            returnValue.userData = data;
            return returnValue;
        }
        /// <summary>
        /// 数据编辑
        /// </summary>
        /// <returns></returns>
        public ReturnValue edit()
        {
            ReturnValue returnValue = new ReturnValue();
            LoginInfo login = new LoginInfo();
            double classId = s_request.getDouble("classId");
            Permissions p = login.value.getColumnPermissions(classId);
            if (!p.write)
            {
                returnValue.errNo = -1;
                returnValue.errMsg = "没有权限";
                return returnValue;
            }
            Dictionary<string, object> model = new Dictionary<string, object>();
            foreach(var field in table.Fields)
            {
                if (context.Request.Form[field.name] != null)
                {
                    model[field.name]=s_request.getString(field.name);
                }
            }
            model["userId"] = loginUser.UserId;
            returnValue.userData= table.Save(model);
            return returnValue;
        }
        /// <summary>
        /// 数据列表
        /// </summary>
        public ReturnValue dataList()
        {
            LoginInfo login = new LoginInfo();
            ReturnValue returnValue = new ReturnValue();
            double moduleId = s_request.getDouble("moduleId");
            double classId = s_request.getDouble("classId");
            int pageNo = s_request.getInt("pageNo");
            string orderBy = s_request.getString("orderBy");
            int sortDirection = s_request.getInt("sortDirection");
            string type = s_request.getString("type");
            string searchField = s_request.getString("searchField");
            string keyword = s_request.getString("keyword").Replace("'", "''");
            double dataTypeId = -1;
            SqlDataReader rs = null;
            Permissions p = null;
            if (moduleId == classId)
            {
                p = login.value.getModulePermissions(classId);
                rs = DBHelper.SqlServer.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
            }
            else
            {
                p = login.value.getColumnPermissions(classId);
                rs = DBHelper.SqlServer.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
            }
            if (!p.read)
            {
                returnValue.errNo = -1;
                returnValue.errMsg = "无权访问";
                return returnValue;
            }
            TableInfo table = new TableInfo(dataTypeId);

            List<FieldInfo> fieldList = table.fields.FindAll(delegate (FieldInfo v)
            {
                return v.visible || v.isNecessary;
            });
            string where = "";// " and A.orderid>-3";
            if (type[0] == '0') where += " and A.orderid<0 ";
            if (type[1] == '0') where += " and A.orderid<>-1 ";
            if (type[2] == '0') where += " and A.orderid<>-2 ";
            if (type[3] == '0') where += " and A.orderid<>-3 ";
            //else if (type == 2) where = " and A.orderid=-3 ";
            if (keyword != "")
            {
                switch (searchField)
                {
                    case "id":
                        where += " and A." + searchField + "=" + keyword + "";
                        break;
                    case "title":
                        where += " and A." + searchField + " like '%" + keyword + "%'";
                        break;
                    case "userId":
                        object userId = DBHelper.SqlServer.ExecuteScalar("select id from m_admin where uname=@uname", new SqlParameter[]{
                        new SqlParameter("uname",keyword)
                    });
                        if (userId != null)
                        {
                            where += " and A." + searchField + "=" + userId.ToString();
                        }
                        else
                        {
                            where += " and A.userId=-1 ";
                        }
                        break;
                    default:
                        where += " and ";
                        FieldInfo list = table.fields.Find(delegate (FieldInfo v)
                        {
                            if (v.name == searchField) return true;
                            else return false;
                        });
                        if (list.type == "String")
                        {
                            where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                            where += searchField + " like '%" + keyword + "%'";
                        }
                        else if (list.type == "DateTime")
                        {
                            string[] item = keyword.Split(',');
                            where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                            where += searchField + ">='" + item[0].ToString() + "' and " + searchField + "<='" + item[1].ToString() + "'";
                        }
                        else
                        {
                            string[] item = keyword.Split(',');
                            where += searchField.IndexOf("u_") == 0 ? "B." : "A.";
                            where += searchField + ">=" + item[0].ToString() + " and " + searchField + "<=" + item[1].ToString();
                        }
                        break;
                }
            }
            if (!p.audit) where += " and A.userId=" + login.value.id.ToString();
            ReturnPageData dataList = table.getDataList(moduleId, classId, pageNo, orderBy, sortDirection, where);
            object[] data = new object[] { fieldList, dataList };
            returnValue.userData = data;
            return returnValue;
        }
    }
}
