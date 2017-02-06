using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using System.Data.SqlClient;
using Helper;

namespace MWMS.Configuration
{
    public class Constant
    {
        public static int maxLink = 0;
        public static int cacheN = 0;
        public static int taskHour = 0, taskMinute = 0;//定时执行任务的时间（随机产生）
        public static bool statisticsToolbar = true;//默认打开
        //public static DateTime CacheDate = DateTime.Parse(ConfigurationManager.AppSettings["CacheDate"]);
        //public static string MobileAddress = ConfigurationManager.AppSettings["MobileAddress"] + "";//手机地址
        //public static bool MobileRedirectType = ((ConfigurationManager.AppSettings["MobileRedirectType"] + "").ToLower() == "js");//手机地址
        public static bool PageRobotTag = false;//关闭
        public static bool ClassRobotTag = false;//关闭
        public static bool HomeRobotTag = false;//关闭
        public static bool ForciblyLoginTag = false;//关闭
        public static bool DisabledLoginTag = false;//关闭
        public static bool SystemExpiredTag = false;//关闭
        public static bool SystemBackTag = false;//关闭
        public static bool BanNonBrowserRequestsTag = false;//禁止非浏览器请求关闭
        public static bool absolutepathFlag = false;//是否自动转换为绝对路径
        public static string mainDomain = Tools.GetAppSettings("maindomain");//主域名
        public static int oldSubdomainsPage = 1;//旧的二级子域名页面处理(0转向404 2 不做处理  1 301跳转正确页)
        public static int AddDataCounter = 0;//随机计数器
        public static int CacheDirType = 0;//0数据目录缓存结构  1随即目录缓存结构
        public static int SqlVersion = 0;//数据库版本
    }
    public class BaseConfig
    {
        public static Uri mainUrl = null;
        public static bool urlConversion = false;
        public static string contentUrlTemplate = "";
        public static string extension = "html";
        public static string[] SensitiveKeyword = null;
        public static bool replaceNull = false;
        public static string mobileUrl = "";
        public static int mobileRedirectType = 0;//0服务器跳转 1js跳转 2不跳转
        public static int managePageSize = 20;//后台列表页页大小
        public static int backupDay = 1;
    }
    public class Config
    {
        public static bool install = (ConfigurationManager.AppSettings["Install"] + "") == "true";
        public static string[] allowAccessManagementIP = (ConfigurationManager.AppSettings["AllowAccessManagementIP"]+"") == "" ? null : ConfigurationManager.AppSettings["AllowAccessManagementIP"].Split(',');//允许访问ip
        public static string webPath = HttpRuntime.AppDomainAppVirtualPath==@"/"?"":HttpRuntime.AppDomainAppVirtualPath.ToLower();
        public static string tempPath = "/temp/";//临时文件存放位置
        public static string uploadPath = "/uploadfile/";//上传文件夹
        public static string managePath = "/manage/";//后台文件夹
        public static string configPath = "/config/";//配制文件所在目录
        public static string appPath = "/manage/app/";//应用配制所在目录
        public static string staticPath = "/static/";//静态文件所在目录
        public static string cachePath = "/cache/";//缓存文件所在目录
        public static string backupPath = "/backup/";//备份文件路径
        public static string webId = ConfigurationManager.AppSettings["WebID"];
        public static Dictionary<string, string> systemVariables = new Dictionary<string, string>();
        public static Dictionary<string, object> viewVariables = new Dictionary<string, object>();
        public static List<string[]> domainList = new List<string[]>();
        public static Dictionary<string, XmlNodeList> userConfig = new Dictionary<string, XmlNodeList>();//用户设置加载
        public static XmlNodeList loadFile(string filename)
        {
            string configFile = filename.IndexOf(".") > -1 ? HttpContext.Current.Server.MapPath("~" + Config.configPath + filename) : HttpContext.Current.Server.MapPath("~" + Config.appPath + filename + @"/app.config");
            if (!System.IO.File.Exists(configFile)) return null;
            XmlDocument link = new XmlDocument();
            try
            {
                link.Load(configFile);
            }
            catch
            {
                throw new NullReferenceException("加载配制文件[" + filename + "]失败");
            }
            return link.DocumentElement.ChildNodes;
        }
        public static void loadUserConfig(string file)
        {
                if (file == "base.config")
                {
                    XmlNodeList list = Config.loadFile(file);
                    try
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string text = list[i].InnerText;
                            switch (list[i].Attributes["name"].Value)
                            {
                                case "managePageSize":
                                    BaseConfig.managePageSize = Convert.ToInt32(int.Parse(text));
                                    break;
                                case "mainUrl":
                                try { 
                                    BaseConfig.mainUrl = new Uri(text);
                                }
                                catch
                                {

                                }
                                    break;
                                case "urlConversion":
                                    BaseConfig.urlConversion = text == "1";
                                    break;
                                case "contentUrlTemplate":
                                    BaseConfig.contentUrlTemplate = text;
                                    break;
                                case "extension":
                                    BaseConfig.extension = text;
                                    break;
                                case "mobileUrl":
                                    BaseConfig.mobileUrl = text.ToLower();
                                    break;
                                case "backupDay":
                                    BaseConfig.backupDay = Convert.ToInt32(text);
                                    break;
                                case "mobileRedirectType":
                                BaseConfig.mobileRedirectType = Convert.ToInt32(text);
                                break;


                            }
                        }

                    }
                    catch
                    {
                    }
                }
                else if (file == "systemVariables.config")
                {
                    loadSystemVariables();
                }
                else if (file == "cache.config")
            {
                XmlNodeList list = Config.loadFile("cache.config");
                //PageCache.loadConfig();
                    //Rewrite.pageCache.loadConfig();
                }
                else if (file == "keywordFiltering.config")
                {
                    XmlNodeList list = Config.loadFile(file);
                    BaseConfig.SensitiveKeyword = list[0].InnerText.Split('\n');
                    if (list.Count > 1) BaseConfig.replaceNull = list[1].InnerText == "1";
                }

