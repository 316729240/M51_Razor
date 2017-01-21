<%@ Page Language="C#" %>
<%@ Import Namespace="System.Xml" %>
<%
    string xml = "<?xml version=\"1.0\" encoding=\"gb2312\"?>\r\n" +
        "<config>\r\n" +
        "<item xtype=\"GridView\" name=\"grid\" columns=\"[{text:'关键词',width:200,name:'key',xtype:'TextBox'},{text:'替换词',width:200,name:'rkey',xtype:'TextBox'}]\">\r\n";
    XmlDocument link = new XmlDocument();
    link.Load(HttpContext.Current.Server.MapPath("~/config/keyword.config"));
    if (link.DocumentElement.Name == "KeywordSet")
    {
        for (int i = 0; i < link.DocumentElement.ChildNodes.Count; i++)
        {
            XmlNode v = link.DocumentElement.ChildNodes[i];
            xml += "<data>\r\n";
            xml += v.ChildNodes[0].OuterXml.Replace("Keyword", "key");
            xml += v.ChildNodes[1].OuterXml.Replace("Keyword", "rkey");
            xml += "</data>\r\n";

        }
        xml += "</item>\r\n";
        xml += "</config>";
        link.LoadXml(xml);
        link.Save(HttpContext.Current.Server.MapPath("~/config/keyword.config"));
    }

    xml = "<?xml version=\"1.0\" encoding=\"gb2312\"?>\r\n" +
            "<config>\r\n" +
            "<item xtype=\"GridView\" name=\"grid\" columns=\"[{text:'关键词',width:200,name:'key',xtype:'TextBox'},{text:'链接',width:300,name:'url',xtype:'TextBox'},{text:'数量',width:50,name:'count',xtype:'TextBox'},{text:'颜色',width:50,name:'color',xtype:'TextBox'}]\">\r\n";

    link = new XmlDocument();
    link.Load(HttpContext.Current.Server.MapPath("~/config/link.config"));
    if (link.DocumentElement.Name == "Link")
    {
        for (int i = 0; i < link.DocumentElement.ChildNodes.Count; i++)
        {
            XmlNode v = link.DocumentElement.ChildNodes[i];
            xml += "<data>\r\n";
            xml += "<key><![CDATA[" + v.ChildNodes[0].InnerText + "]]></key>";
            xml += "<url><![CDATA[" + v.ChildNodes[1].InnerText + "]]></url>";
            xml += "<count><![CDATA[" + v.ChildNodes[2].InnerText + "]]></count>";
            xml += "<color><![CDATA[" + v.ChildNodes[0].Attributes["Color"].Value + "]]></color>";
            xml += "</data>\r\n";

        }
        xml += "</item>\r\n";
        xml += "</config>";
        link.LoadXml(xml);
        link.Save(HttpContext.Current.Server.MapPath("~/config/link.config"));
        
    }
%>