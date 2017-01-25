using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UserFramework;

namespace ManagerFramework
{
    public class User
    {
        public static string Reg(string username,string password,Dictionary<string,object> extendedField)
        {
            new Reg(username, password, extendedField);
            return Login(username,password);
        }
        public static void EditPassword(string token,string oldPassword,string newPassword)
        {
            
        }
        public static string Login(string username,string password)
        {
            Login login = new Login(username,password);
            return login.Token;
        }
        public static void Edit(string token, Dictionary<string, object> extendedField)
        {
            
        }
    }
}
