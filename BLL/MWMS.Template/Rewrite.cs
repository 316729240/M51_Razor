using System;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
namespace MWMS
{
    public class Rewrite : IHttpModule
    {
        public Rewrite()
        {
        }

        public static bool applicationFlag = false;
        public String ModuleName
        {
            get { return "MWMS_Rewrite"; }
        }
        public void Init(HttpApplication application)
        {
            Application_Start();
            #region 系统是否已过期
            if (Constant.SystemExpiredTag) return;
            #endregion
            application.BeginRequest += (new EventHandler(this.Application_BeginRequest));
        }
        protected void Application_Start()
        {
            #region 防止多次执行
            if (applicationFlag) return;
            applicationFlag = true;
            Helper.Sql.connectionString = @"Data Source=" + ConfigurationManager.AppSettings["ServerIP"] + ";Initial Catalog=" + ConfigurationManager.AppSettings["DataBaseName"] + ";Integrated Security=false;UID=" + ConfigurationManager.AppSettings["Username"] + ";PWD=" + ConfigurationManager.AppSettings["Password"] + ";Pooling=true;MAX Pool Size=512;Min Pool Size=8;Connection Lifetime=5";
            #endregion
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath("~" + Config.cachePath));
            if (!dir.Exists) dir.Create();
            if (Config.install) return;
            Config.loadUserConfig("systemVariables.config");
            Config.loadView();
            Config.loadDomain();
            Config.loadUserConfig("url.config");
            Config.loadUserConfig("base.config");
            Config.loadUserConfig("mail.config");
            Config.loadUserConfig("watermark.config");
            Config.loadUserConfig("keyword.config");
            Config.loadUserConfig("link.config");
            Config.loadUserConfig("cache.config");
            Config.loadUserConfig("keywordFiltering.config");
            string path = HttpContext.Current.Server.MapPath("~" + Config.appPath);
            string[] appDir = System.IO.Directory.GetDirectories(path);
            for (int i = 0; i < appDir.Length; i++)
            {
                string reg_xml = appDir[i] + @"\reg.xml";
                string app_config = appDir[i] + @"\app.config";
                if (System.IO.File.Exists(app_config) && System.IO.File.Exists(reg_xml))
                {
                    XmlDocument reg = new XmlDocument();
                    reg.Load(reg_xml);
                    Config.loadUserConfig(reg.ChildNodes[0].Attributes["name"].Value);
                }
            }
        }
        /// <summary>
        /// 是否允许访问后台
        /// </summary>
        /// <returns></returns>
        bool allowAccessManagementIP(HttpRequest Request)
        {
            if (Config.allowAccessManagementIP != null)
            {
                if ((Regex.IsMatch(Request.Url.AbsolutePath, "^" + Config.webPath + Config.managePath, RegexOptions.IgnoreCase)))
                {
                    for (int i = 0; i < Config.allowAccessManagementIP.Length; i++)
                    {
                        if (Config.allowAccessManagementIP[i] != "")
                        {
                            string reg = Config.allowAccessManagementIP[i].Replace("*", @"\d{1,3}");
                            if (Regex.IsMatch(Request.UserHostAddress, reg)) return true;
                        }
                    }
                    return false;
                }
            }
            return true;
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpRequest Request = application.Request;
            HttpResponse Response = application.Response;
            if (!allowAccessManagementIP(Request)) API.ERR404("非法访问");
            #region 非系统网页扩展名时处理方式(非文件跳转目录否则不再处理)

            if (!(Regex.IsMatch(Request.Url.AbsolutePath, "(.*)(/|" + BaseConfig.extension + ")$", RegexOptions.IgnoreCase)))
            {
                if (application.Request.Url.Segments[application.Request.Url.Segments.Length - 1].IndexOf(".") == -1)
                {

                    API.ERR301(application.Request.Url.ToString() + @"/");
                }
                return;
            }
            #endregion
            HttpContext.Current.Response.ContentType = "text/html; charset=" + System.Text.Encoding.Default.HeaderName;
            //injection(Request);//注入过滤
            string acceptTypes = Request.Headers["Accept"];
            if (!string.IsNullOrEmpty(acceptTypes) && acceptTypes.ToLower().IndexOf("text/vnd.wap.wml") > -1)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }

