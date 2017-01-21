using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace MWMS
{
    public class LabelEngine
    {
        public static string getPageUrl(SqlDataReader rs){
            string url = rs["path"].ToString() + @"/" + rs["id"].ToString() + ".html";
            return url;
        }
    }
}
