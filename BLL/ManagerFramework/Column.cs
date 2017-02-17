using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper.Extensions;
namespace ManagerFramework
{
    public class Column: BaseColumn
    {
        /// <summary>
        /// 栏目目录
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 所属模块id
        /// </summary>
        public double ModuleId { get; set; }
        public Column(double columnId): base(columnId)
        {

        }
        public override void SetAttrubite(double id)
        {
            Dictionary<string, object> model = this.GetModel(id);
            if (model == null) throw new Exception("栏目不存在");
            Id = id;
            ParentId = model["classId"].ToDouble();
            Name = model["className"].ToStr();
            MnemonicName = model["dirName"].ToStr();
            Picture = model["maxico"].ToStr();
            RootId = model["rootId"].ToDouble();
            OrderID = model["orderID"].ToInt();
            Layer = model["Layer"].ToInt();
            Info = model["info"].ToStr();
        }
        public static Column Get(double columnId)
        {
            try
            {
                return new Column(columnId);
            }
            catch
            {
                return null;
            }
        }
    }
}
