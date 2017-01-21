<%@ Page Language="C#" %>
<%
System.Data.SqlClient.SqlDataReader rs = null;
rs = Helper.Sql.ExecuteReader("select  custom,id from class ");
while (rs.Read())
{
    string content = rs[0].ToString().ToLower();
    
    
    Helper.Sql.ExecuteNonQuery("update class set custom=@content where id=@id", new System.Data.SqlClient.SqlParameter[]{
                    new System.Data.SqlClient.SqlParameter("content",content),
                    new System.Data.SqlClient.SqlParameter("id",rs[1])
                });
}
rs.Close();
%>