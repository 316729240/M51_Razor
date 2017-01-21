using System;
using System.Collections.Generic;
using System.Text;
using Helper;
using System.Data.SqlClient;
using System.Collections;
using System.Text.RegularExpressions;

namespace MWMS
{
    public class ReturnPageData
    {
        public int pageSize = BaseConfig.managePageSize;
        public int recordCount = 0;
        public int pageNo = 0;
        public int pageCount = 0;
        public ArrayList data = new ArrayList();
    }
    public class TableInfo
    {
        public List<FieldInfo> fields = new List<FieldInfo>();
        public int width = 0;
        public string tableName = "";
        string titleField = "title";
        string [] displayField =null;
        string publicFieldStr = "id-数据ID-Double--15----否---------|classId-数据所属栏目-Double------否---------|createDate-创建时间-Date--20----否---------|updateDate-修改时间-Date--20----否---------|userId-用户ID-Double------否---------|orderId-排序至顶-Long------否---------|title-标题-String-0-30-TextBox---是---------|attribute-属性-String-0-10-TextBox---是---------|auditorId-审核者-Double------否---------|auditDate-审核时间-Date--20----否---------|auditMsg-审核信息-string--20----否---------|moduleId-模块id-Double------否---------|rootId-根id-Double------否---------|datatypeId-类型-Double------否---------|pic-图片-String-0-30-TextBox---是---------|skinId-皮肤-Double------否---------";
        public TableInfo(double id)
        {
            string tableStructure = "";
            SqlDataReader rs = Sql.ExecuteReader("select TableName,TableStructure,displayField from datatype where id=@id", new SqlParameter[] { new SqlParameter("id", id) });
            if (rs.Read())
            {
                tableName = rs[0].ToString(); 
                tableStructure = rs[1].ToString();
                displayField = rs[2].ToString().Split(',');
                //fieldList_Sql = rs[2].ToString(); 
                //dataTypeId = rs.GetDouble(3);
            }
            rs.Close();
            if(id>00)getFields(publicFieldStr,true);
            getFields(tableStructure,false);
        }
        public void getFields(string structure,bool flag)
        {
            string[] TS = structure.Split('|');
            for (int n = 0; n < TS.Length; n++)
            {
                if (TS[n] != "")
                {
                    string[] FL = TS[n].Split('-');
                    FieldInfo info = new FieldInfo();
                    info.name = FL[0];
                    info.text = FL[1];
                    info.type = FL[2];
                    if (FL[3] != "") info.minLenth = int.Parse(FL[3]);
                    if (FL[4] != "") info.maxLenth = int.Parse(FL[4]);
                    if (FL[6] != "") info.format = FL[6];
                    info.control = FL[5];
                    if (info.name == "id") info.isNecessary = true;
                     if (info.name == "orderId") info.isNecessary = true;
                     if (info.name == "auditMsg") info.isNecessary = true;
                    if (info.name == titleField)
                    {
                        info.visible = true;
                        info.isTitle = true;
                    }
                    info.isPublicField = flag;
                    for (int i = 0; i < displayField.Length; i++)
                    {
                        if (displayField[i].ToLower() == info.name.ToLower())
                        {
                            info.visible = true;
                            i = displayField.Length;
                        }
                    }
                    int findex = 0;
                    for(int i = 0; i < fields.Count; i++)
                    {
                        if (fields[i].name == info.name) findex = i;
                    }
                    if (findex > 0) fields[findex] = info;
                    else { fields.Add(info); }
                    
                }
            }
        }
        public ReturnPageData getDataList(double moduleId,double classId, int pageNo, string orderBy, int sortDirection,string where)
        {
            ReturnPageData r = new ReturnPageData();
            string fieldStr = "";
            bool attachedFlag = false;//是否有附加字段
            bool showClassId = false,showUserId=false, showAuditorId=false;
            foreach (FieldInfo f in fields)
            {
                if (f.visible || f.isNecessary)
                {
                    if (fieldStr != "") fieldStr += ",";
                    if (f.name == "attribute")
                    {
                        fieldStr += "(convert(varchar(6),A.orderid)+','+convert(varchar(1),A.attr0)+','+convert(varchar(1),A.attr1)+','+convert(varchar(1),A.attr2)+','+convert(varchar(1),A.attr3)+','+convert(varchar(1),A.attr4)) attribute";
                    }
                    else if (f.name == "classId")
                    {
                        fieldStr += "C.className";
                        showClassId = true;
                    }
                    else if (f.name == "userId")
                    {
                        fieldStr += "D.uname";
                        showUserId = true;
                    }
                    else if (f.name == "auditorId")
                    {
                        fieldStr += "E.uname auditor";
                        showAuditorId = true;
                    }
                    else
                    {
                        fieldStr += (f.name.IndexOf("u_")==-1 ? "A." : "B.") + f.name;
                    }
                    if(!f.isPublicField)attachedFlag = true;
                }
            }
            if(where.IndexOf("u_")>-1)attachedFlag = true;
            string orderStr = "order by A.orderid desc,A.createdate desc";
            if (orderBy != "")
            {
                orderStr = "order by " + ((orderBy.IndexOf("u_") == -1) ? "A." : "B.") + orderBy + " " + (sortDirection == 1 ? "desc" : "");
            }
            string mainWhere = "";
            if (moduleId == classId)
            {
                mainWhere = " A.moduleId=" + moduleId.ToString() + " ";
            }
            else
            {
                string childId = ColumnClass.getChildId(classId);
                mainWhere = " A.classId in (" + childId + ") ";
            }

            string countSql = "select count(1) from [mainTable] A ";
            string sql2 = "select " + fieldStr + ",ROW_NUMBER() Over(" + orderStr + ") as rowNum from ";
            sql2 += "[mainTable] A";
            if (attachedFlag)
            {
                sql2 += " left join [" + tableName + "] B on A.id=B.id ";
                countSql += " left join [" + tableName + "] B on A.id=B.id ";

            }

            r.recordCount = (int)(Sql.ExecuteScalar(countSql+" where" + mainWhere + where));
            //if (attachedFlag) sql2 += " on A.id=B.id ";
            if (showClassId) sql2 += " left join [class] C on A.classId=C.id ";
            if (showUserId) sql2 += " left join [m_admin] D on A.userId=D.id ";
            if (showAuditorId) sql2 += " left join [m_admin] E on A.auditorId=E.id ";
            
            sql2 += " where " + mainWhere + " " + where;
            string sql = "SELECT * FROM (" +
                sql2 +
                " ) as A where rowNum> "+((pageNo-1)*r.pageSize).ToString()+" and rowNum<"+(pageNo*r.pageSize+1).ToString();
            r.data= Sql.ExecuteArrayObj(sql);
            r.pageNo = pageNo;
            return r;
        }

