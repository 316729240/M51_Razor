﻿<%@ WebHandler Language="C#" Class="api" %>

using System;
using System.Web;
using System.Text.RegularExpressions;
using Helper;
using System.Collections.Generic;
using MWMS;
public class api : IHttpHandler {

    public void ProcessRequest(HttpContext context)
    {
        SafeReqeust s_request = new SafeReqeust(0, 0);
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "login")
        {

            ErrInfo err = UserClass.login(context.Request.Form["uname"].ToString(), context.Request.Form["pword"].ToString());
            context.Response.Write(err.ToJson());
        }
        else if (m == "checkManagerLogin")
        {
            ErrInfo err = new ErrInfo();
            LoginInfo info = new LoginInfo();
            err.userData = info.value;
            err.errNo = info.value==null ? -1 : 0;
            context.Response.Write(err.ToJson());
        }
        else if (m == "getDirName")
        {
            string name = s_request.getString("name");
            ErrInfo info = new ErrInfo();
            string pinyin = name.GetPinYin();
            if (pinyin.Length > 15) pinyin = name.GetPinYin('2');
            pinyin = Regex.Replace(pinyin, "[ "+@"\-_"+"`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）——|{}【】‘；：”“'。，、？]", "");
            info.userData = pinyin;//.Replace(" ","").Trim();
            context.Response.Write(info.ToJson());

        }
        else if (m == "readIcon")
        {
            ErrInfo info = new ErrInfo();
            string path = Config.staticPath + "icon/";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath("~" + path));
            System.IO.FileInfo[] f = dir.GetFiles("*.jpg");
            string[] file = new string[f.Length];
            for (int i = 0; i < f.Length; i++)
            {
                file[i] = path + f[i].Name;
            }
            info.userData = file;
            context.Response.Write(info.ToJson());
        }
        else if (m == "getDataUrl")
        {
            double id = s_request.getDouble("id");
            ErrInfo info = new ErrInfo();

            System.Data.SqlClient.SqlDataReader rs1 = Helper.Sql.ExecuteReader("select url from mainTable where id=@id", new System.Data.SqlClient.SqlParameter[]{
                new System.Data.SqlClient.SqlParameter("id",id)
            });
            if (rs1.Read())
            {
                info.userData = Config.webPath + rs1[0] + "." + BaseConfig.extension;
            }
            else
            {
                info.errNo = -1;
            }
            context.Response.Write(info.ToJson());
        }
        context.Response.End();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}