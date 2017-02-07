using System;
using System.Collections.Generic;
using System.Text;
using Helper;
using System.Web;
using System.Data.SqlClient;
using MWMS;

namespace MWMS
{
    /// <summary>
    ///LoginInfo 的摘要说明
    /// </summary>
    public class LoginInfo
    {

        public string sessionId = (HttpContext.Current.Request.Cookies["M5_SessionId"] == null) ? "" : HttpContext.Current.Request.Cookies["M5_SessionId"].Value; 
        public UserInfo value = null;
        public LoginInfo()
        {
            if (sessionId == "")
            {
                sessionId = API.GetId();
                HttpContext.Current.Response.Cookies["M5_SessionId"].Value = sessionId;
            }
            SqlDataReader rs2 = Sql.ExecuteReader("select A.userid,A.logindate from logininfo A where    expirationTime>getdate() and sessionId=@sessionId", new SqlParameter[] { new SqlParameter("sessionId", sessionId) });
            if (rs2.Read())
            {
                value = UserClass.get(rs2.GetDouble(0));
                DateTime loginDate = rs2.GetDateTime(1);
                    if ((System.DateTime.Now - loginDate).TotalMinutes > 40) {//间隔40分钟更新一次
                        Sql.ExecuteNonQuery("update logininfo set logindate=getdate()  where sessionId=@sessionId", new SqlParameter[] { new SqlParameter("sessionId", sessionId) });
                    }
            }
            rs2.Close();
        }
        /// <summary>
        /// 是否是后台登陆
        /// </summary>
        /// <returns></returns>
        public bool isManagerLogin()
        {
            return value != null && value.classId==0;
        }
        /// <summary>
        /// 是否登陆
        /// </summary>
        /// <returns></returns>
        public bool isLogin()
        {
            return (value != null);
        }
        public void checkLogin()
        {
            if (!isLogin())
            {
                HttpContext.Current.Response.Write("{\"errNo\":-1000}");
                HttpContext.Current.Response.End();
            }
        }
        public void checkManagerLogin()
        {
            if (!isManagerLogin())
            {
                HttpContext.Current.Response.Write("{\"errNo\":-1000,\"errMsg\":\"没有登陆\"}");
                HttpContext.Current.Response.End();
            }
        }
        public bool exit()
        {
            Sql.ExecuteNonQuery("delete from logininfo where sessionId=@sessionId", new SqlParameter[] { new SqlParameter("sessionId", sessionId) });
            HttpCookie MyCo = HttpContext.Current.Request.Cookies["M5_Login"];
            if (MyCo != null)
            {
                MyCo.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(MyCo);
            }
            MyCo = HttpContext.Current.Request.Cookies["M5_Login"];
            if (MyCo != null)
            {
                MyCo.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(MyCo);
            }
            MyCo = HttpContext.Current.Request.Cookies["u_name"];
            if (MyCo != null)
            {
                MyCo.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(MyCo);
            }
            MyCo = HttpContext.Current.Request.Cookies["u_id"];
            if (MyCo != null)
            {
                MyCo.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(MyCo);
            }
            return true;
        }
    }
}