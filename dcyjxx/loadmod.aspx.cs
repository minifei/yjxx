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
	/// ģ����ú������Զ����ݴ���ļ����û���Ϣ������½��ת����ģ��·��
	/// ������
    /// user �û���
    /// pass ����
    /// type �û�����T--��ʦ S--ѧ�� P--�ҳ�
    /// from ģ���ַ�����ڵ�ǰĿ¼�����Web��ַ
    /// to ��ʼ���õ�ַ������֤�Ƿ�Ϸ�
    ///     ����ʱ����Request.ServerVariables["HTTP_REFERER"]���ֵ�Ƿ�ƥ��
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

                //��ΪSession�������ⶪʧ�����������������cookie��
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
                Response.Write("��ȫ�������赲��ҳֱ�ӷ��ʣ������µ���Ӧ�Ĺ��ܰ�ť�����ӽ����ҳ�����µ�¼��");
                Response.End();
            }

        }
/// <summary>
/// 20081028Ϊ��������ſ��Գɼ�
/// </summary>
        private void setbasesession(string id)
        {
            /*����д����ѧ����½
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
                    Session["usersub"] = tb.Rows[0]["subname"].ToString();// "Ӣ��";
                    Session["usersubid"] = tb.Rows[0]["subnameid"].ToString(); //"SUB03";
                }
                else
                {
                    Session["username"] = "fff";// tb.Rows[0]["name"].ToString();//"fff"
                    Session["userid"] = ""; //"wupiji";// tb.Rows[0]["usersname"].ToString();// "23";
                    Session["usersub"] = "";// "Ӣ��";// tb.Rows[0]["subname"].ToString();// "Ӣ��";
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
            //ͬʱ����Session
            Session[CookieName] = CookieValue;
            //cookie������������:expires;Ĭ�������,cookieֻ��������Ự�ڴ���.�˳�������Ͷ�ʧ;������expires����ʱ��;�˳��������Ͳ��ᶪʧ����Ϊ�ͻ����������cookie�ļ�;����ʱ���cookieʧЧ,�����Զ�ɾ��cookie�ļ�.
            Response.Cookies[CookieName].Value = CookieValue;
            //�ص������cookie�Զ�ʧЧ
            //			System.TimeSpan tSpan = new System.TimeSpan(365, 0, 0, 0);
            //			Response.Cookies[CookieName].Expires=DateTime.Today + tSpan;

        }
        //����ʧЧ����µ��ж� Modified by CYC @ 20051022
        public string GetCookie(string CookieName)
        {
            string TempResult;
            if (Session[CookieName] != null)
            {
                //���Sessionδ��ʧ����ȡSessionֵ
                TempResult = Session[CookieName].ToString();
            }
            else
            {
                //�ж�Cookie�Ƿ����,�����������дSession,��������������µ�¼ Modified By CYC @ 20051019
                //���Session��ʧ����ȡCookiesֵ
                if (Request.Cookies[CookieName] != null)
                {
                    TempResult = Request.Cookies[CookieName].Value;
                    Session[CookieName] = TempResult;
                }
                else
                {
                    TempResult = "";
                    //Response.Write("<script language='javascript'>if (confirm('ϵͳ�޷���ȷ��ȡ��ǰ�û���Ϣ������������������վ��Cookie�����µ�¼��')){parent.location.href='../default.aspx';}</script>");
                }

            }
            return TempResult;
        }
    }

