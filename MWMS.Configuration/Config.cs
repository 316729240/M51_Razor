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
    public class Config
    {
        /// <summary>
        /// config配制所在路径
        /// </summary>
        public string ConfigPath  { get;set; }
        Dictionary<string, XmlNodeList> userConfig = new Dictionary<string, XmlNodeList>(StringComparer.InvariantCultureIgnoreCase);//用户设置加载
        /// <summary>
        /// 加载配制文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns></returns>
        public void LoadConfig(string filename)
        {
            string configFile = HttpContext.Current.Server.MapPath("~" + ConfigPath + filename);
            string name = filename.Replace(".config","");
            if (!System.IO.File.Exists(configFile)) return; 
            XmlDocument link = new XmlDocument();
            try
            {
                link.Load(configFile);
            }
            catch
            {
                throw new NullReferenceException("加载配制文件[" + filename + "]失败");
            }
            userConfig[name]= link.DocumentElement.ChildNodes;
        }
        public XmlNodeList this[string name]
        {
            get {
                if (!userConfig.ContainsKey(name)) LoadConfig(name);
                return userConfig[name];
            }
            set { userConfig[name] = value; }
        }

    }
}
