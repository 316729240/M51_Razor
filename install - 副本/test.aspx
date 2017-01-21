<%@  Language="C#"  %><%@ Import Namespace="System.Net" %>
<%
    string rHost = "http://localhost:9018";
string m=Request.QueryString["m"]+"";
if(m=="getOs"){
    Response.Write(getOS(Environment.OSVersion.ToString()));
}else if(m=="getVersion"){
    Response.Write(Environment.Version.ToString());
}else if(m=="getIIS"){
    Response.Write(HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"].ToString());
}else if(m=="testDir"){
    try{
     System.IO.Directory.CreateDirectory(Server.MapPath(@"~\mwms4_test\"));
     System.IO.Directory.Delete(Server.MapPath(@"~\mwms4_test\"));
        Response.Write("1");
    }catch{
        Response.Write("0");
    }            
}else if(m=="testFile"){
    try{
        System.IO.File.AppendAllText(Server.MapPath(@"~\mwms4_test.aspx"), "test");
        System.IO.File.Delete(Server.MapPath(@"~\mwms4_test.aspx"));
        Response.Write("1");
    }catch{
        Response.Write("0");
    }
}else if(m=="testWebRead"){
    try{
        string html=readUrl("http://upgrade5.mwms4.com/sqlscript.txt");
        Response.Write("1");
    }catch{
        Response.Write("0");
    }
}
else if (m == "downFile")
{
    int pagecount = int.Parse(Request.QueryString["pagecount"].ToString());
    int pageno = int.Parse(Request.QueryString["pageno"].ToString());
    using (WebClient wc = new WebClient())
    {
        byte[] data = wc.DownloadData("http://upgrade5.mwms4.com/package/down.aspx?pageno=" + pageno.ToString());
        System.IO.FileStream r = new System.IO.FileStream(Server.MapPath("~/m5.zip.tmp"), System.IO.FileMode.Append, System.IO.FileAccess.Write);
        r.Write(data, 0, data.Length);
        r.Close();
    }
    if (pagecount == pageno) System.IO.File.Move(Server.MapPath("~/m5.zip.tmp"), Server.MapPath("~/m5.zip"));
}
else if (m == "readFileSize")
{
    if (System.IO.File.Exists(Server.MapPath(@"~\m5.zip")))
    {
        Response.Write("0");
        Response.End();
    }
    if (System.IO.File.Exists(Server.MapPath(@"~\m5.zip.tmp"))) System.IO.File.Delete(Server.MapPath(@"~\m5.zip.tmp"));
        string html = readUrl("http://upgrade5.mwms4.com/package/down.aspx");
        Response.Write(html);
}
else if (m == "downDll")
{
    using (WebClient wc = new WebClient())
    {
        byte[] data = wc.DownloadData("http://upgrade5.mwms4.com/package/Ionic.Zip.zip");
        System.IO.Directory.CreateDirectory(Server.MapPath("~/bin/"));
        System.IO.FileStream r = new System.IO.FileStream(Server.MapPath("~/bin/Ionic.Zip.dll"), System.IO.FileMode.Create, System.IO.FileAccess.Write);
        r.Write(data, 0, data.Length);
        r.Close();
        data = wc.DownloadData("http://upgrade5.mwms4.com/package/unpack.zip");
        r = new System.IO.FileStream(Server.MapPath("~/unpack.aspx"), System.IO.FileMode.Create, System.IO.FileAccess.Write);
        r.Write(data, 0, data.Length);
        r.Close();
    }
}
    
if(m!="")Response.End();
 %>
 <script runat="server">
    string readUrl(string url)
    {
        WebClient MyWebClient = new WebClient();MyWebClient.Credentials = CredentialCache.DefaultCredentials;
        Byte[] pageData = MyWebClient.DownloadData(url);return (Encoding.Default.GetString(pageData));
    }
    public static string getOS(string Agent)
    {
        Agent = Agent.ToLower();string os = "";
        if (Agent.IndexOf("win") > -1){
            if (Agent.IndexOf("nt 5.1") > -1){os = "Windows XP";}
            else if (Agent.IndexOf("nt 5.2") > -1) { os = "Windows 2003"; }
            else if (Agent.IndexOf("nt 5.0") > -1){os = "Windows 2000";}
            else if (Agent.IndexOf("nt 6.0") > -1){os = "Windows Vista";}
            else if (Agent.IndexOf("nt 6.1") > -1){os = "Windows 7";}
            else if (Agent.IndexOf("nt") > -1){os = "Windows NT";}
        }return (os);
    }
</script>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
<title>无标题页</title><script src="<% =rHost %>/static/js/jquery.min.js" type="text/javascript"></script> 
<script type="text/javascript" src="<% =rHost %>/static/js/ZeroClipboard.js"></script>
<script src="<% =rHost %>/static/js/resize.js"></script>
<script src="<% =rHost %>/static/js/baseControl.js"></script>
<script src="<% =rHost %>/static/js/config.js"></script>
<script src="<% =rHost %>/static/js/Frame.js"></script>
<script src="<% =rHost %>/static/js/dialog.js"></script>
<script src="<% =rHost %>/static/js/extend.js"></script>
<script src="<% =rHost %>/static/js/jquery.validate.min.js"></script>
<link type="text/css" href="<% =rHost %>/static/skin/css/metrostyle.css" rel="stylesheet" />
<link type="text/css" href="<% =rHost %>/static/skin/css/tree.css" rel="stylesheet" />
<link type="text/css" href="<% =rHost %>/static/skin/css/font-awesome.min.css" rel="stylesheet" />
<link rel="stylesheet" href="<% =rHost %>/static/skin/css/ace.css" />
<link rel="stylesheet" href="<% =rHost %>/static/skin/css/jquery.minicolors.css" />
<link rel="stylesheet" href="<% =rHost %>/static/skin/css/bootstrapSwitch.css" />

<!-- Styles -->
<link href="<% =rHost %>/static/skin/css/delighted.css" rel="stylesheet">

</head>

<style> 
    *{font-family: 微软雅黑;font-family:@微软雅黑}
body111{
   -moz-user-select: none; /*火狐*/
   -webkit-user-select: none;  /*webkit浏览器*/
   -ms-user-select: none;   /*IE10*/
   -khtml-user-select: none; /*早期浏览器*/
   user-select: none;
}
.M5_Window .alert {
	line-height: 23px;
	border: none;
    margin-bottom:0px;
}
.M5_Window .alert-icon {
	font-size: 50px;
	float: left;
	margin: -5px 15px 15px 0;
    line-height:0.5;
}
.modal-footer{margin:0px;}
</style>
<style>
.disableSelect {-moz-user-select: none; -khtml-user-select: none; user-select: none;}
html,body{ margin:0; height:100%;overflow:hidden; }
.M5_Frame{}
.M5_Frame .box{float:left;}
.M5_Frame .line{float:left;}
.M5_Window{position: absolute;}
.M5_Del {text-decoration: line-through;color:#CCCCCC;}

.M5_GridView .headDiv{overflow:hidden;}
.M5_GridView .scrollBody{overflow:auto;}
.M5_GridView th,td{word-break:break-all;}

.M5_Editor {
    position: relative;
    border: 1px solid #a9a9a9;
}

.note-toolbar {
    background-color: #f5f5f5;
    border-bottom: 1px solid #a9a9a9;
    padding: 0 0 5px 5px;
    margin: 0;
}

.note-toolbar>.btn-group {
    margin-top: 5px;
    margin-right: 5px;
    margin-left: 0;
}
.M5_Editor .note-editable {overflow:auto;}

.sidebar-module {
    float: left;
    min-height: 0\9;
    height: auto;
    width: 100%;
    padding: 10px;
    padding: 10px 10px 11px\9;
}

.sidebar-module .btn.pull-right {
    float: right;
    margin: 0 0 0 10px;
}

.sidebar-module h3 {
    margin: -3px 0 0 0;
    font-size: 13px;
    font-weight: 600;
    line-height: 20px;
}

.sidebar-module .sidebar-profile-list {
    float: left;
    width: auto;
    margin: -2px 0 0 0;
    padding: 0;
    list-style: none;
}

.sidebar-module .sidebar-profile-list a {
    font-size: 12px;
}

.sidebar-module .sidebar-profile-list h3 {
    margin: -3px 0 0 0;
    font-size: 13px;
    font-weight: 600;
    line-height: 20px;
}
.sidebar-module .avatar {
    float: left;
    z-index: 10px;
    height: 40px;
    width: 40px;
    margin: -5px 15px 0 0;
}




.formSep {margin-bottom:12px;padding-bottom:12px;border-bottom:1px dashed #dcdcdc}
 
	/* input spinner */
		
		.ui-spinner .ui-spinner-up:hover {background-position:-18px 0}
		.ui-spinner .ui-spinner-down:hover {background-position:-18px -14px}
		.ui-spinner input,.ui-spinner input:focus {display:block !important;margin:0 !important;padding:0;min-height:28px !important;height:28px !important;-webkit-box-shadow:none;-moz-box-shadow:none;-ms-box-shadow:none;box-shadow:none}

.ui-spinner-input {
	border: none;
	background: none;
	color: inherit;
	padding: 0;
	margin: .2em 0;
	vertical-align: middle;
	margin-left: .4em;
	margin-right: 22px;
}
.ui-spinner-button {
	width: 16px;
	height: 50%;
	font-size: .5em;
	padding: 0;
	margin: 0;
	text-align: center;
	position: absolute;
	cursor: default;
	display: block;
	overflow: hidden;
	right: 0;
}
/* vertical centre icon */
.ui-spinner .ui-icon {
	position: absolute;
	margin-top: -8px;
	top: 50%;
	left: 0;
}
.ui-spinner-up {
	top: 0;
}
.ui-spinner-down {
	bottom: 0;
}
.ui-spinner .ui-spinner-input {border:none}

.ui-spinner {
	position: relative;
	display: inline-block;
	overflow: hidden;
	padding: 0;
	vertical-align: middle;
}
.ui-spinner{-webkit-box-shadow:inset 0 1px 1px rgba(0,0,0,0.075);-moz-box-shadow:inset 0 1px 1px rgba(0,0,0,0.075);
            box-shadow:inset 0 1px 1px rgba(0,0,0,0.075);position:relative;
            margin-bottom:10px;font-size:13px;height:28px;line-height:28px;color:#555;
            background-color:#fff;border:1px solid #ccc;-webkit-border-radius:3px;-moz-border-radius:3px;
            border-radius:3px;overflow:hidden;padding:0 24px 0 8px}
</style>
<style type="text/css">
        .dropdown-submenu {
            position: relative;
        }
        .dropdown-submenu > .dropdown-menu {
            top: 0;
            left: 100%;
            margin-top: -6px;
            margin-left: -1px;
        }
        .dropdown-submenu:hover > .dropdown-menu {
            display: block;
        }
        .dropdown-submenu > a:after {
            display: block;
            content: " ";
            float: right;
            width: 0;
            height: 0;
            border-color: transparent;
            border-style: solid;
            border-width: 5px 0 5px 5px;
            border-left-color: #ccc;
            margin-top: 5px;
            margin-right: -10px;
        }
        .dropdown-submenu:hover > a:after {
            border-left-color: #fff;
        }
        .dropdown-submenu.pull-left {
            float: none;
        }
        .dropdown-submenu.pull-left > .dropdown-menu {
            left: -100%;
            margin-left: 10px;
            -webkit-border-radius: 6px 0 6px 6px;
            -moz-border-radius: 6px 0 6px 6px;
            border-radius: 6px 0 6px 6px;
        }
        .dropdown-menu .ico{width:20px;}
    </style>
<style>
    .nav-tabs  .close{display:none;}
    .nav-tabs .active .close{display:block;margin-left:10px;font-size:14px;}
 /*
 .btn-toolbar .btn{
border-color: #FFFFFF;}

 .btn-toolbar .btn:hover{
border-color: #ccc;}
 */
    </style>
<style>
.M5_droppicker {
	*zoom:1;
	padding:4px;
	min-width:350px;
	margin-top:1px;
	-webkit-border-radius:4px;
	-moz-border-radius:4px;
	border-radius:4px;
}
.M5_droppicker:before,.droppicker:after {
	display:table;
	content:"";
}
.M5_droppicker:after {
	clear:both;
}
.M5_droppicker:before {
	content:'';
	display:inline-block;
	border-left:7px solid transparent;
	border-right:7px solid transparent;
	border-bottom:7px solid #ccc;
	border-bottom-color:rgba(0,0,0,0.2);
	position:absolute;
	top:-7px;
	left:6px;
}
.M5_droppicker:after {
	content:'';
	display:inline-block;
	border-left:6px solid transparent;
	border-right:6px solid transparent;
	border-bottom:6px solid #ffffff;
	position:absolute;
	top:-6px;
	left:7px;
}
.M5_Color {
        width:163px;
        margin: 0 5px;
        }
.M5_Color .color-btn {
	width: 20px;
	height: 20px;
	padding: 0;
	margin: 0;
	border: 1px solid #fff
}
.M5_Color .color-btn:hover{
	border: 1px solid #000
}
.M5_Color .note-palette-title {
	margin:2px 7px;
	font-size:12px;
	text-align:center;
	border-bottom:1px solid #eee
}
.M5_Color .note-color-reset {
	padding:0 3px;
	margin:3px;
	font-size:11px;
	cursor:pointer;
	-webkit-border-radius:5px;
	-moz-border-radius:5px;
	border-radius:5px
}
.M5_Color .note-color-reset:hover {
	background:#eee
}

.GaussianBlur{-webkit-filter: blur(3px);
-moz-filter: blur(3px);
-o-filter: blur(3px);
-ms-filter: blur(3px);
filter: blur(3px);
}
    </style>
<style>
div.sticky-queue{position:fixed;background:#fff;background:rgba(255,255,255,.9);border-width:0 3px 3px;border-style:solid;border-color:#ccc;border-color:rgba(0,0,0,.6);width:280px;z-index:989}
div.sticky-queue.bottom-right,div.sticky-queue.bottom-left {border-width:3px 3px 0;border-style:solid;border-color:#ccc;border-color:rgba(0,0,0,.6)}
div.sticky-note{padding-right:20px;padding-left:14px;font-weight:700}
div.sticky{font-size:12px;color:#555;position:relative;padding:10px;overflow:hidden;}
div.sticky p {margin-bottom:0}
.st-close{position:absolute;top:4px;right:6px}
.top-right,.top-left,.top-center{border-bottom-right-radius:6px;border-bottom-left-radius:6px;-moz-border-radius-bottomright:6px;-moz-border-radius-bottomleft:6px;-webkit-border-bottom-right-radius:6px;-webkit-border-bottom-left-radius:6px}
.bottom-right,.bottom-left{bottom:-2px;border-top-right-radius:6px;border-top-left-radius:6px;-moz-border-radius-topright:6px;-moz-border-radius-topleft:6px;-webkit-border-top-right-radius:6px;-webkit-border-top-left-radius:6px}
.border-top-right,.border-top-left,.border-top-center{border-top:1px solid #eee;border-top:1px solid rgba(0,0,0,.1)}
.border-bottom-right,.border-bottom-left{border-bottom:1px solid #eee;border-bottom:1px solid rgba(0,0,0,.1)}
.sticky.st-warning{color:#C62626}
.sticky.st-success{color:#7fae00}
.sticky.st-info{color:#00a6fc}
.top-right,.bottom-right{right:20px}
.top-left,.bottom-left{left:20px}
.top-center{left:50%;margin-left:-140px}
div.sticky-queue.top-right .sticky:last-child,div.sticky-queue.top-left .sticky:last-child,div.sticky-queue.top-center .sticky:last-child{border-bottom-right-radius:3px;border-bottom-left-radius:3px;-moz-border-radius-bottomright:3px;-moz-border-radius-bottomleft:3px;-webkit-border-bottom-right-radius:3px;-webkit-border-bottom-left-radius:3px}
div.sticky-queue.bottom-right .sticky:first-child,div.sticky-queue.bottom-left .sticky:first-child {border-top-right-radius:3px;border-top-left-radius:3px;-moz-border-radius-topright:3px;-moz-border-radius-topleft:3px;-webkit-border-top-right-radius:3px;-webkit-border-top-left-radius:3px}
</style>

<style>
.button-back {float: left}
.button-next, .finish {float: right}
.step {background:#f9f9f9;border: 1px solid #ccc; clear: left; padding:10px 20px 14px;-webkit-border-radius: 4px;-moz-border-radius: 4px;-ms-border-radius: 4px;border-radius: 4px}
.step legend { color: #4080BF; font: bold 14px verdana; padding: 0 2px 3px 2px;background:#f9f9f9;-webkit-border-radius: 4px;-moz-border-radius: 4px;-ms-border-radius: 4px;border-radius: 4px}
.stepy-titles {list-style: none; margin: 0; padding: 0; width: 100%}
.stepy-titles li {cursor: pointer;background:#fff;color:#818181;font-weight:700;font-size:18px; display:inline-block; padding: 0 0 14px 45px;margin-right:50px;position:relative;line-height:1.3 !important}
.stepy-titles li:before {background: url('../img/nav_dot.gif') repeat-x 0 0;height:6px;position:absolute;top:50%;left:-43px;width:36px;margin-top:-6px;content: "";display:block}
.stepy-titles li:last-child {margin:0}
.stepy-titles li:first-child:before {display:none}
.stepy-titles li span {font-size:11px;display: block}
.stepy-titles .stepNb {position:absolute;display:block;background: #efefef;color:#818181;-webkit-border-radius: 17px;-moz-border-radius: 17px;-ms-border-radius: 17px;border-radius: 17px;width:34px;left:0;top:3px;line-height:34px;font-size:16px;text-align:center}
.stepy-titles .current-step {color: #067ead; cursor: auto}
.stepy-titles .current-step .stepNb {background:#067ead;color:#fff}
.step .control-group + P {margin:0;line-height: inherit;padding:20px 0 0;overflow:hidden}

.error-image .stepNb {background:#C62626 !important}
.error-image {color:#C62626 !important}
.error-image .stepNb {color:#fff !important}
</style>
</head>

<body>

</body>
<script>
    var win = $(document.body).addControl({
        xtype: "Window",
        style: { width: "800px" },
        isModal: true,
        onClose: function (sender, e) {
        }
    });
    var stepInfo=[{a:"安装检查",b:"Inspection installation"},{a:"数据安装",b:"Data to install"},{a:"信息配制",b:"Information configuration"}];
    var html="<ul  class='stepy-titles clearfix'>";
    for(var i=0;i<3;i++){
        html+="<li>";
        html+="<div>"+stepInfo[i].a+"</div>";
        html+="<span>"+stepInfo[i].b+"</span><span class='stepNb'>"+(i+1)+"</span>";
        html+="</li>";
    }
    html += "</div>";
    var stepMenu=win.append(html).find("li");
    $(stepMenu[0]).attr("class", "current-step");
    var box = win.append("<div class='form-horizontal'></div>");
    var grid = box.addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0,
        style: { height: "360px" },
        columns: [{ name: "title", width: 410 }, { name: "msg", width: 300 }, { name: "status", width: 40}]
    });

    win.append("<div style='clear:both'></div>");
    var footer = win.append("<div class=\"modal-footer\"></div>");
    var nextButton = footer.addControl({ xtype: "Button", text: "下一步", color: 2, enabled: false, onClick: function () {
        if (err > 0) {
            setup(0);
        } else {
            setup(1);
        }
    }
    });
    win.show();
    var comm = ["getOs", "getIIS","getVersion", "testDir", "testFile","testWebRead"];
    var index = 0;
    var err = 0;
    var testInfo = function () {
        if (index == comm.length) {
            nextButton.enabled(true);
            if (err > 0) {
                nextButton.val("重试");
                return;
            }
            nextButton.val("下一步");
             return;
        }
        $.get("test.aspx?m=" + comm[index], function (html) {
            var className = "text-success fa fa-check-circle", msg = "";
            switch (index) {
                case 0:
                    if (html.indexOf("server") == -1) {
                        className = "text-warning fa fa-warning"; msg = "建议使用服务器版操作系统";
                    }
                    break;
                case 1:
                    if (html.indexOf("7") == -1) {
                        className = "text-warning fa fa-warning"; msg = "建议使用IIS7以上版本";
                    }
                    break;
                case 2:
                    if (html.indexOf("4") != 0) {
                        className = "text-danger fa fa-times-circle"; msg = ".net版本不正确请使用 .net 4 以上框架"; err++;
                    }
                    break;
                case 3:
                    if (html != "1") {
                        className = "text-danger fa fa-times-circle"; msg = "无法创建文件夹"; err++;
                    }
                    html = html == 1 ? "有" : "无";
                    break;
                case 4:
                    if (html != "1") {
                        className = "text-danger fa fa-times-circle"; msg = "无法写入文件"; err++;
                    }
                    html = html == 1 ? "有" : "无";
                    break;
                case 5:
                    if (html != "1") {
                        className = "text-danger fa fa-times-circle"; msg = "服务器上无法访问外网"; err++;
                    }
                    html = html == 1 ? "有" : "无";
                    break;

            }
            grid.rows[index].cells[1].val(html);
            grid.rows[index].cells[2].val("<i class='" + className + "' title='" + msg + "' />");
            index++;
            setTimeout(testInfo, 300);
        });
    };
    var index = 0, filecount=0;
    var down = function () {
        if (index == filecount) {
            p.val(100);
            p.remove();
            setup(3);
            return;
        }
        $.get("test.aspx?m=downFile&pagecount=" + filecount + "&pageno=" + (index + 1), function (html) {
            p.val((index + 0.0) / filecount * 100);
            index++;
            setTimeout(down, 100);
        });
    };
    var p = $M.progressDialog({text:"下载安装包..."});
    var setup = function (value) {
        switch (value) {
            case 0:
                err = 0;
                index = 0;
                grid.clear();
                grid.addRow([["操作系统"], ["IIS版本"], [".net framework4.0"], ["文件夹创建权限"], ["文件读写权限"], ["外网访问权限"]]);
                testInfo();
                break;
            case 1:
                $.get("test.aspx?m=downDll", function () {
                    $.get("test.aspx?m=readFileSize", function (html) {
                        filecount = html;
                        setup(2);
                    });
                });
                break;
            case 2:
                index = 0;
                if (filecount == 0) {
                    setup(3);
                    return;
                }
                p.show();
                $.get("test.aspx?m=readFileSize", function (html) {
                    filecount = html;
                    down();
                });
                break;
            case 3:
                $.get("unpack.aspx", function (html) {
                    grid.dispose();
                    box.addControl([
                    { xtype: "TextBox", name: "serverIp", labelText: "数据库服务器", labelWidth: 3, width: 8 },
                    { xtype: "TextBox", name: "serverName", labelText: "数据库名", labelWidth: 3, width: 8 },
                    { xtype: "TextBox", name: "username", labelText: "用户名", labelWidth: 3, width: 8 },
                    { xtype: "TextBox", name: "password", password: true, labelText: "密码", labelWidth: 3, width: 8 }
                    ]);
                });
                break;
        }
    };
    setup(3);
</script>

</html>
