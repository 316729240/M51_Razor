using MWMS.DAL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MWMS.Template
{
    public class PageTemplate: Template
    {
        /// <summary>
        /// 是否手机模板
        /// </summary>
        public bool IsMobile { get; set; }
        /// <summary>
        /// 是否默认模板
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// 参数表
        /// </summary>
        public string ParameterValue { get; set; }
        
        /// <summary>
        /// 模板所属数据类型
        /// </summary>
        public double DatatypeId { get; set; }
        public Dictionary<string, object> Variable { get; set; }
        public PageTemplate()
        {

        }
        public PageTemplate(string url,bool isMobile)
        {
            Get(url,isMobile);
        }
        public PageTemplate(double id)
        {
            Get(id);
        }
        public PageTemplate(double classId, int typeId, double datatypeId, bool isDefault, string title, bool isMobile)
        {
            Get(classId, typeId, datatypeId, IsDefault, title, isMobile);
        }
        public PageTemplate(double classId, int typeId, double datatypeId, bool isDefault,  bool isMobile)
        {
            Get(classId, typeId, datatypeId, IsDefault, "", isMobile);
        }
        void Get(double id)
        {
            this.TemplateId = id;
            Dictionary<string, object> model = this.Get("title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue");
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void Get(string url,bool isMobile)
        {
            if (url == "/")  Get(0, 0, -1,true,"", isMobile);//获取首页模板
            if (url.Substring(url.Length - 1) == "/")
            {
                GetColumnTemplate(url, isMobile);
            }
            else
            {
                try { 
                    GetCustomTemplate(url, IsMobile);
                }catch { 
                    GetContentTemplate(url, isMobile);
                }
            }
        }
        /// <summary>
        /// 跟据地址获取栏目模板信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isMobile"></param>
        void GetColumnTemplate(string url, bool isMobile)
        {
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0;
            int typeId = 1;

            #region 栏目数据
            TableHandle table = new TableHandle("class");
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            Dictionary<string, object> variable = table.GetModel("rootId,moduleId,skinId,classId,id,className,info,orderId,createDate,saveDataType,maxIco,updatedate,dirName,keyword,dirPath,childId,parentId,layer,attribute,custom,url,_SkinID","url=@url",p);
            if (variable == null) throw new Exception("栏目不存在");
            rootId = (double)variable["rootId"];
            moduleId = (double)variable["moduleId"];
            skinId = variable["skinId"]==null?0 : (double)variable["skinId"];
            if (isMobile) skinId = variable["_SkinID"] == null ? 0 : (double)variable["_SkinID"];
            if ((double)variable["classId"] == 7) typeId = 0;
            this.Variable = variable;
            #endregion
            if (skinId > 0)
            {
                Get(skinId);
                return;
            }
            Get(moduleId,rootId,typeId,dataTypeId, isMobile);
        }
        /// <summary>
        /// 获取内容模板信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isMobile"></param>
        void GetContentTemplate(string url,bool isMobile)
        {
            //url = url.Replace("." + BaseConfig.extension, "");
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0, id = 0, classId = 0;
            TableHandle table = new TableHandle("maintable");
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            Dictionary<string, object> variable = table.GetModel("dataTypeId,rootId,moduleId,skinId,id,title,createDate,updateDate,classId,clickCount,url,pic,userId", "url=@url and orderId>-1", p);
            if(variable==null) throw new Exception("访问数据不存在");
            dataTypeId = (double)variable["dataTypeId"];
            rootId = (double)variable["rootId"];
            moduleId = (double)variable["moduleId"];
            skinId = (double)variable["skinId"];
            id = (double)variable["id"];
            classId = (double)variable["classId"];
            SqlDataReader rs = null;
            if (skinId == 0)
            {
                rs = Helper.Sql.ExecuteReader("select " + (isMobile ? "_contentSkinID" : "contentSkinID") + " from class where id=@id", new SqlParameter[] { new SqlParameter("id", classId) });
                if (rs.Read())
                {
                    skinId = rs.IsDBNull(0) ? 0 : rs.GetDouble(0);
                }
                rs.Close();
            }
            string tableName = (string)Helper.Sql.ExecuteScalar("select tableName from datatype where id=" + dataTypeId.ToString());
            rs = Helper.Sql.ExecuteReader("select * from " + tableName + " where id=@id", new SqlParameter[] {
                    new SqlParameter("id",id)});
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    string fieldName = rs.GetName(i);

                    if (rs.IsDBNull(i))
                    {
                        variable[rs.GetName(i)] = "";
                    }
                    else
                    {
                        if (rs.GetDataTypeName(rs.GetOrdinal(fieldName)) == "ntext")
                        {
                            //SystemLink v1 = new SystemLink();
                            string FieldValue = rs[i] + "";
                            //FieldValue = v1.Replace(FieldValue);
                            variable[fieldName] = FieldValue;
                        }
                        else
                        {
                            variable[fieldName] = rs[i];
                        }
                    }
                }
            }
            rs.Close();
            if (skinId > 0)
            {
                try { 
                    Get(skinId);
                }catch
                {
                    Get(moduleId, rootId, 2, dataTypeId, isMobile);
                }
            }
        }
        /// <summary>
        /// 获取自定义模板信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isMobile"></param>
        void GetCustomTemplate(string url, bool isMobile)
        {
            //url = url.Replace("." + BaseConfig.extension, "");//无参数
            string url2 = "";
            Regex r = new Regex(@"/" + ".*(?=" + @"/" + ")", RegexOptions.Singleline | RegexOptions.IgnoreCase); //定义一个Regex对象实例
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
            Get(url, url2, IsMobile);
        }
        void Get(string url,string url2,bool isMobile)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            p.Add("url2", url2);
            p.Add("u_webFAid", isMobile ? 1 : 0);
            string where = "";
            if (url2 != "") where = " or url=@url2";
            Dictionary<string, object> model = null;
            model = this.GetModel("title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue",
                " u_webFAid = @u_webFAid and u_type = 3 and(url = @url" + where + ")", p);
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void Get(double classId, int typeId, double datatypeId, bool isDefault, string title, bool isMobile)
        {
            Dictionary < string, object>  p=new Dictionary<string, object>();
            p.Add("classId", classId);
            p.Add("typeId", typeId);
            p.Add("datatypeId", datatypeId);
            p.Add("isDefault", isDefault?1:0);
            p.Add("title", title);
            p.Add("isMobile", isMobile?1:0);
            Dictionary<string, object> model = null;
            if (isDefault) { 
                model =this.GetModel("title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", "classId=@classId and u_type=@typeId and u_datatypeId=@datatypeId and u_defaultFlag=@isDefault  and u_webFAid=@isMobile", p);
            }else
            {
                model = this.GetModel("title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", "classId=@classId and u_type=@typeId and u_datatypeId=@datatypeId and u_defaultFlag=@isDefault and title=@title and u_webFAid=@isMobile", p);
            }
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void Get(double moduleId, double rootId, int typeId, double  datatypeId,bool isMobile)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("moduleId", moduleId);
            p.Add("rootId", rootId);
            p.Add("typeId", typeId);
            p.Add("datatypeId", datatypeId);
            p.Add("isMobile", isMobile ? 1 : 0);
            Dictionary<string, object> model = null;
            model = this.GetModel("title,u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", 
                "classId in (0,@moduleId,@rootId) and B.u_defaultFlag=1 and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_webFAid=@isMobile", p, "u_layer desc");
            
            if (model == null) throw new Exception("指定模板不存在");
            SetAttr(model);
        }
        void SetAttr(Dictionary<string, object> model)
        {
            this.TemplateContent = (string)model["u_content"];
            this.TemplateName = (string)model["title"];
            this.DatatypeId = (double)model["u_datatypeId"];
            this.TemplateType = (TemplateType)model["u_type"];
            this.EditMode = (EditMode)model["u_editboxStatus"];
            this.IsDefault = (int)model["u_defaultFlag"] == 1 ? true : false;
            this.IsMobile = (int)model["u_webFAid"] == 1 ? true : false;
            this.ColumnId = (double)model["classId"];
            this.ParameterValue = model["u_parameterValue"] + "";
        }
    }

}
