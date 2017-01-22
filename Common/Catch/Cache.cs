using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace Helper
{
    /// <summary>
    ///Catch 的摘要说明
    /// </summary>
    public class Cache
    {
        public object data = null;
        public string file = "";
        public string[] p;
        public int cacheN = 0;
        string cachePath = "/catch/";
        public Cache(string name, string[] p, int cacheN, string _cachePath)
        {
            file = name;
            this.p = p;
            this.cacheN = cacheN;
            this.cachePath = _cachePath;
        }
        /// <summary>
        /// 将变量写入文件
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="data">变量名</param>
        public static void writeObjectFile(string file, object data)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
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
            IFormatter formatter = new BinaryFormatter();
            Stream stream2 = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
            data = formatter.Deserialize(stream2);
            stream2.Close();
            return data;
        }
        /// <summary>
        /// 设置缓存内容
        /// </summary>
        /// <param name="text"></param>
        public void set(object _data)
        {
            string str = file;
            if (p != null)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    str += p[i].Trim() + "|";
                }
            }
            string md5 = str.MD5();
            string path = System.Web.HttpContext.Current.Server.MapPath("~" + cachePath + file.Replace(".html", ""));
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
            string fullfile = path + @"\" + md5 + ".cac";
            bool tag = true;//缓存是否过期
            writeObjectFile(fullfile, _data);
        }
        /// <summary>
        /// 获取缓存是否过期
        /// </summary>
        /// <returns></returns>
        public bool get()
        {
            string str = file;
            if (p != null)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    str += p[i].Trim() + "|";
                }
            }
            string md5 = str.MD5();
            string path = System.Web.HttpContext.Current.Server.MapPath("~" + cachePath + file.Replace(".html", ""));
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
            string fullfile = path + @"\" + md5 + ".cac";
            bool tag = true;//缓存是否过期
            FileInfo f = new FileInfo(fullfile);
            if (f.Exists)
            {
                System.TimeSpan n1 = System.DateTime.Now - f.LastWriteTime;
                if (n1.TotalMinutes < cacheN)//缓存是否超过设置周期
                {
                    tag = false;
                }
            }
            if (!tag)//缓存过期
            {
                data = readObjectFile(fullfile);
            }
            return (tag);
        }
    }
}