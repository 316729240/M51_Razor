using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web;
using Helper;
using System.Collections;
using System.IO;

namespace MWMS
{
    public class ModuleClass
    {
        public static ModuleInfo get(double id)
        {
            ModuleInfo info = null;
            SqlDataReader rs = Sql.ExecuteReader("select A.id,A.moduleName,A.type,A.saveDataType,A.dirName,A.createdate,A.updatedate,A.custom,A.thumbnailWidth,A.thumbnailHeight,A.thumbnailForce,A.saveRemoteImages,A.orderId,B.keyword,B.info,B.domainName,A.titleRepeat,A.watermark,B._domainName from module A left join  class B on A.id=B.id where A.id=@id", new SqlParameter[]{
                new SqlParameter("id",id)});
            if (rs.Read())
            {
                info = new ModuleInfo();
                info.id = rs.GetDouble(0);
                info.moduleName = rs.GetString(1);
                info.type = rs.GetBoolean(2);
                info.saveDataType = rs.GetDouble(3);
                info.dirName = rs.GetString(4);
                info.createDate = rs.GetDateTime(5);
                info.updateDate = rs.GetDateTime(6);
                info.custom = rs.GetString(7);
                info.thumbnailWidth = rs.GetInt32(8);
                info.thumbnailHeight = rs.GetInt32(9);
                info.thumbnailForce = rs.GetInt32(10);
                info.saveRemoteImages = rs.GetInt32(11);
                try
                {
                    info.orderId = rs.IsDBNull(12) ? 0 : rs.GetInt32(12);
                }
                catch { }
                info.keyword = rs[13] + "";
                info.info = rs[14] + "";
                info.domainName = rs[15] + "";
                info.titleRepeat = (rs.IsDBNull(16) || rs.GetInt32(16) == 1) ? 1 : 0;
                info.watermark= (rs.IsDBNull(17) || rs.GetInt32(17) == 1) ? 1 : 0;
                info._domainName = rs[18] + "";
            }
            rs.Close();
            return info;
        }