        public static ErrInfo moveData(string ids,double classId)
        {
            ErrInfo info = new ErrInfo();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            ColumnInfo column = ColumnClass.get(classId);
            if (column == null)
            {
                info.errNo = -1;
                info.errMsg = "栏目不存在";
                return info;
            }
            ColumnInfo channel = ColumnClass.get(column.rootId);
            StringBuilder url = new StringBuilder(BaseConfig.contentUrlTemplate);
            url.Replace("$id", "'+convert(varchar(20),convert(decimal(18,0),id))+'");
            url.Replace("$create.year", "'+convert(varchar(4),year(createdate))+'");
            url.Replace("$create.month", "'+right('00'+cast(month(createdate) as varchar),2)+'");
            url.Replace("$create.day", "'+right('00'+cast(day(createdate) as varchar),2)+'");
            url.Replace("$column.dirPath", column.dirPath);
            url.Replace("$column.dirName", column.dirName);
            url.Replace("$channel.dirName", channel.dirName);
            url.Replace(".$extension", "");
            Sql.ExecuteNonQuery("update mainTable set  classId=@classId,rootId=@rootId,moduleId=@moduleId,url='" + url + "'  where id in (" + ids + ")", new SqlParameter[] { new SqlParameter("classId", classId), new SqlParameter("rootId", column.rootId), new SqlParameter("moduleId", column.moduleId) });
            
            return info;
        }
        public static ErrInfo setAttr(string ids, int type,bool flag)
        {
            ErrInfo info = new ErrInfo();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            if (type < 0 || type > 4)
            {
                info.errNo = -1;
                info.errMsg = "属性字段不存在";
                return info;
            }
            if (flag)
            {
                Sql.ExecuteNonQuery("update mainTable set attr"+type.ToString()+"=1 where id in (" + ids + ")");
            }
            else
            {
                Sql.ExecuteNonQuery("update mainTable set attr" + type.ToString() + "=0 where id in (" + ids + ")");
            }
            return info;
        }

