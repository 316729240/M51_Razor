using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data;
using System.Configuration;
using System.Xml;
using System.Web;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Helper;
using System.Net.Mail;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace MWMS
{

    public class API
    {
        public struct thumbnailSize
        {
            public int width;
            public int height;
            public bool force;
            public bool saveRemoteImages;//是否保存远程图片
        }
        public struct RS
        {
            public int PageCount;
            public int RecordCount;
            public SqlDataReader rs;
        }
        public struct LoginInfo
        {
            public string UserName;
            public string LoginPword;
            public bool Tag;
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
        public static ErrInfo saveImage(HttpPostedFile file,string filePath)
        {
            ErrInfo err = new ErrInfo();

            try
            {

                string path = Config.webPath + filePath + System.DateTime.Now.ToString("yyyy-MM/");
                if (!System.IO.Directory.Exists(HttpContext.Current.Server.MapPath(path))) System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));
                string kzm = "";
                if (file.FileName.LastIndexOf(".") > -1) kzm = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1).ToLower();
                if (!Regex.IsMatch(kzm, "(jpg|gif|png)"))
                {
                    err.errNo = -1;
                    err.errMsg = "文件类型不合法，只能上传jpg,gif,png";
                    return err;
                }
                string fileName = API.GetId() + "." + kzm;
                file.SaveAs(HttpContext.Current.Server.MapPath(path + fileName));
                err.userData = path + fileName;
                return err;
            }catch(Exception ex)
            {
                err.errNo=-1;
                err.errMsg = ex.Message;
                return err;
            }
        }

        public static string getAppSettings(string name)
        {
            string value = "";
            if (ConfigurationManager.AppSettings[name] != null)
            {
                value = ConfigurationManager.AppSettings[name];
            }
            return (value);
        }

        public static bool safetyVerification(string Str)
        {
            bool ReturnValue = true;
            try
            {
                if (Str != "")
                {
                    Regex regex = new Regex(@"\b(or|and|exec|insert|select|delete|update|count|chr|mid|master|truncate|char|declare)\b", RegexOptions.IgnoreCase|RegexOptions.Multiline);
                    if (regex.Matches(Str).Count > 0)
                    {
                        return false;
                    }
                    //string SqlStr = "/**/|or |and |exec |insert |select |delete |update |count | * |chr |mid |master |truncate |char |declare ";
                    //string[] anySqlStr = SqlStr.Split('|');
                    //string str = Str.ToLower();
                    //foreach (string ss in anySqlStr)
                    //{
                    //    if (str.IndexOf(ss) >= 0)
                    //    {
                    //        //ReturnValue = false;
                    //        return false;
                    //    }
                    //}
                }
            }
            catch
            {
                ReturnValue = false;
            }
            return ReturnValue;
        }
        public static string replaceSystemVariables(string name)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(HttpContext.Current.Server.MapPath("~/config/SystemVariables.xml"));
            XmlNodeList root = doc.DocumentElement.ChildNodes;
            for (int n = 0; n < root.Count; n++)
            {
                if (root.Item(n).ChildNodes.Item(1).InnerText == name) return root.Item(n).ChildNodes.Item(2).InnerText;
            }
            return "";
        }
        public static bool isMobileAccess()
        {
            string u = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] + "";
            Regex b = new Regex(@"android.+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (u.Length > 4 && (b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool getWebFAId()
        {
            //return true;
            if (BaseConfig.mobileUrl!=null)
            {
                if (HttpContext.Current.Request.Url.ToString().ToLower().IndexOf(BaseConfig.mobileUrl.ToString()) == 0) return true;
            }
            return false;
        }
        public static thumbnailSize getThumbnailSize(double classId)
        {
            thumbnailSize size;
            size.width = 0;
            size.height = 0;
            size.force = false;
            size.saveRemoteImages = true;
            double moduleId = 0, parentId = 0;
            string inherit = "";//为空或1时继承上级选项
            SqlDataReader rs = Helper.Sql.ExecuteReader("select classid,moduleid,thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,inherit from class where id=" + classId.ToString());
            if (rs.Read())
            {
                if (rs[2].ToString() != "") size.width = int.Parse(rs[2].ToString());
                if (rs[3].ToString() != "") size.height = int.Parse(rs[3].ToString());
                size.force = (rs[4].ToString() == "1");
                size.saveRemoteImages = !(rs[5].ToString() == "-1");
                moduleId = rs.GetDouble(1);
                parentId = rs.GetDouble(0);
                inherit = rs[6].ToString();
            }
            rs.Close();
            if ((inherit == "" || inherit == "1") && parentId >0)
            {
                if (parentId ==7)
                {
                    rs = Helper.Sql.ExecuteReader("select thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages from module where id=" + moduleId.ToString());
                    if (rs.Read())
                    {
                        if (rs[0].ToString() != "") size.width = int.Parse(rs[0].ToString());
                        if (rs[1].ToString() != "") size.height = int.Parse(rs[1].ToString());
                        size.force = (rs[2].ToString() == "1");
                        size.saveRemoteImages = !(rs[3].ToString() == "-1");
                    }
                    rs.Close();
                }
                else
                {
                    size = getThumbnailSize(parentId);
                }
            }
            return (size);
        }
        #region 取得反链
        public static int GetLink(string SS, string Url)
        {
            int C = 0;
            string Html;
            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                if (SS == "Baidu")
                {
                    Byte[] pageData = MyWebClient.DownloadData("http://www.baidu.com/s?wd=domain%3A" + Url);
                    Html = Encoding.Default.GetString(pageData);
                    C = int.Parse(API.GetStrFG(Html, "百度一下，找到相关网页", "篇").Replace(",", "").Replace("约", ""));
                }
                else if (SS == "Google")
                {
                    Byte[] pageData = MyWebClient.DownloadData("http://www.google.com.hk/search?q=link%3A" + Url + "");
                    Html = Encoding.UTF8.GetString(pageData);
                    string v1 = API.GetStrFG(Html, "<div id=resultStats>", "<nobr>");
                    string v2 = API.GetStrFG(v1, " ", " ");
                    if (v2 == "") v1 = API.GetStrFG(v1, "", " ");
                    else { v1 = v2; }
                    if (v1 == "") v1 = API.GetStrFG(Html, "<div id=resultStats>(.*) ", " ");
                    C = int.Parse(v1.Replace(",", ""));
                }
                if (SS == "Yahoo")
                {
                    Byte[] pageData = MyWebClient.DownloadData("http://sitemap.cn.yahoo.com/search?bwm=i&bwmf=s&bwmo=d&p=" + Url + "");
                    Html = Encoding.UTF8.GetString(pageData);
                    C = int.Parse(API.GetStrFG(Html, "共 <strong>", "</strong> 条").Replace(",", ""));
                }
            }
            catch { C = -1; }
            finally
            {
                MyWebClient.Dispose();
            }
            return C;
        }
        #endregion

        public static void directoryCopy(string sourceDirectory, string targetDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                return;
            }
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            DirectoryInfo sourceInfo = new DirectoryInfo(sourceDirectory);
            FileInfo[] fileInfo = sourceInfo.GetFiles();
            foreach (FileInfo fiTemp in fileInfo)
            {
                File.Copy(sourceDirectory + "\\" + fiTemp.Name, targetDirectory + "\\" + fiTemp.Name, true);
            }
            DirectoryInfo[] diInfo = sourceInfo.GetDirectories();
            foreach (DirectoryInfo diTemp in diInfo)
            {
                string sourcePath = diTemp.FullName;
                string targetPath = diTemp.FullName.Replace(sourceDirectory, targetDirectory);
                Directory.CreateDirectory(targetPath);
                directoryCopy(sourcePath, targetPath);
            }
        }
        public static string GetUpdateDate(string url)
        {
            WebClient MyWebClient = new WebClient();
            string Html;
            Byte[] pageData = MyWebClient.DownloadData("http://www.baidu.com/s?wd=http://" + url);
            Html = Encoding.Default.GetString(pageData);
            //return(Html);
            return (API.GetStrFG(Html, "</b>/ ", "  </span> - "));
        }
        public static int GetBaiduUpdataCount(string Url, int day)
        {
            string Html = "";

            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            int C = -1;
            try
            {
                Byte[] pageData = MyWebClient.DownloadData("http://www.baidu.com/s?lm=" + day.ToString() + "&wd=site%3A" + Url);
                Html = Encoding.Default.GetString(pageData);
                C = int.Parse(API.GetStrFG(Html, "找到相关结果", "个").Replace(",", "").Replace("约", ""));
            }
            catch { }
            return (C);
        }
        //取得域名
        public static string getDomain(string url)
        {
            Regex r = new Regex(@"(\w+\.(com.cn|net.cn|com|cn|net|org))", RegexOptions.IgnoreCase);
            MatchCollection M = r.Matches(url);
            if (M.Count > 0)
            {
                return (M[0].Value);
            }
            else
            {
                return (url);
            }
        }
        public static string readUrl(string url, Encoding Encoding)
        {

            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            try
            {
                Byte[] pageData = MyWebClient.DownloadData(url);
                if (Encoding == null) return (Encoding.Default.GetString(pageData));
                else { return (Encoding.GetString(pageData)); }

            }
            catch
            {
                return ("");
            }
        }
        public static string readUrl(string url)
        {
            return (readUrl(url, null));
        }
        public static string watermark(string oldfilename)
        {

            XmlNodeList list = Config.userConfig["watermark"];
            if (list == null) return oldfilename;
            string markpic = HttpContext.Current.Server.MapPath(list[0].InnerText);
            if (!System.IO.File.Exists(markpic)) return oldfilename;

            string _fileName = oldfilename;
            oldfilename = HttpContext.Current.Server.MapPath("~" + oldfilename);
            string filename = oldfilename;
            FileInfo f = new FileInfo(oldfilename);
            int zl = int.Parse(list[6].InnerText); 


            try
                    {
                    float proportion = float.Parse(list[5].InnerText), transparency = float.Parse(list[4].InnerText) / 100;
                        int margins = int.Parse(list[3].InnerText), X = int.Parse(list[1].InnerText), Y = int.Parse(list[2].InnerText);
                        System.Drawing.Image copyImage = System.Drawing.Image.FromFile(markpic);
                        System.Drawing.Image image = System.Drawing.Image.FromFile(oldfilename);
                        Bitmap bm = new Bitmap(image);
                        image.Dispose();
                        if ((bm.Width * proportion) > copyImage.Width && (bm.Height * proportion) > copyImage.Height)
                        {
                            if (list[7].InnerText == "1")
                            {
                                filename = f.Directory.FullName + @"\" + f.Name.Replace(f.Extension, "") + "_1" + f.Extension;
                                _fileName = _fileName.Replace(f.Name, f.Name.Replace(f.Extension, "") + "_1" + f.Extension);
                            }
                    int top = margins, left = margins;
                            Graphics g = Graphics.FromImage(bm);
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            ColorMatrix cmatrix = new ColorMatrix();
                            cmatrix.Matrix33 = transparency;
                            ImageAttributes imgattributes = new ImageAttributes();
                            imgattributes.SetColorMatrix(cmatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                            if (X == 2) left = bm.Width - copyImage.Width - margins;
                            else if (X == 1) { left = (bm.Width - copyImage.Width) / 2; }
                            if (Y == 2) top = bm.Height - copyImage.Height - margins;
                            else if (Y == 1) { top = (bm.Height - copyImage.Height) / 2; }
                            g.DrawImage(copyImage, new Rectangle(left, top, copyImage.Width, copyImage.Height), 0, 0, copyImage.Width, copyImage.Height, GraphicsUnit.Pixel, imgattributes);
                            g.Dispose();
                            MemoryStream ms = new MemoryStream();

                            // 以下代码为保存图片时，设置压缩质量
                            EncoderParameters encoderParams = new EncoderParameters();
                            long[] quality = new long[1];
                            quality[0] = zl;

                            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                            encoderParams.Param[0] = encoderParam;

                            //获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
                            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                            ImageCodecInfo jpegICI = null;
                            for (int x = 0; x < arrayICI.Length; x++)
                            {
                                if (arrayICI[x].FormatDescription.Equals("JPEG"))
                                {
                                    jpegICI = arrayICI[x];//设置JPEG编码
                                    break;
                                }
                            }
                            if (jpegICI != null)
                            {
                                bm.Save(filename, jpegICI, encoderParams);
                            }
                            else
                            {
                                bm.Save(filename);
                            }
                    }
                    bm.Dispose();
                    copyImage.Dispose();
            }
                    catch
                    {
                    }
            return _fileName;
        }
        public struct ImgSize
        {
            public int Height;
            public int Width;
            public int OldHeight;
            public int OldWidth;
        }

        public static ImgSize GetPictureSize(string PicPath, float MaxWidth, float MaxHeight)
        {
            return (GetPictureSize(PicPath, MaxWidth, MaxHeight, "min"));
        }
        public static ImgSize GetPictureSize(string PicPath, float MaxWidth, float MaxHeight, string tag)
        {
            PicPath = HttpContext.Current.Server.MapPath("~" + PicPath);
            ImgSize V;
            V.Height = 0;
            V.Width = 0;
            V.OldHeight = 0;
            V.OldWidth = 0;
            double BL = 1;
            //try
            //{
            if (File.Exists(PicPath))
            {

                System.Drawing.Image myImage = System.Drawing.Image.FromFile(PicPath);
                #region
                double OldWidth = myImage.Width, OldHeight = myImage.Height, NewWidth = MaxWidth, NewHeight = MaxHeight;
                //double OldWidth = 960, OldHeight = 670, NewWidth = MaxWidth, NewHeight = MaxHeight;
                int NewWidth2 = (int)NewWidth, NewHeight2 = (int)NewHeight;
                if (tag == "min")
                {
                    if (OldWidth > NewWidth || OldHeight > NewHeight)
                    {
                        BL = OldWidth / NewWidth;
                        NewHeight2 = (int)(OldHeight / BL);
                        if (NewHeight2 <= NewHeight)
                        {
                            V.Height = NewHeight2;
                            V.Width = (int)NewWidth;
                        }
                        BL = OldHeight / NewHeight;
                        NewWidth2 = (int)(OldWidth / BL);
                        if (NewWidth2 <= NewWidth)
                        {
                            if (NewWidth2 > V.Width)
                            {
                                V.Height = (int)NewHeight;
                                V.Width = NewWidth2;
                            }
                        }
                    }
                    else
                    {
                        V.Height = (int)OldHeight;
                        V.Width = (int)OldWidth;
                    }
                }
                else
                {
                    //throw new NullReferenceException(
                    //NewWidth = 126; NewHeight = 1250;
                    //OldWidth = 1; OldHeight = 200;

                    BL = OldWidth / NewWidth;
                    V.Height = (int)(OldHeight / BL);
                    V.Width = (int)(OldWidth / BL);
                    if (V.Height < NewHeight || V.Width < NewWidth)
                    {
                        BL = OldHeight / NewHeight;
                        V.Height = (int)(OldHeight / BL);
                        V.Width = (int)(OldWidth / BL);
                    }


                }
                #endregion
                myImage.Dispose();
            }
            //}
            //catch{}

            return (V);
        }
        public static string PictureSize(string fileName, string newFile, int maxWidth, int maxHeight, int PZ)
        {
            return (PictureSize(fileName, newFile, maxWidth, maxHeight, PZ, false));
        }
        public static string PictureSize(string fileName, string newFile, int maxWidth, int maxHeight, int PZ, bool tag)
        {
            if (maxWidth == 0 || maxHeight == 0) return fileName;
            if (System.IO.File.Exists(HttpContext.Current.Server.MapPath("~" + fileName)))
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("~" + fileName));
                if ((img.Width > maxWidth && img.Height > maxHeight) || (tag && (img.Width != maxWidth || img.Height != maxHeight)))
                {
                    System.Drawing.Imaging.ImageFormat thisFormat = img.RawFormat;
                    string t = "min";
                    if (tag) t = "max";
                    ImgSize newSize = GetPictureSize(fileName, maxWidth, maxHeight, t);

                    if (newSize.Width == img.Width && newSize.Height == img.Height)
                    {
                        img.Dispose();
                        return (fileName);
                    }
                    int CanvasWidth = newSize.Width, CanvasHeight = newSize.Height, CanvasTop = 0, CanvasLeft = 0;
                    if (tag)
                    {
                        CanvasWidth = maxWidth; CanvasHeight = maxHeight;
                        if (newSize.Width > maxWidth) CanvasLeft = (newSize.Width - maxWidth) / 2;
                        //if (newSize.Height > maxHeight) CanvasTop = (newSize.Height - maxHeight) / 2;
                    }
                    Bitmap outBmp = new Bitmap(CanvasWidth, CanvasHeight);
                    Graphics g = Graphics.FromImage(outBmp);

                    // 设置画布的描绘质量
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //throw new NullReferenceException(newSize.Width.ToString() + "," + newSize.Height.ToString() + "," + CanvasLeft.ToString());
                    g.DrawImage(img, new Rectangle(-CanvasLeft, -CanvasTop, newSize.Width, newSize.Height),
                        0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                    g.Dispose();

                    // 以下代码为保存图片时，设置压缩质量
                    EncoderParameters encoderParams = new EncoderParameters();
                    long[] quality = new long[1];
                    quality[0] = PZ;

                    EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    encoderParams.Param[0] = encoderParam;

                    //获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICI = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICI = arrayICI[x];//设置JPEG编码
                            break;
                        }
                    }
                    img.Dispose();
                    if (jpegICI != null)
                    {
                        outBmp.Save(HttpContext.Current.Server.MapPath("~" + newFile), jpegICI, encoderParams);
                    }
                    else
                    {
                        outBmp.Save(HttpContext.Current.Server.MapPath("~" + newFile), thisFormat);
                    }
                    outBmp.Dispose();
                    return (newFile);
                }
                return fileName;

            }
            return ("");

        }
        public static void loginErr(string uname)
        {
            string ip = API.IPToNumber(HttpContext.Current.Request.UserHostAddress).ToString();
            loginErr(uname, ip);
        }
        public static void loginErr(string uname, string ip)
        {
            SqlDataReader rs = Sql.ExecuteReader("select ip from InvalidLogin where ip=" + ip + " and uname='" + uname + "'");
            if (rs.Read())
            {
                Helper.Sql.ExecuteNonQuery("update InvalidLogin set count=count+1 where  ip=" + ip + " and uname='" + uname + "'");
            }
            else
            {
                Helper.Sql.ExecuteNonQuery("insert into InvalidLogin (ip,uname,count,createdate)values(" + ip + ",'" + uname + "',1,getdate())");
            }
            rs.Close();
        }
        public static void writeCreateCache(string type, string msg)
        {
            return;
            //type: 0登陆  1操作  2错误  3危险  4系统

                string dir = "";
                if (HttpContext.Current == null) dir = HttpRuntime.AppDomainAppPath + @"manage\log\cache\";
                else
                {
                    dir = System.Web.HttpContext.Current.Server.MapPath("~/manage/log/cache/");
                }
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch
                    {
                        HttpContext.Current.Response.Write("创建日志文件夹失败!您可以手动进行创建" + HttpContext.Current.Server.MapPath("~/manage/log/"));
                        HttpContext.Current.Response.End();
                    }
                }
                try
                {
                    StringBuilder text = new StringBuilder(DateTime.Now.ToString());
                    StreamWriter f = new StreamWriter(dir + "/" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + ".log", true, Encoding.GetEncoding("GB2312"));
                    text.Append("   ");
                    text.Append(type);
                    text.Append("   ");
                    string username = "未知用户";
                    if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["AdminUsername"] != null) username = HttpContext.Current.Request.Cookies["AdminUsername"].Value;
                    text.Append(username);
                    text.Append("   ");
                    try
                    {
                        if (HttpContext.Current != null) text.Append(HttpContext.Current.Request.Url.Host);
                    }
                    catch { }
                    text.Append("   ");
                    text.Append(msg);
                    text.Append("           ");
                    try
                    {
                        if (HttpContext.Current != null) text.Append(HttpContext.Current.Request.RawUrl);
                    }
                    catch { }
                    f.WriteLine(text.ToString());

                    f.Close();
                }
                catch
                {
                    try
                    {
                        //  HttpContext.Current.Response.Write("创建日志文件失败!");
                        //   HttpContext.Current.Response.End();
                    }
                    catch { }
                }
        }
        public static void writeLog(string type, string msg)
        {
            //type: 0登陆  1操作  2错误  3危险  4系统

                string dir = "";
                if (HttpContext.Current == null) dir = HttpRuntime.AppDomainAppPath + @"manage\log\"+type+@"\";
                else
                {
                    dir = System.Web.HttpContext.Current.Server.MapPath("~/manage/log/"+type+@"/");
                }
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch
                    {
                        HttpContext.Current.Response.Write("创建日志文件夹失败!您可以手动进行创建" + HttpContext.Current.Server.MapPath("~/manage/log/"));
                        HttpContext.Current.Response.End();
                    }
                }
                try
                {
                    string username = "未知用户";
                    StringBuilder text = new StringBuilder(DateTime.Now.ToString());

                    if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["AdminUsername"] != null) username = HttpContext.Current.Request.Cookies["AdminUsername"].Value;
                    StreamWriter f = new StreamWriter(dir + "/" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + ".log", true, Encoding.GetEncoding("GB2312"));
                    text.Append("   ");
                    text.Append(username);
                    text.Append("   ");
                    try
                    {
                        if (HttpContext.Current != null)
                        {
                            if (HttpContext.Current.Request.Headers["X-Forwarded-For"] != null)
                            {
                                text.Append(HttpContext.Current.Request.Headers["X-Forwarded-For"].ToString());
                            }
                            else
                            {
                                text.Append(HttpContext.Current.Request.UserHostAddress);
                            }
                        }
                    }
                    catch { }
                    text.Append("   ");
                    text.Append(type);
                    text.Append("   ");
                    text.Append(msg);
                    text.Append("   ");
                    try
                    {
                        if (HttpContext.Current != null) text.Append(HttpContext.Current.Request.UrlReferrer.ToString());
                    }
                    catch { }
                    string adminid = "-1", Permissions = "";
                    if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["AdminUserID"] != null)
                    {
                        adminid = HttpContext.Current.Request.Cookies["AdminUserID"].Value;
                        Permissions = HttpContext.Current.Request.Cookies["Permissions"].Value;
                    }
                    text.Append("   ");
                    text.Append(adminid);
                    text.Append("   ");
                    text.Append(Permissions);
                    f.WriteLine(text.ToString());

                    f.Close();
                }
                catch
                {
                    try
                    {
                        //  HttpContext.Current.Response.Write("创建日志文件失败!");
                        //   HttpContext.Current.Response.End();
                    }
                    catch { }
                }

        }
        public static bool isNumeric(string str)
        {
            Regex reg1 = new Regex(@"^[-]?(\d+\.?\d*|\.\d+)$");
            return reg1.IsMatch(str);
        }

        #region 添加js引用
        public static void addJS(string Name, string js)
        {
            string filename = System.Web.HttpContext.Current.Server.MapPath(@"~\config\" + Name + ".xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList root = doc.DocumentElement.ChildNodes;
            bool tag = false;
            for (int n = 0; n < root.Count; n++)
            {
                if (String.Compare(root[n].InnerText, js, true) == 0) tag = true;
            }
            if (!tag)
            {
                XmlElement none = doc.CreateElement("item");
                none.InnerText = js;
                doc.DocumentElement.AppendChild(none);
                doc.Save(filename);
            }
        }
        #endregion
        #region 删除js引用
        public static void delJS(string Name, string js)
        {
            string filename = System.Web.HttpContext.Current.Server.MapPath(@"~\config\" + Name + ".xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList root = doc.DocumentElement.ChildNodes;
            bool tag = false;

            for (int n = 0; n < root.Count; n++)
            {
                if (String.Compare(root[n].InnerText, js, true) == 0) { doc.DocumentElement.RemoveChild(root[n]); tag = true; }
            }
            if (tag)
            {
                doc.Save(filename);
            }
        }
        #endregion
        public static ErrInfo sendMail(string StrTo, string strSubject, string StrBody)
        {

            ErrInfo err = new ErrInfo();
            XmlNodeList root = Config.userConfig["mail"];
            if (root != null) { 
                if (root.Count == 4)
                {
                    string strFrom = root[0].InnerText;
                    string strUserName = root[1].InnerText;
                    string strPwd = root[2].InnerText;
                    string strServer = root[3].InnerText;

                    MailMessage mailMessage = new MailMessage();
                    mailMessage.To.Add(StrTo);
                    mailMessage.From = new System.Net.Mail.MailAddress(strFrom);
                    mailMessage.Subject = strSubject;
                    mailMessage.Body = StrBody;
                    mailMessage.IsBodyHtml = true;
                   // mailMessage.en
                    //mailMessage.BodyEncoding = System.Text.Encoding.GetEncoding("GBK");
                    mailMessage.Priority = System.Net.Mail.MailPriority.Normal;

                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Credentials = new System.Net.NetworkCredential(strUserName, strPwd);//设置发件人身份的票据  
                    smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtpClient.Host = strServer;
                    smtpClient.Send(mailMessage);

                }
                else
                {
                    err.errNo = -1;
                    err.errMsg = "系统邮箱设置不正确";
//                    throw new NullReferenceException("系统邮箱设置不正确");
                }
            }
            else
            {
                err.errNo = -1;
                err.errMsg = "没有找到邮箱配置文件";
                //throw new NullReferenceException("没有找到邮箱配置文件");
            }
            return err;
        }
        #region 设置变量
        public static void AppSeting(string Name, string Value)
        {
            string filename = "";
            if (HttpContext.Current == null) { filename = HttpRuntime.AppDomainAppPath + @"web.config"; }
            else { filename = System.Web.HttpContext.Current.Server.MapPath(@"~\web.config"); }
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(filename);
            XmlNodeList topM = xmldoc.DocumentElement.ChildNodes;
            foreach (XmlElement element in topM)
            {
                if (element.Name.ToLower() == "appsettings")
                {
                    XmlNodeList _node = element.ChildNodes;
                    bool tag = false;
                    if (_node.Count > 0)
                    {
                        for (int i = 0; i < _node.Count; i++)
                        {
                            if (_node[i].Attributes != null)
                            {
                                if (_node[i].Attributes["key"].Value.ToLower() == Name.ToLower())
                                {
                                    _node[i].Attributes["value"].Value = Value;
                                    tag = true;
                                }
                            }
                        }
                    }
                    if (!tag)
                    {
                        XmlElement none4 = xmldoc.CreateElement("add");
                        none4.SetAttribute("key", Name);
                        none4.SetAttribute("value", Value);
                        element.AppendChild(none4);
                    }
                }
            }
            xmldoc.Save(filename);
        }
        public static void handlersSeting()
        {
            string Value = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory().ToLower() + "aspnet_isapi.dll";
            string Version = System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion().Substring(0, 4);
            string pre = "classicMode,runtimeVersion" + Version + ",bitness" + (Value.IndexOf("framework64") > -1 ? "64" : "32");
            string filename = "";
            if (HttpContext.Current == null) { filename = HttpRuntime.AppDomainAppPath + @"web.config"; }
            else { filename = System.Web.HttpContext.Current.Server.MapPath(@"~\web.config"); }
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(filename);
            XmlNodeList topM = xmldoc.DocumentElement.ChildNodes;
            foreach (XmlElement element in topM)
            {
                if (element.Name.ToLower() == "system.webserver")
                {
                    XmlNodeList _node = element.ChildNodes;
                    XmlNode node = null;
                    if (_node.Count > 0)
                    {
                        for (int i = 0; i < _node.Count; i++)
                        {
                            if (_node[i].Name.ToLower() == "handlers")
                            {
                                node = _node[i];
                                i = _node.Count;
                            }
                        }
                    }
                    else
                    {
                        node = xmldoc.CreateElement("handlers");
                        element.AppendChild(node);
                    }
                    if (node != null) node.InnerXml = "<add name=\"mwms4\" path=\"*\" verb=\"*\" modules=\"IsapiModule\" scriptProcessor=\"" + Value + "\" resourceType=\"Unspecified\"  requireAccess=\"None\" preCondition=\"" + pre + "\" />";
                }
            }
            xmldoc.Save(filename);
        }
        #endregion
        #region 26进制转换
        public static string v10to26(double ID)
        {
            StringBuilder M = new StringBuilder("0123456789abcdefghijklmnopqrstuvwxyz");
            StringBuilder t = new StringBuilder("");
            while (ID != 0)
            {
                int b = (int)(ID % 26);
                t.Insert(0, M[b]);
                ID = (ID - b) / 26;
            }
            return (t.ToString());
        }
        public static string v26to10(string ID)
        {
            string M = ("0123456789abcdefghijklmnopqrstuvwxyz");
            StringBuilder M1 = new StringBuilder(ID.ToLower());

            double t = 0, c = 1;
            for (int n = M1.Length - 1; n >= 0; n--)
            {
                t += c * (M.IndexOf(M1[n]));
                c *= 26;
            }
            return (t.ToString());
        }
        #endregion
        public static long IPToNumber(string strIP)
        {
            if (strIP == "::1") strIP = "127.0.0.1";
            long Ip = 0;
            string[] addressIP = strIP.Split('.');
            Ip = Convert.ToUInt32(addressIP[3]) + Convert.ToUInt32(addressIP[2]) * 256 + Convert.ToUInt32(addressIP[1]) * 256 * 256 + Convert.ToUInt32(addressIP[0]) * 256 * 256 * 256;
            return (Ip);
        }
        public static string ReplaceLowOrderASCIICharacters(string tmp)
        {
            StringBuilder info = new StringBuilder();
            foreach (char cc in tmp)
            {

                int ss = (int)cc;
                if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss < 32)))
                    info.AppendFormat("&#x{0:X};", ss);
                else info.Append(cc);

            }
            return info.ToString();
        }
        public static string noLinkHtml(string str)
        {
            return (noTagHtml(str, "a"));
        }
        public static string noTagHtml(string str, string tagname)
        {
            string zz = @"(<" + tagname + ".*?>)|(</" + tagname + ">)";
            if (tagname == "script") zz = "(<" + tagname + ".*?>)*(</" + tagname + ">)";

            Regex r = new Regex(zz, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            str = r.Replace(str, "");
            return (str);
        }
        public static void PutTableXMl()
        {
            StringBuilder Out = new StringBuilder();
            XmlDocument xmlDoc = new System.Xml.XmlDocument();
            XmlElement Root = xmlDoc.CreateElement("Data");
            xmlDoc.AppendChild(Root);
            XmlElement none = null;
            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", null, null);

            xmldecl.Encoding = "GB2312";
            xmlDoc.InsertBefore(xmldecl, Root);
            System.Data.SqlClient.SqlDataReader rs = Helper.Sql.ExecuteReader("select id,classid,datatype,TableName,Attribute from datatype");
            while (rs.Read())
            {
                none = xmlDoc.CreateElement("Table");
                none.SetAttribute("ID", rs[0].ToString());
                none.SetAttribute("ClassID", rs[1].ToString());
                none.SetAttribute("TableName", rs[3].ToString());
                none.SetAttribute("Attribute", rs[4].ToString());
                none.InnerText = rs[2].ToString();
                Root.AppendChild(none);
            }
            XmlWriter writer = XmlWriter.Create(Out);
            xmlDoc.WriteContentTo(writer);
            writer.Flush();
            API.PutFileText(HttpContext.Current.Server.MapPath("~" + ConfigurationManager.AppSettings["SystemConfigDir"] + @"/Table.xml"), Out.ToString());
        }
        #region 清理临时图片文件
        public static void clearTempUploadfile()
        {
            string SystemUploadDir = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["SystemUploadDir"].ToString();
            System.Data.SqlClient.SqlDataReader rs = Helper.Sql.ExecuteReader("SELECT top 100 createdate,filepath FROM UploadTempFile where DATEADD(hour,-1,getdate())>createdate order by createdate desc");
            while (rs.Read())
            {
                try
                {
                    string filepath = SystemUploadDir + rs[1].ToString();
                    if (System.IO.File.Exists(filepath)) System.IO.File.Delete(filepath);
                    Helper.Sql.ExecuteNonQuery("delete from uploadtempfile where filepath='" + rs[1].ToString() + "'");
                }
                catch
                {
                }
            }
            rs.Close();
        }
        #endregion
        #region 上传文件登记
        public static string UploadReg(string Value, string DataID, ref DataTable Pics)
        {
            string NewDir = System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString();
            string SystemUploadDir = ConfigurationManager.AppSettings["SystemUploadDir"];
            string StringFilePath = HttpContext.Current.Server.MapPath("~" + SystemUploadDir);
            StringBuilder V = new StringBuilder(Value);
            if (!Directory.Exists(StringFilePath + NewDir)) API.CreateDir(StringFilePath + NewDir);
            Regex r = new Regex(@"Temp\/(\d){5,20}.(.){3,3}", RegexOptions.IgnoreCase); //定义一个Regex对象实例
            MatchCollection mc = r.Matches(Value);
            string FileName = null, NewFile = null;
            for (int n = 0; n < mc.Count; n++)
            {

                FileName = HttpContext.Current.Server.MapPath("~" + SystemUploadDir + mc[n].Value);
                NewFile = mc[n].Value.ToLower().Replace("temp", "");
                Helper.Sql.ExecuteNonQuery("insert into UploadFile (filepath,dataid,createdate)values('" + NewDir + NewFile + "'," + DataID + ",'" + System.DateTime.Now.ToString() + "')");
                Helper.Sql.ExecuteNonQuery("delete from uploadtempfile where filepath='temp" + NewFile + "'");
                V.Replace(mc[n].Value, NewDir + NewFile);
                string[] ta1 = new string[2];
                ta1[0] = "0";
                ta1[1] = NewDir + NewFile;
                if (File.Exists(FileName))
                {
                    ta1[0] = "1";
                    File.Move(FileName, StringFilePath + NewDir + NewFile);
                }
                Pics.Rows.Add(ta1);
            }
            r = new Regex(@"(\d){4,4}-(\d){1,2}\/(\d){5,20}.(.){3,3}", RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(Value);
            for (int n = 0; n < mc.Count; n++)
            {
                Helper.Sql.ExecuteNonQuery("insert into UploadFile (filepath,dataid,createdate)values('" + mc[n].Value + "'," + DataID + ",'" + System.DateTime.Now.ToString() + "')");
            }
            return (V.ToString());
        }
        #endregion
        #region 创建文件夹
        public static bool CreateDir(string Path)
        {
            bool bz = true;
            //	try
            //	{
            Directory.CreateDirectory(Path);
            //	}
            /*	catch
                {
                    try
                    {
                        Scripting.FileSystemObject  fso=new  Scripting.FileSystemObjectClass();  
                        fso.CreateFolder(Path);
                    }
                    catch
                    {
                        bz=false;
                    }
                
                }
            */
            return (bz);

        }
        #endregion
        #region 字符串截取返回
        public struct String2
        {
            public bool V;//返回bool型
            public string String;//返回字符型
        }
        #endregion
        #region 加密字符串
        public static string Gncrypt(string Str)
        {
            return (System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Str, "sha1"));
        }
        #endregion
        #region 去掉html中的html格式字符
        public static string nohtml(string str)
        {
            str = Regex.Replace(str, @"<!--(.[^$]*?)-->", "");
            Regex r = new Regex(@"(\<.[^\<]*\>)"); //定义一个Regex对象实例
            str = r.Replace(str, "");
            Regex r1 = new Regex(@"(\<\/[^\<]*\>)"); //定义一个Regex对象实例
            str = r1.Replace(str, "");
            str = str.Replace("&nbsp;", "");
            str = str.Replace("<", "");
            str = str.Replace(">", "");
            //str=str.Replace("&","");
            return (str);
        }
        #endregion
        public static int GetStringLength(string oString)
        {
            byte[] strArray = System.Text.Encoding.Default.GetBytes(oString);
            int res = strArray.Length;
            return res;

        }

        #region 为字符串加省略
        public static String2 GetString(string str, int count)
        {
            String2 Value;
            if (count == 0)
            {
                Value.V = true;
                Value.String = str;
                return (Value);
            }
            char v;
            int n1 = 0;
            string str1 = "", str2 = "";
            for (int n = 0; n < str.Length; n++)
            {
                v = char.Parse(str.Substring(n, 1));
                if (v >= 0 && v <= 255)
                {
                    n1 = n1 + 1;
                }
                else { n1 = n1 + 2; }
                str1 = str1 + v;
                if (n1 >= count) { n = str.Length + 1; }
                if (n1 == count - 2 || n1 == count - 1) { str2 = str1; }
            }
            if (str1 == str) { Value.String = str; Value.V = true; }
            else { Value.String = str2 + "..."; Value.V = false; }
            return (Value);
        }
        #endregion


        #region 生成一个随机ID
        public static string GetId()
        {
            return (GetId(0));
        }
        public static string GetId(int n)
        {
            if (Constant.AddDataCounter > int.MaxValue - 100) Constant.AddDataCounter = 0;
            Constant.AddDataCounter++;
            string id;
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            //id = ((long)((System.DateTime.Now.ToOADate() - 39781) * 1000000) - 432552).ToString() + rnd.Next(99).ToString("D2");
            //long webid = long.Parse(Config.webId.Substring(0, Config.webId.Length-2));
            id = ((System.DateTime.Now.Ticks - System.DateTime.Parse("2012-8-1").Ticks) / 10000000 + Constant.AddDataCounter).ToString() + rnd.Next(99).ToString("D2");
            return (id);
        }
        #endregion
        #region 保存远程文件到指定路径下
        public static string zhContent(string Url, string content)
        {
            StringBuilder out1 = new StringBuilder(content);
            MatchCollection mc;
            string YH = @"\b";
            string YH2 = "\"";
            string headstr = "src=(\0| |\"|" + YH + "|'|)";
            Regex r = new Regex(@"(?<=" + headstr + @")((\S*\/)((\S)+[.]{1}(gif|jpg|png|bmp)))(?=( |}|" + YH2 + "|'))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            mc = r.Matches(content);
            for (int n = 0; n < mc.Count; n++)
            {
                StringBuilder url = new StringBuilder(mc[n].ToString());
                url.Replace("\"", "");
                url.Replace("'", "");
                out1.Replace(url.ToString(), API.BuildUrl(Url, url.ToString()));
            }
            return (out1.ToString());
        }
        public static bool SaveDownFile(string Url, string Path)
        {
            try
            {

                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;
                Byte[] pageData = MyWebClient.DownloadData(Url);
                FileStream fs = new FileStream(Path, FileMode.Create);
                BinaryWriter w = new BinaryWriter(fs);
                w.Write(pageData);
                w.Close();
                fs.Close();
                return (true);
            }
            catch (Exception ex)
            {
                API.writeLog("2", "方法API.SaveDownFile错误：" + ex.Message);
                return (false);
            }
        }
        public static string SaveDownFile(string Url)
        {
            string rtn = "";
            string SystemUploadDir = ConfigurationManager.AppSettings["SystemUploadDir"];
            string F = Url.Substring(Url.LastIndexOf("/") + 1);
            string kzm = "jpg";
            if (F.LastIndexOf(".") > -1) kzm = F.Substring(F.LastIndexOf(".") + 1);
            string StringFileName = API.GetId() + "." + kzm;
            string StringFilePath = HttpContext.Current.Server.MapPath("~" + SystemUploadDir);
            string NewDir = System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString();
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + SystemUploadDir + NewDir))) Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~" + SystemUploadDir + NewDir));
            rtn = SystemUploadDir + NewDir + @"/" + StringFileName;
            if (SaveDownFile(Url, HttpContext.Current.Server.MapPath("~" + rtn))) return (rtn);
            return "";
        }
        #endregion
        #region 写入文本文件内容
        public static bool PutFileText(string Path, string Content)
        {
            return (PutFileText(Path, Content, Encoding.Default));
        }
        public static bool PutFileText(string Path, string Content, System.Text.Encoding BianMa)
        {
            FileStream objFile;
            byte[] bContent = BianMa.GetBytes(Content);
            try
            {
                objFile = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                if (BianMa == Encoding.Unicode)
                {
                    byte[] buff = new byte[2 + bContent.Length];
                    buff[0] = 255;
                    buff[1] = 254;
                    bContent.CopyTo(buff, 2);
                    objFile.Write(buff, 0, buff.Length);
                }
                else if (BianMa == Encoding.UTF8)
                {
                    byte[] buff = new byte[3 + bContent.Length];
                    buff[0] = 239;
                    buff[1] = 187;
                    buff[2] = 191;
                    bContent.CopyTo(buff, 3);
                    objFile.Write(buff, 0, buff.Length);
                }
                else
                {
                    objFile.Write(bContent, 0, bContent.Length);
                }
                objFile.Close();
                objFile = null;
                return (true);
            }
            catch
            {
                objFile = null;
                return (false);
            }
        }
        #endregion
        #region 取得两个符串中间的部分
        public static string GetStrFG(string str1, string headstr, string endstr)
        {
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + ").*?(?=" + endstr + ")", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(str1);
            if (mc.Count > 0) return (mc[0].Value); else { return (""); }
        }
        #endregion
        #region 格式化字符串
        public static string FormatStr(string DateTime, string Type)
        {
            System.DateTime Date;
            try
            {
                switch (Type.ToUpper())
                {
                    case "HTMLENCODE":
                        return (System.Web.HttpContext.Current.Server.HtmlEncode(DateTime));
                        break;
                    case "ICO":
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(HttpContext.Current.Server.MapPath("~/config/DataAttribute.xml"));
                        string icostr = "";
                        XmlNode xn = xmlDoc.SelectSingleNode("Type");
                        XmlNodeList xnl = xn.ChildNodes;
                        if (xnl.Count > 0)
                        {
                            foreach (XmlNode xnf in xnl)
                            {
                                XmlNodeList xnf1 = xnf.ChildNodes;
                                if (DateTime.IndexOf(xnf1[2].InnerText) > -1)
                                {
                                    icostr += "<img src='" + xnf1[1].InnerText + "' class='ico'/>";
                                }
                            }
                        }
                        return (icostr);
                    case "MAXPIC":
                        DateTime = DateTime.ToUpper();
                        if (DateTime.IndexOf("MIN") > -1) { DateTime = DateTime.Replace("MIN", ""); }
                        return (DateTime);
                    case "YY.MM.DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString().Substring(2, 2) + "." + Date.Month.ToString().PadLeft(2, '0') + "." + Date.Day.ToString().PadLeft(2, '0'));
                    case "YY-MM-DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString().Substring(2, 2) + "-" + Date.Month.ToString().PadLeft(2, '0') + "-" + Date.Day.ToString().PadLeft(2, '0'));
                    case "YYYY.MM.DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "." + Date.Month.ToString().PadLeft(2, '0') + "." + Date.Day.ToString().PadLeft(2, '0'));
                    case "YYYY-MM-DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "-" + Date.Month.ToString().PadLeft(2, '0') + "-" + Date.Day.ToString().PadLeft(2, '0'));
                    case "YY-MM":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "-" + Date.Month.ToString().PadLeft(2, '0'));
                    case "MM-DD":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Month.ToString().PadLeft(2, '0') + "-" + Date.Day.ToString().PadLeft(2, '0'));
                    case "YYYY年MM月DD日":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "年" + Date.Month.ToString().PadLeft(2, '0') + "月" + Date.Day.ToString().PadLeft(2, '0') + "日");
                    case "YYYY年MM月":
                        Date = System.DateTime.Parse(DateTime);
                        return (Date.Year.ToString() + "年" + Date.Month.ToString().PadLeft(2, '0') + "月");
                    case "999,999":
                        return (long.Parse(DateTime).ToString("###,###"));
                    case "999.99":
                        return (double.Parse(DateTime).ToString("###.##"));
                    case "PAGE":
                        string[] sArray = Regex.Split(DateTime, "<!-- PageSpacer -->");
                        int pageno = 1;
                        if (HttpContext.Current.Request.QueryString["pageno"] != null && HttpContext.Current.Request.QueryString["pageno"].ToString() != "") pageno = int.Parse(HttpContext.Current.Request.QueryString["pageno"].ToString());
                        return (sArray[pageno - 1]);
                    case "PAGEBAR":
                        StringBuilder O = new StringBuilder("页码：");
                        int pn = 1;
                        if (HttpContext.Current.Request.QueryString["pageno"] != null && HttpContext.Current.Request.QueryString["pageno"].ToString() != "") pn = int.Parse(HttpContext.Current.Request.QueryString["pageno"].ToString());
                        string[] s = Regex.Split(DateTime, "<!-- PageSpacer -->", RegexOptions.IgnoreCase);
                        string filename = "";
      
                            filename = HttpContext.Current.Request.QueryString["dataid"];
                            if (filename == null) filename = HttpContext.Current.Request.QueryString["S_FileName"];
                
                        if (s.Length > 1)
                        {
                            for (int i = 1; i <= s.Length; i++)
                            {
                                if (i == pn) O.Append("<font>" + i.ToString() + "</font>");
                                else
                                {
                                    if (i == 1) O.Append("<a href='" + filename + "."+ BaseConfig.extension + "'>" + i.ToString() + "</a>");
                                    else
                                    {
                                        O.Append("<a href='" + filename + "_" + i.ToString() + "." + BaseConfig.extension + "'>" + i.ToString() + "</a>");
                                    }
                                }
                            }
                            return (O.ToString());
                        }
                        else
                        {
                            return ("");
                        }
                    default:
                        return (DateTime);
                }
            }
            catch { return (DateTime); }
        }
        #endregion
        #region 取得url中参数项的值
        public static string GetUrlValue(string HTML, string FieldName)
        {
            string YH = @"\b";
            string YH2 = "\"";
            string headstr = YH + FieldName + "=";
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + @")(.*?)(?=(\&|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            mc = r.Matches(HTML);
            if (mc.Count > 0) return (mc[0].Value.Replace("\"", ""));
            return ("");
        }
        #endregion
        #region 取得HTML中字段项的值
        public static string GetHTMLValue(string HTML, string FieldName)
        {
            string YH = @"\b";
            string YH2 = "\"";
            string headstr = FieldName + "=(\0| |\"|" + YH + ")";
            MatchCollection mc;
            Regex r = new Regex(@"(?<=" + headstr + @")(.*?)(?=( |}|" + YH2 + "))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            mc = r.Matches(HTML);
            if (mc.Count > 0) return (mc[0].Value.Replace("\"", ""));
            return ("");
        }
        #endregion
        #region 读取文本文件内容
        public static string GetFileText(string Path)
        {
            string Content = null;
            if (System.IO.File.Exists(Path))
            {
                Content = File.ReadAllText(Path, Encoding.Default);
            }
            else
            {
                Content = "";
            }
            return (Content);
        }
        #endregion
        public static string NToFsize(long Size)
        {
            string dw = "byte";
            double FileSize = Size;
            double ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "KB"; }
            ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "MB"; }
            ys = FileSize / 1024;
            if ((long)ys > 0) { FileSize = ys; dw = "GB"; }
            Size = (long)(FileSize * 100);
            FileSize = (double)Size / 100;
            return (FileSize.ToString() + " " + dw);
        }
        public static string BuildUrl(string Url1, string Url2)
        {
            try
            {
                System.Uri baseurl = new Uri(Url1);
                System.Uri u2 = new Uri(baseurl, Url2);
                return (u2.ToString());
            }
            catch
            {
                return (Url2);
            }
        }
        #region 取得网页地址
        public static string GetWebUrl()
        {
            string Host = HttpContext.Current.Request.Url.Host;
            if (HttpContext.Current.Request.Url.Port == 80) return ("http://" + Host);
            else { return ("http://" + Host + ":" + HttpContext.Current.Request.Url.Port.ToString()); }
        }
        #endregion
        public static API.RS ReadSql(string Sql, int PageNo, int PageSize)
        {
            API.RS R;
            int RC = 0;
            #region 参数定义
            SqlParameter[] param = new SqlParameter[]{
         new SqlParameter("@sql",SqlDbType.VarChar),
         new SqlParameter("@currentpage",SqlDbType.Int),
         new SqlParameter("@pagesize",SqlDbType.Int),
         };
            #endregion
            #region 参数设置
            param[0].Value = Sql;
            param[1].Value = PageNo;
            param[2].Value = PageSize;
            #endregion
            R.rs = Helper.Sql.ExecuteReader(Helper.Sql.connectionString, CommandType.StoredProcedure, "p_splitpage", param);
            R.rs.NextResult();
            if (R.rs.Read()) RC = int.Parse(R.rs[0].ToString());
            R.rs.NextResult();
            R.PageCount = (RC - 1) / PageSize + 1;
            R.RecordCount = RC;
            return (R);
        }

        public static void ERR301(string url)
        {

            HttpContext.Current.Response.Status = "301 Moved Permanently";
            HttpContext.Current.Response.AddHeader("Location", url);
            HttpContext.Current.Response.End();
        }
        public static void ERR404()
        {
            HttpContext.Current.Response.Status = "404 Not Found";
            HttpContext.Current.Response.End();
        }
        public static void ERR404(string msg)
        {
            HttpContext.Current.Response.Status = "404 Not Found";
            if (HttpContext.Current.Request.Cookies["AdminClassID"] != null && HttpContext.Current.Request.Cookies["AdminClassID"].Value == "6") //管理员浏览网页时不使用缓存
            {
                HttpContext.Current.Response.Write(msg);
            }
            else
            {
                string file = HttpContext.Current.Server.MapPath("~/404.html");
                if (File.Exists(file))
                {

                    HttpContext.Current.Response.Write(API.GetFileText(file));
                }
                else
                {
                    HttpContext.Current.Response.Write(msg);
                }
            }
            HttpContext.Current.Response.End();
        }
        public static string GetTableName(string Sql)
        {
            MatchCollection mc;
            Regex r = new Regex(@"from( *)(\[*).\S*(\]*)( *)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            mc = r.Matches(Sql);
            string V = null;
            if (mc.Count > 0)
            {
                V = mc[0].Value;
                Regex r1 = new Regex(@"from( *)(\[*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                V = r1.Replace(V, "");
                Regex r2 = new Regex(@"(\]*)( *)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                V = r2.Replace(V, "");
            }
            return (V);
        }
        public static void SetBackup(string Date, string Day)
        {
            XmlDocument doc2 = new XmlDocument();
            //doc2.DocumentType.Entities.
            string XmlFile = HttpContext.Current.Server.MapPath("~" + ConfigurationManager.AppSettings["SystemConfigDir"]) + "DataBackupSet.xml";
            doc2.Load(XmlFile);
            XmlNodeList root2 = doc2.DocumentElement.ChildNodes;
            if (Date != null) root2.Item(0).InnerText = Date;
            if (Day != null) root2.Item(1).InnerText = Day;
            XmlWriter writer = XmlWriter.Create(HttpContext.Current.Server.MapPath("~" + ConfigurationManager.AppSettings["SystemConfigDir"]) + "DataBackupSet.xml");
            doc2.WriteContentTo(writer);
            writer.Flush();
            writer.Close();
        }

        public static string XmlToJSON(XmlDocument xmlDoc)
        {
            StringBuilder sbJSON = new StringBuilder();
            sbJSON.Append("{ ");
            XmlToJSONnode(sbJSON, xmlDoc.DocumentElement, true);
            sbJSON.Append("}");
            return sbJSON.ToString();
        }

        //  XmlToJSONnode:  Output an XmlElement, possibly as part of a higher array
        private static void XmlToJSONnode(StringBuilder sbJSON, XmlElement node, bool showNodeName)
        {
            if (showNodeName)
                sbJSON.Append("\"" + SafeJSON(node.Name) + "\": ");
            sbJSON.Append("{");
            // Build a sorted list of key-value pairs
            //  where   key is case-sensitive nodeName
            //          value is an ArrayList of string or XmlElement
            //  so that we know whether the nodeName is an array or not.
            SortedList childNodeNames = new SortedList();

            //  Add in all node attributes
            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes)
                    StoreChildNode(childNodeNames, attr.Name, attr.InnerText);

            //  Add in all nodes
            foreach (XmlNode cnode in node.ChildNodes)
            {
                if (cnode is XmlText)
                    StoreChildNode(childNodeNames, "value", cnode.InnerText);
                else if (cnode is XmlElement)
                    StoreChildNode(childNodeNames, cnode.Name, cnode);
                else
                {
                    StoreChildNode(childNodeNames, "value", cnode.InnerText);
                }
            }

            // Now output all stored info
            foreach (string childname in childNodeNames.Keys)
            {
                ArrayList alChild = (ArrayList)childNodeNames[childname];
                if (alChild.Count == 1)
                    OutputNode(childname, alChild[0], sbJSON, true);
                else
                {
                    sbJSON.Append(" \"" + SafeJSON(childname) + "\": [ ");
                    foreach (object Child in alChild)
                        OutputNode(childname, Child, sbJSON, false);
                    sbJSON.Remove(sbJSON.Length - 2, 2);
                    sbJSON.Append(" ], ");
                }
            }
            sbJSON.Remove(sbJSON.Length - 2, 2);
            sbJSON.Append(" }");
        }

        //  StoreChildNode: Store data associated with each nodeName
        //                  so that we know whether the nodeName is an array or not.
        private static void StoreChildNode(SortedList childNodeNames, string nodeName, object nodeValue)
        {
            // Pre-process contraction of XmlElement-s
            if (nodeValue is XmlElement)
            {
                // Convert  <aa></aa> into "aa":null
                //          <aa>xx</aa> into "aa":"xx"
                XmlNode cnode = (XmlNode)nodeValue;
                if (cnode.Attributes.Count == 0)
                {
                    XmlNodeList children = cnode.ChildNodes;
                    if (children.Count == 0)
                        nodeValue = null;
                    else if (children.Count == 1 && (children[0] is XmlText || children[0] is XmlCDataSection))
                        nodeValue = children[0].InnerText;
                }
            }
            // Add nodeValue to ArrayList associated with each nodeName
            // If nodeName doesn't exist then add it
            object oValuesAL = childNodeNames[nodeName];
            ArrayList ValuesAL;
            if (oValuesAL == null)
            {
                ValuesAL = new ArrayList();
                childNodeNames[nodeName] = ValuesAL;
            }
            else
                ValuesAL = (ArrayList)oValuesAL;
            ValuesAL.Add(nodeValue);
        }

        private static void OutputNode(string childname, object alChild, StringBuilder sbJSON, bool showNodeName)
        {
            if (alChild == null)
            {
                if (showNodeName)
                    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                sbJSON.Append("null");
            }
            else if (alChild is string)
            {
                if (showNodeName)
                    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                string sChild = (string)alChild;
                sChild = sChild.Trim();
                sbJSON.Append("\"" + SafeJSON(sChild) + "\"");
            }
            else
                XmlToJSONnode(sbJSON, (XmlElement)alChild, showNodeName);
            sbJSON.Append(", ");
        }

        // Make a string safe for JSON
        private static string SafeJSON(string sIn)
        {
            StringBuilder sbOut = new StringBuilder(sIn.Length);
            foreach (char ch in sIn)
            {
                if (Char.IsControl(ch) || ch == '\'')
                {
                    int ich = (int)ch;
                    sbOut.Append(@"\u" + ich.ToString("x4"));
                    continue;
                }
                else if (ch == '\"' || ch == '\\' || ch == '/')
                {
                    sbOut.Append('\\');
                }
                sbOut.Append(ch);
            }
            return sbOut.ToString();
        }
    }
}
