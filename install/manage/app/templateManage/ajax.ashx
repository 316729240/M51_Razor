﻿<%@ WebHandler Language="C#" Class="ajax"%>
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
using System.Text.RegularExpressions;
public class ajax : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    int webFAId = 0;//默认网站模板方案
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain"; 
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "templateList") templateList(context);
        else if (m == "readTemplateLable") readTemplateLable(context);
        else if (m == "readTemplateViewClass") readTemplateViewClass(context);
        else if (m == "readTemplateView") readTemplateView(context);
        else if (m == "saveTemplate") saveTemplate(context);
        else if (m == "readTemplate") readTemplate(context);
        else if (m == "delTemplate") delTemplate(context);
        else if (m == "backupTemplate") backupTemplate(context);
        else if (m == "readView") readView(context);
        else if (m == "saveView") saveView(context);
        else if (m == "readDataTypeLable") readDataTypeLable(context);
        else if (m == "readSqlLable") readSqlLable(context);
        else if (m == "delViewClass") delViewClass(context);
        else if (m == "addViewClass") addViewClass(context);
        else if (m == "delView") delView(context);
        else if (m == "readBackupList") readBackupList(context);
        else if (m == "readBackup") readBackup(context);
        else if (m == "find") find(context);
        else if (m == "replace") replace(context);
        else if (m == "locateTemplate") locateTemplate(context);
        else if (m == "setTemplateType") setTemplateType(context);
        else if (m == "getTemplateType") getTemplateType(context);
        
        
    }
    void getTemplateType(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string value = "1";
        System.IO.FileInfo f = new System.IO.FileInfo(context.Server.MapPath("~" + Config.tempPath + @"user\" + login.value.id.ToString() + @"\templateType.config"));
        if(f.Exists)value=System.IO.File.ReadAllText(f.FullName);
        info.userData = value;
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void setTemplateType(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string value = s_request.getString("value");
        System.IO.FileInfo f = new System.IO.FileInfo(context.Server.MapPath("~" + Config.tempPath + @"user\" + login.value.id.ToString() + @"\templateType.config"));
        if (!f.Directory.Exists) f.Directory.Create();
        System.IO.File.WriteAllText(f.FullName,value);
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    string _replaceUrl(Match m)
    {
        if (Regex.IsMatch(m.Value, "^default_", RegexOptions.IgnoreCase))
        {
            return "";
        }
        else
        {
            return Regex.Replace(m.Value, @"_((\d){1,5})", "");
        }
    }
    void locateTemplate(HttpContext context)
    {

        ErrInfo info = new ErrInfo();
        Uri url = new Uri(s_request.getString("url"));
        int u_webFAid = s_request.getInt("u_webFAid");
        //Regex r = new Regex(@"(?<=/)((.[^/]*)_((\d){1,5}))(." + BaseConfig.extension + ")", RegexOptions.IgnoreCase);
        //string newUrl = r.Replace(url.AbsolutePath.ToLower(), new MatchEvaluator(_replaceUrl));
        bool isMobilePage = false;
        string virtualWebDir = "";
        string newUrl=Rewrite.urlZhuanyi(url, ref isMobilePage, ref virtualWebDir);
        TemplateInfo v = TemplateClass.get(newUrl, isMobilePage);
        if ((u_webFAid == 1 ^ isMobilePage))
        {
            info.errNo = -1;
            info.errMsg = "查找的网址为" + (isMobilePage ? "手机页面" : "PC页面") + ",请选择" + (isMobilePage ? "手机" : "PC") + "模板再操作";
        }
        //ColumnInfo column = null;
        double moduleId = 0, classId=0;
        if (v == null)
        {
            info.errNo = -1;
            info.errMsg = "定位失败";
        }
        else
        {
            if (v.classId > 0)
            {
                object value = Sql.ExecuteScalar("select id from module where id=@id", new SqlParameter[] { new SqlParameter("id", v.classId) });
                if (value == null)
                {
                    IDataReader rs = Sql.ExecuteReader("select id,moduleId from class where id=@id", new SqlParameter[] { new SqlParameter("id", v.classId) });
                    if (rs.Read())
                    {
                        classId = v.classId;
                        moduleId = rs.GetDouble(1);
                    }
                    rs.Close();
                }
                else
                {
                    moduleId = (double)value;
                    classId = 1;
                }
            }
            object[] obj = new object[] { moduleId, classId, v.u_type, v.id };
            info.userData = obj;
        }
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void replace(HttpContext context)
    {

        ErrInfo info = new ErrInfo();
        if (!login.value.isAdministrator)
        {
            info.errNo = -1;
            info.errMsg = "没有权限";
            context.Response.Write(info.ToJson());
            return;
        }
        double id = s_request.getDouble("id");
        int type = s_request.getInt("type");
        int mbType = s_request.getInt("mbType");
        int u_webFAid = s_request.getInt("u_webFAid");
        string keyword = s_request.getString("keyword");
        if (keyword == "")
        {
            info.errMsg = "查询内容不能为空";
            info.errNo = -1;
            return;
        }
        string keyword2 = s_request.getString("keyword2");
        string sql = "select id,classid,title,u_content,u_datatypeid,u_type,0 from HtmlTemplate where  id=@id ", sql2 = "update HtmlTemplate set u_content=@content where id=@id";
        if (mbType == 1)
        {
            sql = "select B.id,B.classid,A.className+'.'+B.title,B.u_html,B.u_datatypeid,0,1 from dataview B inner join Class A on B.classId=A.id where B.id=@id ";
            sql2 = "update dataview set u_html=@content where id=@id";
        }
        SqlDataReader rs = Sql.ExecuteReader(sql, new SqlParameter[]{
            new SqlParameter("webFAid",u_webFAid),
            new SqlParameter("id",id)
        });
        MatchCollection mc;
        Regex r = null;
        if (rs.Read())
        {
            string content = rs.GetString(3);
            bool flag = false;
            if (type == 0)
            {
                flag = content.IndexOf(keyword) > -1;
                if (flag)content = content.Replace(keyword,keyword2);
            }
            else
            {
                r = new Regex(@keyword, RegexOptions.IgnoreCase); //定义一个Regex对象实例
                mc = r.Matches(content);
                flag = mc.Count > 0;
                if (flag)
                {
                    content=r.Replace(content, keyword2);
                }
            }
            if (flag)
            {
                Sql.ExecuteNonQuery(sql2, new SqlParameter[]{
                    new SqlParameter("content",content),
                    new SqlParameter("id",rs[0])
                });
            }
        }
        rs.Close();
        context.Response.Write(info.ToJson());
        context.Response.End();
        
    }
    void find(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        if (!login.value.isAdministrator)
        {
            info.errNo = -1;
            info.errMsg = "没有权限";
            context.Response.Write(info.ToJson());
            return;
        }
        string keyword = s_request.getString("keyword");
        if (keyword == "")
        {
            info.errMsg = "查询内容不能为空";
            info.errNo = -1;
            context.Response.Write(info.ToJson());
            context.Response.End();
        }
        int type = s_request.getInt("type");
        int u_webFAid = s_request.getInt("u_webFAid");
        List<object[]> data = new List<object[]>();
        SqlDataReader rs = Sql.ExecuteReader("select id,classid,title,u_content,u_datatypeid,u_type,0 from HtmlTemplate where   u_webFAid=@webFAid ", new SqlParameter[]{
            new SqlParameter("webFAid",u_webFAid)
        });
        MatchCollection mc;
        Regex r = null;
        while (rs.Read())
        {
            string content = rs.GetString(3);
            bool flag = false;
            if (type == 0)
            {
                flag=content.IndexOf(keyword)>-1;
            }
            else
            {
                r = new Regex(@keyword, RegexOptions.IgnoreCase); //定义一个Regex对象实例
                mc = r.Matches(content);
                flag=mc.Count > 0;
            }
            if (flag)
            {
                object[] value = new object[]{
                rs[0],rs[1],rs[4],rs[5],rs[6],rs[2]
                };
                data.Add(value);
            }            
        }
        rs.Close();
        rs = Sql.ExecuteReader("select B.id,B.classid,A.className+'.'+B.title,B.u_html,B.u_datatypeid,0,1 from dataview B inner join Class A on B.classId=A.id");
        while (rs.Read())
        {
            string content = rs.GetString(3);
            bool flag = false;
            if (type == 0)
            {
                flag = content.IndexOf(keyword) > -1;
            }
            else
            {
                mc = r.Matches(content);
                flag = mc.Count > 0;
            }
            if (flag)
            {
                object[] value = new object[]{
                rs[0],rs[1],rs[4],rs[5],rs[6],rs[2]
                };
                data.Add(value);
            }
        }
        rs.Close();
        info.userData = data;
        context.Response.Write(info.ToJson());
        context.Response.End();
        
    }
    void readBackup(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        info.userData = Sql.ExecuteScalar("select u_content from backupTemplate where id=@id", new SqlParameter[]{
            new SqlParameter("id",id )
        });
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void readBackupList(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double classId = s_request.getDouble("classId");
        int u_type = s_request.getInt("u_type");
        int u_defaultFlag = s_request.getInt("u_defaultFlag");
        double u_datatypeId = s_request.getDouble("u_datatypeId");
        string title = s_request.getString("title");
        int u_webFAid = s_request.getInt("u_webFAid");
        string where = "";
        if (u_defaultFlag == 0) where = " and title=@title";
        info.userData = Sql.ExecuteArrayObj("select id,updateDate,userName from backupTemplate where classid=@classid and u_type=@u_type and u_webFAid=@u_webFAid and u_defaultFlag=@u_defaultFlag and u_datatypeId=@u_datatypeId " + where + "  order by updatedate desc", new SqlParameter[]{
            new SqlParameter("classid",classId ),
            new SqlParameter("u_type",u_type),
            new SqlParameter("u_webFAid",u_webFAid),
            new SqlParameter("u_defaultFlag",u_defaultFlag),
            new SqlParameter("u_datatypeId",u_datatypeId),
            new SqlParameter("title",title)
        });
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void delView(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string ids = s_request.getString("ids");
        Sql.ExecuteNonQuery("delete from dataview where id in (" + ids + ")");
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void addViewClass(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id =double.Parse(API.GetId());
        double classId = 12;
        string className = s_request.getString("className");
        object obj=Sql.ExecuteScalar("select id from class where className=@className", new SqlParameter[] { new SqlParameter("className", className) });
        if (obj != null)
        {
            info.errNo = -1;
            info.errMsg = "视图已存在";
        }
        else
        {
            Sql.ExecuteNonQuery("insert into class (id,classId,className)values(@id,@classId,@className)", new SqlParameter[] { 
            new SqlParameter("id", id) ,
            new SqlParameter("classId", classId) ,
            new SqlParameter("className", className) }
                );
        }
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void delViewClass(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        Sql.ExecuteNonQuery("delete from class where id=@id", new SqlParameter[] { new SqlParameter("id", id) });
        Sql.ExecuteNonQuery("delete from dataview where classid=@id", new SqlParameter[] { new SqlParameter("id", id) });
        context.Response.Write(info.ToJson());
        context.Response.End();
    }

    void readSqlLable(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string sql = s_request.getString("sql");
        if (sql.IndexOf(" top ") == -1) sql = sql.Replace("select ","select top 1 ");
        IDataReader rs = Sql.ExecuteReader(sql);
        object [] field = new object[rs.FieldCount];
        for (int i = 0; i < rs.FieldCount; i++)
        {
            string[] f = new string[] { rs.GetName(i), rs.GetName(i) };
            field[i] =  f;
        }
        rs.Close();
        info.userData = field;
        context.Response.Write(info.ToJson());
        context.Response.End();
    }

    void readDataTypeLable(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double dataTypeId = s_request.getDouble("dataTypeId");
        info.userData = new TableInfo(dataTypeId);
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    //保存模板
    //参数1:模板ID
    void saveView(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        double classId = s_request.getDouble("classId");
        int u_viewType = s_request.getInt("u_viewType");
        string title = s_request.getString("title");
        string u_html = s_request.getString("u_html");
        int u_editboxStatus = s_request.getInt("u_editboxStatus");
        double u_datatypeId = s_request.getDouble("u_datatypeId");
        string u_parameterValue = s_request.getString("u_parameterValue");
        if (!login.value.isAdministrator)
        {
            info.errNo = -1;
            info.errMsg = "没有权限";
            context.Response.Write(info.ToJson());
            return;
        }
        //-------------------保存模板----------------------------
        SqlParameter[] p = new SqlParameter[]{
                    new SqlParameter("id",id),
                    new SqlParameter("classId",classId),
                    new SqlParameter("u_viewType",u_viewType),
                    new SqlParameter("title",title),
                    new SqlParameter("u_pinyin",chs2py.convert(title,'0')),
                    new SqlParameter("u_html",u_html),
                    new SqlParameter("u_editboxStatus",u_editboxStatus),
                    new SqlParameter("u_datatypeId",u_datatypeId),
                    new SqlParameter("u_parameterValue",u_parameterValue),
                    new SqlParameter("createDate",System.DateTime.Now),
                    new SqlParameter("updateDate",System.DateTime.Now)
                };
        if (id <1)
        {
            id = double.Parse(API.GetId());
            p[0].Value = id;
            Sql.ExecuteNonQuery("insert into dataview (id,classId,u_viewType,title,u_pinyin,u_html,u_editboxStatus,u_datatypeId,u_parameterValue,createDate,updateDate)values(@id,@classId,@u_viewType,@title,@u_pinyin,@u_html,@u_editboxStatus,@u_datatypeId,@u_parameterValue,@createDate,@updateDate)", p);
            
        }else{
            Sql.ExecuteNonQuery("update dataview set updateDate=@updateDate,classId=@classId,u_viewType=@u_viewType,title=@title,u_pinyin=@u_pinyin,u_html=@u_html,u_editboxStatus=@u_editboxStatus,u_datatypeId=@u_datatypeId,u_parameterValue=@u_parameterValue where id=@id", p);
        }
        info.userData = id;
        if (info.errNo > -1)
        {
            string className = "";
            SqlDataReader rs=Sql.ExecuteReader("select classname from class where id=@id",new SqlParameter[]{new SqlParameter("id",classId)});
            if (rs.Read())
            {
                className = rs[0].ToString();
            }
            rs.Close();
            Dictionary<string, object> list=null;
            if (Config.viewVariables.ContainsKey(className))
            {
                list = (Dictionary<string, object>)Config.viewVariables[className];
            }
            else
            {
                list= new Dictionary<string, object>();
                Config.viewVariables[className] =list;
            }
            list[title] = u_html;
        }
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    //读取模板
    //参数1:模板ID
    void readView(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        info.userData = Helper.Sql.ExecuteDictionary("select B.title,B.u_html,B.u_editboxStatus,B.u_parameterValue,B.u_viewType,B.u_datatypeId,B.classId from dataView B where B.id=@id",
            new SqlParameter[]{
                    new SqlParameter("id",id)
                });
        context.Response.Write(info.ToJson());
    }
    //备份模板
    void backupTemplate(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        TemplateClass.backupTemplate(id, login.value.username);
        context.Response.Write(info.ToJson());
    }
    //删除模板
    //参数1:模板ID
    void delTemplate(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        info = TemplateClass.del(id, login.value);
        //double classId = s_request.getDouble("classId");
        //int typeId = s_request.getInt("typeId");
        //double datatypeId = s_request.getDouble("datatypeId");
        //if (id > 0)
        //{
        //    Helper.Sql.ExecuteNonQuery("delete from HtmlTemplate where id=@id",
        //           new SqlParameter[]{
        //            new SqlParameter("id",id)
        //        });
        //    info.userData = Helper.Sql.ExecuteNonQuery("delete from HtmlTemplate where id=@id",
        //        new SqlParameter[]{
        //            new SqlParameter("id",id)
        //        });
        //}
        //else
        //{
        //    info.userData = Helper.Sql.ExecuteNonQuery("delete from HtmlTemplate where ClassID=@classId and u_datatypeid=@datatypeId and u_type=@typeId and u_webFAid=@webFAid",
        //        new SqlParameter[]{
        //            new SqlParameter("classId",classId),
        //            new SqlParameter("datatypeid",datatypeId),
        //            new SqlParameter("typeId",typeId),
        //            new SqlParameter("webFAid" ,API.getWebFAId()?1:0),
        //        });
        //}
        context.Response.Write(info.ToJson());
    }
    //读取模板
    //参数1:模板ID
    void readTemplate(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double id = s_request.getDouble("id");
        double classId = s_request.getDouble("classId");
        int typeId = s_request.getInt("typeId");
        double datatypeId = s_request.getDouble("datatypeId");
        int defaultFlag = s_request.getInt("defaultFlag");
        string title = s_request.getString("title");
        int u_webFAid = s_request.getInt("u_webFAid");
        if (id > 0)
        {
            info.userData = TemplateClass.get(id);
        }
        else
        {
            info.userData = TemplateClass.get(classId, typeId, datatypeId, defaultFlag == 1, title, u_webFAid==1);

        }
        context.Response.Write(info.ToJson());
    }
    /// <summary>
    /// 保存模板
    /// </summary>
    /// <param name="context"></param>
    void saveTemplate(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        TemplateInfo value = new TemplateInfo();
        value.id = s_request.getDouble("id");
        value.title = s_request.getString("title");
        value.u_content = s_request.getString("u_content");
        value.u_type= s_request.getInt("u_typeId");
        value.u_defaultFlag = s_request.getInt("u_defaultFlag");
        value.classId = s_request.getDouble("classId");
        value.u_datatypeId = s_request.getDouble("u_datatypeId");
        value.u_editboxStatus = s_request.getInt("u_editboxStatus");
        value.u_parameterValue = s_request.getString("u_parameterValue");
        value.u_webFAid = s_request.getInt("u_webFAid");
        info = TemplateClass.edit(value, login.value);
        if (info.errNo < 0)
        {
            context.Response.Write(info.ToJson());
            return;
        }
        int count = 0;
        IDataReader rs = Sql.ExecuteReader("select count(1) from backupTemplate where classid=@classid and u_type=@u_type and u_webFAid=@u_webFAid and u_defaultFlag=@u_defaultFlag and u_datatypeId=@u_datatypeId and title=@title and  getdate()<DATEADD(minute,200,updatedate)", new SqlParameter[]{
            new SqlParameter("classid",value.classId ),
            new SqlParameter("u_type",value.u_type),
            new SqlParameter("u_webFAid",value.u_webFAid),
            new SqlParameter("u_defaultFlag",value.u_defaultFlag),
            new SqlParameter("u_datatypeId",value.u_datatypeId),
            new SqlParameter("title",value.title)
        });
        if (rs.Read()) count = rs.GetInt32(0);
        rs.Close();
        if (count == 0) TemplateClass.backupTemplate((double)info.userData, login.value.username);
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void readTemplateView(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double classId = s_request.getDouble("classId");
        info.userData = Sql.ExecuteArray("select B.id,B.title text,6 type,B.u_pinyin from dataview B  where B.classid=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void readTemplateViewClass(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        info.userData=Sql.ExecuteArray("select id,className text,5 type from class where classid=12");
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void readTemplateLable(HttpContext context)
    {
        double dataTypeId =  s_request.getDouble("dataTypeId");
        ErrInfo info = new ErrInfo();
        Dictionary<string, object> value = new Dictionary<string, object>();
        XmlNodeList list = Config.loadFile("systemVariables.config");
        Dictionary<string, string[]> systemVariables = new Dictionary<string, string[]>();
        for (int i = 0; i < list.Count; i++)
        {
            systemVariables.Add(list[i].Attributes["name"].Value, new string[] { list[i].Attributes["labelText"].Value, list[i].InnerText });
        }
        value.Add("systemVariables",systemVariables);
        if (dataTypeId > -1)
        {
            TableInfo table = new TableInfo(dataTypeId);
            value.Add("pageVariables",table.fields);
        }
        //Constant.systemVariables
        //"{\"systemVariables\": Constant.systemVariables.ToJson}
        info.userData = value;
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    void templateList(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        double moduleId =s_request.getDouble("moduleId");
        double faId = s_request.getDouble("faId");
        int typeId = s_request.getInt("typeId");
        double dataTypeID = s_request.getDouble("dataTypeId");
        int u_webFAid = s_request.getInt("u_webFAid");
        SqlDataReader rs = null;
        List<object[]> list = new List<object[]>();
        string[] templateRange = new string[] { "全站", "模块", "频道" };
        //ArrayList list = new ArrayList();
        /*
        rs = Helper.Sql.ExecuteReader("select type from module where id=" + ModuleID);
        if (rs.Read())
        {
            if (rs[0].ToString() == "False")//虚拟目录
            {
                ModuleType = false;
            }
        }
        rs.Close();*/
        if (typeId == 0)
        {
            list.Add(new object[] { 0, "全站", "站点通用模板", "", 1, 0,0 });
            if(moduleId>0)list.Add(new object[] { 0, "模块", "模块通用模板", "", 1, moduleId, 0 });
            if (moduleId>0 && faId != moduleId) list.Add(new object[] { 0, "频道", "频道页模板", "", 1, faId, 0 });
            rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid from  HtmlTemplate B  where B.u_type=0 and B.u_datatypeid=0 and B.u_defaultFlag=1 and B.u_webFAId=@webFAId and B.classid in (0,@moduleId,@id)",
                new SqlParameter[] { 
                    new SqlParameter("id", faId),
                    new SqlParameter("moduleId", moduleId),
                    new SqlParameter("webFAId",u_webFAid)
                });
            int index = 0;
            while (rs.Read())
            {
                if (rs.GetDouble(2) == 0) index = 0;
                else if (rs.GetDouble(2) == moduleId) index = 1;
                else if (rs.GetDouble(2) == faId) index = 2;
                list[index][0] = rs.GetDouble(0);
                list[index][3] = rs.GetDateTime(1).ToString();
                list[index][5] = rs.GetDouble(2);
            }
            rs.Close();
        }
        else if (typeId == 1)
        {
            list.Add(new object[] { 0, "全站", "站点通用模板", "", 1, 0, 0 });
            if(moduleId>0)list.Add(new object[] { 0, "模块", "模块通用模板", "", 1, moduleId, 0 });
            if (faId != moduleId) list.Add(new object[] { 0, "频道", "频道页模板", "", 1, faId, 0 });
            rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.u_defaultFlag,B.title from  HtmlTemplate B  where B.u_type=1 and B.u_datatypeid=0 and B.u_webFAId=@webFAId and B.classid in (0,@moduleId,@id) order by B.classid ",
                new SqlParameter[] { 
                    new SqlParameter("id", faId),
                    new SqlParameter("moduleId", moduleId),
                    new SqlParameter("webFAId",u_webFAid)
                });
            int index = 0;
            while (rs.Read())
            {
                bool defaultFlag = rs.GetBoolean(3);
                if (rs.GetDouble(2) == 0 ) index = 0;
                else if (rs.GetDouble(2) == moduleId) index = 1;
                else if (rs.GetDouble(2) == faId ) index = 2;

                if (defaultFlag)
                {
                    list[index][0] = rs.GetDouble(0);
                    list[index][3] = rs.GetDateTime(1).ToString();
                    list[index][5] = rs.GetDouble(2);
                }
                else
                {
                    list.Add(new object[] { rs[0], templateRange[index], rs[4], rs.GetDateTime(1).ToString(), rs.GetDouble(2),0 });
                }
            }
            rs.Close();
        }
        else if (typeId ==2)
        {
            List<object[]> datatypeList = new List<object[]>();
            string sql = "select SaveDataType from class where moduleid=@moduleId group by SaveDataType";
            if (moduleId == 0)sql = "select SaveDataType from class where moduleId in (select id from module)";

            rs= Sql.ExecuteReader("select id,datatype from datatype where id in ("+sql+")",
            new SqlParameter[] { 
                    new SqlParameter("moduleId", moduleId)
                });
            while(rs.Read()){
                datatypeList.Add(new object []{rs.GetDouble(0),rs.GetString(1)});
            }
            rs.Close();
            int templateRangeCount = templateRange.Length;
            if (moduleId ==0) templateRangeCount = 1;
            else if (faId == moduleId) templateRangeCount = 2;
            for (int i1 = 0; i1 < templateRangeCount; i1++)
            {
                for (int i = 0; i < datatypeList.Count; i++)
                {
                    double classId = 0;
                    if (i1 == 0) classId = 0;
                    else if (i1 == 1) classId = moduleId;
                    else if (i1==2) classId = faId;
                    list.Add(new object[] { 0, templateRange[i1], datatypeList[i][1].ToString() + "模板", "", 1, classId, datatypeList[i][0] });
                }
            }
            #region 获取默认模板
            for (int i = 0; i < datatypeList.Count; i++)
            {
                rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.title,B.u_defaultFlag,B.u_datatypeId from  HtmlTemplate B where B.u_type=2 and B.u_datatypeid=@datatypeId and B.u_webFAId=@webFAId and B.u_defaultFlag=1 and B.classid in (0,@moduleId,@id)",
                    new SqlParameter[] { 
                    new SqlParameter("id", faId),
                    new SqlParameter("moduleId", moduleId),
                    new SqlParameter("datatypeId", datatypeList[i][0]),
                    new SqlParameter("webFAId",u_webFAid)
                    
                });
                int index = 0;
                while (rs.Read())
                {
                    if (rs.GetDouble(2) == 0 ) index = i;//全站
                    else if (rs.GetDouble(2) == moduleId) index = datatypeList.Count+i;//模块
                    else if (rs.GetDouble(2) == faId)
                    {
                        index = datatypeList.Count*2 + i;//频道
                    }
                    list[index][0] = rs.GetDouble(0);
                    list[index][3] = rs.GetDateTime(1).ToString();
                    list[index][5] = rs.GetDouble(2);
                    list[index][6] = rs.GetDouble(5);
                    
                }
                rs.Close();
            }
            #endregion
            rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.title,B.u_defaultFlag,B.u_datatypeId from  HtmlTemplate B where B.u_type=2 and B.u_webFAId=@webFAId and B.u_defaultFlag=0 and B.classid = @id",
                new SqlParameter[] { 
                    new SqlParameter("id", faId),
                    new SqlParameter("webFAId",u_webFAid)
                });
                while (rs.Read())
                {
                    int index = 0;
                    if (rs.GetDouble(2) == 0) index = 0;//全站
                    else if (rs.GetDouble(2) == moduleId) index = 1;//模块
                    else if (rs.GetDouble(2) == faId) index = 2;
                    list.Add(new object[] { rs.GetDouble(0), templateRange[index], rs[3], rs.GetDateTime(1).ToString(), 0, rs.GetDouble(2), 0, rs.GetDouble(5) });
                }
                rs.Close();
        }
        else if (typeId == 3)
        {
            string path = @"/";
            if (faId > 1)
            {
                rs = Sql.ExecuteReader("select dirname from  class where id = @id", new SqlParameter[] { new SqlParameter("id", faId) });
                if (rs.Read())
                {
                    path = @"/"+rs.GetString(0)+@"/";
                }
                rs.Close();
            }
            rs = Sql.ExecuteReader("select B.id,B.updatedate,B.classid,B.title,B.u_defaultFlag from   HtmlTemplate B  where B.u_type=3 and B.u_webFAId=@webFAId  and B.classid = @id",
            new SqlParameter[] { 
                new SqlParameter("id", faId),
                new SqlParameter("webFAId",u_webFAid)
            });
            while (rs.Read())
            {
                list.Add(new object[] { rs.GetDouble(0), path, rs[3], rs.GetDateTime(1).ToString(), 0, 0 });
            }
            rs.Close();
        }

        info.userData = list;
        context.Response.Write(info.ToJson());
        context.Response.End();
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}