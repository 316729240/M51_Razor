using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
using System.Data.SqlClient;
using Helper;
using System.IO;

namespace MWMS.Configuration
{
    public class ConfigCollect
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
    public class Config
    {
        string _filepath = "";
        XmlDocument xmlDoc = new XmlDocument();
        //Dictionary<string, XmlNodeList> node = new Dictionary<string, XmlNodeList>(StringComparer.InvariantCultureIgnoreCase);//用户设置加载
        Dictionary<string, Node> nodeList = new Dictionary<string, Node>();
        public Config(string filepath)
        {
            _filepath = filepath;
            try
            {
                xmlDoc.Load(filepath);
            }
            catch
            {
                throw new NullReferenceException("加载配制文件[" + filepath + "]失败");
            }
            for (int i=0;i<xmlDoc.DocumentElement.ChildNodes.Count;i++)
            {
                XmlNode _node = xmlDoc.DocumentElement.ChildNodes[i];
                if (_node.Attributes["name"] != null) { 
                    nodeList[_node.Attributes["name"].InnerText] = new Node(_node);
                }
            }
        }
        public Node this[string name]
        {
            get { return nodeList[name]; }
        }
        public void Save()
        {
            for (int i = 0; i < xmlDoc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode _node = xmlDoc.DocumentElement.ChildNodes[i];
                string name = _node.Attributes["name"] == null ? "" : _node.Attributes["name"].Value;
                string xtype = _node.Attributes["xtype"] == null ? "" : _node.Attributes["xtype"].Value;
                if (name != "")
                {
                    if (xtype != "")
                    {
                        if (xtype == "GridView")
                        {
                            _node.InnerXml = nodeList[name].Value;
                        }
                        else
                        {
                            _node.InnerText = nodeList[name].Value;
                        }
                    }
                    else
                    {
                        _node.InnerText = nodeList[name].Value;
                    }
                }
            }
            xmlDoc.Save(_filepath);
        }

}
    public class Node
    {
        XmlNode node = null;
        public string Value
        {
            get
            {
                return node.InnerText.ToString();
            }
            set
            {
                node.InnerText = value;
            }
        }
        public Node(XmlNode _node)
        {
            node = _node;
            if(this["xtype"]== "GridView")
            {
                Grid = new List<Dictionary<string, string>>();
                for (int i = 0; i < _node.ChildNodes.Count; i++)
                {
                    Dictionary<string, string> attr = new Dictionary<string, string>();
                    for (int i1 = 0; i1 < _node.ChildNodes[i].ChildNodes.Count; i1++)
                    {
                        attr[_node.ChildNodes[i].ChildNodes[i1].Name] = _node.ChildNodes[i].ChildNodes[i1].InnerText;
                    }
                    Grid.Add(attr);
                }
            }
        }
        public List<Dictionary<string, string>> Grid = null;
        public string this[string name]
        {
            get {
                return node.Attributes[name]==null?"":node.Attributes[name].Value;
            }
        }
    }
}
