using ManagerFramework;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Login login=User.Login("", "");

            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
            {
                CatchPath = @"F:\web\my\M51_Razor\MainWeb\cache\assembly"
            };
            Razor.SetTemplateService(new TemplateService(templateConfig));
            string _html = "";
             _html = "@using System.Collections\r\n" + _html;


            RazorEngine.Razor.Compile(_html, typeof(object[]), "11211", true);
        }
    }
}
