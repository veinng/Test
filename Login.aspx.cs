using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using YTL.Common;
using System.Data;
using System.Data.SqlClient;
using APSCv4.Class;
using YTL.Security.Login;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace APSCv4
{
    public partial class LoginV3 : System.Web.UI.Page
    {

private void MsgBox(string strMessage)
        {
            string strScript = "<SCRIPT LANGUAGE=\"JavaScript\">";
            strScript += "alert(\"" + strMessage + "\")";
            strScript += "</SCRIPT>";
            ClientScript.RegisterStartupScript(typeof(string), "messageBox", strScript);
        }
        
     }
}
