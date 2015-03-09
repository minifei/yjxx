using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Diagnostics;

/// <summary>
/// ��Ҫ�������ֶ�
/// </summary>
public struct DBfeild
{
    public string feild;
    public SqlDbType feildtype;
    public string feildval;
    public string iskey;
    public string isnogetval;//�Ƿ���Ҫ��ֵ,Ĭ��0,1���踳ֵ
    public DBfeild(string fd, SqlDbType fdtype, string fdval, string isKey, string isNogetval)
    {
        feild = fd;
        feildtype = fdtype;
        feildval = fdval;
        iskey = isKey == "1" ? isKey : "0";
        isnogetval = isNogetval == "1" ? isNogetval : "0";
    }
}
public static class JsonStrToList
{
    //json�ַ���תlist����------------------
    public static List<T> JSONStringToList<T>(this string JsonStr)
    {

        System.Web.Script.Serialization.JavaScriptSerializer Serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        List<T> objs = Serializer.Deserialize<List<T>>(JsonStr);

        return objs;

    }

    public static T Deserialize<T>(string json)
    {

        T obj = Activator.CreateInstance<T>();

        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
        {

            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());

            return (T)serializer.ReadObject(ms);

        }

    }
    //------------------------------------------------------

}

/// <summary>
/// 
/// </summary>
public class SqlHelperN
{

    /// <summary>
    /// �ֽ��ַ���Ϊ�ַ������飬����Ӣ�ĵ����Զ��ָ�
    /// </summary>
    /// <param name="txt">��Ҫ�ֽ���ַ���</param>
    /// <returns></returns>
    public string[] getstrs(string txt)
    {

        string[] tp = new string[txt.Length];
        int n = 0;
        string tpstr2 = "";
        //string tpstr3 = "";
        for (int i = 0; i < txt.Length; i++)
        {
            //int   charCode   =   (int)s[i];   
            //if   (   charCode> =0x4e00   &&   charCode <=0x9fa5) 


            // Regex   rx   =   new   Regex( "^[\u4e00-\u9fa5]$ ");
            string tpstr = txt[i].ToString();//Convert.ToInt32(txt[i]).ToString();// txt.Substring(i, 1);
            int charCode = (int)txt[i];
            if (charCode >= 0x4e00 && charCode <= 0x9fa5) //rx.IsMatch(tpstr)
            {
                //   ������ 
                if (tpstr2 != "")
                {
                    tp[n] = tpstr2;
                    tpstr2 = "";
                    n++;
                }

                tp[n] = tpstr;
                n++;
            }
            else
            {
                //   ������


                if (SqlHelper.IsNumeric(tpstr))
                {
                    //������
                    if (SqlHelper.IsNumeric(tpstr2) || tpstr2 == "")
                    {
                        tpstr2 += tpstr;
                    }
                    else
                    {
                        tp[n] = tpstr2;
                        tpstr2 = "";
                        n++;
                    }


                }
                else
                {
                    if (tpstr == " ")
                    {

                        //�ǿո�
                        if (tpstr2 != "")
                        {
                            tp[n] = tpstr2;
                            tpstr2 = "";
                            n++;
                        }
                    }
                    else
                    {
                        tpstr2 += tpstr;
                    }

                }
            }

        }
        string[] rt = new string[n];
        for (int i = 0; i < n; i++)
        {
            rt[i] = tp[i];
        }
        return rt;
    }

    //Excel���ݿ�
    public  DataTable OleDB_Excel_select(string sqlstr, string dbpath)
    {
        string Excelconnstring = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbpath + @";Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
        string connstr = Excelconnstring;//@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\hunpai.mdb;Persist Security Info=True";


        System.Data.OleDb.OleDbConnection oledbcon = new System.Data.OleDb.OleDbConnection(connstr);
        string selstr = sqlstr;//"SELECT [id], [stuid], [cj1]+[cj2] as [cj], [sub], [ksnameid], [mcsch], [mccls] FROM [cj] WHERE ([ksnameid] = '" + ksid + "' and [sub]='" + sub + "') order by cj desc";

        System.Data.OleDb.OleDbDataAdapter oledbdptr = new System.Data.OleDb.OleDbDataAdapter(selstr, oledbcon);
        DataSet oledbdtst = new DataSet();
        oledbdptr.Fill(oledbdtst, "rslt");
        oledbcon.Close();

        return oledbdtst.Tables["rslt"];

    }

    /// <summary>
    /// ö������sql��������
    /// </summary>
    public  enum sqlStrType { select, insert, update, delete };

    public SqlHelperN() {

        CONN_STRING = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;
    }

