<%@ Page Language="C#" %>
<%
    System.Data.SqlClient.SqlDataReader rs = null;
    rs = Helper.Sql.ExecuteReader("select id,classid from class where rootid in (select id from class where classid=7)");
    while (rs.Read())
    {
        Response.Write(rs[0].ToString()+"<br>");
        MWMS.ColumnClass.resetContentUrl(rs.GetDouble(0));
    }
    rs.Close();
   %>