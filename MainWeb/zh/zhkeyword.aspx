<%@ Page Language="C#" %>
<%
    System.Data.SqlClient.SqlDataReader rs = null;
       rs = Helper.Sql.ExecuteReader("select id,u_keyword from article where  u_keyword<>''");
        while (rs.Read())
        {
            MWMS.RecordClass.addKeyword(rs.GetDouble(0), rs[1] + "");
        }
        rs.Close();
%>