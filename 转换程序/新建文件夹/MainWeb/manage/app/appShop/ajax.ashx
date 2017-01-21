<%@ WebHandler Language="C#" Class="ajax"%>
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
using System.IO;
using System.Web.Script.Serialization;
using Ionic.Zip;
public class ajax : IHttpHandler {
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain"; 
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "cardPermissions")
        {
            context.Response.Write(login.value.isAdministrator);
        }
        if (m == "readAppList")
        {
            ErrInfo err = new ErrInfo();
            string http=Http.getUrl("http://www.mwms4.com/shop/app/");
            
            JavaScriptSerializer ser = new JavaScriptSerializer();
            err.userData =ser.DeserializeObject(http);
            context.Response.Write(err.ToJson());
        }
        else if (m == "setup")
        {
            ErrInfo err = new ErrInfo();
            string appName=s_request.getString("appName");
            try
            {
                byte[] data = Http.getByte("http://www.mwms4.com/shop/app/" + appName + ".zip");
                string path = context.Server.MapPath("~" + Config.tempPath);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                System.IO.File.WriteAllBytes(path + appName + ".zip", data);
                using (ZipFile zip = new ZipFile(path + appName + ".zip"))
                {
                    zip.ExtractAll(context.Server.MapPath("~" + Config.appPath + appName + @"/"), ExtractExistingFileAction.OverwriteSilently);
                }
                System.IO.File.Delete(path + appName + ".zip");
            }
            catch (Exception ex)
            {
                err.errMsg = ex.Message;
            }
            context.Response.Write(err.ToJson());
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}