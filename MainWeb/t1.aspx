<%@ Page Language="C#" %>
<%@ Import Namespace="MWMS" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%
    if (Request.Cookies["u_id"] == null) return;
    string userId = Request.Cookies["u_id"].Value+"";
    string userName = Request.Cookies["u_name"].Value+"";
    System.Data.DataTable rs = Helper.Sql.ExecuteDataset("select domainName,classname from [class] where classid=7").Tables[0];
    int index = 8;
    "dd".Substring()
    for (int i = 0; i < 10; i++)
    {
        index++;
        if (index >= rs.Rows.Count) index = 0;
        %><a href="<% =rs.Rows[index][0] %>"><% =rs.Rows[index][1] %></a> <%
    }

    Helper.Sql.ExecuteNonQuery("insert into u_accessData (id,u_url,u_title,u_userId,u_userName,u_createDate)values(@id,@u_url,@u_title,@userId,@userName,getdate())",new SqlParameter[] {
        new SqlParameter("id",API.GetId()),
        new SqlParameter("u_url",Request["url"]),
        new SqlParameter("u_title",Request["title"]),
        new SqlParameter("userId",userId),
        new SqlParameter("userName",userName)
    } );
%>
