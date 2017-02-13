using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MWMS.DAL.Datatype
{
    [Serializable]
    public class Field
    {
        public string name = "";
        public string text = "";
        public string type = "";
        public int minLenth = 0;
        public int width = 150;
        public string control = "";
        public bool visible = false;
        public bool isTitle = false;
        public bool isPublicField = false;//是否为公共字段
        public bool isNecessary = false;//是否必要字段
        public string format = "";//格式
        public Field(string structure)
        {
            string[] TS = structure.Split('|');
            for (int n = 0; n < TS.Length; n++)
            {
                if (TS[n] != "")
                {
                    string[] FL = TS[n].Split('-');
                    name = FL[0];
                    text = FL[1];
                    type = FL[2];
                    if (FL[3] != "") minLenth = int.Parse(FL[3]);
                    if (FL[4] != "") maxLenth = int.Parse(FL[4]);
                    if (FL[6] != "") format = FL[6];
                    control = FL[5];
                    if (name == "id") isNecessary = true;
                    if (name == "orderId") isNecessary = true;
                    if (name == "auditMsg") isNecessary = true;

                }
            }
        }
        int _maxLenth = 0;
        public int maxLenth
        {
            get { return _maxLenth; }
            set { width = value * 8; _maxLenth = value; if (width > 300) width = 300; }
        }
    }
}
