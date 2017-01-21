<%@ WebHandler Language="C#" Class="ajax"%>
using System;
using System.Web;
using System.Collections.Generic;
using MWMS;
using Helper;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Script.Serialization;
using Ionic.Zip;
using System.Text;
public class ajax : IHttpHandler {
    
    SafeReqeust s_request = new SafeReqeust(0, 0);
    LoginInfo login = new LoginInfo();
    public void ProcessRequest(HttpContext context)
    {
        login.checkLogin();
        context.Response.ContentType = "text/plain";
        if (context.Request.Form["_m"] == null) context.Response.End();
        string m = context.Request.Form["_m"].ToString();
        if (m == "cardPermissions")
        {
            context.Response.Write(login.value.isAdministrator);
        }
        else if (m == "saveTableStructure") saveTableStructure(context);
        else if (m == "saveTableData") saveTableData(context);
        else if (m == "zipData") zipData(context);
        else if (m == "readTableStructure") readTableStructure(context);
        else if (m == "readTableData") readTableData(context);
        else if (m == "runScript") runScript(context);
        else if (m == "readTableList") readTableList(context);
        else if (m == "dataList") dataList(context);
        else if (m == "readBackupList") readBackupList(context);
        context.Response.End();
    }
    void readBackupList(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        DirectoryInfo dir = new DirectoryInfo(context.Server.MapPath("~"+ Config.backupPath));
        FileInfo [] f= dir.GetFiles("*.zip");
        List<string[]> list = new List<string[]>();
        for (int i = 0; i < f.Length; i++)
        {
            list.Add(new string[] {f[i].Name.Replace(".zip",""),f[i].CreationTime.ToString("yyyy-MM-dd HH:mm:ss") });
        }
        info.userData = list;
        context.Response.Write(info.ToJson());
    }
    void dataList(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string sql=s_request.getString("sql");
        int pageNo = s_request.getInt("pageNo");
        List<FieldInfo> flist =new List<FieldInfo>();
        ReturnPageData r = new ReturnPageData();
        string orderBy = "";
        string[] temp = Regex.Split(sql, "order by", RegexOptions.IgnoreCase);
        if (temp.Length > 1)
        {
            sql = temp[0];
            orderBy = "order by " + temp[1];
        }
        string fieldList = sql.SubString("select", "from");
        r.recordCount = (int)(Sql.ExecuteScalar(sql.Replace(fieldList, " count(1) ")));
        if (orderBy == "") orderBy = "order by (select 0)";
        sql = sql.Replace(fieldList, fieldList + ",row_number() OVER(" + orderBy + ") row_number ");
        ArrayList arrayList = new ArrayList();
        SqlDataReader rs = Sql.ExecuteReader(sql);
        for (int i = 0; i < rs.FieldCount-1; i++)
        {
            FieldInfo f = new FieldInfo();
            f.name = rs.GetName(i);
            f.text = f.name;
            f.visible = true;
            f.width = 100;
            flist.Add(f);
        }
        while (rs.Read())
        {
            object[] dictionary = new object[rs.FieldCount];
            for (int i = 0; i < rs.FieldCount-1; i++)
            {
                if (rs.GetDataTypeName(i) == "ntext")
                {
                    dictionary[i] = "[ntext]";
                }
                else
                {
                    dictionary[i] = rs[i].ToString();
                }
            }
            arrayList.Add(dictionary);
        }
        rs.Close();
        r.data = Sql.ExecuteArrayObj(sql);
        r.pageNo = pageNo;
        r.data = arrayList;
        object[] data = new object[] { flist, r };
        info.userData = data;
        context.Response.Write(info.ToJson());
        
    }
    void readTableList(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        List<string[]> list = new List<string[]>();
        DataSet rs1 = Sql.ExecuteDataset("select tablename,id,datatype from datatype where Attribute='N'");
        SqlDataReader rs = Sql.ExecuteReader("select name from sysobjects where (xtype = 'U') AND (name <> 'dtproperties')");
        while (rs.Read())
        {
            bool flag = false;
            for (int i = 0; i < rs1.Tables[0].Rows.Count; i++)
            {
                if (rs[0].ToString() == rs1.Tables[0].Rows[i][0].ToString())
                {
                    list.Add(new string []{"1",rs1.Tables[0].Rows[i][2].ToString(),rs[0].ToString()});
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                list.Add(new string[] { "0", rs[0].ToString(), rs[0].ToString() });
            }
        }
        rs.Close();
        info.userData = list;
        context.Response.Write(info.ToJson());
    }
    void runScript(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string backName = s_request.getString("backName");
        string path = context.Server.MapPath("~" + Config.backupPath + backName);//备份文件
        string sqlScript = "";
        using (ZipFile zip = new ZipFile(@path))
        {
            var r = zip["table.dat"].OpenReader();
            byte[] buffer = new byte[zip["table.dat"].UncompressedSize];
            r.Read(buffer, 0, buffer.Length);
            r.Close();

            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            ArrayList table = (ArrayList)formatter.Deserialize(stream);
            stream.Close();
            for (int i = 0; i < table.Count; i++)
            {
                Dictionary<string, object> list = (Dictionary<string, object>)(table[i]);
                string TableName = (string)list["tableName"];
                Helper.Sql.ExecuteNonQuery("DROP TABLE [" + TableName + "]");
                Helper.Sql.ExecuteNonQuery("exec sp_rename 'temp_" + TableName + "','" + TableName + "' ");
            }
            info.userData = table;
        }

        try
        {
            sqlScript = Http.getUrl("http://" + ConfigurationManager.AppSettings["OfficialWeb"] + "/sqlscript.txt");
        }
        catch
        {
            info.errNo = -1;
            info.errMsg = "无法获取最新数据库脚本信息，还原未完成";
            context.Response.Write(info.ToJson());
            return;
        }
        string[] script = Regex.Split(sqlScript,"GO");
        for(int i=0;i<script.Length;i++){
            if (script[i].Trim() != "")
            {
                try
                {
                    Helper.Sql.ExecuteNonQuery(script[i]);
                }
                catch
                {
                }
            }
        }
        context.Response.Write(info.ToJson());
    }
    void readTableData(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string backName = s_request.getString("backName");
        string path = context.Server.MapPath("~" + Config.backupPath + backName);//备份文件
        string tableName = s_request.getString("tableName");
        int pageNo = s_request.getInt("pageNo");
        int pageSize = s_request.getInt("pageSize");

        using (ZipFile zip = new ZipFile(@path))
        {
            string name = tableName + "_" + pageNo.ToString() + ".dat";
            var r = zip[name].OpenReader();
            byte[] buffer = new byte[zip[name].UncompressedSize];
            r.Read(buffer, 0, buffer.Length);
            r.Close();
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            DataTable table = (DataTable)formatter.Deserialize(stream);
            stream.Close();
            string fieldList = "",fieldList2="";
            for (int i = 0; i < table.Columns.Count-1; i++)
            {
                if (i > 0)
                {
                    fieldList += ",";
                    fieldList2 += ",";
                }
                fieldList += table.Columns[i].ColumnName;
                fieldList2 += "@"+table.Columns[i].ColumnName ;
            }
            string sql = "insert into [" + "temp_" + tableName + "] (" + fieldList + ")values(" + fieldList2 + ")";
            using (SqlConnection cn = new SqlConnection(Sql.connectionString))
            {
                SqlParameter[] p = new SqlParameter[table.Columns.Count-1];
                for (int i = 0; i < p.Length; i++) p[i] = new SqlParameter(table.Columns[i].ColumnName, null);

                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();
                try
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        for (int i1 = 0; i1 < p.Length; i1++) p[i1].Value = table.Rows[i][i1];
                        Sql.ExecuteNonQuery(tran, CommandType.Text, sql, p);
                    }
                    tran.Commit();
                }
                catch
                {
                }
                cn.Close();
            }
        }
        context.Response.Write(info.ToJson());
    }
    void readTableStructure(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string backName = s_request.getString("backName");
        string path = context.Server.MapPath("~" + Config.backupPath+backName);//备份文件
        using (ZipFile zip = new ZipFile(@path))
        {
            var r = zip["table.dat"].OpenReader();
            byte[] buffer = new byte[zip["table.dat"].UncompressedSize];
            r.Read(buffer, 0, buffer.Length);
            r.Close();
            
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            ArrayList table = (ArrayList)formatter.Deserialize(stream);
            stream.Close();
            for (int i = 0; i < table.Count; i++)
            {
                bool IDTag = false;
                Dictionary<string, object> list = (Dictionary<string, object>)(table[i]);
                List<FieldInfo> f = (List<FieldInfo>)list["fields"];
                StringBuilder Sql = new StringBuilder();
                string TableName = (string)list["tableName"];
                string tableNameTemp = TableName + "_" + (TableName + API.GetId()).MD5();
                Sql.Append("CREATE TABLE ["); Sql.Append("temp_" + list["tableName"]); Sql.Append("] (");
                for (int i1 = 0; i1 < f.Count; i1++)
                {
                    if (f[i1].type == "float")
                    {
                        Sql.Append("[" + f[i1].name + "] [float] CONSTRAINT [DF_" + tableNameTemp + "_" + f[i1].name + "] DEFAULT (0),");
                    }
                    else if (f[i1].type == "nvarchar")
                    {
                        Sql.Append("[" + f[i1].name + "] [nvarchar] (" + f[i1].maxLenth + ") COLLATE Chinese_PRC_CI_AS NULL ,");
                    }
                    else if (f[i1].type == "ntext")
                    {
                        Sql.Append("[" + f[i1].name + "] [ntext] COLLATE Chinese_PRC_CI_AS NULL ,");
                    }
                    else if (f[i1].type == "int")
                    {
                        Sql.Append("[" + f[i1].name + "]  [int]  NULL CONSTRAINT [DF_" + tableNameTemp + "_" + f[i1].name + "] DEFAULT (0),");
                    }
                    else if (f[i1].type == "varchar")
                    {
                        Sql.Append("[" + f[i1].name + "] [varchar] (" + f[i1].maxLenth + ") COLLATE Chinese_PRC_CI_AS NULL,");
                    }
                    else if (f[i1].type == "bit")
                    {
                        Sql.Append("[" + f[i1].name + "] [bit]  NULL CONSTRAINT [DF_" + tableNameTemp + "_" + f[i1].name + "] DEFAULT (0),");
                    }
                    else
                    {
                        Sql.Append("[" + f[i1].name + "] [" + f[i1].type + "] NULL ,");
                    }
                    if (String.Compare(f[i1].name, "id", true) == 0) IDTag = true;
                }
                if (IDTag)
                {
                    #region 设置索引字段
                    Sql.Append("	CONSTRAINT [PK_" + tableNameTemp+"] PRIMARY KEY  CLUSTERED ");
                    Sql.Append("	(");
                    Sql.Append("	[ID]");
                    Sql.Append("	)  ON [PRIMARY] ");
                    #endregion
                }
                Sql.Append("	)");
                object obj = Helper.Sql.ExecuteScalar("select id from sysobjects where name='" + "temp_" + TableName + "'");
                if (obj != null) Helper.Sql.ExecuteNonQuery("DROP TABLE [" + "temp_" + TableName + "]");
                Helper.Sql.ExecuteNonQuery(Sql.ToString());
            }
            info.userData = table;
        }
        context.Response.Write(info.ToJson());
        
    }
    void zipData(HttpContext context)
    {
        ErrInfo info = new ErrInfo();

        string backName = s_request.getString("backName");
        string path = context.Server.MapPath("~" + Config.tempPath + "backDataBase/" + login.sessionId + "/");//临时文件夹

        string newFile = context.Server.MapPath("~" + Config.backupPath + backName + ".zip");
        FileInfo f = new FileInfo(newFile);
        if (f.Exists) f.Delete();
        if (!f.Directory.Exists) f.Directory.Create();
        System.IO.File.Move(@path + login.sessionId+ ".zip", newFile);
        System.IO.Directory.Delete(path, true);
        context.Response.Write(info.ToJson());
    }
    void saveTableData(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string backName = s_request.getString("backName");
        string tableName = s_request.getString("tableName");
        int pageNo = s_request.getInt("pageNo");
        int pageSize = s_request.getInt("pageSize");
        string path = context.Server.MapPath("~"+Config.tempPath+"backDataBase/"+login.sessionId+"/");//临时文件夹
        
            DataTable table = Sql.ExecuteDataset("select * from ( select *, ROW_NUMBER() OVER(order by (select 0)) AS RowNumber from [" + tableName + "]  WITH(NOLOCK) ) as b where RowNumber BETWEEN @s and @e ",
                new SqlParameter[]{
                new SqlParameter("s",(pageNo-1)*pageSize+1),
                new SqlParameter("e",pageNo*pageSize)
            }).Tables[0];
            using (ZipFile zip = new ZipFile(@path + login.sessionId+ ".zip"))
            {
                MemoryStream stream = new MemoryStream();
                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, table);
                try
                {
                    zip.AddEntry(tableName + "_" + pageNo.ToString() + ".dat", stream.ToArray());
                }catch{}
                zip.Save();
                stream.Close();
            }
            System.IO.File.WriteAllText(path+"temp",tableName + "_" + pageNo.ToString());
        context.Response.Write(info.ToJson());
    }
    void saveTableStructure(HttpContext context)
    {
        ErrInfo info = new ErrInfo();
        string path = context.Server.MapPath("~" + Config.tempPath + "backDataBase/" + login.sessionId + "/");//临时文件夹
        if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
        ArrayList tables = new ArrayList();
        if (System.IO.File.Exists(path + "table.dat"))//文件存在时
        {

            string [] lastTemp=System.IO.File.ReadAllText(path + "temp").Split('_');
            tables = (ArrayList)API.readObjectFile(path + "table.dat");
            bool flag=false;
            for (int i = 0; i < tables.Count; i++)
            {
                Dictionary<string, object> obj =(Dictionary<string, object>)tables[i];
                int pageCount=(int)obj["pageCount"];
                string tableName = (string)obj["tableName"];
                if(!flag)obj["start"] = pageCount;
                if (tableName == lastTemp[0])
                {
                    flag = true;
                    obj["start"] = Convert.ToInt32(lastTemp[1]) + 1;
                }
            }
        }
        else
        {
            SqlDataReader rs = Sql.ExecuteReader("select name,id from sysobjects where (xtype = 'U') and name<>'logininfo'  and name<>'invalidLogin'");
            int pageSize = 500;
            while (rs.Read())
            {
                Dictionary<string, object> obj = new Dictionary<string, object>();
                int recordCount = (int)Sql.ExecuteScalar("select count(1) from [" + rs[0] + "]");
                int pageCount = ((recordCount - 1) / pageSize + 1);
                #region 取得表结构
                List<FieldInfo> list = new List<FieldInfo>();
                SqlDataReader rs1 = Sql.ExecuteReader("select A.name,B.name,A.length from SYSCOLUMNS as A,systypes as B where  A.xtype=B.xusertype and A.id=" + rs[1].ToString());
                while (rs1.Read())
                {
                    FieldInfo f = new FieldInfo();
                    f.name = rs1.GetString(0);
                    f.type = rs1.GetString(1);
                    f.maxLenth = int.Parse(rs1[2].ToString());
                    list.Add(f);
                }
                rs1.Close();
                #endregion
                obj["tableName"] = rs[0];
                obj["recordCount"] = recordCount;
                obj["pageCount"] = pageCount;
                obj["pageSize"] = pageSize;
                obj["fields"] = list;
                tables.Add(obj);
            }
            rs.Close();
            API.writeObjectFile(path + "table.dat", tables);
            using (ZipFile zip = new ZipFile(@path + login.sessionId + ".zip"))
            {
                MemoryStream stream = new MemoryStream();
                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, tables);
                try
                {
                    zip.AddEntry("table.dat", stream.ToArray());
                }
                catch { }
                zip.Save();
                stream.Close();
            }
        }
        info.userData = tables;
        context.Response.Write(info.ToJson());
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}