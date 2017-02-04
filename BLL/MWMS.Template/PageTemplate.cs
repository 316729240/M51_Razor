using MWMS.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        }
        void GetColumnTemplate(string url, bool isMobile)
        {
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0;
            int typeId = 1;

            #region 栏目数据
            TableHandle table = new TableHandle("class");
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("url", url);
            Dictionary<string, object> variable = table.GetModel("rootId,moduleId,skinId,classId,id,className,info,orderId,createDate,saveDataType,maxIco,updatedate,dirName,keyword,dirPath,childId,parentId,layer,attribute,custom,url,_SkinID","url=@url",p);
            if (variable == null) throw new Exception("栏目地址不存在");
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
        void GetContentTemplate(string url,bool isMobile)
        {
            //url = url.Replace("." + BaseConfig.extension, "");
            Dictionary<string, object> variable = new Dictionary<string, object>();
            double dataTypeId = 0, rootId = 0, moduleId = 0, skinId = 0, id = 0, classId = 0;
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
                model =this.GetModel("u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", "classId=@classId and u_type=@typeId and u_datatypeId=@datatypeId and u_defaultFlag=@isDefault  and u_webFAid=@isMobile", p);
            }else
            {
                model = this.GetModel("u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", "classId=@classId and u_type=@typeId and u_datatypeId=@datatypeId and u_defaultFlag=@isDefault and title=@title and u_webFAid=@isMobile", p);
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
            model = this.GetModel("u_content,u_datatypeId,u_type,u_editboxStatus,u_defaultFlag,u_webFAid,classId,u_parameterValue", 
                "classId in (0,@moduleId,@rootId) and B.u_defaultFlag=1 and B.u_datatypeid=@datatypeId and B.u_type=@typeId and B.u_webFAid=@isMobile", p, u_layer desc);
            
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
