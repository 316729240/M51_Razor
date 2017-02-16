using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MWMS.DAL;
namespace MWMS.DAL.Datatype
{
    public class TableStructure
    {
        string publicFieldStr = "id-数据ID-Double--15----否---------|classId-数据所属栏目-Double------否---------|createDate-创建时间-Date--20----否---------|updateDate-修改时间-Date--20----否---------|userId-用户ID-Double------否---------|orderId-排序至顶-Long------否---------|title-标题-String-0-30-TextBox---是---------|attribute-属性-String-0-10-TextBox---是---------|auditorId-审核者-Double------否---------|auditDate-审核时间-Date--20----否---------|auditMsg-审核信息-string--20----否---------|moduleId-模块id-Double------否---------|rootId-根id-Double------否---------|datatypeId-类型-Double------否---------|pic-图片-String-0-30-TextBox---是---------|skinId-皮肤-Double------否---------";
        /// <summary>
        /// 数据类型id
        /// </summary>
        public double DatatypeId { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 字段列表
        /// </summary>
        public List<Field> Fields = null;
        public TableStructure(double datatypeId)
        {
            this.DatatypeId = datatypeId;
            MWMS.DAL.TableHandle table = new MWMS.DAL.TableHandle("Datatype");
            Dictionary<string, object> model = table.GetModel(datatypeId, "tableName,tableStructure,displayField");
            Init(model);
        }
        public TableStructure(string tableName)
        {
            MWMS.DAL.TableHandle table = new MWMS.DAL.TableHandle("Datatype");
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("tableName", tableName);
            Dictionary<string, object> model = table.GetModel( "tableName,tableStructure,displayField", "tableName=@tableName",p);
            Init(model);
        }
        void Init(Dictionary<string, object> model)
        {
            TableName = model["tableName"].ToString();
            Fields = new List<Field>();
            List<Field> Structure = new List<Field>();
            if (model == null) throw new Exception("表类型不存在");
            LoadPublicField();
            string[] list = model["tableStructure"].ToString().Split('|');
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] != "") { 
                Field f = new Field(list[i]);
                f.isPublicField = false;
                Fields.Add(f);
                }
            }
        }
        /// <summary>
        /// 加载公共字段
        /// </summary>
        void LoadPublicField()
        {
            string[] list = publicFieldStr.Split('|');
            for (int i = 0; i < list.Length; i++)
            {
                Field f = new Field(list[i]);
                f.isPublicField = true;
                Fields.Add(f);
            }
        }
    }
}
