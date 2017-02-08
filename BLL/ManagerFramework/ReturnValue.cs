using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerFramework
{
    public class ReturnValue
    {
        public int errNo = 0;
        public string errMsg = "";
        public object userData = null;//用户数据
        public ReturnValue()
        {

        }
        public ReturnValue(int errNo,string errMsg)
        {
            this.errNo = errNo ;
            this.errMsg = errMsg;
        }
    }
}
