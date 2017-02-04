using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Xml;
using System.Data.SqlClient;
using RazorEngine.Configuration;
using RazorEngine;
using RazorEngine.Templating;
using Helper;
namespace MWMS
{
    public class PageCache
    {
        static int cacheCycle = 10;//分钟
        static DateTime cacheTime = System.DateTime.Parse("2000-1-1");//缓存过期时间
        struct cacheConfig
        {
            public string regstr;
            public int cacheCycle;
            public DateTime cacheTime;
        }
        static List<cacheConfig> cacheConfigList = new List<cacheConfig>();//自定义缓存配制信息
        Queue<string> task = new Queue<string>();
        public PageCache()
        {
            //System.Timers.Timer myTimer = new System.Timers.Timer(200);
            //myTimer.Elapsed += new System.Timers.ElapsedEventHandler(runTask);
            //myTimer.Enabled = true;
        }
        /// <summary>
        /// 加载缓存配制
        /// </summary>
        public static void loadConfig(){
            XmlNodeList list = Config.loadFile("cache.config");
            try
            {
                cacheCycle = int.Parse(list[0].InnerText);
                cacheTime = DateTime.Parse(list[1].InnerText);
            }
            catch { }
            cacheConfigList.Clear();
            list = list[2].ChildNodes;
            for (int i = 0; i < list.Count; i++)
            {
                cacheConfig value = new cacheConfig();
                value.regstr = list[i].ChildNodes[0].InnerText.Trim();
                if (value.regstr != "")
                {
                    try
                    {
                        value.cacheCycle = list[i].ChildNodes[1].InnerText == "" ? cacheCycle : int.Parse(list[i].ChildNodes[1].InnerText);
                        value.cacheTime = list[i].ChildNodes[2].InnerText == "" ? cacheTime : DateTime.Parse(list[i].ChildNodes[2].InnerText);
                    }
                    catch
                    {
                    }
                }
                cacheConfigList.Add(value);
            }
            //Config.loadFile
        }
        void runTask(object source, System.Timers.ElapsedEventArgs e)
        {
            if (task.Count == 0) return;
            string url=task.Dequeue();

        }
        public void addTask(string url){
            task.Enqueue(url);
        }
        /// <summary>
        /// 缓存文件是否过期
        /// </summary>
        /// <param name="filePath">缓存文件路径</param>
        /// <returns></returns>
        bool expiration(string url,FileInfo filePath)
        {
            if (filePath.Exists)
            {
                DateTime f = filePath.LastWriteTime;
                int _cacheCycle = cacheCycle;
                DateTime _cacheTime = cacheTime;
                for (int i = 0; i < cacheConfigList.Count; i++)
                {
                    Regex regex = new Regex("^" +cacheConfigList[0].regstr + "$", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(url))
                    {
                        _cacheCycle = cacheConfigList[0].cacheCycle;
                        _cacheTime = cacheConfigList[0].cacheTime;
                        i = cacheConfigList.Count;
                    }
                }
                return (_cacheTime>f ||  (System.DateTime.Now-f).TotalMinutes > _cacheCycle);
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 获取缓存信息
        /// </summary>
        /// <param name="virtualWebDir">虚拟站点目录</param>
        /// <param name="url">访问地址</param>
        /// <param name="isMobile">是否手机站</param>
        /// <returns></returns>
        public string getCache(string virtualWebDir,string url,bool isMobile){
            //url = url.ToLower();
            string html =null;
            FileInfo filePath = getCacheFileName(url, isMobile);
            string M5_Login = (HttpContext.Current.Request.Cookies["M5_Login"] == null) ? "" : HttpContext.Current.Request.Cookies["M5_Login"].Value;
            bool isLogin=M5_Login == "true";
            if (isLogin || expiration(url, filePath))//缓存是否过期
            {
                try
                {
                    html = getHtml(virtualWebDir, url, isMobile);
                    if (html != null)
                    {
                        if (!filePath.Directory.Exists) filePath.Directory.Create();
                        System.IO.File.WriteAllText(filePath.FullName, html);
                    }
                }
                catch (Exception ex)
                {
                    if(System.IO.File.Exists(filePath.FullName))html = File.ReadAllText(filePath.FullName);
                    if (isLogin)
                    {
                        if(HttpContext.Current.Request.QueryString["_edit"] != null && HttpContext.Current.Request.QueryString["_edit"].ToString() == "true"){
                            html = "<table width=100% bgcolor=\"#ffffcc\"><tr><td>" +
                            "<code><font color=red>页面发生错误：" + ex.Message + "</font></code>" +
                            "</td></tr></table>";
                        }
                        else { 
                            html = "<table width=100% bgcolor=\"#ffffcc\"><tr><td>"+
                            "<code><font color=red>页面发生错误："+ex.Message+"</font></code>"+
                            "</td></tr></table>"+html;
                        }
                    }
                }
            }
            else
            {
                html = File.ReadAllText(filePath.FullName);
            }
            return html;
        }
        FileInfo getCacheFileName(string url, bool isMobile)
        {
            string hostDir = @"default\";
            if (isMobile) hostDir = @"mobile\";
            string str = url.MD5();
            string dir = "";
            for (int i = 0; i < 16; i += 2)
            {
                dir += str.Substring(i, 2) + @"\";
            }
            string cacheDir = HttpContext.Current.Server.MapPath("~" + Config.cachePath + hostDir + dir);
            //if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
            return (new FileInfo(cacheDir + str + ".cac"));
        }
        int _pageNo = 1;
        string _fileName = "";
        public string getHtml(string virtualWebDir, string url, bool isMobile)
        {
            Config.systemVariables["webUrl"]= "http://" + HttpContext.Current.Request.Url.Authority + Config.webPath;
//            Config.systemVariables["webUrl"] = "http://" + HttpContext.Current.Request.Url.Authority + Config.webPath;
            //Config.systemVariables["pageUrl"] = HttpContext.Current.Request.Url.AbsoluteUri.ToString();// "http://" + HttpContext.Current.Request.Url.Authority +""+ Config.webPath;

            Regex r = new Regex(@"(?<=/)((.[^/]*)_((\d){1,5}))(." + BaseConfig.extension + ")", RegexOptions.IgnoreCase);
           string newUrl = r.Replace(url, new MatchEvaluator(_replaceUrl));
            TemplateInfo info = TemplateClass.get(newUrl, isMobile);
            if (info == null)
            {
                API.ERR404("模板不存在");
            }
            else{
                if (info.u_type == 2)
                {
                    Helper.Sql.ExecuteNonQuery("update mainTable set clickCount=clickCount+1 where id=@id", new SqlParameter[]{
                    new SqlParameter("id",info.variable["id"])
                });
                }
                if (newUrl.IndexOf(".") > -1)
                {
                    string[] u = newUrl.Split('/');
                    _fileName = u[u.Length - 1].Replace("." + BaseConfig.extension, "");
                }
                else
                {
                    _fileName = "default";
                }
                TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
                {
                    CatchPath = HttpContext.Current.Server.MapPath("~" + Config.cachePath + "assembly/")
                };
                Razor.SetTemplateService(new TemplateService(templateConfig));
                RazorEngine.Razor.Compile(info.u_content, typeof(object[]), info.id.ToString(),false);
                

                string html = RazorEngine.Razor.Run(info.id.ToString(),new object[] { Config.systemVariables,info.variable });
                
                /*
                TE_statistical TE_statistical = new TE_statistical();
                TemplateEngine page = new TemplateEngine();
                page.isMobile = isMobile;
                page.TE_statistical = TE_statistical;
                page.addVariable("sys", Config.systemVariables);
                page.addVariable("view", Config.viewVariables);
                page.addVariable("page", info.variable);
                Dictionary<string, object> _public = new Dictionary<string, object>();
                _public.Add("_pageNo", _pageNo);
                _public.Add("_url", HttpContext.Current.Request.Url.ToString());
                _public.Add("_fileName", _fileName);
                page.addVariable("public", _public);
                string html = info.u_content;
                page.isEdit = HttpContext.Current.Request.QueryString["_edit"] != null && HttpContext.Current.Request.QueryString["_edit"].ToString() == "true";
                page.render(ref html);
                
                //HttpContext.Current.Response.Write("<!--模板解析时间：" + sw.ElapsedMilliseconds.ToString() + "-->");
                TemplateEngine.replaceKeyword(ref html);
                //HttpContext.Current.Response.Write("<!--关键词替换时间：" + sw.ElapsedMilliseconds.ToString() + "-->");
                //page.replaceSubdomains(ref html, isMobile);
                //if (BaseConfig.urlConversion)  
                    
                    page.replaceUrl(ref html);
                if (page.isEdit)
                {
                    html = html.Replace("</head>", "<script src='"+Config.webPath+"/manage/app/visualTemplateEditer/templetEdit.js'></script>\n</head>");
                }
                //sw.Stop();
                //HttpContext.Current.Response.Write("<!--全部解析时间：" + sw.ElapsedMilliseconds.ToString() + "-->");
                */
                return html;
            }
            return null;
        }
        string _replaceUrl(Match m)
        {
            _pageNo = int.Parse(Regex.Match(m.Value, @"(?<=_)((\d){1,5})(?=\.)").Value);
            if (Regex.IsMatch(m.Value, "^default_", RegexOptions.IgnoreCase))
            {
                return "";
            }
            else
            {
                return Regex.Replace(m.Value, @"_((\d){1,5})", "");
            }
        }
    }
}