        public static ErrInfo auditData(string ids, bool flag,UserInfo value)
        {
            return auditData(ids, flag, "",value);
        }
        public static ErrInfo auditData(string ids, bool flag,string auditMsg, UserInfo value)
        {
            ErrInfo info = new ErrInfo();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            if (flag)
            {
 
                Sql.ExecuteNonQuery("update mainTable set orderId=0,auditDate=getdate(),auditorId=@auditorId where id in (" + ids + ") ", new SqlParameter[] {  new SqlParameter("auditorId", value.id) });
            }
            else
            {
                Sql.ExecuteNonQuery("update mainTable set orderId=-2,auditMsg=@auditMsg,auditorId=@auditorId,auditDate=getdate() where id in (" + ids + ") ",new SqlParameter[] { new SqlParameter("auditMsg", auditMsg), new SqlParameter("auditorId", value.id) });
            }
            return info;
        }
        public static ErrInfo setTop(string ids,bool flag)
        {
            ErrInfo info = new ErrInfo();
            string [] id=ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            if (flag)
            {
                int newOrderId = 0;
                try{
                newOrderId= (int)(Sql.ExecuteScalar("select max(orderId) from mainTable where id in (" + ids + ") and orderId>-1"));
                }catch{
                }
                newOrderId++;
                Sql.ExecuteNonQuery("update mainTable set orderId="+newOrderId.ToString()+" where id in (" + ids + ") and orderId>-1");
            }
            else
            {
                Sql.ExecuteNonQuery("update mainTable set orderId=0 where id in ("+ids+ ") and orderId>-1");
            }
            return info;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="dataTypeId"></param>
        /// <param name="ids"></param>
        /// <param name="deleteFlag">是否物理删除</param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static ErrInfo delData(double dataTypeId, string ids, bool deleteFlag, UserInfo user)
        {
            ErrInfo info = new ErrInfo();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
            string tablename = "";
            object value= Helper.Sql.ExecuteScalar("select tablename from datatype where id=@id",new SqlParameter []{
                new SqlParameter("id",dataTypeId)
            });
            if (value == null)
            {
                info.errMsg = "表类型不存在";
                info.errNo = -1;
                return info;
            }
            tablename = (string)value;
            if (deleteFlag)
            {
                Helper.Sql.ExecuteNonQuery("delete from " + tablename + " where id in (" + ids + ")");
                Helper.Sql.ExecuteNonQuery("delete from maintable where id in (" + ids + ")");
            }
            else
            {
                Helper.Sql.ExecuteNonQuery("update  maintable set orderId=-3 where id in (" + ids + ")");
            }
            API.writeLog("1", "删除数据[" + tablename + "][" + ids + "]");
            return (info);
        }
        public static ErrInfo reductionData(string ids,  UserInfo user)
        {
            ErrInfo info = new ErrInfo();
            string[] id = ids.Split(',');
            try
            {
                for (int i = 0; i < id.Length; i++)
                {
                    double.Parse(id[i]);
                }
            }
            catch
            {
                info.errNo = -1;
                info.errMsg = "参数不合法";
                return info;
            }
             Helper.Sql.ExecuteNonQuery("update  maintable set orderId=0 where id in (" + ids + ")");
            return (info);
        }
    }
}