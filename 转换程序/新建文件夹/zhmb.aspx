<%@ Page Language="C#" %>
<%
    System.Data.SqlClient.SqlDataReader rs = null;
    rs = Helper.Sql.ExecuteReader("select  u_content,id from htmltemplate ");
    while (rs.Read())
    {
        string content = rs[0].ToString();
        content = view(content);
        content = label(content);
        content = content.Replace("{if ", "{if(");
        content = content.Replace(" then", ")}");
        content = content.Replace("{end if}", "{/if}");
        content = content.Replace("$Parameter1$", "$parameter1");
        content = content.Replace("$Parameter2$", "$parameter2");
        content = content.Replace("$Parameter3$", "$parameter3");
        content = content.Replace("$Parameter4$", "$parameter4");
        //Response.Write(content);
        Response.Write(rs[1] + "<br>");
        Helper.Sql.ExecuteNonQuery("update htmltemplate set u_content=@content where id=@id", new System.Data.SqlClient.SqlParameter[]{
                    new System.Data.SqlClient.SqlParameter("content",content),
                    new System.Data.SqlClient.SqlParameter("id",rs[1])
                });
    }
    rs.Close();
    rs = Helper.Sql.ExecuteReader("select u_html,id from dataview ");
    while (rs.Read())
    {
        string content = rs[0].ToString();
        content = view(content);
        content = label(content);
        content = content.Replace("{if ", "{if(");
        content = content.Replace(" then", ")}");
        content = content.Replace("{end if}", "{/if}");
        content = content.Replace("$Parameter1$", "$parameter1");
        content = content.Replace("$Parameter2$", "$parameter2");
        content = content.Replace("$Parameter3$", "$parameter3");
        content = content.Replace("$Parameter4$", "$parameter4");
        //Response.Write(content);
        Response.Write(rs[1] + "<br>");
        Helper.Sql.ExecuteNonQuery("update dataview set u_html=@content where id=@id", new System.Data.SqlClient.SqlParameter[]{
                    new System.Data.SqlClient.SqlParameter("content",content),
                    new System.Data.SqlClient.SqlParameter("id",rs[1])
                });
    }
    rs.Close();
%>
%>
<script runat=server>
    string label(string _html)
    {
        Regex r = null;
        r = new Regex(@"(<|&lt;)!-- #Label[\s\S]*?LabelID[\s\S]*?--(>|&gt;)", RegexOptions.IgnoreCase);
        MatchCollection mc = r.Matches(_html);
        //for (int n = 0; n <mc.Count; n++)
        Response.Write(mc.Count.ToString());
        for (int n = 0; n < mc.Count; n++)
        {
            string fieldList = "url";
            string item = MWMS.API.GetStrFG(mc[n].Value, "<HtmlTemplate>", "</HtmlTemplate>");
            item=item.Replace("$WebPageUrl$","$url");
            item=item.Replace("$Index$","$index");
            Regex r2 = new Regex(@"{FieldName(.*?)}", RegexOptions.IgnoreCase);
            MatchCollection mc2 = r2.Matches(item);
            for (int i = 0; i < mc2.Count; i++)
            {
                string fieldName = MWMS.API.GetHTMLValue(mc2[i].Value, "FieldName");
                string Length = MWMS.API.GetHTMLValue(mc2[i].Value, "Length");
                string format = MWMS.API.GetHTMLValue(mc2[i].Value, "Format");
                switch(fieldName){
                    case "Info":
                        fieldName = "u_info";
                        break;
                    case "Title":
                        fieldName = "title";
                        break;
                    case "Content":
                        fieldName = "u_content";
                        break;
                }
                if ((fieldList + ",").IndexOf("," + fieldName + ",") == -1)
                {
                    fieldList += ","+fieldName;
                }
                if (Length != "")
                {
                    item = item.Replace(mc2[i].Value, "${api.left(" + fieldName + "," + Length + ")}");
                }
                else if (format != "")
                {
                    item = item.Replace(mc2[i].Value, "${api.format(" + fieldName + ",'" + format + "')}");
                }else
                {
                    item = item.Replace(mc2[i].Value, "${" + fieldName + "}");
                }
            }
            string html = "<!-- #Label#\n" + 
                "labelId=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "labelId") + "\n" +
            "moduleId=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "ModuleID") + "\n" +
"classId=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "ClassID") + "\n" +
"subClassTag=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "SubClassTag") + "\n" +
"orderby=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "SelectWhere") + "\n" +
"fields=" + fieldList + "\n" +
"attribute=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "Attribute") + "\n" +
"datatypeId=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "DataTypeID") + "\n" +
"recordCount=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "RecordCount") + "\n" +
"pageSize=" + MWMS.TemplateEngine.getFieldString(mc[n].Value, "PageSize") + "\n" +
"<htmlTemplate>" +
item+
"</htmlTemplate>-->";
            _html = _html.Replace(mc[n].Value,html);
        }
        return _html;
    }
    string view(string _html)
    {
        Regex r = null;
        r = new Regex(@"{ViewName(.*?)}", RegexOptions.IgnoreCase);
        MatchCollection mc = r.Matches(_html);
        for (int n = 0; n < mc.Count; n++)
        {
            string viewId = MWMS.API.GetHTMLValue(mc[n].Value, "ViewID");
            string viewValues = MWMS.API.GetHTMLValue(mc[n].Value, "values");
            System.Data.SqlClient.SqlDataReader rs = Helper.Sql.ExecuteReader("select 'view.'+B.className+'.'+A.title from dataview A,class B where A.classid=B.id and A.id="+viewId);
            if (rs.Read())
            {
                _html = _html.Replace(mc[n].Value, "${"+rs[0]+"("+viewValues+")}");

                //Response.Write(mc[n].Value + "<br>");
                //Response.Write("{" + rs[0] + "(" + viewValues + ")}" + "<br>");
            }
            rs.Close();
            
        }
        return _html;
    }
</script>