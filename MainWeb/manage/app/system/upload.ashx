<%@ WebHandler Language="C#" Class="upload"%>
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
using ManagerFramework;
public class upload : IHttpHandler
{
    //LoginInfo login = new LoginInfo();
    SafeReqeust s_request = new SafeReqeust(0, 0);
    public class File
    {
        public string title = "";
        public int size = 0;
        public string path = "";
    }
    public void ProcessRequest(HttpContext context)
    {
        //login.checkLogin();

        ReturnValue returnValue = new ReturnValue();
        context.Response.ContentType = "text/plain";
        string[] fp = new string[context.Request.Files.Count];
        List<File> list = new List<File>();
        for(int i=0;i<context.Request.Files.Count;i++){
            try {
                string newfile = Helper.Tools.SaveImage(context.Request.Files[i], Config.tempPath);
                list.Add(new File { title=context.Request.Files[0].FileName,size=context.Request.Files[0].ContentLength,path=newfile});
            }catch
            {

            }
        }
        returnValue.userData = list;
        context.Response.Write(list.ToJson());
        context.Response.End();
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}