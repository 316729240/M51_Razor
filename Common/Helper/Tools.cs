using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        public static string GetAppSettings(string name)
        {
            string value = "";
            if (ConfigurationManager.AppSettings[name] != null)
            {
                value = ConfigurationManager.AppSettings[name];
            }
            return (value);
        }

        /// <summary>
        /// 将变量写入文件
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="data">变量名</param>
        public static void writeObjectFile(string file, object data)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            formatter.Serialize(stream, data);
            stream.Close();
        }
        /// <summary>
        /// 将变量从文件中读出
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns></returns>
        public static object readObjectFile(string file)
        {
            object data = null;
            if (System.IO.File.Exists(file))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream2 = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                data = formatter.Deserialize(stream2);
                stream2.Close();
            }
            return data;
        }
    }
}
