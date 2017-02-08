using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MWMS.DAL;
using System.Data.SqlClient;
using Helper;

namespace MWMS.Template
{
    public enum TemplateType
    {
        主页=0,
        频道页=1,
        栏目页=2,
        自定义页=3,
        视图=10
    }
    public enum EditMode
    {
        代码模式 = 1,
        设计模式 = 0
    }
    public class Template:TableHandle
    {
        /// <summary>
        /// 参数表
        /// </summary>
        public string ParameterValue { get; set; }
        /// <summary>
        /// 编辑模型
        /// </summary>
        public EditMode EditMode { get; set; }
        /// <summary>
        /// 模板id
        /// </summary>
        public double TemplateId { get; set; }
        private TemplateType _templateType;
        /// <summary>
        /// 模板类型
        /// </summary>
        public TemplateType TemplateType {
            get { return _templateType; }
            set { _templateType = value; this.TableName = value == TemplateType.视图 ? "dataView" : "htmlTemplate"; }
        }
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string TemplateContent { get; set; }
        /// <summary>
        /// 栏目id
        /// </summary>
        public double ColumnId { get; set; }
        /// <summary>
        /// 模板备份
        /// </summary>
        //public abstract void Backup();
        public void Remove()
        {
            if(TemplateType== TemplateType.视图)
            {
                DBHelper.SqlServer.ExecuteNonQuery("delete  dataView where id=@id", new SqlParameter[] {
                    new SqlParameter("id",TemplateId)
                });
            }
            else { 
                DBHelper.SqlServer.ExecuteNonQuery("delete  htmlTemplate where id=@id", new SqlParameter[] {
                    new SqlParameter("id",TemplateId)
                });
            }
        }
        /// <summary>
        /// 备份模板
        /// </summary>
        /// <param name="username">操作人</param>
        public void Backup(string username)
        {

            if (TemplateType == TemplateType.视图)
            {
                Sql.ExecuteNonQuery("insert into backupTemplate (id,dataId,u_content,updatedate,username)"+
                    "select @id,@dataId,u_html,getdate(),@username from dataView where id=@id", new SqlParameter[]{
                 new SqlParameter("id",double.Parse(Tools.GetId())),
                 new SqlParameter("dataId",this.TemplateId),
                 new SqlParameter("username",username)
                });
            }
            else
            {
                                DBHelper.SqlServer.ExecuteNonQuery("insert into backupTemplate (id,dataId,classId,u_type,title,u_content,updateDate,userName,u_webFAid,u_defaultflag,u_datatypeId)" +
"select @id,B.id,B.classid,B.u_type,B.title,B.u_content,getdate(),@username,B.u_webFAid,B.u_defaultflag,u_datatypeId from htmlTemplate B  " +
" where B.id=@dataId", new SqlParameter[]{
                 new SqlParameter("id",double.Parse(Tools.GetId())),
                 new SqlParameter("dataId",this.TemplateId),
                 new SqlParameter("username",username),
            });
            }
        }
        public static void Backup(double templateId, string html, string username)
        {
            Sql.ExecuteNonQuery("insert into backupTemplate (id,dataId,classId,u_type,title,u_content,updateDate,userName,u_webFAid,u_defaultflag,u_datatypeId)" +
"select @id,B.id,B.classid,B.u_type,B.title,@u_content,getdate(),@username,B.u_webFAid,B.u_defaultflag,u_datatypeId from htmlTemplate B  " +
" where B.id=@dataId", new SqlParameter[]{
                 new SqlParameter("id",double.Parse(API.GetId())),
                 new SqlParameter("dataId",templateId),
                 new SqlParameter("u_content",html),
                 new SqlParameter("username",username),
            });
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Dictionary<string,object> Get(string fields)
        {
            return GetModel(this.TemplateId, fields);
        }
        /// <summary>
        /// 编译模板
        /// </summary>
        public void Build(bool flag)
        {
            BuildCode build = new BuildCode(this.TemplateId.ToString(), this.TemplateContent);
            build.compile(flag);
        }
        public void Build()
        {
            Build(false);
        }
    }
}