        public static ErrInfo add(ModuleInfo info, UserInfo user)
        {
            ErrInfo err = new ErrInfo();
            #region 验证
            if (info.moduleName.Trim() == "")
            {
                err.errNo = -1;
                err.errMsg = "栏目名不能为空";
                return err;
            }
            if (info.dirName.Trim() == "")
            {
                err.errNo = -1;
                err.errMsg = "目录名不能为空";
                return err;
            }
            #endregion
            info.id = double.Parse(API.GetId());
            int count=(int)Sql.ExecuteScalar("select count(1) from module where dirName=@dirName",new SqlParameter[]{
                new SqlParameter("dirName",info.dirName.ToLower())
            });
            if (count > 0)
            {
                err.errNo = -1;
                err.errMsg = "频道目录名已存在";
                return err;
            }

            if (info.type)
            {
                ColumnInfo column = new ColumnInfo();
                column.id = info.id;
                column.className = info.moduleName;
                column.classId = 7;
                column.moduleId = info.id;
                column.custom = info.custom;
                column.thumbnailWidth = info.thumbnailWidth;
                column.thumbnailHeight = info.thumbnailHeight;
                column.thumbnailForce = info.thumbnailForce;
                column.saveRemoteImages = info.saveRemoteImages;
                column.inherit = info.inherit;
                column.dirName = info.dirName;
                column.saveDataType = info.saveDataType;
                column.titleRepeat = info.titleRepeat;
                err =ColumnClass.add(column, user);
                if (err.errNo < 0) return err;
            }
            Sql.ExecuteNonQuery("insert into [module] " +
                "(id,moduleName,type,saveDataType,dirName,createDate,updateDate,custom,thumbnailWidth,thumbnailHeight,thumbnailForce,saveRemoteImages,orderId,titleRepeat,watermark)" +
                "values" +
                "(@id,@moduleName,@type,@saveDataType,@dirName,@createDate,@updateDate,@custom,@thumbnailWidth,@thumbnailHeight,@thumbnailForce,@saveRemoteImages,@orderId,@titleRepeat,@watermark)"
                , new SqlParameter[]{
                new SqlParameter("id",info.id),
                new SqlParameter("moduleName",info.moduleName),
                new SqlParameter("type",info.type),
                new SqlParameter("saveDataType",info.saveDataType),
                new SqlParameter("dirName",info.dirName),
                new SqlParameter("createDate",info.createDate),
                new SqlParameter("updateDate",info.updateDate),
                new SqlParameter("custom",info.custom),
                new SqlParameter("thumbnailWidth",info.thumbnailWidth),
                new SqlParameter("thumbnailHeight",info.thumbnailHeight),
                new SqlParameter("thumbnailForce",info.thumbnailForce),
                new SqlParameter("saveRemoteImages",info.saveRemoteImages),
                new SqlParameter("orderId",info.orderId),
                new SqlParameter("titleRepeat",info.titleRepeat),
                new SqlParameter("watermark",info.watermark)
                }
                );
            err.userData = info.id;
            return err;
        }
        public static ErrInfo edit(ModuleInfo info,UserInfo user)
        {
            ErrInfo err = new ErrInfo();
            #region 验证
            if (info.moduleName.Trim() == "")
            {
                err.errNo = -1;
                err.errMsg= "模块名不能为空";
                return err;
            }
            #endregion
            if (info.id > 0)
            {
                Sql.ExecuteNonQuery("update [module] " +
                "set id=@id,moduleName=@moduleName,saveDataType=@saveDataType,updateDate=@updateDate,custom=@custom,thumbnailWidth=@thumbnailWidth,thumbnailHeight=@thumbnailHeight,thumbnailForce=@thumbnailForce,saveRemoteImages=@saveRemoteImages,titleRepeat=@titleRepeat,watermark=@watermark" +
                " where id=@id"
                , new SqlParameter[]{
                new SqlParameter("id",info.id),
                new SqlParameter("moduleName",info.moduleName),
                new SqlParameter("saveDataType",info.saveDataType),
                new SqlParameter("createDate",info.createDate),
                new SqlParameter("updateDate",info.updateDate),
                new SqlParameter("custom",info.custom),
                new SqlParameter("thumbnailWidth",info.thumbnailWidth),
                new SqlParameter("thumbnailHeight",info.thumbnailHeight),
                new SqlParameter("thumbnailForce",info.thumbnailForce),
                new SqlParameter("saveRemoteImages",info.saveRemoteImages),
                new SqlParameter("titleRepeat",info.titleRepeat),
                new SqlParameter("watermark",info.watermark)
                }
                );
                if (info.type)
                {
                    Sql.ExecuteNonQuery("update [class] " +
                        "set className=@className,classId=@classId,moduleId=@moduleId,rootId=@rootId,custom=@custom,thumbnailWidth=@thumbnailWidth,thumbnailHeight=@thumbnailHeight,thumbnailForce=@thumbnailForce,saveRemoteImages=@saveRemoteImages,inherit=@inherit,updateDate=@updateDate,dirName=@dirName,dirPath=@dirPath,keyword=@keyword,info=@info,domainName=@domainName,_domainName=@_domainName,titleRepeat=@titleRepeat where id=@id", new SqlParameter[]{
                new SqlParameter("id",info.id),
                new SqlParameter("className",info.moduleName),
                new SqlParameter("classId",7),
                new SqlParameter("moduleId",info.id),
                new SqlParameter("rootId",info.id),
                new SqlParameter("custom",info.custom),
                new SqlParameter("thumbnailWidth",info.thumbnailWidth),
                new SqlParameter("thumbnailHeight",info.thumbnailHeight),
                new SqlParameter("thumbnailForce",info.thumbnailForce),
                new SqlParameter("saveRemoteImages",info.saveRemoteImages),
                new SqlParameter("inherit",info.inherit),                
                new SqlParameter("createDate",info.createDate),                
                new SqlParameter("updateDate",info.updateDate),
                new SqlParameter("dirName",info.dirName),
                new SqlParameter("dirPath",info.dirName),
                new SqlParameter("keyword",info.keyword),
                new SqlParameter("info",info.info),
                new SqlParameter("domainName",info.domainName),
                new SqlParameter("_domainName",info._domainName),
                new SqlParameter("titleRepeat",info.titleRepeat)
                });
                }
            }
            else
            {
                err = add(info, user);
                return err;
            }
            err.userData = info.id;
            return err;
        }
        public static ErrInfo del(double id,double classId,UserInfo user)
        {
            ErrInfo err = new ErrInfo();
            Permissions p = user.getModulePermissions(id);
            if (!p.all)
            {
                err.errNo = -1;
                err.errMsg = "没有删除该模块的权限";
                return err;
            }
            Sql.ExecuteNonQuery("delete from module where id=@moduleId", new SqlParameter[]{
                new SqlParameter("moduleId",id)
            });
            if (id == classId)
            {
                Sql.ExecuteNonQuery("delete from class where id=@id", new SqlParameter[]{
                new SqlParameter("id",id)
            });
            }
            return err;
        }

