using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AuthtokenFramework;
using MWMS.DAL;

namespace ManagerFramework
{
    public class User
    {
        public double UserId { get; set; }
        public string Username { get; set; }
        public static string Reg(string username,string password,Dictionary<string,object> extendedField)
        {
            //new Reg(username, password, extendedField);
            //return Login(username,password);
            return "";
        }
        internal AuthToken Login(string username, string password)
        {
            return Login(username, password,0);
        }

        internal AuthToken Login(string username, string password,int expirationTime)
        {
            AuthToken authToken = AuthToken.Login(username, password, expirationTime);
            HttpContext.Current.Response.Cookies["M5_SessionId"].Value = authToken.Token;
            /*HttpCookie cook = new HttpCookie("m5_token");
            if (expirationTime > 0) cook.Expires = System.DateTime.Now.AddMinutes(expirationTime);
            cook.Value = login.Token;*/
            return authToken;
        }
        public static void Edit(string token, Dictionary<string, object> extendedField)
        {
            
        }
        public Dictionary<string, object> Get(string token)
        {
            return null;
            //Dictionary<string, object> data=UserFramework.User.
        }
    }
    public class LoginUser :User
    {
        public string Token = "";
        public LoginUser()
        {
        }
        public Dictionary<string,object> GetModel(string fields)
        {
            TableHandle table = new TableHandle("m_admin");
            return table.GetModel(this.UserId, fields);
        }
        public static LoginUser GetLoginUser()
        {
            LoginUser user = null;
            string token = (HttpContext.Current.Request.Cookies["M5_SessionId"] == null) ? "" : HttpContext.Current.Request.Cookies["M5_SessionId"].Value;
            AuthToken authToken= AuthToken.Create(token);
            //Login Login.GetLogin(token);
            if (authToken!=null) { 
                user=new LoginUser();
                user.Token = token;
                user.UserId = authToken.UserId;
                user.Username = authToken.Username;
            }
            return user;
        }
        public LoginUser(string username, string password)
        {
            AuthToken authToken=Login(username, password);
            Username = authToken.Username;
            UserId = authToken.UserId;
            Token = authToken.Token;
        }
        public LoginUser(string username, string password, int expirationTime)
        {
            AuthToken authToken = Login(username, password, expirationTime);
            Username = authToken.Username;
            UserId = authToken.UserId;
            Token = authToken.Token;
        }
        public LoginUser(string token)
        {

        }
        public void EditPassword( string oldPassword, string newPassword)
        {
            //User.EditPassword(token, oldPassword, newPassword);
        }
    }
}