            Config.userConfig[file.Replace(".config","")] = loadFile(file);
        }
        public static void loadSystemVariables()
        {
            Config.systemVariables.Clear();
            XmlNodeList list = Config.loadFile("systemVariables.config");
            //string xml = API.GetFileText(HttpContext.Current.Server.MapPath("~" + Config.configPath + "systemVariables.config"));
            for (int i = 0; i < list.Count; i++)
            {
                Config.systemVariables[list[i].Attributes["name"].Value]= list[i].InnerText;
            }
            
        }
        public static void loadDomain()
        {
            string cache_file = HttpContext.Current.Server.MapPath("~" + Config.cachePath + "config_domainList");
            try
            {
                domainList.Clear();
                SqlDataReader rs = Helper.Sql.ExecuteReader("select domainName,dirpath,dirName,_domainName from class where domainName <>''");
                while (rs.Read())
                {
                    domainList.Add(new string[] { rs.GetString(0) == rs.GetString(2) ? "" : rs.GetString(0), rs.GetString(1), rs[3] + "" });
                }
                rs.Close();
                Tools.writeObjectFile(cache_file, domainList);
            }
            catch
            {
                domainList = (List<string[]>)Tools.readObjectFile(cache_file);
            }
        }
        public static void loadView()
        {
            string cache_file = HttpContext.Current.Server.MapPath("~" + Config.cachePath + "config_viewVariables");
            try
            {
                SqlDataReader rs = Helper.Sql.ExecuteReader("select id,classname from class where classid=12");
                while (rs.Read())
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    System.Data.SqlClient.SqlDataReader rs2 = Helper.Sql.ExecuteReader("select B.title,B.id,B.u_html from dataview B  where B.classid=" + rs[0].ToString());
                    while (rs2.Read())
                    {
                        data[rs2[0].ToString()] = new object[] { rs2[1], rs2[2] };
                    }
                    rs2.Close();
                    viewVariables[rs[1].ToString()] = data;
                }
                rs.Close();
                Tools.writeObjectFile(cache_file, viewVariables);
            }
            catch
            {
                viewVariables = (Dictionary<string, object>)Tools.readObjectFile(cache_file);
            }
        }
    }
}
