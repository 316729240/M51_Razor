using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MWMS;
using System.Web.Razor;
using RazorEngine.Configuration;
using RazorEngine;
using RazorEngine.Templating;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
        {
            CatchPath = HttpContext.Current.Server.MapPath("~" + Config.cachePath + "assembly/")
        };
        Razor.SetTemplateService(new TemplateService(templateConfig));

        RazorEngine.Razor.Compile("kkkkkk", typeof(object[]), "test", true);

        //attr
        // string a = Request["a"].ToString();
        //UserClass.addCash(9665577556, -10, "充值", "系统");
        //UserClass.addCash(9665577556, -50, "充值", "系统");

    }
}
class DynamicAttr : System.Dynamic.DynamicObject
{
    public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
    {
        if (map != null)
        {
            string name = binder.Name;
            object value;
            if (map.TryGetValue(name, out value))
            {
                result = value;
                return true;
            }
        }
        return base.TryGetMember(binder, out result);
    }

    System.Collections.Generic.Dictionary<string, object> map;

    public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
    {
        if (binder.Name == "set" && binder.CallInfo.ArgumentCount == 2)
        {
            string name = args[0] as string;
            if (name == null)
            {
                //throw new ArgumentException("name");
                result = null;
                return false;
            }
            if (map == null)
            {
                map = new System.Collections.Generic.Dictionary<string, object>();
            }
            object value = args[1];
            map.Add(name, value);
            result = value;
            return true;

        }
        return base.TryInvokeMember(binder, args, out result);
    }
}