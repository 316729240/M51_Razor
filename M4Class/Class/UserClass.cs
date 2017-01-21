using System;
using System.Collections.Generic;
using System.Text;
using Helper;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Text.RegularExpressions;

namespace MWMS
{

    /// <summary>
    ///UserClass 的摘要说明
    /// </summary>
    public class UserClass
    {
        public UserClass()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }
        public static ErrInfo addCash(double userId, int cash, string message, string Operator)
        {
            ErrInfo v = new ErrInfo();

            SqlParameter[] p = new SqlParameter[]{
            new SqlParameter("amount",cash),
            new SqlParameter("message",message),
            new SqlParameter("userid",userId),
            new SqlParameter("Operator",Operator)
        };
            if (cash > 0) Sql.ExecuteNonQuery("update m_admin set cash=cash+@cash where id=@userId",new SqlParameter[] {
                new SqlParameter("cash", cash),
                new SqlParameter("userId", userId)
            });
            if (cash < 0)
            {
                int c = Sql.ExecuteNonQuery("update m_admin set cash=cash+@cash where id=@userId and cash>@cash", new SqlParameter[] {
                new SqlParameter("cash", -cash),
                new SqlParameter("userId", userId)
                });
                if (c == 0)
                {

                    v.errNo = -1;
                    v.errMsg = "剩余网站币不足";
                    return (v);
                }
            }
            Sql.ExecuteNonQuery("insert into  cashChange (id,cash,message,userId,operator,createDate)values(@id,@cash,@message,@userId,@operator,getDate())", new SqlParameter[] {
                new SqlParameter("id", API.GetId()),
                new SqlParameter("cash", cash),
                new SqlParameter("message", message),
                new SqlParameter("userId", userId),
                new SqlParameter("operator", Operator)
                });
            v.errNo = 0;
            v.errMsg = "操作成功";
            return (v);
        }
        /// <summary>
        /// 密码找回
        /// </summary>
        /// <param name="email">用户名</param>
        /// <param name="email">邮箱</param>
        /// <returns></returns>
        public static ErrInfo passwordRecovery(string username,string email,string backUrl)
        {
            ErrInfo err = new ErrInfo();
            
            object id=Sql.ExecuteScalar("select id from m_admin where status=1 and uname=@username and email=@email", new SqlParameter[] {
                new SqlParameter("username",username),
                new SqlParameter("email",email)
            });
            if (id == null)
            {
                err.errNo = -1;
                err.errMsg = "用户名或邮箱不正确";
                return err;
            }
            string pId = API.GetId();
            string sId = pId.Encryption(Config.webId).MD5();
            backUrl = backUrl.Replace("$sId",sId);
            string html = "<p>亲爱的用户：wangxu 您好</strong></p>"+
            "<p> 您于 "+System.DateTime.Now.ToString("yyyy年MM月dd日 hh: mm")+" 提交了密码重置请求：<br>请在24小时内点击下面链接设置密码</p>"+
            "<p><a href = \""+ backUrl + "\" target=\"_blank\">"+ backUrl + "</a></p>"+
            "<p>(如果您无法点击此链接，请将它复制到浏览器地址栏后访问)</p>";
            err=API.sendMail(email, Config.systemVariables["webName"] + "-密码找回", html);
            if (err.errNo<0) return err;
            Sql.ExecuteScalar("insert into passwordRecovery (id,sId,userId,createDate)values(@id,@sId,@userId,getdate())", new SqlParameter[] {
                new SqlParameter("id",pId),
                new SqlParameter("sId",sId),
                new SqlParameter("userId",id)
            });
            
            return err;
        }
        public static ErrInfo editPassword(string sId, string password)
        {
            ErrInfo v = new ErrInfo();

            object id = Sql.ExecuteScalar("select userId from passwordRecovery where sId=@sId and createDate>dateadd(day,-1,getdate())", new SqlParameter[] {
                new SqlParameter("sId",sId)
            });
            if (id == null)
            {
                v.errNo = -1;
                v.errMsg = "无效id或已过期";
                return v;
            }
            SqlParameter[] p = new SqlParameter[]{
                new SqlParameter("pword",password.Encryption(Config.webId).MD5())
            };
            int c = Sql.ExecuteNonQuery("update  m_admin set pword=@pword where id=" + id.ToString(), p);
            if (c > 0)
            {
                v.errNo = 0; v.errMsg = "设置成功";
            }
            else
            {
                v.errNo = -1; v.errMsg = "设置失败";
            }
            Sql.ExecuteNonQuery("delete  from passwordRecovery where sId=@sId", new SqlParameter[] {
                new SqlParameter("sId",sId)
            });
            return (v);
        }
        public static ErrInfo login(double userId)
        {
            return login(userId,1);
        }
        public static ErrInfo login(double userId,int hour)
        {
            string ip = API.IPToNumber(HttpContext.Current.Request.UserHostAddress).ToString();
            ErrInfo err = new ErrInfo();
            UserInfo info = UserClass.get(userId);
            if (!info.ipAccess(HttpContext.Current.Request.UserHostAddress))
            {
                err.errNo = -1;
                err.errMsg = "您的ip不能登陆";
                return err;
            }
            err.errNo = 0;
            //添加登陆信息
            //添加登陆日志

            string sessionId = (HttpContext.Current.Request.Cookies["M5_SessionId"] == null) ? "" : HttpContext.Current.Request.Cookies["M5_SessionId"].Value;
            if (sessionId == "")
            {
                sessionId = API.GetId();
                HttpContext.Current.Response.Cookies["M5_SessionId"].Value = sessionId;
            }
            Sql.ExecuteNonQuery("update [m_admin] set loginDateTime=getdate() where id=@id", new SqlParameter[]{
                        new SqlParameter("id",userId),
                    });
            Helper.Sql.ExecuteNonQuery("delete from logininfo where logindate<DATEADD(hh, - 1, GETDATE()) or sessionId=@sessionId", new SqlParameter[]{
                        new SqlParameter("sessionId",sessionId),
                    });//删除超时用户及重登陆用户
            Helper.Sql.ExecuteNonQuery("insert logininfo (sessionId,ip,logindate,userid)values(@sessionId,@ip,GETDATE(),@userId)",
            new SqlParameter[]{
                        new SqlParameter("sessionId",sessionId),
                        new SqlParameter("ip",ip),
                        new SqlParameter("userId",userId)
                });
            err.userData = info;
            if (info.classId == 0) HttpContext.Current.Response.Cookies["M5_Login"].Value = "true";
            else { HttpContext.Current.Response.Cookies["M5_Login"].Value = ""; }
            HttpCookie cook = new HttpCookie("u_name");
            if (hour > 0) cook.Expires = System.DateTime.Now.AddHours(hour);
            cook.Value = HttpUtility.UrlEncode(info.username);
            HttpContext.Current.Response.Cookies.Add(cook);
            cook = new HttpCookie("u_id");
            if(hour>0)cook.Expires = System.DateTime.Now.AddHours(hour);
            cook.Value = info.id.ToString();
            HttpContext.Current.Response.Cookies.Add(cook);

            //HttpContext.Current.Response.Cookies["u_name"].Value = HttpUtility.UrlEncode(info.username);
            //HttpContext.Current.Response.Cookies["u_id"].Value = info.id.ToString();
            API.writeLog("login", info.username + "登陆成功");
            return err;
        }