    //Database connection strings
    public   string CONN_STRING;
    /// <summary>
    /// ���������ַ���
    /// </summary>
    /// <param name="ConnectionString">web.config�ж�������ݿ����ӱ�������</param>
    public  void SetConn_String(string ConnectionString)
    {
        CONN_STRING=ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;
    }
    /// <summary>
    /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the results</returns>
    public  SqlDataReader ExecuteReader(CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(CONN_STRING);

        // we use a try/catch here because if the method throws an exception we want to 
        // close the connection throw code, because no datareader will exist, hence the 
        // commandBehaviour.CloseConnection will not work
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //SqlDataAdapter apt = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return rdr;
        }
        catch
        {
            conn.Close();
            throw;
        }
    }

    /// <summary>
    /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the results</returns>
    public  DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(CONN_STRING);

        // we use a try/catch here because if the method throws an exception we want to 
        // close the connection throw code, because no datareader will exist, hence the 
        // commandBehaviour.CloseConnection will not work
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            SqlDataAdapter apt = new SqlDataAdapter(cmd);
            DataSet mydataset = new DataSet();
            apt.Fill(mydataset, "temptb");
            apt.Dispose();
            //SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //SqlDataAdapter apt = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return mydataset;
        }
        catch
        {
            conn.Close();
            throw;
        }
    }
    /// <summary>
    /// ���ز�ѯ���
    /// </summary>
    /// <param name="cmdtype">���ͣ��洢���̻���sql���</param>
    /// <param name="cmdText">�洢���̵����ֻ�sql���</param>
    
    /// <param name="fieldandVal">�ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <param name="tb">������</param>
    /// <param name="cond">����</param>
    /// <param name="condFieldandVal">�����е��ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <returns>���ز�ѯ�����dataset</returns>
    public DataSet ExecuteDataSet(CommandType cmdtype,string cmdText,  fieldtoVal[] fieldandVal, string tb, string cond, fieldtoVal[] condFieldandVal)
    {
        SqlParameter[] param;
        string sqlstr=getSqlString( sqlStrType.select,fieldandVal, tb, cond, condFieldandVal,out param);
        if (cmdtype == CommandType.Text)
        {
            return ExecuteDataSet(cmdtype, sqlstr, param);
        }
        else
        {
            return ExecuteDataSet(cmdtype, cmdText, param);
        }
    }
    /// <summary>
    /// ���ز�ѯ���
    /// </summary>
    /// <param name="cmdtype">���ͣ��洢���̻���sql���</param>
    /// <param name="cmdText">�洢���̵����ֻ�sql���</param>
   
    /// <param name="fieldandVal">�ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <param name="tb">������</param>
    /// <param name="cond">����</param>
    /// <param name="condFieldandVal">�����е��ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <returns>���ز�ѯ�����sqlDataReader</returns>
    public  SqlDataReader ExecuteReader(CommandType cmdtype, string cmdText, fieldtoVal[] fieldandVal, string tb, string cond, fieldtoVal[] condFieldandVal)
    {
        SqlParameter[] param;
        string sqlstr = getSqlString(sqlStrType.select, fieldandVal, tb, cond, condFieldandVal, out param);
        if (cmdtype == CommandType.Text)
        {
            return ExecuteReader(cmdtype, sqlstr, param);
        }
        else
        {
            return ExecuteReader(cmdtype, cmdText, param);
        }
    }


    /// <summary>
    /// ִ��sql���
    /// </summary>
    /// <param name="cmdtype">���ͣ��洢���̻���sql���</param>
    /// <param name="cmdText">�洢���̵����ֻ�sql���</param>
   
    /// <param name="sqlStringType">sql��������insert��update��delete</param>
    /// <param name="fieldandVal">�ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <param name="tb">������</param>
    /// <param name="cond">����</param>
    /// <param name="condFieldandVal">�����е��ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <returns>���ز�ѯ�����sqlDataReader</returns>
    public  object ExecuteScalar(CommandType cmdtype, string cmdText, sqlStrType sqlStringType, fieldtoVal[] fieldandVal, string tb, string cond, fieldtoVal[] condFieldandVal)
    {
        SqlParameter[] param;
        string sqlstr = getSqlString(sqlStringType, fieldandVal, tb, cond, condFieldandVal, out param);
        if (cmdtype == CommandType.Text)
        {
            return ExecuteScalar(cmdtype, sqlstr, param);
        }
        else
        {
            return ExecuteScalar(cmdtype, cmdText, param);
        }
    }
    /// <summary>
    /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
    public  object ExecuteScalar(CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {
        SqlCommand cmd = new SqlCommand();

        using (SqlConnection conn = new SqlConnection(CONN_STRING))
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
    }

    /// <summary>
    /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="conn">an existing database connection</param>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
    public  object ExecuteScalar(SqlConnection conn, CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {

        SqlCommand cmd = new SqlCommand();

        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        object val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }


    /// <summary>
    /// Prepare a command for execution
    /// </summary>
    /// <param name="cmd">SqlCommand object</param>
    /// <param name="conn">SqlConnection object</param>
    /// <param name="trans">SqlTransaction object</param>
    /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
    /// <param name="cmdText">Command text, e.g. Select * from Products</param>
    /// <param name="cmdParms">SqlParameters to use in the command</param>
    private  void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
    {

        if (conn.State != ConnectionState.Open)
            conn.Open();

        cmd.Connection = conn;
        cmd.CommandText = cmdText;

        if (trans != null)
            cmd.Transaction = trans;

        cmd.CommandType = cmdType;

        if (cmdParms != null)
        {
            foreach (SqlParameter parm in cmdParms)
                cmd.Parameters.Add(parm);
        }
    }

    

    /// <summary>
    /// �õ�sql���
    /// </summary>
    /// <param name="sqlStringType">����sql�������ͣ�ö��</param>
    /// <param name="fieldandVal">�ֶΣ��ֶζ�Ӧ��ֵ���ֶε����ͣ��ṹfieldtoVal</param>
    /// <param name="tb">�������</param>
    /// <param name="cond">sql��������</param>
    /// <param name="condFieldandVal">sql��������еı����б��ṹfieldtoVal</param>
    /// <param name="param">�����sqlparam����</param>
    /// <returns>����sql���</returns>
    public  string getSqlString(sqlStrType sqlStringType,fieldtoVal[] fieldandVal,string tb,string cond,fieldtoVal[] condFieldandVal,out SqlParameter[] param)
    {
        string sqlstr = "";
        int plength=fieldandVal.Length+condFieldandVal.Length;
        param = new SqlParameter[plength];
        int i = 0;
        for (int j=0; j < fieldandVal.Length; j++)
        {
            param[i] = new SqlParameter("@" + fieldandVal[j].fieldName, fieldandVal[j].sqldbtype);
            param[i].Value = fieldandVal[j].fieldVal;
            i++;
        }
        for (int j = 0; j < condFieldandVal.Length; j++)
        {
            param[i] = new SqlParameter("@" + condFieldandVal[j].fieldName, condFieldandVal[j].sqldbtype);
            param[i].Value = condFieldandVal[j].fieldVal;
            i++;
        }
        
        if (sqlStringType == sqlStrType.select)
        {
            sqlstr = " select ";
            for (int j = 0; j < fieldandVal.Length; j++)
            {
                if (j == 0)
                    sqlstr += fieldandVal[j].fieldName;
                else
                    sqlstr += " ," + fieldandVal[j].fieldName;
            }
            sqlstr += " from " + tb;
            sqlstr += " where "+cond;
            
        }

        if (sqlStringType == sqlStrType.insert)
        {
            sqlstr = " insert into "+tb+" (";
            for (int j = 0; j < fieldandVal.Length; j++)
            {
                if (j == 0)
                    sqlstr += fieldandVal[j].fieldName;
                else
                    sqlstr += "," + fieldandVal[j].fieldName;
            }
            sqlstr += ") values ( ";
            for (int j = 0; j < fieldandVal.Length; j++)
            {
                if (j == 0)
                    sqlstr += fieldandVal[j].fieldVal;
                else
                    sqlstr += ","+fieldandVal[j].fieldVal;
            }
            sqlstr += ")";
        }
           
        
        if (sqlStringType == sqlStrType.update)
            {
                sqlstr = " update "+tb+" set ";
                for (int j = 0; j < fieldandVal.Length; j++)
                {
                    if (j == 0)
                        sqlstr += fieldandVal[j].fieldName + "=" + fieldandVal[j].fieldVal;
                    else
                        sqlstr += "," + fieldandVal[j].fieldName + "=" + fieldandVal[j].fieldVal;
                }
                sqlstr += " where " + cond;
            }


            if (sqlStringType == sqlStrType.delete)
            {
                sqlstr="delete from "+tb+" where " +cond;
            }
        
        return sqlstr;
        
        
    }
}




/// <summary>
/// The SqlHelper class is intended to encapsulate high performance, 
/// scalable best practices for common uses of SqlClient.
/// </summary>
public sealed class SqlHelper
{
    //�������cookie
   public static void delallcookie()
    {
        HttpCookie aCookie;
        string cookieName;
        int limit = System.Web.HttpContext.Current.Request.Cookies.Count;
        for (int i = 0; i < limit; i++)
        {
            cookieName = System.Web.HttpContext.Current.Request.Cookies[i].Name;
            aCookie = new HttpCookie(cookieName);
            aCookie.Expires = DateTime.Now.AddDays(-1);
            System.Web.HttpContext.Current.Response.Cookies.Add(aCookie);
        }
    }


    ///   <summary>
    ///   ȥ��HTML���
    ///   </summary>
    ///   <param   name="Htmlstring">����HTML��Դ��   </param>
    ///   <returns>�Ѿ�ȥ���������</returns>
    public static string NoHTML(string Htmlstring)
    {
        //ɾ���ű�
        Htmlstring = Htmlstring.Replace("\r\n", "");
        Htmlstring = Regex.Replace(Htmlstring, @"<script.*?</script>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<style.*?</style>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<.*?>", "", RegexOptions.IgnoreCase);
        //ɾ��HTML
        Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
        Htmlstring = Htmlstring.Replace("<", "");
        Htmlstring = Htmlstring.Replace(">", "");
        Htmlstring = Htmlstring.Replace("\r\n", "");
        Htmlstring = HttpContext.Current.Server.HtmlEncode(Htmlstring).Trim();
        return Htmlstring;
    }

    /// <summary>
    /// ��ȡHTML���������ֵ�C#����
    /// </summary>
    public static string StripHTML(string strHtml)
    {
        string[] aryReg ={
           @"<script[^>]*?>.*?</script>",
           @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
           @"([\r\n])[\s]+",
           @"&(quot|#34);",
           @"&(amp|#38);",
           @"&(lt|#60);",
           @"&(gt|#62);",
           @"&(nbsp|#160);",
           @"&(iexcl|#161);",
           @"&(cent|#162);",
           @"&(pound|#163);",
           @"&(copy|#169);",
           @"&#(\d+);",
           @"-->",
           @"<!--.*\n"
          };
        string[] aryRep =   {
             "",
             "",
             "",
             "\"",
             "&",
             "<",
             ">",
             "   ",
             "\xa1",//chr(161),
             "\xa2",//chr(162),
             "\xa3",//chr(163),
             "\xa9",//chr(169),
             "",
             "\r\n",
             ""
            };
        string newReg = aryReg[0];
        string strOutput = strHtml;
        for (int i = 0; i < aryReg.Length; i++)
        {
            Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
            strOutput = regex.Replace(strOutput, aryRep[i]);
        }
        strOutput.Replace("<", "");
        strOutput.Replace(">", "");
        strOutput.Replace("\r\n", "");
        return strOutput;
    }


    ///   <summary>
    ///   ȡ���ı��е�ͼƬ��ַ
    ///   </summary>
    ///   <param   name="HTMLStr">HTMLStr</param>
    public static string GetImgUrl(string HTMLStr)
    {
        string str = string.Empty;
        string sPattern = @"^<img\s+[^>]*>";
        Regex r = new Regex(@"<img\s+[^>]*\s*src\s*=\s*([']?)(?<url>\S+)'?[^>]*>",
         RegexOptions.Compiled);
        Match m = r.Match(HTMLStr.ToLower());
        if (m.Success)
            str = m.Result("${url}");
        return str;
    }



    /// <summary>
    /// �滻��ǵ�����Ϊȫ�ǵ�����
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string replacedlh(string str)
    {
        return str.Replace("'", "��");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inFilename">�ĵ�����·��</param>
    /// <param name="swfFilename">swf����·��</param>
    /// <param name="FlashPaperPath">flashpaper����·��</param>
    public static string ConvertTxtToSwf(string inFilename, string swfFilename,string FlashPaperPath)
    {

        try
        {

            string flashPrinter = FlashPaperPath; //string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"FlashPaper2.2\FlashPrinter.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo(flashPrinter);

            startInfo.Arguments = string.Concat(inFilename, " -o ", swfFilename);

            Process process = new Process();

            process.StartInfo = startInfo;

            bool isStart = process.Start();

            process.WaitForExit();

            process.Close();
            return "ok";

        }

        catch (Exception ex)
        {

            return(ex.Message);

        }

    }



    /// <summary>
    /// ��֤Ʊ��
    /// </summary>
    /// <param name="username"></param>
    public static HttpCookie setFormsAuthenticationCookie(string username, string roles)
    {
        //string roles = "dczxusers";
        //������֤Ʊ�ݶ���
        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMinutes(20), false, roles);
        //������֤Ʊ��
        string encrytedTicket = FormsAuthentication.Encrypt(authTicket);
        //����Cookie����
        //FormsAuthentication.FormsCookieNameȡ��WebConfig��<Authentication>
        //���ý���Name��ֵ��ΪCookie�����֣�
        HttpCookie rt = new HttpCookie(FormsAuthentication.FormsCookieName, encrytedTicket);
        rt.Path = "/";
        return rt;
        ////HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName,encrytedTicket);
        /////Response.Cookies.Add(authCookie);

    }


    /// <summary> 
    /// ���ַ���ʹ��base64�㷨���� 
    /// </summary> 
    /// <param name="code_type">�������ͣ��������ƣ� 
    /// * ����ҳ ���� 
    /// * 1200 "UTF-16LE"��"utf-16"��"ucs-2"��"unicode"��"ISO-10646-UCS-2" 
    /// * 1201 "UTF-16BE"��"unicodeFFFE" 
    /// * 1252 "windows-1252" 
    /// * 65000 "utf-7"��"csUnicode11UTF7"��"unicode-1-1-utf-7"��"unicode-2-0-utf-7"��"x-unicode-1-1-utf-7"��"x-unicode-2-0-utf-7" 
    /// * 65001 "utf-8"��"unicode-1-1-utf-8"��"unicode-2-0-utf-8"��"x-unicode-1-1-utf-8"��"x-unicode-2-0-utf-8" 
    /// * 20127 "us-ascii"��"us"��"ascii"��"ANSI_X3.4-1968"��"ANSI_X3.4-1986"��"cp367"��"csASCII"��"IBM367"��"iso-ir-6"��"ISO646-US"��"ISO_646.irv:1991" 
    /// * 54936 "GB18030"    
    /// </param> 
    /// <param name="code">�����ܵ��ַ���</param> 
    /// <returns>���ܺ���ַ���</returns> 
    public static string EncodeBase64(string code_type, string code)
    {
        string encode = "";
        byte[] bytes = Encoding.GetEncoding(code_type).GetBytes(code);  //��һ���ַ�����Ϊһ���ֽ�����. 
        try
        {
            encode = Convert.ToBase64String(bytes);  //��8λ�޷�������������Ӽ�ת��Ϊ���Ч��,��64Ϊ�������ֱ�����ַ�����ʽ. 
        }
        catch
        {
            encode = code;
        }
        return encode;
    }

    /// <summary> 
    /// ���ַ���ʹ��base64�㷨���� 
    /// </summary> 
    /// <param name="code_type">��������</param> 
    /// <param name="code">����base64�㷨���ܵ��ַ���</param> 
    /// <returns>���ܺ���ַ���</returns> 
    public static string DecodeBase64(string code_type, string code)
    {
        string decode = "";
        byte[] bytes = Convert.FromBase64String(code);  //��2���Ʊ���ת��Ϊ8λ�޷�����������. 
        try
        {
            decode = Encoding.GetEncoding(code_type).GetString(bytes);  //��ָ���ֽ������е�һ���ֽ����н���Ϊһ���ַ����� 
        }
        catch
        {
            decode = code;
        }
        return decode;
    }


    ///   <summary>   
    ///   ���Ӳ���ļ����ṩ���أ����ӣ�SqlHelper.ResponseFile(Page.Request, Page.Response, filenameshow, Server.MapPath(filenamepath), 10240000); 
    ///   </summary>   
    ///   <param   name="_Request">Page.Request����</param>   
    ///   <param   name="_Response">Page.Response����</param>   
    ///   <param   name="_fileName">�����ļ���</param>   
    ///   <param   name="_fullPath">���ļ�����������·��</param>   
    ///   <param   name="_speed">ÿ���������ص��ֽ���</param>   
    ///   <returns>�����Ƿ�ɹ�</returns>
    public static bool ResponseFile(HttpRequest _Request, HttpResponse _Response, string _fileName, string _fullPath, long _speed)
    {
        try
        {
            FileStream myFile = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(myFile);
            try
            {
                _Response.AddHeader("Accept-Ranges", "bytes");
                _Response.Buffer = false;
                long fileLength = myFile.Length;
                long startBytes = 0;

                int pack = 10240;   //10K   bytes   
                //int   sleep   =   200;       //ÿ��5��       ��5*10K   bytesÿ��   
                int sleep = (int)Math.Floor(1000 * Convert.ToDouble(pack) / _speed) + 1;
                if (_Request.Headers["Range"] != null)
                {
                    _Response.StatusCode = 206;
                    string[] range = _Request.Headers["Range"].Split(new char[] { '=', '-' });
                    startBytes = Convert.ToInt64(range[1]);
                }
                _Response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                if (startBytes != 0)
                {
                    _Response.AddHeader("Content-Range", string.Format("   bytes   {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                }
                _Response.AddHeader("Connection", "Keep-Alive");

                string exfilename = Path.GetExtension(_fullPath);
                string filetype = "application/octet-stream";
                switch (exfilename)
                {
                    case ".doc":
                        {
                            filetype = "application/msword";
                            break;
                        }
                    case ".docx":
                        {
                            filetype = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                            break;
                        }
                    case ".rtf":
                        {
                            filetype = "application/rtf";
                            break;
                        }
                    case ".xls":
                        {
                            filetype = "application/ms-excel";
                            break;
                        }
                    case ".xlsx":
                        {
                            filetype = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            break;
                        }
                    case ".ppt":
                        {
                            filetype = "application/vnd.ms-powerpoint";
                            break;
                        }
                    case ".pptx":
                        {
                            filetype = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                            break;
                        }
                    case ".pdf":
                        {
                            filetype = "application/pdf";
                            break;
                        }
                    case ".swf":
                        {
                            filetype = "application/x-shockwave-flash";
                            break;
                        }
                    case ".zip":
                        {
                            filetype = "application/x-zip-compressed";
                            break;
                        }
                    case ".wmv":
                        {
                            filetype = "video/x-ms-wmv";
                            break;
                        }
                    case ".mpg":
                        {
                            filetype = "audio/mpeg";
                            break;
                        }
                    case ".rm":
                        {
                            filetype = "application/vnd.rn-realmedia";
                            break;
                        }
                    case ".txt ":
                        {
                            filetype = "text/plain";
                            break;
                        }
                    case ".bmp":
                        {
                            filetype = "image/bmp";
                            break;
                        }
                    case ".gif":
                        {
                            filetype = "image/gif";
                            break;
                        }
                    case ".png":
                        {
                            filetype = "image/png";
                            break;
                        }
                    case ".tif":
                        {
                            filetype = "image/tiff";
                            break;
                        }
                     case ".tiff":
                        {
                            filetype = "image/tiff";
                            break;
                        }
                      case ".jpg":
                        {
                            filetype = "image/jpeg";
                            break;
                        }
                      case ".jpe":
                        {
                            filetype = "image/jpeg";
                            break;
                        }
                      case ".jpeg":
                        {
                            filetype = "image/jpeg";
                            break;
                        }
                        
                }
                //if (exfilename == ".xls")
                //    filetype = "application/ms-excel";
                //if (exfilename == ".doc")
                //    filetype = "application/ms-Word";

                //filetype; //
                //System.Web.HttpException
                
                _Response.ContentType = filetype; //"application/octet-stream";
                _Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(_fileName, System.Text.Encoding.UTF8));

                br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                int maxCount = (int)Math.Floor((fileLength - startBytes) / Convert.ToDouble(pack)) + 1;

                for (int i = 0; i < maxCount; i++)
                {
                    if (_Response.IsClientConnected)
                    {
                        _Response.BinaryWrite(br.ReadBytes(pack));
                        Thread.Sleep(sleep);
                    }
                    else
                    {
                        i = maxCount;
                    }
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                br.Close();
                myFile.Close();
            }
        }
        catch
        {
            return false;
        }
        return true;
    }   
    



    /// <summary>�ϲ�gragview�е���</summary> 
    /// <param name="GridView1">GridView</param> 
    /// <param name="cellNum">�ڼ���</param> 
    /// <param name="ii">��ʼ�е��кţ���0��ʼ��</param>
    /// <param name="l">����</param>
    /// <param name="labelname">Label��id��</param>
    public static void GroupCols(GridView GridView1, int cellNum, int ii, int l, string labelname)
    {
        //int i = 0, 
        int rowSpanNum = 1;//�ϲ�������
        //string cellnumstr = Convert.ToString(cellNum + 1);
        int rowscount = ii + l;
        int i = ii;
        while (i < rowscount)//GridView1.Rows.Count - 1
        {
            GridViewRow gvr = GridView1.Rows[i];

            for (++i; i < rowscount; i++)//GridView1.Rows.Count
            {
                GridViewRow gvrNext = GridView1.Rows[i];
                Label gvrlb = (Label)gvr.Cells[cellNum].FindControl(labelname);
                Label gvrNextlb = (Label)gvrNext.Cells[cellNum].FindControl(labelname);
                //Response.Write(gvrNext.Cells[cellNum].Text.Trim());
                //gvr.Cells[cellNum].Text.Trim() == gvrNext.Cells[cellNum].Text.Trim() || gvrNext.Cells[cellNum].Text.Trim() == "&nbsp;"
                if (gvrlb.Text == gvrNextlb.Text || gvrNextlb.Text == "")//
                {
                    gvrNext.Cells[cellNum].Visible = false;
                    //Response.Write("+--+" + gvr.Cells[cellNum].Text.Trim() + "--" + gvrNext.Cells[cellNum].Text.Trim());
                    rowSpanNum++;
                }
                else
                {
                    gvr.Cells[cellNum].RowSpan = rowSpanNum;
                    rowSpanNum = 1;
                    break;
                }

                if (i == rowscount - 1)
                {
                    gvr.Cells[cellNum].RowSpan = rowSpanNum;
                }
            }
        }
    }

    /// <summary>�ϲ�gragview�е���</summary> 
    /// <param name="GridView1">GridView</param> 
    /// <param name="cellNum">�ڼ���</param> 
    /// <param name="i">��ʼ�е��кţ���0��ʼ��</param>
    /// <param name="l">����</param>
    public static void GroupCols(GridView GridView1, int cellNum, int i, int l)
    {
        //int i = 0, 
        int rowSpanNum = 1;//�ϲ�������
        //string cellnumstr = Convert.ToString(cellNum + 1);
        int rowscount = i + l;
        while (i < rowscount)//GridView1.Rows.Count - 1
        {
            GridViewRow gvr = GridView1.Rows[i];

            for (++i; i < rowscount; i++)//GridView1.Rows.Count
            {
                GridViewRow gvrNext = GridView1.Rows[i];
                //Label gvrlb = (Label)gvr.Cells[cellNum].FindControl(labelname);
                //Label gvrNextlb = (Label)gvrNext.Cells[cellNum].FindControl(labelname);
                //Response.Write(gvrNext.Cells[cellNum].Text.Trim());
                if (gvr.Cells[cellNum].Text.Trim() == gvrNext.Cells[cellNum].Text.Trim() || gvrNext.Cells[cellNum].Text.Trim() == "&nbsp;")//gvrlb.Text == gvrNextlb.Text
                {
                    gvrNext.Cells[cellNum].Visible = false;
                    rowSpanNum++;
                }
                else
                {
                    gvr.Cells[cellNum].RowSpan = rowSpanNum;
                    rowSpanNum = 1;
                    break;
                }

                if (i == rowscount - 1)
                {
                    gvr.Cells[cellNum].RowSpan = rowSpanNum;
                }
            }
        }
    }




    /// <summary> 
    /// ȡ�ÿͻ�����ʵIP������д�����ȡ��һ����������ַ 
    /// </summary> 
    public static string IPAddress
    {

        get
        {
            string result = String.Empty;

            result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (result != null && result != String.Empty)
            {
                //�����д��� 
                if (result.IndexOf(".") == -1)    //û�С�.���϶��Ƿ�IPv4��ʽ 
                    result = null;
                else
                {
                    if (result.IndexOf(",") != -1)
                    {
                        //�С�,�������ƶ������ȡ��һ������������IP�� 
                        result = result.Replace(" ", "").Replace("'", "");
                        string[] temparyip = result.Split(",;".ToCharArray());
                        for (int i = 0; i < temparyip.Length; i++)
                        {

                            if (IsIPAddress(temparyip[i])
                                && temparyip[i].Substring(0, 3) != "10."
                                && temparyip[i].Substring(0, 7) != "192.168"
                                && temparyip[i].Substring(0, 7) != "172.16.")
                            {
                                return temparyip[i];    //�ҵ����������ĵ�ַ 
                            }
                        }
                    }
                    else if (IsIPAddress(result)) //������IP��ʽ 
                        return result;
                    else
                        result = null;    //�����е����� ��IP��ȡIP 
                }

            }

            string IpAddress = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != String.Empty) ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];


            if (null == result || result == String.Empty)
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (result == null || result == String.Empty)
                result = HttpContext.Current.Request.UserHostAddress;

            return result;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="str1"></param>
    /// <returns></returns>
    public static bool IsIPAddress(string str1)
    {
        if (str1 == null || str1 == string.Empty || str1.Length < 7 || str1.Length > 15) return false;

        string regformat = @"^\d{1,3}[\.]\d{1,3}[\.]\d{1,3}[\.]\d{1,3}$";

        Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
        return regex.IsMatch(str1);
    } 

    /// <summary>
    /// ��datatableת��Ϊjson  
    /// </summary>
    /// <param name="dtb">Dt</param>
    /// <returns>JSON�ַ���</returns>
    public static string Dtb2Json(DataTable dtb)
    {
        JavaScriptSerializer jss = new JavaScriptSerializer();
        System.Collections.ArrayList dic = new System.Collections.ArrayList();
        foreach (DataRow dr in dtb.Rows)
        {
            System.Collections.Generic.Dictionary<string, object> drow = new System.Collections.Generic.Dictionary<string, object>();
            foreach (DataColumn dc in dtb.Columns)
            {
                drow.Add(dc.ColumnName, dr[dc.ColumnName]);
            }
            dic.Add(drow);

        }
        //���л�  
        return jss.Serialize(dic);
    }
    /// <summary>  
    /// ��������ͼ  
    /// </summary>  
    /// <param name="originalImagePath">Դͼ·��������·����</param>  
    /// <param name="thumbnailPath">����ͼ·��������·����</param>  
    /// <param name="width">����ͼ���</param>  
    /// <param name="height">����ͼ�߶�</param>  
    /// <param name="mode">��������ͼ�ķ�ʽ"HW"ָ���߿�"W"ָ����"H"ָ����</param>  
    public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
    {
        System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

        int towidth = width;
        int toheight = height;

        int x = 0;
        int y = 0;
        int tox = 0;
        int toy = 0;
        int ow = originalImage.Width;
        int oh = originalImage.Height;

        if (towidth > ow)
        {
            width = ow;
            towidth = ow;
        }
        if (toheight > oh)
        {
            height = oh;
            toheight = oh;
        }

        switch (mode)
        {
            case "HW"://ָ���߿����ţ����ܱ��Σ�                  
                break;
            case "W"://ָ�����߰�����                      
                toheight = originalImage.Height * width / originalImage.Width;
                break;
            case "H"://ָ���ߣ�������  
                towidth = originalImage.Width * height / originalImage.Height;
                break;
            case "Cut"://ָ���߿�ü��������Σ�                  
                if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                {
                    oh = originalImage.Height;
                    ow = originalImage.Height * towidth / toheight;
                    y = 0;
                    x = (originalImage.Width - ow) / 2;
                }
                else
                {
                    ow = originalImage.Width;
                    oh = originalImage.Width * height / towidth;
                    x = 0;
                    y = (originalImage.Height - oh) / 2;
                }
                break;
            case "HWnochg":
                towidth = width;
                toheight = height;
                if (ow > oh)
                {
                    //toheight = originalImage.Height * width / originalImage.Width;
                    int tp = originalImage.Height * width / originalImage.Width;
                    toy = (toheight - tp) / 2;
                    //y = (toheight - oh) / 2;
                }
                else
                {
                    //towidth = originalImage.Width * height / originalImage.Height;
                    int tp = originalImage.Width * height / originalImage.Height;
                    tox = (towidth - tp) / 2;
                    //x = (towidth - ow) / 2;
                }
                break;
            default:
                break;
        }

        //�½�һ��bmpͼƬ  
        System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

        //�½�һ������  
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

        //���ø�������ֵ��  
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

        //���ø�����,���ٶȳ���ƽ���̶�  
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        //��ջ�������͸������ɫ���  
        g.Clear(System.Drawing.Color.Transparent);

        //��ָ��λ�ò��Ұ�ָ����С����ԭͼƬ��ָ������  
        g.DrawImage(originalImage, new System.Drawing.Rectangle(tox, toy, towidth, toheight),
            new System.Drawing.Rectangle(x, y, ow, oh),
            System.Drawing.GraphicsUnit.Pixel);

        try
        {
            //��jpg��ʽ��������ͼ  
            bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        catch (System.Exception e)
        {
            throw e;
        }
        finally
        {
            originalImage.Dispose();
            bitmap.Dispose();
            g.Dispose();
        }
    }


     /// <summary>  
    /// ��������ͼ ���ױ� 
    /// </summary>  
    /// <param name="originalImagePath">Դͼ·��������·����</param>  
    /// <param name="thumbnailPath">����ͼ·��������·����</param>  
    /// <param name="width">����ͼ���</param>  
    /// <param name="height">����ͼ�߶�</param>  
    
    public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height)
    {
        System.Drawing.Image oimage = System.Drawing.Image.FromFile(originalImagePath);

        string backcolor = "#FFFFFF";
        string borderColor = "#FFFFFF";
        int desWidth = width;
        int desHeight = height;
        int penwidth = 0;
        int penhight = 0;

        int owidth = oimage.Width;
        int oheight = oimage.Height;
        string hw = GetImageSize(owidth, oheight, desWidth, desHeight);
        string[] aryhw = hw.Split(';');
        int twidth = Convert.ToInt32(aryhw[0]);
        int theight = Convert.ToInt32(aryhw[1]);
        //�½�һ��bmpͼƬ                                          
        System.Drawing.Bitmap timage = new System.Drawing.Bitmap(desWidth, desHeight);
        //�½�һ������
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(timage);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.Clear(System.Drawing.ColorTranslator.FromHtml(backcolor));
        if (twidth < desWidth & theight == desHeight)
        {
            penwidth = desWidth - twidth;
        }
        else if (twidth == desWidth && theight < desHeight)
        {
            penhight = desHeight - theight;
        }
        else if (twidth < desWidth && theight < desHeight)
        {
            penwidth = desWidth - twidth;
            penhight = desHeight - theight;
        }
        int top = penhight / 2;
        int left = penwidth / 2;
        g.DrawImage(oimage, new System.Drawing.Rectangle(left, top, twidth, theight), new System.Drawing.Rectangle(0, 0, owidth, oheight), System.Drawing.GraphicsUnit.Pixel);
        System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(borderColor));
        g.DrawRectangle(pen, 0, 0, desWidth - 2, desHeight - 2);
        //string pathifile = Server.HtmlEncode(Request.PhysicalApplicationPath) + "image\\" +"t"+ DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg";
        //string pathifiles = Server.HtmlEncode(Request.PhysicalApplicationPath) + "image\\" + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".jpg";
        try
        {
            //ԭͼ����
            // oimage.Save(pathifile,System.Drawing.Imaging.ImageFormat.Jpeg);
            //��ͼͼ����
            timage.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            oimage.Dispose();
            g.Dispose();
            timage.Dispose();
        }
    }

    //��ȡ����ͼ�ĸ����
    public static string GetImageSize(int LoadImgW, int LoadImgH, int oldW, int oldH)
    {
        int xh = 0;
        int xw = 0;
        //���������
        //int oldW = 200;
        //int oldH = 300;
        //ͼƬ�ĸ߿�����������ͬ
        if (LoadImgH == oldH && LoadImgW == (oldW))
        {//1.������ʾ 
            xh = LoadImgH;
            xw = LoadImgW;
        }
        if (LoadImgH == oldH && LoadImgW > (oldW))
        {//2��ԭ��==�ݸߣ�ԭ��>�ݿ� ��ԭ��Ϊ���� 
            xw = (oldW);
            xh = LoadImgH * xw / LoadImgW;
        }
        if (LoadImgH == oldH && LoadImgW < (oldW))
        {//3��ԭ��==�ݸߣ�ԭ��<�ݿ�  ������ʾ    
            xw = LoadImgW;
            xh = LoadImgH;
        }
        if (LoadImgH > oldH && LoadImgW == (oldW))
        {//4��ԭ��>�ݸߣ�ԭ��==�ݿ� ��ԭ��Ϊ����    
            xh = oldH;
            xw = LoadImgW * xh / LoadImgH;
        }
        if (LoadImgH > oldH && LoadImgW > (oldW))
        {//5��ԭ��>�ݸߣ�ԭ��>�ݿ�            
            if ((LoadImgH / oldH) > (LoadImgW / (oldW)))
            {//ԭ�ߴ�Ķ࣬��ԭ��Ϊ���� 
                xh = oldH;
                xw = LoadImgW * xh / LoadImgH;
            }
            else
            {//��ԭ��Ϊ���� 
                xw = (oldW);
                xh = LoadImgH * xw / LoadImgW;
            }
        }
        if (LoadImgH > oldH && LoadImgW < (oldW))
        {//6��ԭ��>�ݸߣ�ԭ��<�ݿ� ��ԭ��Ϊ����         
            xh = oldH;
            xw = LoadImgW * xh / LoadImgH;
        }
        if (LoadImgH < oldH && LoadImgW == (oldW))
        {//7��ԭ��<�ݸߣ�ԭ��=�ݿ� ������ʾ        
            xh = LoadImgH;
            xw = LoadImgW;
        }
        if (LoadImgH < oldH && LoadImgW > (oldW))
        {//8��ԭ��<�ݸߣ�ԭ��>�ݿ� ��ԭ��Ϊ����     
            xw = (oldW);
            xh = LoadImgH * xw / LoadImgW;
        }
        if (LoadImgH < oldH && LoadImgW < (oldW))
        {//9��ԭ��<�ݸߣ�ԭ��<�ݿ�//������ʾ     
            xh = LoadImgH;
            xw = LoadImgW;
        }
        return xw + ";" + xh;
    }

    /// <summary>
    /// �ж��Ƿ�������
    /// </summary>
    /// <param name="str">�ַ���</param>
    /// <returns></returns>
    public static bool IsNumeric(string str)
    {
        if (str == null || str.Length == 0)
            return false;
        System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
        byte[] bytestr = ascii.GetBytes(str);
        foreach (byte c in bytestr)
        {
            if (c < 48 || c > 57)
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// ����ǰ̨��Ҫ��ֵ���ֶ����ƣ�json��ʽ
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string getDBfeildstr(DBfeild[] f)
    {
        string rlt = "[{";
        for (int i = 0; i < f.Length; i++)
        {
            if (f[i].isnogetval == "0")
                rlt += rlt == "[{" ? "'" + f[i].feild + "':'" + f[i].feildval + "'" : ",'" + f[i].feild + "':'" + f[i].feildval + "'";
        }
        rlt += "}]";
        return rlt;
    }
    /// <summary>
    /// ���ô洢���̱༭���ݿ�
    /// </summary>
    /// <param name="fd">�ֶ�����,�Ѿ���ֵ</param>
    /// <param name="SP">�洢��������</param>
    /// <returns></returns>
    public static string dbeditbysqlSP(DBfeild[] fd, string SP)
    {
        string rlt = "";
        if (fd.Length > 0)
        {
            SqlParameter[] pm = new SqlParameter[fd.Length];
            for (int i = 0; i < fd.Length; i++)
            {
                pm[i] = new SqlParameter("@" + fd[i].feild, fd[i].feildtype);
                pm[i].Value = fd[i].feildval;
            }
            //try
            //{
            SqlHelper.ExecuteScalar(CommandType.StoredProcedure, SP, pm);
            rlt = "ok";
            // }
            //catch
            //  {
            //      rlt = "err";
            //  }
        }
        else
        {
            //try
            //{
            SqlHelper.ExecuteScalar(CommandType.StoredProcedure, SP, null);
            rlt = "ok";
            // }
            //catch
            //  {
            //      rlt = "err";
            //  }
        }
        return rlt;
    }
    /// <summary>
    /// ����sql��䣬�༭���ݿ�
    /// </summary>
    /// <param name="fd">�ֶ�����,�Ѿ���ֵ</param>
    /// <param name="tablename">���ݿ�������</param>
    /// <param name="option">insert��update��delete</param>
    /// <returns></returns>
    public static string dbeditbysqltext(DBfeild[] fd, string tablename, string option)
    {
        string rlt = "";

        string insertstr = "insert into " + tablename;
        string inserttp1 = "";
        string inserttp2 = "";
        string updatestr = "update " + tablename + " set ";
        string updatetp = "";
        string deletestr = "delete from " + tablename;
        string keyfd = "";
        SqlParameter[] pm = new SqlParameter[fd.Length];
        for (int i = 0; i < fd.Length; i++)
        {
            pm[i] = new SqlParameter("@" + fd[i].feild, fd[i].feildtype);
            pm[i].Value = fd[i].feildval;

            if (fd[i].iskey == "1")
                keyfd += keyfd == "" ? fd[i].feild + "=@" + fd[i].feild : " and " + fd[i].feild + "=@" + fd[i].feild;
            else
            {
                inserttp1 += inserttp1 == "" ? fd[i].feild : "," + fd[i].feild;
                inserttp2 += inserttp2 == "" ? "@" + fd[i].feild : ",@" + fd[i].feild;
                updatetp += updatetp == "" ? fd[i].feild + "=@" + fd[i].feild : "," + fd[i].feild + "=@" + fd[i].feild;
            }


        }
        //sqlstr
        string sqlstr = "";

        if (option == "insert")
        {
            sqlstr = insertstr + "(" + inserttp1 + ") values(" + inserttp2 + ")";
        }
        if (option == "update")
        {
            sqlstr = updatestr + updatetp + " where " + keyfd;
        }
        if (option == "delete")
        {
            sqlstr = deletestr + " where " + keyfd;
        }

        //try
        //{
        SqlHelper.ExecuteScalar(CommandType.Text, sqlstr, pm);
        rlt = "ok";
        // }
        //catch
        //  {
        //      rlt = "err";
        //  }


        return rlt;
    }




    //Excel���ݿ�
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlstr">sql���</param>
    /// <param name="dbpath">excel�ļ�������·��</param>
    /// <returns></returns>
    public static DataTable OleDB_Excel_select(string sqlstr, string dbpath)
    {
        //string Excelconnstring = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbpath + @";Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
        string Excelconnstring = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + dbpath + ";Extended Properties='Excel 12.0; HDR=YES; IMEX=1'"; //�����ӿ��Բ���xls��.xlsx�ļ�
        string connstr = Excelconnstring;//@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\hunpai.mdb;Persist Security Info=True";


        System.Data.OleDb.OleDbConnection oledbcon = new System.Data.OleDb.OleDbConnection(connstr);
        string selstr = sqlstr;//"SELECT [id], [stuid], [cj1]+[cj2] as [cj], [sub], [ksnameid], [mcsch], [mccls] FROM [cj] WHERE ([ksnameid] = '" + ksid + "' and [sub]='" + sub + "') order by cj desc";

        System.Data.OleDb.OleDbDataAdapter oledbdptr = new System.Data.OleDb.OleDbDataAdapter(selstr, oledbcon);
        DataSet oledbdtst = new DataSet();
        oledbdptr.Fill(oledbdtst, "rslt");
        oledbcon.Close();

        return oledbdtst.Tables["rslt"];

    }

    /// <summary>
    /// ö������sql��������
    /// </summary>
    public  enum sqlStrType { select, insert, update, delete };

    private SqlHelper() { }

    //Database connection strings
    public static  string CONN_STRING = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;
    /// <summary>
    /// ���������ַ���
    /// </summary>
    /// <param name="ConnectionString">web.config�ж�������ݿ����ӱ�������</param>
    public static void SetConn_String(string ConnectionString)
    {
        CONN_STRING=ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;
    }
    /// <summary>
    /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the results</returns>
    public static SqlDataReader ExecuteReader(CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(CONN_STRING);

        // we use a try/catch here because if the method throws an exception we want to 
        // close the connection throw code, because no datareader will exist, hence the 
        // commandBehaviour.CloseConnection will not work
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //SqlDataAdapter apt = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            conn.Close();
            return rdr;
        }
        catch
        {
            conn.Close();
            throw;
        }
    }

    /// <summary>
    /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  DataSet r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>A SqlDataReader containing the results</returns>
    public static DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(CONN_STRING);

        // we use a try/catch here because if the method throws an exception we want to 
        // close the connection throw code, because no datareader will exist, hence the 
        // commandBehaviour.CloseConnection will not work
        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            SqlDataAdapter apt = new SqlDataAdapter(cmd);
            DataSet mydataset = new DataSet();
            apt.Fill(mydataset, "temptb");
            apt.Dispose();
            //SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            //SqlDataAdapter apt = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            conn.Close();
            return mydataset;
        }
        catch
        {
            conn.Close();
            throw;
        }
    }
    /// <summary>
    /// ���ز�ѯ���
    /// </summary>
    /// <param name="cmdtype">���ͣ��洢���̻���sql���</param>
    /// <param name="cmdText">�洢���̵����ֻ�sql���</param>
    
    /// <param name="fieldandVal">�ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <param name="tb">������</param>
    /// <param name="cond">����</param>
    /// <param name="condFieldandVal">�����е��ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <returns>���ز�ѯ�����dataset</returns>
    public static DataSet ExecuteDataSet(CommandType cmdtype,string cmdText,  fieldtoVal[] fieldandVal, string tb, string cond, fieldtoVal[] condFieldandVal)
    {
        SqlParameter[] param;
        string sqlstr=getSqlString( sqlStrType.select,fieldandVal, tb, cond, condFieldandVal,out param);
        if (cmdtype == CommandType.Text)
        {
            return ExecuteDataSet(cmdtype, sqlstr, param);
        }
        else
        {
            return ExecuteDataSet(cmdtype, cmdText, param);
        }
    }
    /// <summary>
    /// ���ز�ѯ���
    /// </summary>
    /// <param name="cmdtype">���ͣ��洢���̻���sql���</param>
    /// <param name="cmdText">�洢���̵����ֻ�sql���</param>
   
    /// <param name="fieldandVal">�ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <param name="tb">������</param>
    /// <param name="cond">����</param>
    /// <param name="condFieldandVal">�����е��ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <returns>���ز�ѯ�����sqlDataReader</returns>
    public static SqlDataReader ExecuteReader(CommandType cmdtype, string cmdText, fieldtoVal[] fieldandVal, string tb, string cond, fieldtoVal[] condFieldandVal)
    {
        SqlParameter[] param;
        string sqlstr = getSqlString(sqlStrType.select, fieldandVal, tb, cond, condFieldandVal, out param);
        if (cmdtype == CommandType.Text)
        {
            return ExecuteReader(cmdtype, sqlstr, param);
        }
        else
        {
            return ExecuteReader(cmdtype, cmdText, param);
        }
    }


    /// <summary>
    /// ִ��sql���
    /// </summary>
    /// <param name="cmdtype">���ͣ��洢���̻���sql���</param>
    /// <param name="cmdText">�洢���̵����ֻ�sql���</param>
   
    /// <param name="sqlStringType">sql��������insert��update��delete</param>
    /// <param name="fieldandVal">�ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <param name="tb">������</param>
    /// <param name="cond">����</param>
    /// <param name="condFieldandVal">�����е��ֶμ���Ӧ��ֵ��ֵ����</param>
    /// <returns>���ز�ѯ�����sqlDataReader</returns>
    public static object ExecuteScalar(CommandType cmdtype, string cmdText, sqlStrType sqlStringType, fieldtoVal[] fieldandVal, string tb, string cond, fieldtoVal[] condFieldandVal)
    {
        SqlParameter[] param;
        string sqlstr = getSqlString(sqlStringType, fieldandVal, tb, cond, condFieldandVal, out param);
        if (cmdtype == CommandType.Text)
        {
            return ExecuteScalar(cmdtype, sqlstr, param);
        }
        else
        {
            return ExecuteScalar(cmdtype, cmdText, param);
        }
    }
    /// <summary>
    /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
    public static object ExecuteScalar(CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {
        SqlCommand cmd = new SqlCommand();

        using (SqlConnection conn = new SqlConnection(CONN_STRING))
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
    }

    /// <summary>
    /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
    /// using the provided parameters.
    /// </summary>
    /// <remarks>
    /// e.g.:  
    ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
    /// </remarks>
    /// <param name="conn">an existing database connection</param>
    /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
    /// <param name="cmdText">the stored procedure name or T-SQL command</param>
    /// <param name="cmdParms">an array of SqlParamters used to execute the command</param>
    /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
    public static object ExecuteScalar(SqlConnection conn, CommandType cmdType, string cmdText, params SqlParameter[] cmdParms)
    {

        SqlCommand cmd = new SqlCommand();

        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        object val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
    }


    /// <summary>
    /// Prepare a command for execution
    /// </summary>
    /// <param name="cmd">SqlCommand object</param>
    /// <param name="conn">SqlConnection object</param>
    /// <param name="trans">SqlTransaction object</param>
    /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
    /// <param name="cmdText">Command text, e.g. Select * from Products</param>
    /// <param name="cmdParms">SqlParameters to use in the command</param>
    private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
    {

        if (conn.State != ConnectionState.Open)
            conn.Open();

        cmd.Connection = conn;
        cmd.CommandText = cmdText;

        if (trans != null)
            cmd.Transaction = trans;

        cmd.CommandType = cmdType;

        if (cmdParms != null)
        {
            foreach (SqlParameter parm in cmdParms)
                cmd.Parameters.Add(parm);
        }
    }

    

    /// <summary>
    /// �õ�sql���
    /// </summary>
    /// <param name="sqlStringType">����sql�������ͣ�ö��</param>
    /// <param name="fieldandVal">�ֶΣ��ֶζ�Ӧ��ֵ���ֶε����ͣ��ṹfieldtoVal</param>
    /// <param name="tb">�������</param>
    /// <param name="cond">sql��������</param>
    /// <param name="condFieldandVal">sql��������еı����б��ṹfieldtoVal</param>
    /// <param name="param">�����sqlparam����</param>
    /// <returns>����sql���</returns>
    public static string getSqlString(sqlStrType sqlStringType,fieldtoVal[] fieldandVal,string tb,string cond,fieldtoVal[] condFieldandVal,out SqlParameter[] param)
    {
        string sqlstr = "";
        int plength = 0;
        if (fieldandVal == null)
        { }
        else
        {
            plength = fieldandVal.Length;
        }
        if (condFieldandVal == null)
        {
        }
        else
        {
            plength = plength + condFieldandVal.Length;
        }
        param = new SqlParameter[plength];
        int i = 0;
        for (int j=0; j < fieldandVal.Length; j++)
        {
            param[i] = new SqlParameter("@" + fieldandVal[j].fieldName, fieldandVal[j].sqldbtype);
            param[i].Value = fieldandVal[j].fieldVal;
            i++;
        }
        if (condFieldandVal == null)
        {
        }
        else
        {
            for (int j = 0; j < condFieldandVal.Length; j++)
            {
                param[i] = new SqlParameter("@" + condFieldandVal[j].fieldName, condFieldandVal[j].sqldbtype);
                param[i].Value = condFieldandVal[j].fieldVal;
                i++;
            }
        }
        if (sqlStringType == sqlStrType.select)
        {
            sqlstr = " select ";
            for (int j = 0; j < fieldandVal.Length; j++)
            {
                if (j == 0)
                    sqlstr += fieldandVal[j].fieldName;
                else
                    sqlstr += " ," + fieldandVal[j].fieldName;
            }
            sqlstr += " from " + tb;
            sqlstr += " where "+cond;
            
        }

        if (sqlStringType == sqlStrType.insert)
        {
            sqlstr = " insert into "+tb+" (";
            for (int j = 0; j < fieldandVal.Length; j++)
            {
                if (j == 0)
                    sqlstr += fieldandVal[j].fieldName;
                else
                    sqlstr += "," + fieldandVal[j].fieldName;
            }
            sqlstr += ") values ( ";
            for (int j = 0; j < fieldandVal.Length; j++)
            {
                if (j == 0)
                    sqlstr += "@"+fieldandVal[j].fieldName;
                else
                    sqlstr += ",@"+fieldandVal[j].fieldName;
            }
            sqlstr += ")";
        }
           
        
        if (sqlStringType == sqlStrType.update)
            {
                sqlstr = " update "+tb+" set ";
                for (int j = 0; j < fieldandVal.Length; j++)
                {
                    if (j == 0)
                        sqlstr += fieldandVal[j].fieldName + "=@" + fieldandVal[j].fieldName;
                    else
                        sqlstr += "," + fieldandVal[j].fieldName + "=@" + fieldandVal[j].fieldName;
                }
                sqlstr += " where " + cond;
            }


            if (sqlStringType == sqlStrType.delete)
            {
                sqlstr="delete from "+tb+" where " +cond;
            }
        
        return sqlstr;
        
        
    }
}
/// <summary>
/// �ֶμ����Ӧ��ֵ
/// </summary>
public  struct fieldtoVal
{
    public  string fieldName;
    public  string fieldVal;
    public SqlDbType sqldbtype;
    
}

