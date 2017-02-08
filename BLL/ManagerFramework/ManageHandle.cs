﻿using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace ManagerFramework
{
    public class  ManageHandle: IHttpHandler
    {
        protected SafeReqeust s_request = new SafeReqeust(0, 0);
        protected HttpContext context;
        protected LoginUser loginUser = null;
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            this.context = context;
            loginUser = LoginUser.GetLoginUser();
            if (loginUser == null)
            {
                context.Response.Write((new ReturnValue(-1000,"没有登录")).ToJson());
                context.Response.End();
            }
            Type t = this.GetType();
            object value=t.InvokeMember(s_request.getString("_m"), BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, this, null);
            if (value != null)
            {
                context.Response.Write(value.ToJson());
                context.Response.End();
            }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}