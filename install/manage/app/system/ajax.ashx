<%@ WebHandler Language="C#" Class="ajax" %>

using System;
using System.Web;

using System.Collections.Generic;
using MWMS;
using Helper;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Text;
using System.Web.Script.Serialization;
public class ajax : IHttpHandler {
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain"; 
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "exit") {
            login.exit();
            ErrInfo err = new ErrInfo(); 
            context.Response.Write(err.ToJson());
        } if (m == "saveCardLayout")
        {
            string layout = s_request.getString("layout");
            string path=context.Server.MapPath("~" + Config.tempPath + @"user\" + login.value.id.ToString() + @"\");
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(path + "cardLayout.config", layout);
            ErrInfo err = new ErrInfo(); 
            context.Response.Write(err.ToJson());
            
        } if (m == "cardPermissions")
        {
            context.Response.Write(login.value.isAdministrator);
            context.Response.End();
        }
        else if (m == "saveDisplayField")
        {
            ErrInfo err = new ErrInfo();
            double id = s_request.getDouble("id");
            string fields = s_request.getString("fields");
            Sql.ExecuteNonQuery("update datatype set displayField=@fields where id=@id",new SqlParameter[]{
                new SqlParameter("id",id),
                new SqlParameter("fields",fields)
            });
            context.Response.Write(err.ToJson());
            context.Response.End();
            
        }
        else if (m == "fieldInfo")
        {
            ErrInfo err = new ErrInfo();
            double id = s_request.getDouble("id");
            TableInfo t = new TableInfo(id);
            err.userData = t.fields;
            context.Response.Write(err.ToJson());
            context.Response.End();
        }
        else if (m == "getCustomField")
        {
            ErrInfo err = new ErrInfo();
            double classId = s_request.getDouble("classId");
            object moduleId = Sql.ExecuteScalar("select moduleId from class where id=@id", new SqlParameter[] { new SqlParameter("id", classId) });
            if (moduleId != null)
            {
                object custom = Sql.ExecuteScalar("select custom from module where id=@id", new SqlParameter[] { new SqlParameter("id", moduleId) });
                string xml = "";
                if (custom != null) xml = custom.ToString();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<?xml version=\"1.0\"?>" + xml);
                context.Response.Write("{\"errNo\":0,\"errMsg\":\"\",\"userData\":" + doc.ToJson() + "}");
            }
        }
        else if (m == "getUpdateLog")
        {
            ErrInfo err = new ErrInfo();
            string dateTime = s_request.getString("dateTime");
            string xml = Http.getUrl("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/Updatelog.aspx?datetime=" + dateTime, Encoding.UTF8);
            XmlDocument reg = new XmlDocument();
            reg.LoadXml(xml);

            JavaScriptSerializer ser = new JavaScriptSerializer();
            err.userData = ser.DeserializeObject(reg.ToJson());
            context.Response.Write(err.ToJson());

        }
        else if (m == "getUpdateFile")
        {
            ErrInfo err = new ErrInfo();
            string dateTime = s_request.getString("dateTime");
            string xml = Http.getUrl("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/UpdateXML.aspx?datetime=" + dateTime, Encoding.UTF8);
            XmlDocument reg = new XmlDocument();
            reg.LoadXml(xml);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            err.userData = ser.DeserializeObject(reg.ToJson());
            context.Response.Write(err.ToJson());

        }
        else if (m == "downUpdateFile")
        {
            ErrInfo err = new ErrInfo();
            string fileName = s_request.getString("fileName");
            string tempPath = context.Server.MapPath("~" + Config.tempPath);
            if (!System.IO.Directory.Exists(tempPath)) System.IO.Directory.CreateDirectory(tempPath);
            try
            {
                Http.saveFile("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/manage/files/" + fileName + ".zip?" + API.GetId(), tempPath + fileName + ".zip");
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "updateSystem")
        {
            ErrInfo err = new ErrInfo();
            string tempPath = context.Server.MapPath("~" + Config.tempPath);
            string[] files = s_request.getString("files").Split(',');
            string[] fileIds = s_request.getString("fileIds").Split(',');
            string[] types = s_request.getString("types").Split(',');
            string dateTime = s_request.getString("dateTime");
            try
            {
                for (int i = 0; i < fileIds.Length; i++)
                {
                    if (fileIds[i].Trim() != "")
                    {
                        if (types[i] == "0")
                        {
                            files[i] = files[i].Replace("[manage]", Config.managePath);
                            System.IO.FileInfo f= new System.IO.FileInfo(context.Server.MapPath("~" + files[i]));
                            if (!f.Directory.Exists) f.Directory.Create();
                            System.IO.File.Copy(tempPath + fileIds[i] + ".zip", f.FullName, true);
                            System.IO.File.Delete(tempPath + fileIds[i] + ".zip");
                        }
                        else
                        {
                            string text = System.IO.File.ReadAllText(tempPath + fileIds[i] + ".zip");
                            try
                            {
                                Sql.ExecuteNonQuery(text);
                            }
                            catch
                            {
                            }
                            System.IO.File.Delete(tempPath + fileIds[i] + ".zip");
                        }
                    }
                }
                #region 更新系统版本
                if (dateTime != "")
                {
                    string fileName = HttpContext.Current.Server.MapPath("~" + Config.configPath + "Version.xml");
                    XmlDocument doc2 = new XmlDocument();
                    doc2.Load(fileName);
                    doc2.DocumentElement.ChildNodes.Item(1).InnerText = dateTime;
                    doc2.DocumentElement.ChildNodes.Item(2).InnerText = "false";
                    doc2.Save(fileName);
                }
                #endregion
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());

        }

        else if (m == "checkUpdate")
        {
            ErrInfo err = new ErrInfo();
            string fileName = HttpContext.Current.Server.MapPath("~" + Config.configPath + "Version.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            string dateTime = doc.DocumentElement.ChildNodes.Item(1).InnerText;
            string flag = doc.DocumentElement.ChildNodes.Item(2).InnerText;
            if (flag == "true")
            {
                err.userData = dateTime.Trim();
                context.Response.Write(err.ToJson());
                context.Response.End();
            }
            try
            {
                XmlDocument doc2 = new XmlDocument();
                doc2.Load("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/updatexml.aspx?datetime=" + dateTime);
                XmlNodeList root2 = doc2.DocumentElement.ChildNodes;
                if (root2.Count > 0)
                {
                    doc.DocumentElement.ChildNodes.Item(2).InnerText = "true";
                    doc.Save(fileName);
                    err.userData = dateTime.Trim();
                }
                else
                {
                    err.userData = "";
                }
            }
            catch (Exception ex)
            {
                err.errNo = -1;
                err.errMsg = "检查升级失败，" + ex.Message;
            }
            context.Response.Write(err.ToJson());

        }
        else if (m == "getUserList")
        {
            ErrInfo info = new ErrInfo();
            int type = s_request.getInt("type");
            if (type == 0)
            {
                info.userData = Sql.ExecuteArray("select id,1 type,name from role UNION " +
                    "select id,2 type,uname name from m_admin where status=1");
            }
            else if (type == 1)
            {
                info.userData = Sql.ExecuteArray("select id,1 type,name from role");
            }
            else if (type == 2)
            {
                info.userData = Sql.ExecuteArray("select id,2 type,uname name from m_admin where status=1");
            }
            context.Response.Write(info.ToJson());
        }
        else if (m == "getPermissions")
        {
            ErrInfo info = new ErrInfo();
            double classId = s_request.getDouble("classId");
            info.userData = Sql.ExecuteArrayObj("select A.dataId,A.type,B.name,p0,p1,p2,p3,p4 from permissions A inner join role B  on A.dataId=B.id where A.classId=@classId " +
            " union " +
            " select A.dataId,A.type,B.uname name,p0,p1,p2,p3,p4 from permissions A inner join m_admin B  on A.dataId=B.id where A.classId=@classId",
            new SqlParameter[]{
                new SqlParameter("classId",classId)
            });
            context.Response.Write(info.ToJson());
        }
        else if (m == "setPermissions")
        {
            ErrInfo info = new ErrInfo();
            double classId = s_request.getDouble("classId");
            string[] permissions = s_request.getString("permissions").Split('\n');
            SqlParameter[] p = new SqlParameter[]{
                new SqlParameter("classId",classId)
            };
            Sql.ExecuteNonQuery("delete from permissions where classId=@classId", p);
            for (int i = 0; i < permissions.Length; i++)
            {
                if (permissions[i] != "")
                {
                    string[] item = permissions[i].Split(',');
                    Sql.ExecuteNonQuery("insert into permissions (classId,type,dataId,p0,p1,p2,p3,p4)values(@classId,@type,@dataId,@p0,@p1,@p2,@p3,@p4)",
                            new SqlParameter[]{
                    new SqlParameter("classId",classId),
                    new SqlParameter("type",item[0]),
                    new SqlParameter("dataId",item[1]),
                    new SqlParameter("p0",item[2]),
                    new SqlParameter("p1",item[3]),
                    new SqlParameter("p2",item[4]),
                    new SqlParameter("p3",item[5]),
                    new SqlParameter("p4",item[6])
                    }
                        );
                }

            }
            context.Response.Write(info.ToJson());
        }
        else if (m == "setIcon")
        {
            ErrInfo info = new ErrInfo();
            info = UserClass.setIcon(s_request.getString("icon"), login.value);
            context.Response.Write(info.ToJson());
        }
        else if (m == "columnSorting")
        {
            ErrInfo info = new ErrInfo();
            double classId = s_request.getDouble("classId");
            string ids = s_request.getString("ids");
            string[] id = ids.Split(',');
            for (int i = 0; i < id.Length; i++)
            {
                Sql.ExecuteNonQuery("update class set orderid=@orderid,classId=@classId   where id=@id",
                new SqlParameter[]{
                    new SqlParameter("orderid",i),
                    new SqlParameter("id",double.Parse(id[i])),
                    new SqlParameter("classId",classId)
            });
            }
            context.Response.Write(info.ToJson());
        }

        else if (m == "moduleDel")
        {
            ErrInfo info = new ErrInfo();
            double moduleId = s_request.getDouble("moduleId");
            double classId = s_request.getDouble("classId");
            int type = s_request.getInt("type");
            Permissions p = login.value.getModulePermissions(moduleId);
            if (!p.all)
            {
                info.errNo = -1;
                info.errMsg = "没有删除该模块的权限";
                context.Response.Write(info.ToJson());
                return;
            }
            if (type == 0)
            {
                Sql.ExecuteNonQuery("delete from module where id=@moduleId", new SqlParameter[]{
                new SqlParameter("moduleId",moduleId)
                });
                if (moduleId == classId)
                {
                    Sql.ExecuteNonQuery("delete from class where id=@classId", new SqlParameter[]{
                new SqlParameter("classId",classId)
                });
                }
                info.userData = Sql.ExecuteArrayObj("select id from class where moduleId=@moduleId and classId=@classId", new SqlParameter[]{
                new SqlParameter("moduleId",moduleId),
                new SqlParameter("classId",classId)
                });
            }
            else
            {
                Sql.ExecuteNonQuery("delete from class where id=@classId", new SqlParameter[]{
                new SqlParameter("classId",classId)
                });
            }
            context.Response.Write(info.ToJson());
        }
        else if (m == "resetContent")
        {
            ErrInfo info = new ErrInfo();
            double id = s_request.getDouble("id");
            info = ColumnClass.resetContentUrl(id);
            context.Response.Write(info.ToJson());
        }
        else if (m == "resetColumn")
        {
            ErrInfo info = new ErrInfo();
            double id = s_request.getDouble("id");
            ColumnClass.reset(id);
            string childId = ColumnClass.getChildId(id);
            info.userData = childId;
            context.Response.Write(info.ToJson());
        }
        else if (m == "moduleList")
        {
            ErrInfo info = new ErrInfo();
            string sql = "";
            if (login.value.isAccess)
            {
                sql = "select A.id value,A.moduleName text,A.saveDataType ,A.type,sum(P0) p0,sum(P1)  p1,sum(P2) p2,sum(P3) p3 from module A left join permissions B on A.id=B.classId group by A.id,A.moduleName,A.saveDataType,A.type";
            }
            else
            {
                sql = "select A.id value,A.moduleName text,A.saveDataType ,A.type,sum(P0) p0,sum(P1)  p1,sum(P2) p2,sum(P3) p3 from module A inner join permissions B on A.id=B.classId group by A.id,A.moduleName,A.saveDataType,A.type";
            }
            ArrayList arrayList = new ArrayList();
            SqlDataReader rs = Sql.ExecuteReader(sql);
            while (rs.Read())
            {
                Permissions p = new Permissions(login.value);

                bool p0 = (rs[4] != System.DBNull.Value && rs.GetInt32(4) > 0);
                bool p1 = (rs[5] != System.DBNull.Value && rs.GetInt32(5) > 0);
                bool p2 = (rs[6] != System.DBNull.Value && rs.GetInt32(6) > 0);
                bool p3 = (rs[7] != System.DBNull.Value && rs.GetInt32(7) > 0);
                p.read = true;
                p.write |= p0;
                p.delete |= p0;
                p.audit |= p0;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                for (int i = 0; i < 4; i++)
                {
                    dictionary.Add(rs.GetName(i), rs[i]);
                }
                dictionary.Add("permissions", p);
                arrayList.Add(dictionary);
            }
            rs.Close();
            info.userData = arrayList;
            context.Response.Write(info.ToJson());
        }
        else if (m == "columnList")
        {
            ErrInfo info = new ErrInfo();
            double moduleId = s_request.getDouble("moduleId");
            double classId = s_request.getDouble("classId");
            if (moduleId == -1 && classId == 7)
            {
                //获取共享栏目
                ArrayList arrayList = new ArrayList();
                string role = login.value.id.ToString();
                if (login.value.role != "") role += "," + login.value.role;
                SqlDataReader rs = Sql.ExecuteReader("select distinct B.id,B.classId,B.className text,moduleId,case when B.orderid<0 then 'M4_Del' end  class,B.saveDataType dataTypeId,A.p0,A.p1,A.p2,A.p3 from permissions A inner join class B on A.classId=B.id where  A.dataId in (" + role + ") ");
                while (rs.Read())
                {
                    Permissions p = new Permissions(login.value);

                    bool p0 = (rs[6] != System.DBNull.Value && rs.GetInt32(6) > 0);
                    bool p1 = (rs[7] != System.DBNull.Value && rs.GetInt32(7) > 0);
                    bool p2 = (rs[8] != System.DBNull.Value && rs.GetInt32(8) > 0);
                    bool p3 = (rs[9] != System.DBNull.Value && rs.GetInt32(9) > 0);
                    p.read = true;
                    p.write |= p0;
                    p.delete |= p0;
                    p.audit |= p0;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    for (int i = 0; i < 6; i++)
                    {
                        dictionary.Add(rs.GetName(i), rs[i]);
                    }
                    dictionary.Add("permissions", p);
                    arrayList.Add(dictionary);

                }
                rs.Close();
                info.userData = arrayList;
            }
            else
            {
                Permissions p = null;
                //获取所选栏目或模块的权限
                if (classId == 7) p = login.value.getModulePermissions(moduleId);
                else
                {
                    p = login.value.getColumnPermissions(classId);
                }
                //如果有该栏目的读取权限时列出子栏目
                if (p.read)
                {
                    ArrayList list = Sql.ExecuteArray("select id,classId,className text,moduleId,case when orderid<0 then 'M4_Del' end  class,saveDataType dataTypeId from class where  moduleId=@moduleId and classId=@classId order by orderid",
                        new SqlParameter[]{
                        new SqlParameter("moduleId",moduleId),
                        new SqlParameter("classId",classId)
                });
                    for (int i = 0; i < list.Count; i++)
                    {
                        Dictionary<string, object> obj = (Dictionary<string, object>)list[i];
                        obj.Add("permissions", p);
                    }
                    info.userData = list;
                }
            }
            context.Response.Write(info.ToJson());

        }
        else if (m == "columnDel")
        {
            ErrInfo info = new ErrInfo();
            double classId = s_request.getDouble("classId");
            //ColumnInfo value = ColumnClass.get(classId);
            //Permissions p = login.value.getColumnPermissions(value.id);
            //if (!p.all)
            //{
            //    info.errNo = -1;
            //    info.errMsg = "没有删除该栏目的权限";
            //    context.Response.Write(info.ToJson());
            //    return;
            //}
            context.Response.Write(ColumnClass.del(classId, login.value).ToJson());
        }
        else if (m == "templateList")
        {
            double classId = s_request.getDouble("classId");
            int type = s_request.getInt("type");
            ColumnInfo info = ColumnClass.get(classId);
            ErrInfo err = new ErrInfo();
            if (info != null)
            {
                err.userData = info.getTemplateList(type);
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "栏目不存在";
            }
            context.Response.Write(err.ToJson());
        }
        else if (m == "dataTypeList")
        {
            ErrInfo err = new ErrInfo();
            err.userData = Sql.ExecuteArray("select id value,datatype text from datatype where id>100 and classid=16");
            context.Response.Write(err.ToJson());
        }
        else if (m == "columnMove")
        {
            ErrInfo err = ColumnClass.move(s_request.getDouble("id"), s_request.getDouble("moduleId"), s_request.getDouble("classId"),login.value);
            context.Response.Write(err.ToJson());
        }
        else if (m == "dataList") dataList(context);
        else if (m == "delData") delData(context);
        else if (m == "setTop") setTop(context);
        else if (m == "setAttr") setAttr(context);
        else if (m == "getConfig") getConfig(context);
        else if (m == "saveConfig") saveConfig(context);
        else if (m == "moveData") moveData(context);
        
    }
    void saveConfig(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        if (!login.value.isAdministrator)
        {
            err.errNo = -1;
            err.errMsg = "无权访问";
            context.Response.Write(err.ToJson());
            return;
        }
        try
        {
            string configFile = s_request.getString("_configFile");
            string filePath = HttpContext.Current.Server.MapPath("~" + Config.configPath + configFile);
            string xml = API.GetFileText(filePath);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            for (int i = 1; i < context.Request.Form.Count; i++)
            {
                string name = context.Request.Form.AllKeys[i];
                XmlNode node = doc.SelectSingleNode("/*/item[@name='" + name + "']");
                if (node != null)
                {
                    if (name == "grid")
                    {
                        node.InnerXml = context.Request.Form[i].ToString();
                    }
                    else
                    {
                        node.InnerText = context.Request.Form[i].ToString();
                    }
                }
            }
            doc.Save(filePath);
            Config.loadUserConfig(configFile);
        }
        catch (Exception ex)
        {
            err.errNo = -1;
            err.errMsg = ex.Message;
        }
        context.Response.Write(err.ToJson());
    }
    void getConfig(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        if (!login.value.isAdministrator)
        {
            err.errNo = -1;
            err.errMsg = "无权访问";
            context.Response.Write(err.ToJson());
            return;
        }
        string xml = API.GetFileText(HttpContext.Current.Server.MapPath("~" + Config.configPath + context.Request.Form["file"].ToString()));
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        context.Response.Write("{\"errNo\":0,\"errMsg\":\"\",\"userData\":" + API.XmlToJSON(doc) + "}");
    }
    void setAttr(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string ids = context.Request.Form["ids"].ToString();
        int type = s_request.getInt("type");
        int flag = s_request.getInt("flag");
        info = TableInfo.setAttr(ids,type, flag == 1);
        context.Response.Write(info.ToJson());
    }
    void moveData(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string ids = context.Request.Form["ids"].ToString();
        double classId = s_request.getDouble("classId");
        info = TableInfo.moveData(ids, classId);
        context.Response.Write(info.ToJson());
    }
    void setTop(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string ids = context.Request.Form["ids"].ToString();
        int flag = s_request.getInt("flag");
        info = TableInfo.setTop(ids, flag==1);
        context.Response.Write(info.ToJson());
    }
    void delData(HttpContext context)
    {
        double dataTypeId = -1;
        string ids = context.Request.Form["ids"].ToString();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int tag =int.Parse(context.Request.Form["tag"].ToString());
        Permissions p = null;
        if (classId < 8)
        {
            SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
            p = login.value.getModulePermissions(moduleId);
            
        }
        else
        {
            SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
            p = login.value.getColumnPermissions(classId);
        }
        ErrInfo info = new ErrInfo();
        if (p.delete)
        {
            if (p.audit)
            {
                info = TableInfo.delData(dataTypeId, ids, tag == 1, login.value);
            }
            else
            {
                info = TableInfo.delData(dataTypeId, ids, tag == 1, login.value);
            }
        }
        else{
            info.errNo = -1;
            info.errMsg = "权限不足";
        }
        context.Response.Write(info.ToJson());
    }
    void dataList(HttpContext context)
    {
        ErrInfo err = new ErrInfo();
        double moduleId = s_request.getDouble("moduleId");
        double classId = s_request.getDouble("classId");
        int pageNo =s_request.getInt("pageNo");
        string orderBy =s_request.getString("orderBy");
        int sortDirection = s_request.getInt("sortDirection");
        int type = s_request.getInt("type");
        string searchField = s_request.getString("searchField");
        string keyword = s_request.getString("keyword");
        double dataTypeId = -1;
        SqlDataReader rs = null;
        Permissions p = null;
        if (moduleId == classId)
        {
            p = login.value.getModulePermissions(classId);
            rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
        }
        else
        {
            p = login.value.getColumnPermissions(classId);
            rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
            if (rs.Read()) dataTypeId = rs.GetDouble(0);
            rs.Close();
        }
        if (!p.read)
        {
            err.errNo = -1;
            err.errMsg = "无权访问";
            context.Response.Write(err.ToJson());
            return;
        }
        TableInfo table = new TableInfo(dataTypeId);
        List<FieldInfo> fieldList = table.fields.FindAll(delegate(FieldInfo v)
        {
            return v.visible;
        });
        string where = " and A.orderid>-1";
        if (type == 1) where = " and A.orderid=-1 ";
        else if (type == 2) where = " and A.orderid=-2 ";
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
                    object userId = Sql.ExecuteScalar("select id from m_admin where uname=@uname", new SqlParameter[]{
                        new SqlParameter("uname",keyword)
                    });
                    if (userId != null)
                    {
                        where += " and A." + searchField + "="+userId.ToString();
                    }
                    break;
            }
        }
        if (!p.audit) where += " and A.userId="+login.value.id.ToString();
        ReturnPageData dataList = table.getDataList(moduleId,classId, pageNo, orderBy, sortDirection, where);
        object[] data = new object[] { fieldList, dataList };
        err.userData = data;
        context.Response.Write(err.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}