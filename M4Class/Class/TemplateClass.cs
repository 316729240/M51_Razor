using System;
using System.Collections.Generic;
using System.Text;
using Helper;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Text.RegularExpressions;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine;

namespace MWMS
{

    /// <summary>
    ///UserClass 的摘要说明
    /// </summary>
    public class TemplateClass
    {
        public TemplateClass()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }
        /// <summary>
        /// 自定义模板是否重名
        /// </summary>
        /// <param name="id"></param>
        /// <param name="classId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool customPageExist(double id, double classId, string title)
        {
            int count = (int)Sql.ExecuteScalar("select count(1) from HtmlTemplate B  where B.u_datatypeid=-3 and A.classId=@classId and B.id<>@id and B.title=@title", new SqlParameter[]{
                new SqlParameter("classId",classId),
                new SqlParameter("id",id),
                new SqlParameter("title",title)
            });
            return count > 0;

        }
        /// <summary>
        /// 备份模板
        /// </summary>
        /// <param name="id">模板id</param>
        public static void backupTemplate(double dataId,  string username)
        {
            Sql.ExecuteNonQuery("insert into backupTemplate (id,dataId,classId,u_type,title,u_content,updateDate,userName,u_webFAid,u_defaultflag,u_datatypeId)" +
"select @id,B.id,B.classid,B.u_type,B.title,B.u_content,getdate(),@username,B.u_webFAid,B.u_defaultflag,u_datatypeId from htmlTemplate B  " +
" where B.id=@dataId", new SqlParameter[]{
                 new SqlParameter("id",double.Parse(API.GetId())),
                 new SqlParameter("dataId",dataId),
                 new SqlParameter("username",username),
            });
        }
        public static void backupTemplate(double dataId,string html, string username)
        {
            Sql.ExecuteNonQuery("insert into backupTemplate (id,dataId,classId,u_type,title,u_content,updateDate,userName,u_webFAid,u_defaultflag,u_datatypeId)" +
"select @id,B.id,B.classid,B.u_type,B.title,@u_content,getdate(),@username,B.u_webFAid,B.u_defaultflag,u_datatypeId from htmlTemplate B  " +
" where B.id=@dataId", new SqlParameter[]{
                 new SqlParameter("id",double.Parse(API.GetId())),
                 new SqlParameter("dataId",dataId),
                 new SqlParameter("u_content",html),
                 new SqlParameter("username",username),
            });
        }
        /// <summary>
        /// 备份视图
        /// </summary>
        /// <param name="id">模板id</param>
        public static void backupView(double dataId, string html, string username)
        {
            Sql.ExecuteNonQuery("insert into backupTemplate (id,dataId,u_content,updatedate,username)values(@id,@dataId,@u_content,getDate(),@username)", new SqlParameter[]{
                 new SqlParameter("id",double.Parse(API.GetId())),
                 new SqlParameter("dataId",dataId),
                 new SqlParameter("u_content",html),
                 new SqlParameter("username",username),
            });
        }
        //public static string homePage()
        //{
        //    string html = getDefaultPage(0, -1, 0,API.getWebFAId());
        //    return html;
        //}
        /// <summary>
        /// 获取默认模板
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="datatypeId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static string getDefaultPage(double classId, double datatypeId, int typeId, bool isMobile)
        {
            string html = "";
            SqlDataReader rs = Helper.Sql.ExecuteReader("select u_content from HtmlTemplate B  where B.ClassID=@classId and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_webFAid=@webFAid and B.u_defaultFlag=1",
            new SqlParameter[]{
                    new SqlParameter("classId",classId),
                    new SqlParameter("datatypeid",datatypeId),
                    new SqlParameter("typeId",typeId),
                    new SqlParameter("webFAid" ,isMobile?1:0),
                });
            if (rs.Read()) html = rs.GetString(0);
            rs.Close();
            return html;
        }
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public static ErrInfo del(double id,UserInfo user)
        {
            ErrInfo v = new ErrInfo();
            TemplateInfo info = TemplateClass.get(id);
            if (info != null)
            {
                #region 权限验证
                if (info.classId == 0)
                {
                    if (!user.isAdministrator)
                    {
                        v.errNo = -1;
                        v.errMsg = "没有权限";
                        return v;
                    }
                }
                else
                {
                    Permissions _perm = null;
                    ModuleInfo moduleInfo = ModuleClass.get(info.classId);
                    if (moduleInfo == null)
                    {
                        ColumnInfo columnInfo = ColumnClass.get(info.classId);
                        _perm = user.getColumnPermissions(info.classId);
                    }
                    else
                    {
                        _perm = user.getModulePermissions(info.classId);
                    }
                    if (!_perm.all)
                    {
                        v.errNo = -1;
                        v.errMsg = "没有权限";
                        return v;
                    }
                }
                #endregion
                if ((info.u_type == 1 || info.u_type == 2) && info.u_defaultFlag == 0)//自定义栏目模板时
                {
                    string flag = "";
                    if (info.u_webFAid == 1) flag = "_";

                    if (info.u_type==1) Sql.ExecuteNonQuery("update class set " + flag + "skinId=0 where " + flag + "skinId=@skinId", new SqlParameter[] { new SqlParameter("skinId", info.id) });
                    if (info.u_type == 2) Sql.ExecuteNonQuery("update class set " + flag + "contentSkinId=0 where " + flag + "contentSkinId=@skinId", new SqlParameter[] { new SqlParameter("skinId", info.id) });
                }
                Sql.ExecuteNonQuery("delete  htmlTemplate where id=@id", new SqlParameter[] {
                new SqlParameter("id",id) 
            });
            }
            v.errNo = 0;
            return v;
        }
        public static TemplateInfo get(double classId, int typeId, double datatypeId,  bool isMobile)
        {
            return get(classId, typeId, datatypeId, true, "", isMobile);
        }
        public static TemplateInfo get(double classId, int typeId, double datatypeId,bool defaultFlag,string title, bool isMobile)
        {
            TemplateInfo value = null;
            SqlDataReader rs = null;
            if (defaultFlag)
            {
                rs=Helper.Sql.ExecuteReader("select B.title,B.u_content,B.u_editboxStatus,B.u_parameterValue,B.id from HtmlTemplate B where B.ClassID=@classId and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_defaultFlag=@u_defaultFlag and B.u_webFAid=@webFAid",
                     new SqlParameter[]{
                    new SqlParameter("classId",classId),
                    new SqlParameter("datatypeid",datatypeId),
                    new SqlParameter("typeId",typeId),
                    new SqlParameter("u_defaultFlag" ,defaultFlag?1:0),
                    new SqlParameter("webFAid" ,isMobile?1:0),
                });
            }
            else
            {
                rs=Helper.Sql.ExecuteReader("select B.title,B.u_content,B.u_editboxStatus,B.u_parameterValue,B.id from HtmlTemplate B where B.ClassID=@classId and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_defaultFlag=@u_defaultFlag and title=@title and B.u_webFAid=@webFAid",
                     new SqlParameter[]{
                    new SqlParameter("classId",classId),
                    new SqlParameter("datatypeid",datatypeId),
                    new SqlParameter("typeId",typeId),
                    new SqlParameter("webFAid" ,isMobile?1:0),
                    new SqlParameter("u_defaultFlag" ,defaultFlag?1:0),
                    new SqlParameter("title" ,title),
                });
            }
            if (rs.Read())
            {
                value = new TemplateInfo();
                value.id = rs.GetDouble(4);
                value.title = rs[0].ToString();
                value.u_content = rs[1].ToString();
                value.u_editboxStatus =rs.GetInt32(2);
                value.u_parameterValue = rs[3].ToString();
                value.u_datatypeId = datatypeId;
                value.classId = classId;
                value.u_type = typeId;
                value.u_defaultFlag = defaultFlag ? 1 : 0;
            }
            rs.Close();
            return value;
        }
        public static TemplateInfo getCustomTemplate(string url,bool isMobile)
        {
            url = url.Replace("." + BaseConfig.extension, "");//无参数
            string url2="";
            Regex r = new Regex(@"/"+".*(?="+@"/"+")", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            MatchCollection mc = r.Matches(url);
            if (mc.Count > 0) url2 = mc[0].Value;
            Dictionary<string, object> variable = new Dictionary<string, object>();
            if (url2 != "")
            {
                string[] parameter = Regex.Replace(url, "^" + url2 + @"/", "", RegexOptions.IgnoreCase).Split('-');
                for (int i = 0; i < parameter.Length; i++)
                {
                    variable["parameter" + (i + 1).ToString()] = HttpContext.Current.Server.UrlDecode(parameter[i]);
//                    variable.Set("parameter" + (i + 1).ToString(), HttpContext.Current.Server.UrlDecode(parameter[i]));
                }
            }
            TemplateInfo value = null;
            string where = "";
            if(url2!="")where=" or url=@url2";

            SqlDataReader rs = Helper.Sql.ExecuteReader("select B.title,B.u_content,B.u_editboxStatus,B.u_parameterValue,B.id,B.classId from HtmlTemplate B where B.u_webFAid=@u_webFAid and B.u_type=3 and (B.url=@url"+where+")",
    new SqlParameter[]{
                    new SqlParameter("u_webFAid",isMobile?1:0),
                    new SqlParameter("url",url),
                    new SqlParameter("url2",url2)
                });
            if (rs.Read())
            {
                value = new TemplateInfo();
                value.id = rs.GetDouble(4);
                value.title = rs[0].ToString();
                value.u_content = rs[1].ToString();
                value.u_editboxStatus = rs.GetBoolean(2) ? 1 : 0;
                value.u_parameterValue = rs[3].ToString();
                value.u_type = 3;
                value.classId = rs.GetDouble(5);
                value.variable = variable;
            }
            rs.Close();
            return value;
        }

        /// <summary>
        /// 获取栏目页模板
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static TemplateInfo getColumnTemplate(string url, bool isMobile)
        {
            TemplateInfo info = null;
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0;
            int typeId = 1;

            Dictionary<string,object> variable = new Dictionary<string, object>();
            //dynamic variable = new System.Dynamic.ExpandoObject();
            SqlDataReader rs = Helper.Sql.ExecuteReader("select rootId,moduleId,skinId,classId,id,className,info,orderId,createDate,saveDataType,maxIco,updatedate,dirName,keyword,dirPath,childId,parentId,layer,attribute,custom,url,_SkinID from class where url=@url", new SqlParameter[] { 
                    new SqlParameter("url",url)});
            if (rs.Read())
            {
                rootId = rs.GetDouble(0);
                moduleId = rs.GetDouble(1);
                skinId =rs.IsDBNull(2)?0: rs.GetDouble(2);
                if (isMobile) skinId = rs.IsDBNull(21) ? 0 : rs.GetDouble(21);
                    
                if (rs.GetDouble(3) == 7) typeId = 0;
                for (int i = 0; i < rs.FieldCount; i++) variable[rs.GetName(i)]= rs[i];

            }
            rs.Close();
            if (rootId == 0) {
                API.ERR404("栏目不存在");
            }
            if (skinId > 0)
            {
                info = get(skinId);
            }
            if (info == null)
            {
                SqlDataReader rs2 = Helper.Sql.ExecuteReader("select top 1 B.title,B.u_content,B.u_editboxStatus,B.u_parameterValue,B.id,B.classId from HtmlTemplate B where B.ClassID in (0,@moduleId,@rootId) and B.u_defaultFlag=1 and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_webFAid=@webFAid order by u_layer desc",
    new SqlParameter[]{
                    new SqlParameter("moduleId",moduleId),
                    new SqlParameter("rootId",rootId),
                    new SqlParameter("datatypeid",dataTypeId),
                    new SqlParameter("typeId",typeId),
                    new SqlParameter("webFAid" ,isMobile?1:0)
                });
                if (rs2.Read())
                {
                    info = new TemplateInfo();
                    info.id = rs2.GetDouble(4);
                    info.title = rs2[0].ToString();
                    info.u_content = rs2[1].ToString();
                    info.u_editboxStatus = rs2.GetBoolean(2) ? 1 : 0;
                    info.u_parameterValue = rs2[3].ToString();
                    info.classId = rs2.GetDouble(5);
                    info.u_type = typeId;
                }
                rs2.Close();
            }
            if (info != null) info.variable = variable;
            return info;
        }
        /// <summary>
        /// 获取内容页模板
        /// </summary>
        /// <returns></returns>
        public static TemplateInfo getContentTemplate(string url, bool isMobile)
        {

            url = url.Replace("." + BaseConfig.extension, "");

            Dictionary<string, object> variable = new Dictionary<string, object>();
            TemplateInfo info = null;
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0,id=0,classId=0;
            SqlDataReader rs = Helper.Sql.ExecuteReader("select dataTypeId,rootId,moduleId,skinId,id,title,createDate,updateDate,classId,clickCount,url,pic,userId from mainTable where url=@url and orderId>-1", new SqlParameter[] { 
                    new SqlParameter("url",url)});
            if (rs.Read())
            {
                dataTypeId = rs.GetDouble(0);
                rootId = rs.GetDouble(1);
                moduleId = rs.GetDouble(2);
                skinId =rs.GetDouble(3);
                id = rs.GetDouble(4);
                classId = rs.GetDouble(8);
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    if (i == 10)
                    {
                        variable[rs.GetName(i)]= Config.webPath + rs[i] + "." + BaseConfig.extension;
                    }
                    else
                    {
                            variable[rs.GetName(i)]= rs[i];

                    }
                }
            }
            rs.Close();
            if (rootId==0) return null;
            if (skinId == 0)
            {
                rs = Helper.Sql.ExecuteReader("select " + (isMobile ? "_contentSkinID" : "contentSkinID") + " from class where id=@id", new SqlParameter[] {new SqlParameter("id",classId) });
                if (rs.Read())
                {
                    skinId =rs.IsDBNull(0)?0:rs.GetDouble(0);
                }
                rs.Close();
            }
            string tableName = (string)Sql.ExecuteScalar("select tableName from datatype where id="+dataTypeId.ToString());
            rs = Helper.Sql.ExecuteReader("select * from "+tableName+" where id=@id", new SqlParameter[] { 
                    new SqlParameter("id",id)});
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    string fieldName = rs.GetName(i);

                    if (rs.IsDBNull(i))
                    {
                        variable[rs.GetName(i)]= "";
                    }
                    else { 
                        if (rs.GetDataTypeName(rs.GetOrdinal(fieldName)) == "ntext")
                        {
                            SystemLink v1 = new SystemLink();
                            string FieldValue = rs[i]+"";
                            FieldValue = v1.Replace(FieldValue);
                            variable[fieldName]=FieldValue;
                        }
                        else
                        {
                            variable[fieldName]=rs[i];
                        }
                    }
                }
            }
            rs.Close();


            if (skinId > 0)
            {
                info = get(skinId);
            }
            if (info==null)
            {
                SqlDataReader rs2 = Helper.Sql.ExecuteReader("select top 1 B.title,B.u_content,B.u_editboxStatus,B.u_parameterValue,B.id,B.classId from HtmlTemplate B where B.ClassID in (0,@moduleId,@rootId) and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_defaultFlag=1 and B.u_webFAid=@webFAid order by u_layer desc",
    new SqlParameter[]{
                    new SqlParameter("moduleId",moduleId),
                    new SqlParameter("rootId",rootId),
                    new SqlParameter("datatypeid",dataTypeId),
                    new SqlParameter("typeId",2),
                    new SqlParameter("webFAid" ,isMobile?1:0)
                });
                if (rs2.Read())
                {
                    info = new TemplateInfo();
                    info.id = rs2.GetDouble(4);
                    info.title = rs2[0].ToString();
                    info.u_content = rs2[1].ToString();
                    info.u_editboxStatus = rs2.GetBoolean(2) ? 1 : 0;
                    info.u_parameterValue = rs2[3].ToString();
                    info.classId = rs2.GetDouble(5);
                    info.u_type = 2;
                }
                rs2.Close();
            }
            if (info != null) info.variable = variable;
            return info;
        }
        public static TemplateInfo get(string u, bool isMobile)
        {
//            string u=Regex.Replace(url,"^"+Config.webPath,"",RegexOptions.IgnoreCase).ToLower();
            if (u == "/") return get(0, 0, -1, isMobile);//获取首页模板
            if (u.Substring(u.Length - 1) == "/")
            {
                return getColumnTemplate(u, isMobile);
            }
            else
            {
                TemplateInfo t = getCustomTemplate(u, isMobile);
                if (t == null) t = getContentTemplate(u, isMobile);
                return t;
            }
        }
        public static TemplateInfo get(double id){
            TemplateInfo value = null;
            SqlDataReader rs = Helper.Sql.ExecuteReader("select B.title,B.u_content,B.u_editboxStatus,B.u_parameterValue,B.u_datatypeId,B.classId,B.u_type,B.u_defaultFlag,B.u_webFAid from HtmlTemplate B  where B.id=@id",
                new SqlParameter[]{
                    new SqlParameter("id",id)
                });
            if (rs.Read())
            {
                value = new TemplateInfo();
                value.id = id;
                value.title = rs[0].ToString();
                value.u_content = rs[1].ToString();
                value.u_editboxStatus = rs.GetBoolean(2)?1:0;
                value.u_parameterValue = rs[3].ToString();
                value.u_datatypeId = rs.GetDouble(4);
                value.classId = rs.GetDouble(5);
                value.u_type = rs.GetInt32(6);
                value.u_defaultFlag = rs.GetBoolean(7)? 1 : 0;
                value.u_webFAid = rs.GetInt32(8);
            }
            rs.Close();
               return value;
        }
        public static ErrInfo edit(TemplateInfo value,UserInfo user)
        {
            ErrInfo info = new ErrInfo();
            string url = @"/" + value.title;
            if (value.classId == 0)
            {
                if (!user.isAdministrator)
                {
                    info.errNo = -1;
                    info.errMsg = "没有权限";
                    return info;
                }
            }
            else
            {
                Permissions _perm = null;
                ModuleInfo moduleInfo = ModuleClass.get(value.classId);
                if (moduleInfo == null)
                {
                    ColumnInfo columnInfo = ColumnClass.get(value.classId);
                    url = @"/" + columnInfo.dirName + "/"+value.title;
                    _perm = user.getColumnPermissions(value.classId);
                }
                else
                {
                    if (moduleInfo.type) url = @"/" + moduleInfo.dirName + "/" + value.title;
                    _perm = user.getModulePermissions(value.classId);
                }
                if (!_perm.all)
                {
                    info.errNo = -1;
                    info.errMsg = "没有权限";
                    return info;
                }
            }


            MatchCollection mc, mc2;
            Regex r = new Regex(@"(</title>).*?(</head>)", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
            mc = r.Matches(value.u_content);
            if (mc.Count > 0)//如果找到头部信息时
            {
                string H = mc[0].Value;
                Regex r2 = new Regex(@"<script(.*)</script>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                mc2 = r2.Matches(H);
                if (mc2.Count == 0)//如果没有js时
                {
                    string H2 = H.Replace("</head>", "<script type=\"text/javascript\" src=\"" + Config.webPath + "/static/m5_public.js\"></script>\n</head>");
                    value.u_content = value.u_content.Replace(H, H2);
                }
                else
                {
                    bool tag = false;
                    for (int i = 0; i < mc2.Count; i++)
                    {
                        if (mc2[i].Value.ToLower().IndexOf("m5_public.js") > -1)//如果包含系统js时
                        {
                            i = mc2.Count;
                            tag = true;
                        }
                    }
                    if (!tag)//如果所有的js都不是系统js时
                    {
                        string H2 = H.Replace(mc2[0].Value, "<script type=\"text/javascript\" src=\"" + Config.webPath + "/static/m5_public.js\"></script>\n" + mc2[0].Value);
                        value.u_content = value.u_content.Replace(H, H2);
                    }
                }
            }
            //-----------------自定义页面重名检测--------------------
            if (value.u_type == -3 && TemplateClass.customPageExist(value.id, value.classId, value.title))
            {
                info.errNo = -1;
                info.errMsg = "页面“" +value.title + "”已存在请不要重复创建";
                return info;
            }
            //-------------------获取默认模板信息--------------------
            if (value.u_defaultFlag == 1)
            {
                object _id = Sql.ExecuteScalar("select B.id from htmlTemplate B where B.classId=@classId and B.u_type=@typeId and B.u_defaultFlag=@defaultTag and B.u_datatypeId=@datatypeId and B.u_webFAid=@webFAid",
                    new SqlParameter[] { 
                    new SqlParameter("classId",value.classId),
                    new SqlParameter("typeId",value.u_type),
                    new SqlParameter("defaultTag",value.u_defaultFlag),
                    new SqlParameter("datatypeId",value.u_datatypeId),
                    new SqlParameter("webFAid",value.u_webFAid)
                });
                if (_id != null) value.id = (double)_id;
            }

            //-------------------编译模板----------------------------
            try
            {
                TemplateCode code = new TemplateCode(value.id.ToString(), value.u_content);
                code.compile();
            }
            catch (Exception ex)
            {
                info.errNo = -1;
                info.errMsg = ex.Message;
                return info;
            }
            //-------------------保存模板----------------------------
            int u_layer = 0;
            if (value.classId > 0) { 
                object  obj=Sql.ExecuteScalar("select id from module where id="+value.classId);
                u_layer = 1;
                if (obj == null)
                {
                    obj = Sql.ExecuteScalar("select id from class where id=" + value.classId);
                    u_layer = 2;
                }
            }
            SqlParameter[] p=new SqlParameter[]{
                    new SqlParameter("id",value.id),
                    new SqlParameter("classId",value.classId),
                    new SqlParameter("u_type",value.u_type),
                    new SqlParameter("title",value.title),
                    new SqlParameter("u_content",value.u_content),
                    new SqlParameter("u_defaultFlag",value.u_defaultFlag),
                    new SqlParameter("u_datatypeId",value.u_datatypeId),
                    new SqlParameter("u_editboxStatus",value.u_editboxStatus),
                    new SqlParameter("u_parameterValue",value.u_parameterValue),
                    new SqlParameter("u_webFAId",value.u_webFAid),
                    new SqlParameter("createDate",System.DateTime.Now),
                    new SqlParameter("updateDate",System.DateTime.Now),
                    new SqlParameter("url",url),
                    new SqlParameter("u_layer",u_layer)

                };
            if (value.id < 1)
            {
                p[0].Value =double.Parse(API.GetId());
                Sql.ExecuteNonQuery("insert into htmlTemplate (id,classId,u_type,title,u_content,u_defaultFlag,u_editboxStatus,u_parameterValue,u_webFAId,u_datatypeId,createDate,updateDate,url,u_layer)values(@id,@classId,@u_type,@title,@u_content,@u_defaultFlag,@u_editboxStatus,@u_parameterValue,@u_webFAId,@u_datatypeId,@createDate,@updateDate,@url,@u_layer)", p);
            }
            else
            {
                Sql.ExecuteNonQuery("update htmlTemplate set updateDate=@updateDate,classId=@classId,u_type=@u_type,title=@title,u_content=@u_content,u_defaultFlag=@u_defaultFlag,u_editboxStatus=@u_editboxStatus,u_parameterValue=@u_parameterValue,u_webFAId=@u_webFAId,u_datatypeId=@u_datatypeId,url=@url,u_layer=@u_layer where id=@id", p);
            }
            info.userData = p[0].Value;


            return info;
        }
    }
    public class TemplateInfo
    {
        public double id =-1;
        public double classId = -1;
        public double u_datatypeId =0;
        public string title="";
        public int u_type =0;
        public string u_content="";
        public int u_editboxStatus=0;
        public string u_parameterValue="";
        public int u_defaultFlag = 0;
        public int u_webFAid = 0;
        public Dictionary<string,object> variable =null;

    }

}