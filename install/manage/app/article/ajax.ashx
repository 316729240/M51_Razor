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
public class ajax : IHttpHandler
{
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        SafeReqeust s_request = new SafeReqeust(0, 0);
        context.Response.ContentType = "text/plain"; 
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "read")
        {
            double id = s_request.getDouble("id");
            ErrInfo info = new ErrInfo();
            Dictionary<string,object> data=Helper.Sql.ExecuteDictionary("select A.title,A.classId,A.skinId,A.url,B.* from mainTable A inner join  article B on A.id=B.id where A.id=@id", new SqlParameter[] { new SqlParameter("id", id) });
            data["url"] = Config.webPath + data["url"].ToString() + "." + BaseConfig.extension;
            info.userData = data;
            context.Response.Write(info.ToJson());
        }else if(m=="edit"){
            ErrInfo info = new ErrInfo();
            RecordClass value = new RecordClass(login.value);
            string keyword = s_request.getString("u_keyword");
            string u_defaultPic = s_request.getString("u_defaultPic");
            value.tableName = "article";
            double id=s_request.getDouble("id");
            double classId = s_request.getDouble("classId");
            value.addField("classId", classId);
            value.addField("skinId", s_request.getDouble("skinId"));
            value.addField("title", s_request.getString("title"));
            value.addField("u_keyword", keyword);
            if (context.Request.Files["u_defaultPic"] != null)
            {
                info = API.saveImage(context.Request.Files["u_defaultPic"], Config.uploadPath);
                if (info.errNo < 0)
                {
                    context.Response.Write(info.ToJson());
                    return;
                }
                value.addField("u_defaultPic", info.userData);
            }
            value.addField("u_info", s_request.getString("u_info"));
            value.addField("u_custom", s_request.getString("u_custom"));
            string u_content=s_request.getString("u_content");
            value.addField("u_content", u_content);
            if (id > 0)
            {
                info = value.update(id);
                if (info.userData != null)
                {
                    Sql.ExecuteNonQuery("delete from indextable where dataId=@dataId",new SqlParameter[]{
                        new SqlParameter("dataId",info.userData)
                    });
                    RecordClass.addKeyword((double)info.userData, keyword);
                }
            }
            else
            {
                info = value.insert();
                if (info.userData != null) RecordClass.addKeyword((double)info.userData, keyword);
            }
            context.Response.Write(info.ToJson());
            
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}