        public static ErrInfo editDirName(double id, string dirName, UserInfo user)
        {
            ErrInfo err = new ErrInfo();
            int count = (int)Sql.ExecuteScalar("select count(1) from module where id<>@id and dirName=@dirName", new SqlParameter[]{
                new SqlParameter("id",id),
                new SqlParameter("dirName",dirName.ToLower())
            });
            count = count+(int)Sql.ExecuteScalar("select count(1) from class where classid=7 and id<>@id and dirName=@dirName", new SqlParameter[]{
                new SqlParameter("id",id),
                new SqlParameter("dirName",dirName.ToLower())
            });
            if (count > 0)
            {
                err.errNo = -1;
                err.errMsg = "模块目录名已存在";
                return err;
            }
            Sql.ExecuteNonQuery("update module set dirName=@dirName where id=@id", new SqlParameter[]{
                new SqlParameter("id",id),
                new SqlParameter("dirName",dirName)});
            Sql.ExecuteNonQuery("update class set dirName=@dirName where id=@id", new SqlParameter[]{
                new SqlParameter("id",id),
                new SqlParameter("dirName",dirName)});
            return err;
        }
    }
    public class ModuleInfo
    {
        public double id = -1;
        public string moduleName = "";
        public bool type = true;//是否为真实目录
        public double saveDataType = -1;
        public string dirName = "";
        public DateTime createDate = System.DateTime.Now;
        public DateTime updateDate = System.DateTime.Now;
        public int orderId = 0;
        /// <summary>
        /// 扩展字段
        /// </summary>
        public string custom = "";
        /// <summary>
        /// 栏目图片宽度
        /// </summary>
        public int thumbnailWidth = 0;
        /// <summary>
        /// 栏目图片高度
        /// </summary>
        public int thumbnailHeight = 0;
        /// <summary>
        /// 栏目图片是否剪裁（0 否 1是）
        /// </summary>
        public int thumbnailForce = 0;
        /// <summary>
        /// 是否保存远程图片（0 否 1是）
        /// </summary>
        public int saveRemoteImages = 0;
        /// <summary>
        /// 是否加水印
        /// </summary>
        public int watermark = 1;
        /// <summary>
        /// 是否继承（0 否 1是）
        /// </summary>
        public int inherit = 0;
        /// <summary>
        /// pc域名
        /// </summary>
        public string domainName = "";
        /// <summary>
        /// 手机域名
        /// </summary>
        public string _domainName = "";
        public int titleRepeat = 1;//是否允许标题重复
        public string keyword = "";
        public string info = "";
    }
}
