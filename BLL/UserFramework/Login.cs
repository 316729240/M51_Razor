﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成
//     如果重新生成代码，将丢失对此文件所做的更改。
// </auto-generated>
//------------------------------------------------------------------------------
namespace AuthtokenFramework
{
    using DBHelper;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 登录
    /// </summary>
    public class AuthToken
    {
		/// <summary>
		/// 登录用户
		/// </summary>
		public object User
		{
			get;
			set;
		}

		/// <summary>
		/// 登录钥匙
		/// </summary>
		public string Token
		{
			get;
			set;
		}

		/// <summary>
		/// 是否为管理员
		/// </summary>
		public object IsAdministrator
		{
			get;
			set;
		}

		/// <summary>
		/// 退出
		/// </summary>
		public void Exit()
		{
			throw new System.NotImplementedException();
		}

        /// <summary>
        /// 用户名密码登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public void Login(string username, string password)
        {

        }
        public static AuthToken Create(string token)
        {

        }
        public static AuthToken Login(string username, string password,int expirationTime)
        {
            AuthToken authToken = new AuthToken();
            //string ip = API.IPToNumber(HttpContext.Current.Request.UserHostAddress).ToString();
            //ErrInfo err = new ErrInfo();
            string ip = "";
            string pword2 = "";
            double userId = 0;
            #region 用户不存在
            SqlDataReader rs = SqlServer.ExecuteReader("select id,pword from [m_admin] where status=1 and uname=@uname", new SqlParameter[] { new SqlParameter("uname", username) });
            if (rs.Read())
            {
                pword2 = rs[1].ToString();
                userId = rs.GetDouble(0);
            }
            rs.Close();
            if (userId==0) throw new Exception("用户名或密码不正确");
            #endregion
            #region 出错次数过多ip封锁
            SqlServer.ExecuteNonQuery("delete from InvalidLogin where createdate<DATEADD(hh,-3,GETDATE()) ");//清空3天前的登陆错误日志
            rs = SqlServer.ExecuteReader("select ip from InvalidLogin where createdate>DATEADD(hh,-2,GETDATE()) and uname=@uname and ip=@ip and count>10", new SqlParameter[]{
                new SqlParameter("uname",username),
                new SqlParameter("ip",ip)
            });
            if (rs.Read()) {
                rs.Close();
                throw new Exception("由于您登录后台错误次数过多，系统自动屏蔽您的登录");
            }
            rs.Close();
            #endregion
            #region 密码错误
            if (pword2 != Common.Encryption(password))
            {
                SqlParameter[] p = new SqlParameter[] {
                            new SqlParameter ( "ip", ip ),
                            new SqlParameter ( "uname", username ) };
                rs = SqlServer.ExecuteReader("select ip from InvalidLogin where uname=@uname and ip=@ip", p);
                if (rs.Read())
                {
                    SqlServer.ExecuteNonQuery("update InvalidLogin set count=count+1 where uname=@uname and ip=@ip", p);
                }
                else
                {
                    SqlServer.ExecuteNonQuery("insert into InvalidLogin (ip,uname,count,createdate)values(@ip,@uname,1,getdate())", p);
                }
                rs.Close();
                throw new Exception("用户名或密码不正确");
            }
            #endregion
            authToken.Token =Common.GetId();// ; Guid.NewGuid().ToString();
            SqlServer.ExecuteNonQuery("delete from logininfo where logindate<DATEADD(hh, - 1, GETDATE()) or sessionId=@sessionId", new SqlParameter[]{
                        new SqlParameter("sessionId",authToken.Token),
                    });//删除超时用户及重登陆用户
            SqlServer.ExecuteNonQuery("insert logininfo (sessionId,ip,logindate,userid)values(@sessionId,@ip,GETDATE(),@userId)",
            new SqlParameter[]{
                        new SqlParameter("sessionId",authToken.Token),
                        new SqlParameter("ip",ip),
                        new SqlParameter("userId",userId)
                });
            return authToken;
        }

	}
}

