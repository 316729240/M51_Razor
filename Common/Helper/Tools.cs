using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper
{
    public class Tools
    {
        static int addDataCounter=0;
        #region 生成一个随机ID
        public static string GetId()
        {
            return (GetId(0));
        }
        public static string GetId(int n)
        {
            if (addDataCounter > int.MaxValue - 100) addDataCounter = 0;
            addDataCounter++;
            string id;
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            //id = ((long)((System.DateTime.Now.ToOADate() - 39781) * 1000000) - 432552).ToString() + rnd.Next(99).ToString("D2");
            //long webid = long.Parse(Config.webId.Substring(0, Config.webId.Length-2));
            id = ((System.DateTime.Now.Ticks - System.DateTime.Parse("2012-8-1").Ticks) / 10000000 + addDataCounter).ToString() + rnd.Next(99).ToString("D2");
            return (id);
        }
        #endregion
    }
}
