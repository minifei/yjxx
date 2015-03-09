using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Text;
using System.Web.Security;
using util;
//using myclass;

	/// <summary>
	/// 模块调用函数：自动根据传入的加密用户信息参数登陆后转调用模块路径
	/// 参数：
    /// user 用户名
    /// pass 密码
    /// type 用户类型T--教师 S--学生 P--家长
    /// from 模块地址，基于当前目录的相对Web地址
    /// to 初始调用地址，在验证是否合法
    ///     访问时会检测Request.ServerVariables["HTTP_REFERER"]与该值是否匹配
    /// Created by CYC @ 20060801
	/// </summary>
    /// 
    public partial class LoadMod : System.Web.UI.Page
    {

        basefunc mybase = new basefunc();

        protected void Page_Load(object sender, System.EventArgs e)
        {
            try
            {
                Session.RemoveAll();
                Session["HostName"] = Request.UserHostName;
                Session["HostIP"] = Request.UserHostAddress;

                string lbUser = "user";
                string lbPass = "pass";
                string lbType = "type";
                string lbFrom = "from";
                string lbTo = "to";
                string lbSchSerID = "schid";
                string lbUserSerID = "uid";

                string username = Request[lbUser].ToString();
                string password = Request[lbPass].ToString();
                string usertype = Request[lbType].ToString();
                string fromURL = Request[lbFrom].ToString();
                string toURL = Request[lbTo].ToString();
                string schserid = Request[lbSchSerID].ToString();
                string userserid = Request[lbUserSerID].ToString();

                string strRefUrl = Request.ServerVariables["HTTP_REFERER"];

                username = mybase.decode(username);
                password = mybase.decode(password);
                usertype = mybase.decode(usertype);
                fromURL = mybase.decode(fromURL);
                toURL = mybase.decode(toURL);
                schserid = mybase.decode(schserid);
                userserid = mybase.decode(userserid);

//Response.Write(GetCookie("SchSerID"));
//Response.End();

               // if (toURL != strRefUrl)
                //{
               //     Response.End();
               // }

                string sUserIdWrk = username.ToUpper().Trim();
                sUserIdWrk += "," + userserid;
                Session["UserID"] = username;
                Session["UserSerID"] = userserid;
                Session["UserType"] = usertype;
                Session["UserPass"] = password;
                Session["SchSerID"] = schserid;

                //因为Session经常意外丢失，所以以下两项保存在cookie中
                SetCookie("UserSerID", userserid);
                SetCookie("UserID", username);
                SetCookie("SchSerID", schserid);
                SetCookie("UserType", usertype);
                SetCookie("UserPass", password);


                //setbasesession(userserid);

//Response.Write(sUserIdWrk);
//Response.End();
                FormsAuthentication.SetAuthCookie(sUserIdWrk, true);
                Response.Redirect(fromURL);

            }
            catch
            {
                Response.Write("安全检测程序阻挡该页直接访问，请重新点相应的功能按钮或链接进入该页或重新登录。");
                Response.End();
            }

        }
/// <summary>
/// 20081028为了输入混排考试成绩
/// </summary>
        private void setbasesession(string id)
        {
            /*需重写考虑学生登陆
            DBEdit db20 = new DBEdit();
            db20.SetSQLConnString("sqlconncxda");
            string sqlstr20 = "SELECT [UserID2] FROM [SchInfo_DCZX].[dbo].[IAM_FwUser] where teaserid='" + id + "'";
            DataTable tb20 = db20.SQL_DB_select(sqlstr20); ;
            if (tb20.Rows.Count > 0)
            {
                DBEdit db = new DBEdit();
                db.SetSQLConnString("sqlconn");
                string sqlstr = "SELECT [usersid], [usersname], [name], [subname], [deptid], [subnameid], [zwid], [jyzid] FROM [dczx].[dbo].[B_userotherinfo] where usersname='" + tb20.Rows[0][0].ToString() + "'";
                DataTable tb = db.SQL_DB_select(sqlstr);
                if (tb.Rows.Count > 0)
                {

                    Session["username"] = tb.Rows[0]["name"].ToString();//"fff"
                    Session["userid"] = tb.Rows[0]["usersname"].ToString();// "23";
                    Session["usersub"] = tb.Rows[0]["subname"].ToString();// "英语";
                    Session["usersubid"] = tb.Rows[0]["subnameid"].ToString(); //"SUB03";
                }
                else
                {
                    Session["username"] = "fff";// tb.Rows[0]["name"].ToString();//"fff"
                    Session["userid"] = ""; //"wupiji";// tb.Rows[0]["usersname"].ToString();// "23";
                    Session["usersub"] = "";// "英语";// tb.Rows[0]["subname"].ToString();// "英语";
                    Session["usersubid"] = "";//"SUB03";
                }
             *
            }
            else
            {
                return;
            } */
        }


        public void SetCookie(string CookieName, string CookieValue)
        {
            //同时设置Session
            Session[CookieName] = CookieValue;
            //cookie的生存期属性:expires;默认情况下,cookie只在浏览器会话期存在.退出浏览器就丢失;可以用expires设置时间;退出浏览器后就不会丢失并存为客户端浏览器的cookie文件;过了时间后cookie失效,还会自动删除cookie文件.
            Response.Cookies[CookieName].Value = CookieValue;
            //关掉浏览器cookie自动失效
            //			System.TimeSpan tSpan = new System.TimeSpan(365, 0, 0, 0);
            //			Response.Cookies[CookieName].Expires=DateTime.Today + tSpan;

        }
        //修正失效情况下的判断 Modified by CYC @ 20051022
        public string GetCookie(string CookieName)
        {
            string TempResult;
            if (Session[CookieName] != null)
            {
                //如果Session未丢失，则取Session值
                TempResult = Session[CookieName].ToString();
            }
            else
            {
                //判断Cookie是否存在,如果存在则重写Session,如果不存在则重新登录 Modified By CYC @ 20051019
                //如果Session丢失，则取Cookies值
                if (Request.Cookies[CookieName] != null)
                {
                    TempResult = Request.Cookies[CookieName].Value;
                    Session[CookieName] = TempResult;
                }
                else
                {
                    TempResult = "";
                    //Response.Write("<script language='javascript'>if (confirm('系统无法正确获取当前用户信息，请设置浏览器允许该站点Cookie后重新登录。')){parent.location.href='../default.aspx';}</script>");
                }

            }
            return TempResult;
        }
    }

