<%@ Page Language="C#" %>
<%     
    System.Data.SqlClient.SqlDataReader rs = null;
    rs = Helper.Sql.ExecuteReader("select id,classid from class where classid=7");
    while (rs.Read())
    {
        
        Response.Write(rs[0].ToString()+"<br>");
        MWMS.ColumnClass.reset(rs.GetDouble(0));
    }
    rs.Close();
   %>