            RewriteUrl(application);//载入映射规则


        }
        /// <summary>
        /// 手机访问
        /// </summary>
        /// <param name="Request"></param>
        //void mobileAccess(HttpResponse Response, HttpRequest Request)
        //{
        //    if (API.isMobileAccess() && !API.getWebFAId())//手机访问 但非手机域名 转向
        //    {
        //        string newUrl=Regex.Replace(Request.Url.AbsolutePath, "^" + Config.webPath+@"/", "", RegexOptions.IgnoreCase);
        //        string murl = (new Uri(BaseConfig.mobileUrl, newUrl)).ToString();
        //        Response.Redirect(murl);
        //    }
        //}

        /// <summary>
        /// 防止sql注入请求
        /// </summary>
        void injection(HttpRequest Request)
        {
            for (int n = 0; n < Request.QueryString.Count; n++)
            {
                if (!API.safetyVerification(Request.QueryString[n]))
                {
                    string UName = "-1";
                    if (Request.Cookies["AdminUserID"] != null) UName = Request.Cookies["AdminUserID"].Value;
                    API.writeLog("3", "危险请求" + Request.RawUrl);
                    API.ERR301("/");
                }
            }
        }
        static void mobileRedirect(string url, ref bool isMobilePage)
        {
            if (BaseConfig.mobileRedirectType == 1)
            {
                HttpContext.Current.Response.Write("<script>location.href='" + url + "';</script>");
                HttpContext.Current.Response.End();
            }
            else if (BaseConfig.mobileRedirectType == 2)
            {
                isMobilePage = true;
            }
            else
            {
                HttpContext.Current.Response.Redirect(url);
            }
        }
        public static string urlZhuanyi(Uri rUrl, ref bool isMobilePage, ref string virtualWebDir)
        {
            string url = rUrl.AbsolutePath;
            url = Regex.Replace(url, "^" + Config.webPath, "", RegexOptions.IgnoreCase).ToLower();
            string newUrl = url;
            virtualWebDir = "/";//虚拟站点目录
            #region 自定义绑定域名处理
            if (Config.domainList != null)
            {
                for (int i = 0; i < Config.domainList.Count; i++)
                {
                    if (String.Compare(Config.domainList[i][0], rUrl.Host, true) == 0)//域名转换为目录
                    {
                        newUrl = "/" + Config.domainList[i][1] + url;
                        if (Config.domainList[i][2] != "" && API.isMobileAccess())
                        { //使用手机访问
                            mobileRedirect("http://" + Config.domainList[i][2] + url, ref isMobilePage);
                        }
                    }
                    else if (String.Compare(Config.domainList[i][2], rUrl.Host, true) == 0)//手机域名转换
                    {
                        newUrl = "/" + Config.domainList[i][1] + url;
                        isMobilePage = true;
                    }
                    else if (Regex.IsMatch(url, "^/" + Config.domainList[i][1] + "/", RegexOptions.IgnoreCase))//访问是原路径
                    {
                        virtualWebDir = "/" + Config.domainList[i][1] + "/";
                        string newurl = "/" + Regex.Replace(url, "^/" + Config.domainList[i][1] + "/", "", RegexOptions.IgnoreCase);
                        if (Config.domainList[i][2] != "" && API.isMobileAccess()) mobileRedirect("http://" + Config.domainList[i][2] + newurl, ref isMobilePage);
                        if (Config.domainList[i][0] != "") API.ERR301("http://" + Config.domainList[i][0] + newurl);

                    }
                }
            }
            #endregion
            if (!isMobilePage)
            {
                if (BaseConfig.mobileUrl != "")
                {
                    if (BaseConfig.mobileUrl.IndexOf("http") > -1)
                    {
                        isMobilePage = Regex.IsMatch(rUrl.AbsoluteUri, "^" + BaseConfig.mobileUrl);
                    }
                    else
                    {
                        isMobilePage = Regex.IsMatch(newUrl, "^" + virtualWebDir + BaseConfig.mobileUrl);
                    }
                }
                if (isMobilePage)
                {

                    if (BaseConfig.mobileUrl.IndexOf("http") > -1)
                    {
                        //isMobilePage = Regex.IsMatch(newUrl, "^" + BaseConfig.mobileUrl);
                    }
                    else
                    {
                        newUrl = Regex.Replace(newUrl, "^" + virtualWebDir + BaseConfig.mobileUrl, virtualWebDir);
                    }
                }
            }
            return newUrl;
        }
        bool RewriteUrl(HttpApplication application)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            HttpRequest Request = application.Request;
            HttpResponse Response = application.Response;
            HttpContext Context = application.Context;
            #region 已存在文件处理方式
            string file = Request.Path;
            if (file.Substring(file.Length - 1) == "/") file += "index." + BaseConfig.extension;
            if (System.IO.File.Exists(Request.MapPath(file))) return (false);
            #endregion

            //mobileAccess(Response, Request);//手机访问
            bool isMobilePage = false;
            string virtualWebDir = "/";//虚拟站点目录
            string newUrl = urlZhuanyi(Request.Url, ref isMobilePage, ref virtualWebDir);
            if (API.isMobileAccess() && !isMobilePage)//手机访问 但非手机域名 转向
            {
                string murl = "";
                if (BaseConfig.mobileUrl.IndexOf("http") > -1)
                {
                    if (!Regex.IsMatch(newUrl, "^" + BaseConfig.mobileUrl))
                    {
                        murl = BaseConfig.mobileUrl + newUrl.Substring(1) + Request.Url.Query;
                        if (BaseConfig.mobileRedirectType == 1)
                        {
                            Response.Write("<script>location.href='" + murl + "';</script>");
                            Response.End();
                        }
                        else if (BaseConfig.mobileRedirectType == 2)
                        {
                            isMobilePage = true;
                        }
                        else
                        {
                            Response.Redirect(murl);
                        }
                    }
                }
                else
                {
                    if (!Regex.IsMatch(newUrl, "^" + virtualWebDir + BaseConfig.mobileUrl))
                    {
                        string _murl = Regex.Replace(newUrl, "^" + virtualWebDir, virtualWebDir + BaseConfig.mobileUrl, RegexOptions.IgnoreCase);
                        murl = new Uri(Request.Url, Config.webPath + _murl).ToString() + Request.Url.Query;
                        if (BaseConfig.mobileRedirectType == 1)
                        {
                            Response.Write("<script>location.href='" + Config.webPath + _murl + "';</script>");
                            Response.End();
                        }
                        else if (BaseConfig.mobileRedirectType == 2)
                        {
                            isMobilePage = true;
                        }
                        else
                        {
                            Response.Redirect(murl);
                        }
                    }
                }
            }
            PageCache pageCache = new PageCache();
            string html = pageCache.getCache(virtualWebDir, newUrl, isMobilePage);
            if (html != null)
            {
                Response.Write(html);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 100)
                {
                    API.writeLog("time", sw.ElapsedMilliseconds.ToString() + "\t" + Request.Url.ToString());
                }
                //Response.Write("<!--页面执行时间：" + sw.ElapsedMilliseconds.ToString()+"-->");
                Response.End();
            }
            return (false);
        }
        void UpdateCookie(string cookie_name, string cookie_value)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(cookie_name);
            if (cookie == null)
            {
                cookie = new HttpCookie(cookie_name);
                HttpContext.Current.Request.Cookies.Add(cookie);
            }
            cookie.Value = cookie_value;
            HttpContext.Current.Request.Cookies.Set(cookie);
        }


        public void Dispose() { }
    }
}