        public static ErrInfo login(string uname, string pword)
        {
            return login(uname, pword, 1);
        }
        public static ErrInfo login(string uname, string pword,int hour)
        {
            string ip = API.IPToNumber(HttpContext.Current.Request.UserHostAddress).ToString();
            ErrInfo err = new ErrInfo();
            string pword2 = "";
            double userId = 0;
            SqlDataReader rs = Sql.ExecuteReader("select id,pword from [m_admin] where status=1 and uname=@uname",new SqlParameter[]{new SqlParameter("uname",uname)});
            if (rs.Read())
            {
                pword2 = rs[1].ToString();
                userId = rs.GetDouble(0);
            }
            rs.Close();
            #region 用户不存在
            if (userId == 0)
            {
                API.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                return err;
            }
            #endregion
            #region 出错次数过多ip封锁
            Sql.ExecuteNonQuery("delete from InvalidLogin where createdate<DATEADD(hh,-3,GETDATE()) ");//清空3天前的登陆错误日志
            rs = Sql.ExecuteReader("select ip from InvalidLogin where createdate>DATEADD(hh,-2,GETDATE()) and uname=@uname and ip=@ip and count>10",new SqlParameter []{
                new SqlParameter("uname",uname),
                new SqlParameter("ip",ip)
            });
            if (rs.Read()) { err.errNo = -1;  err.errMsg = "由于您登录后台错误次数过多，系统自动屏蔽您的登录！"; }
            rs.Close();
            if (err.errNo < 0) return err;
            #endregion

            if (pword.Encryption(Config.webId).MD5() == pword2)
            {
                return login(userId,hour);
            }
            else
            {
                #region 密码错误
                SqlParameter[] p = new SqlParameter[] { 
                            new SqlParameter ( "ip", ip ), 
                            new SqlParameter ( "uname", uname ) };
                rs = Sql.ExecuteReader("select ip from InvalidLogin where uname=@uname and ip=@ip", p);
                if (rs.Read())
                {
                    Helper.Sql.ExecuteNonQuery("update InvalidLogin set count=count+1 where uname=@uname and ip=@ip", p);
                }
                else
                {
                    Helper.Sql.ExecuteNonQuery("insert into InvalidLogin (ip,uname,count,createdate)values(@ip,@uname,1,getdate())",p);
                }
                rs.Close();
                API.writeLog("login", uname+"用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                #endregion
            }
            return err;
        }

        public static ErrInfo manageLogin(string uname, string pword, int hour)
        {
            string ip = API.IPToNumber(HttpContext.Current.Request.UserHostAddress).ToString();
            ErrInfo err = new ErrInfo();
            string pword2 = "";
            double userId = 0;
            SqlDataReader rs = Sql.ExecuteReader("select id,pword from [m_admin] where status=1 and classId=0 and uname=@uname", new SqlParameter[] { new SqlParameter("uname", uname) });
            if (rs.Read())
            {
                pword2 = rs[1].ToString();
                userId = rs.GetDouble(0);
            }
            rs.Close();
            #region 用户不存在
            if (userId == 0)
            {
                API.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                return err;
            }
            #endregion
            #region 出错次数过多ip封锁
            Sql.ExecuteNonQuery("delete from InvalidLogin where createdate<DATEADD(hh,-3,GETDATE()) ");//清空3天前的登陆错误日志
            rs = Sql.ExecuteReader("select ip from InvalidLogin where createdate>DATEADD(hh,-2,GETDATE()) and uname=@uname and ip=@ip and count>10", new SqlParameter[]{
                new SqlParameter("uname",uname),
                new SqlParameter("ip",ip)
            });
            if (rs.Read()) { err.errNo = -1; err.errMsg = "由于您登录后台错误次数过多，系统自动屏蔽您的登录！"; }
            rs.Close();
            if (err.errNo < 0) return err;
            #endregion

            if (pword.Encryption(Config.webId).MD5() == pword2)
            {
                return login(userId, hour);
            }
            else
            {
                #region 密码错误
                SqlParameter[] p = new SqlParameter[] {
                            new SqlParameter ( "ip", ip ),
                            new SqlParameter ( "uname", uname ) };
                rs = Sql.ExecuteReader("select ip from InvalidLogin where uname=@uname and ip=@ip", p);
                if (rs.Read())
                {
                    Helper.Sql.ExecuteNonQuery("update InvalidLogin set count=count+1 where uname=@uname and ip=@ip", p);
                }
                else
                {
                    Helper.Sql.ExecuteNonQuery("insert into InvalidLogin (ip,uname,count,createdate)values(@ip,@uname,1,getdate())", p);
                }
                rs.Close();
                API.writeLog("login", uname + "用户名密码错误");
                err.errNo = -1;
                err.errMsg = "用户名密码错误";
                #endregion
            }
            return err;
        }
        public static UserInfo get(double id)
        {
            UserInfo value = new UserInfo();
            System.Data.SqlClient.SqlDataReader rs = Sql.ExecuteReader("select id,uname,status,createdate,updatedate,email,mobile,phone,icon,filteringIP,sex,classId,cash from [m_admin] where  id=@id", new SqlParameter[] { new SqlParameter("id", id) });
            if (rs.Read())
            {
                value.id = rs.GetDouble(0);
                value.username = rs[1].ToString();
                value.classId = rs.GetDouble(11);
                value.status = rs.GetInt32(2);
                value.createdate = rs.GetDateTime(3);
                value.updatedate = rs.GetDateTime(4);
                value.email = rs[5]+"";
                value.mobile = rs[6] + "";
                value.phone = rs[7] + "";
                value.icon = rs[8]+"";
                value.filteringIP = rs[9] + "";
                value.sex =rs.IsDBNull(10)?true:rs.GetBoolean(10);
                value.cash = rs.IsDBNull(12) ? 0 : rs.GetInt32(12);

            }
            rs.Close();
            if (value.id < 1) return null;
            rs = Sql.ExecuteReader("select roleId from [admin_role] where  userId=@id", new SqlParameter[] { new SqlParameter("id", id) });
            string roleId = "";
            while (rs.Read())
            {
                if(roleId!="")roleId += ",";
                roleId += rs.GetDouble(0);
                value.role = roleId;
            }
            rs.Close();
            return value;
        }
        public static ErrInfo edit(UserInfo value, UserInfo loginInfo)
        {
            ErrInfo v = null;
            v = edit(value);
            return (v);
        }
        public static ErrInfo setIcon(string icon, UserInfo loginInfo)
        {

            ErrInfo v = new ErrInfo();
            Sql.ExecuteNonQuery("update  m_admin set icon=@icon where id=@id", new SqlParameter[]{
            new SqlParameter("icon",icon),
            new SqlParameter("id",loginInfo.id)
            });
            return v;
        }
        public static ErrInfo edit(UserInfo value)
        {
            ErrInfo v = new ErrInfo();
            SqlParameter[] p = new SqlParameter[]{
            new SqlParameter("id",value.id),
            new SqlParameter("updateDate",System.DateTime.Now),
            new SqlParameter("email",value.email),
            new SqlParameter("phone",value.phone),
            new SqlParameter("mobile",value.mobile),
            new SqlParameter("filteringIP",value.filteringIP),
            new SqlParameter("sex",value.sex),
            new SqlParameter("name",value.name)
        };
            Sql.ExecuteNonQuery("update  m_admin set name=@name,sex=@sex,updateDate=@updateDate,email=@email,phone=@phone,mobile=@mobile,filteringIP=@filteringIP where id=@id", p);
            Sql.ExecuteNonQuery("delete from admin_role where userId=@id", new SqlParameter[]{
                        new SqlParameter("id",value.id)
                    });
            for (int i = 0; i < value.roleList.Count; i++)
            {
                Sql.ExecuteNonQuery("insert into  [admin_role] (id,userId,roleId)values(@id,@userId,@roleId) ", new SqlParameter[]{
                        new SqlParameter("id",API.GetId()),
                        new SqlParameter("userId",value.id),
                        new SqlParameter("roleId",value.roleList[i])
                    });

            }
            API.writeLog("user", "修改帐号 username:" + value.username);
            v.userData = value.id;
            v.errNo = 0;
            v.errMsg = "修改成功";
            return (v);
        }
        public static ErrInfo add(UserInfo value, UserInfo loginInfo)
        {
            if(value.classId==0 && loginInfo!=null && !loginInfo.isAdministrator) {
                //只有管理员才能建立后台帐号
                ErrInfo err = new ErrInfo();
                err.errMsg = "无权创建管理帐号";
                err.errNo = -1;
            }
            return add(value);
        }
        static ErrInfo add(UserInfo value)
        {
            ErrInfo v = new ErrInfo();
            if ((int)(Sql.ExecuteScalar("select count(1) from m_admin where uname=@uname",new SqlParameter[]{
                    new SqlParameter("uname",value.username)
            }))>0)
            {
                v.errNo = -1;
                v.errMsg = "用户名被占用请换一个用户名重试";
                return (v);
            }
            if (value.mobile != "" && (int)(Sql.ExecuteScalar("select count(1) from m_admin where mobile=@mobile", new SqlParameter[]{
                    new SqlParameter("mobile",value.mobile)
            })) > 0)
            {
                v.errNo = -1;
                v.errMsg = "手机已被注册";
                return (v);
            }
            if (value.email!="" && (int)(Sql.ExecuteScalar("select count(1) from m_admin where email=@email", new SqlParameter[]{
                    new SqlParameter("email",value.email)
            })) > 0)
            {
                v.errNo = -1;
                v.errMsg = "邮箱已被注册";
                return (v);
            }
            //int subcount = (int)(Sql.ExecuteScalar("select count(1) from cms_admin where userid=" + loginInfo.id.ToString()));

            try
            {
                value.id=double.Parse(API.GetId());
                SqlParameter[] p = new SqlParameter[]{
                    new SqlParameter("id",value.id),
                    new SqlParameter("sId",value.id.ToString().Encryption(Config.webId).MD5()),
                    new SqlParameter("uname",value.username),
                    new SqlParameter("pword",value.password.Encryption(Config.webId).MD5()),
                    new SqlParameter("createDate",System.DateTime.Now),
                    new SqlParameter("updateDate",System.DateTime.Now),
                    new SqlParameter("loginDateTime",System.DateTime.Now),
                    new SqlParameter("status",value.status),
                    new SqlParameter("integral",value.integral),
            new SqlParameter("email",value.email),
            new SqlParameter("phone",value.phone),
            new SqlParameter("mobile",value.mobile),
            new SqlParameter("classId",value.classId),
            new SqlParameter("filteringIP",value.filteringIP),
            new SqlParameter("sex",value.sex)
                };
                Sql.ExecuteNonQuery("insert into  [m_admin] (id,uname,pword,createDate,updateDate,loginDateTime,status,integral,email,phone,mobile,filteringIP,classId,sex,sId,cash)values(@id,@uname,@pword,@createDate,@updateDate,@loginDateTime,@status,@integral,@email,@phone,@mobile,@filteringIP,@classId,@sex,@sId,0) ", p);
                for (int i = 0; i < value.roleList.Count; i++)
                {
                    Sql.ExecuteNonQuery("insert into  [admin_role] (id,userId,roleId)values(@id,@userId,@roleId) ", new SqlParameter[]{
                        new SqlParameter("id",API.GetId()),
                        new SqlParameter("userId",value.id),
                        new SqlParameter("roleId",value.roleList[i])
                    });
                
                }

                if (value.id > 0)
                {
                    API.writeLog("user", "新增帐号 username:" + value.username);
                    v.errNo = 0;
                    v.errMsg = "添加成功";
                    v.userData = value.id;
                    return (v);
                }
                else
                {
                    v.errNo = -1;
                    v.errMsg = "添加失败";
                    return (v);
                }
            }
            catch (Exception ex)
            {

                v.errNo = -1;
                v.errMsg = ex.Message;
                return (v);
            }
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public static ErrInfo del(string ids)
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
            ErrInfo v = new ErrInfo();
            Sql.ExecuteNonQuery("delete  m_admin where id in (" + ids + ")");
            Sql.ExecuteNonQuery("delete  admin_role where userId  in (" + ids + ")");
            v.errNo = 0;
            return v;
        }
        /// <summary>
        /// 设置用户状态
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ErrInfo setState(string ids, bool status)
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
            ErrInfo v = new ErrInfo();
            Sql.ExecuteNonQuery("update m_admin set status=" + (status ? 1 : 0).ToString() + " where id in (" + ids + ")");
            v.errNo = 0;
            return v;
        }
        public static ErrInfo editPassword(double id, string oldPassword, string password, UserInfo loginInfo)
        {
            ErrInfo v = new ErrInfo();
            int c = (int)(Sql.ExecuteScalar("select count(1) from  m_admin where id=" + id + " and pword='" + oldPassword.Encryption(Config.webId).MD5() + "'"));
            if (c > 0)
            {
                v = editPassword(id, password, loginInfo);
            }
            else
            {
                v.errNo = -1; v.errMsg = "原密码不正确";
            }
            return (v);
        }
        public static ErrInfo editPassword(double id, string password, UserInfo loginInfo)
        {
            ErrInfo v = new ErrInfo();

            bool qx = false;
            if (loginInfo.id == id || loginInfo.isAdministrator)
            {
                qx = true;
            }
            if (!qx)
            {
                v.errNo = -1;
                v.errMsg = "无权修改";
                return (v);
            }
            SqlParameter[] p = new SqlParameter[]{
                new SqlParameter("pword",password.Encryption(Config.webId).MD5())
            };
            int c = Sql.ExecuteNonQuery("update  m_admin set pword=@pword where id=" + id.ToString(), p);
            if (c > 0)
            {
                v.errNo = 0; v.errMsg = "设置成功";
            }
            else
            {
                v.errNo = -1; v.errMsg = "设置失败";
            }
            return (v);
        }
    }
    public class UserInfo
    {
        public double id = -1;
        public string username = "";
        public string password = "";
        public double classId = 0;
        public DateTime lastLoginTime;
        public string lastLoginIp = "";
        public string email = "";//邮箱
        public string phone = "";//电话
        public string mobile = "";//手机
        public int integral = 0;
        public DateTime createdate = System.DateTime.Now;//开户时间
        public DateTime updatedate = System.DateTime.Now;
        public string question = "";//密码问题
        public string answer = "";//密码答案
        public string icon = "img/default_icon.png";//头像
        public string name = "";//名称
        public bool sex = false;//性别
        public int status = 1;
        public bool isAccess = false;//浏览权限
        public bool isAdministrator=false;//管理权限
        public int cash = 0;
        string[] _ipList = null;
        string _filteringIP = "";
        public string filteringIP//ip访问过滤
        {
            get { return _filteringIP; }
            set { _filteringIP = value; if (_filteringIP != "") { _ipList = _filteringIP.Split('\n'); }else{_ipList=null;} }
        }
        /// <summary>
        /// 是否允许访问
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool ipAccess(string ip)
        {
            if (_ipList != null)
            {
                for (int i = 0; i < _ipList.Length; i++)
                    {
                        if (_ipList[i] != "")
                        {
                            string reg = _ipList[i].Replace("*", @"\d{1,3}");
                            if (Regex.IsMatch(ip, reg)) return true;
                        }
                    }
                    return false;

            }
            return true;
        }

        string _role = "";
        public List<double> roleList = new List<double>();
        public string role
        {
            get {
                return _role; 
            }
            set { 
                _role = value;
                string [] list =_role.Split(',');
                roleList.Clear();
                for (int i = 0; i < list.Length; i++)
                {

                    if (list[i] != "")
                    {
                        
                        double roleId = double.Parse(list[i]);
                        if (roleId > 0 && roleId < 6) isAccess = true;
                        roleList.Add(roleId);
                    }
                }
                isAdministrator = roleList.IndexOf(1)>-1;
            }
        }
        public Permissions getModulePermissions(double moduleId,double classId)
        {
            double dataTypeId = -1;
            Permissions p = null;
            if (classId < 8)
            {
                SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from module where id=@moduleId", new SqlParameter[] { new SqlParameter("moduleId", moduleId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
                p = getModulePermissions(moduleId);

            }
            else
            {
                SqlDataReader rs = Sql.ExecuteReader("select  savedatatype from class where id=@classId", new SqlParameter[] { new SqlParameter("classId", classId) });
                if (rs.Read()) dataTypeId = rs.GetDouble(0);
                rs.Close();
                p = getColumnPermissions(classId);
            }
            return p;
        }
        
/// <summary>
/// 获取模块权限
/// </summary>
/// <param name="moduleId">模块Id</param>
/// <returns></returns>
public Permissions getModulePermissions(double moduleId)
        {
            Permissions value = new Permissions(this);
            string role = this.id.ToString();
            if (this.role != "") role += "," + this.role;
            SqlDataReader rs = Sql.ExecuteReader("select count(1),sum(p0),sum(p1),sum(p2),sum(p3) from permissions where classid=" + moduleId.ToString() + " and dataId in (" + role + ") ");
            if (rs.Read())
            {
                if (rs.GetInt32(0) > 0)
                {
                    value.read = true;
                    value.write = value.write | (rs.GetInt32(1) > 0);
                    value.delete = value.delete | (rs.GetInt32(2) > 0);
                    value.audit = value.audit | (rs.GetInt32(3) > 0);
                    value.all = value.all | (rs.GetInt32(4) > 0);
                }
            }
            if (value.all) value.read = value.write = value.audit = true;
            return value;
        }
        /// <summary>
        /// 获取栏目权限
        /// </summary>
        /// <param name="moduleId">模块id</param>
        /// <param name="columnId">栏目id</param>
        /// <returns></returns>
        public Permissions getColumnPermissions(double columnId)
        {
            ColumnInfo column = ColumnClass.get(columnId);
            return getColumnPermissions(column);
        }
        public Permissions getColumnPermissions(ColumnInfo column)
        {
            Permissions value = new Permissions(this);
            string role = this.id.ToString();
            if (this.role != "") role += "," + this.role;
            string parentId = column.parentId;
            if (parentId != "") parentId += ",";
            parentId +=column.moduleId.ToString();
            int count = 0;
            SqlDataReader rs = Sql.ExecuteReader("select count(1),sum(p0),sum(p1),sum(p2),sum(p3) from permissions where classid in (" + parentId + ") and dataId in (" + role + ") ");
            if (rs.Read())
            {
                if (rs.GetInt32(0) > 0)
                {
                    value.read = true;
                    value.write = value.write | (rs.GetInt32(1) > 0);
                    value.delete = value.delete | (rs.GetInt32(2) > 0);
                    value.audit = value.audit | (rs.GetInt32(3) > 0);
                    value.all = value.all | (rs.GetInt32(4) > 0);
                }
            }
            rs.Close();
            if (value.all) value.read = value.write = value.audit = true;
            return value;
        }
        public ErrInfo editPassword(string oldPassword, string password)
        {
            return UserClass.editPassword(id, oldPassword, password, this);
        }
    }
}