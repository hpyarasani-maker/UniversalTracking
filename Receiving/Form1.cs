using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Receiving
{
    public partial class Form1 : Form
    {
        HTMLParserNewTask WOWS = new HTMLParserNewTask();
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        int noResult;

        public Form1()
        {
            InitializeComponent();
            noResult = 0;
        }

        void timerExit()
        {
            timer.Interval = 30 * 60000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Environment.Exit(Environment.ExitCode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Text = "Sogou_277_Universal_Receiving_1";//changes
            //this.Text = "Universal_Receiving_276_GT0_P";//changes
            this.Text = "Universal_Receiving_48_GT0";//changes



            Thread t = new Thread(new ThreadStart(StartProcess));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            noResult = 0;

        }

        string seid = "";
        string kw = "";
        ArrayList al = new ArrayList();
        string resultsurl = "";

        public void StartProcess()
        {
            Task<ArrayList> ta = null;
            while (true)
            {
                string myDate = DateTime.Today.ToString("yyyy-MM-dd");

                string kwQry = "exec [dbo].[GetOxyResultsApi] '" + myDate + "'";


                GetKeywords(kwQry);
                if (lstKWs.Items.Count <= 0)
                    Environment.Exit(Environment.ExitCode);
                string html = "";
                int count = 0;

                foreach (string s in lstKWs.Items)
                {
                    seid = s.Split('|')[0];
                    kw = s.Split('|')[1];
                    count++;

                    if (count <= 10)
                    {
                        resultsurl = s.Split('|')[2];
                        al.Add(resultsurl);
                        if (al.Count == 10)
                        {
                            html = WOWS.Gethtml(Convert.ToInt32(seid), kw, al);
                            //System.IO.File.WriteAllText(@"c:\inetpub\wwwroot\html\" + kw + ".html", html, Encoding.UTF8);
                            try
                            {

                                // ta = WOWS.getTop100YahooHKDesktopPattern(html);              // 38
                                //ta = WOWS.getTop100YahooJapanDesktopPattern(html);            // 48
                                //ta = WOWS.getTop100NaverDesktopPattern(html);                 // 138
                                //ta = WOWS.getTop100HaoSou360DesktopPattern(html);             // 175
                                //ta = WOWS.getTop100SogouDesktopPattern(html);                 // 276
                                //ta = WOWS.getTop100PriceSearcherPattern(html);                // 340 


                                //ta = WOWS.getTop100HaoSou360MobilePattern(html);               // 193        
                                //ta = WOWS.getTop100YahooJapanMobilePattern(html);              // 194
                                //ta = WOWS.getTop100YahooHKMobilePattern(html);                 // 256
                                //ta = WOWS.getTop100SogouMobilePattern(html);                   // 277
                                ta = WOWS.getTop100SmCnMobilePattern(html);                    // 278
                                //ta = WOWS.getTop100NaverMobilePattern(html);                   // 440 


                                if (ta.Result.Count >= 0)
                                {
                                    sendtoAPI(ta, seid, kw);
                                    count = 0;
                                    ta = null;
                                    al.Clear();
                                    html = "";
                                }
                            }
                            catch (Exception ex)
                            {
                                sendtoAPI(ta, seid, kw);
                                Invoke((MethodInvoker)delegate ()
                                {
                                    errorList.Items.Add("Error: " + ex.Message);
                                });
                            }
                        }
                    }
                }
                noResult = 0;
            }
        }
        private void GetKeywords(string qry)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                lstKWs.Items.Clear();
                //lstKWs.Items.Add("277:organic search");          
            });
            //return;

            try
            {

                using (SqlConnection con = new SqlConnection(Common.ReadConnection()))
                {
                    con.Open();
                    using (SqlCommand comm = new SqlCommand(qry, con))
                    {
                        comm.CommandTimeout = 0;
                        using (SqlDataReader dr = comm.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (dr.Read())
                            {
                                this.Invoke((MethodInvoker)delegate ()
                                {
                                    //lstKWs.Items.Add(dr[0].ToString());
                                    lstKWs.Items.Add(dr[0].ToString() + "|" + dr[1].ToString() + "|" + dr[2].ToString());
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    errorList.Text += ex.Message + "\r\n";
                });
            }
            finally { }
        }


        public void sendtoAPI(Task<ArrayList> ta, string seid, string kn)
        {
            const string path = @"C:\Inetpub\wwwroot\48_Universal_Receive_1_GT0.xml";//changes

            string myDate = DateTime.Today.ToString("yyyy-MM-dd");

            ArrayList seresults = ta.Result;
            if (seresults.Count < 1)
            {
                noResult++;
                results.Invoke((MethodInvoker)(delegate ()
                {
                    results.Text = "no results";
                    label1.Text = "completed " + noResult.ToString() + " of " + (lstKWs.Items.Count)/(10);

                    results.Refresh();
                }));

                /*XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);

                 writer.Formatting = System.Xml.Formatting.Indented;
                 writer.Indentation = 2;

                 writer.WriteStartDocument();

                 writer.WriteStartElement("", "searchResults", "");
                 writer.WriteStartElement("", "searchResult", "");
                 writer.WriteStartAttribute("searchEngineId");
                 writer.WriteString(seid);
                 writer.WriteStartAttribute("keyword");
                 writer.WriteString(kn);
                 writer.WriteStartAttribute("date");
                 writer.WriteString(myDate);

                 writer.WriteEndElement();

                 writer.WriteEndDocument();

                 writer.Close();

                 sendDatatoURL(path);
                 InsertDashBoardData(myDate, kn, seid, string.Empty);*/

            }
            else if (seresults[0].ToString().Contains("e100") && seresults[0].ToString().Trim().StartsWith("e100"))
            {
                //int a1 = lstKWs.Items.Count/10;
                noResult++;
                results.Invoke((MethodInvoker)(delegate ()
                {
                    results.Text = "e100: no results";
                    label1.Text = "completed " + noResult.ToString() + " of " + (lstKWs.Items.Count)/(10);
                    //label1.Text = "completed " + noResult.ToString() + " of " + a1;
                    results.Refresh();
                }));


            }
            else
            {
                noResult++;

                results.Invoke((MethodInvoker)(delegate ()
                {
                    lblCount.Text = "No. of Urls : " + seresults.Count;
                    label1.Text = "completed " + noResult.ToString() + " of " + (lstKWs.Items.Count)/(10);
                    results.Text = seid + " " + kn;
                }));


                XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8);

                writer.Formatting = System.Xml.Formatting.Indented;
                writer.Indentation = 2;
                writer.WriteStartDocument();
                writer.WriteStartElement("", "searchResults", "");
                writer.WriteStartElement("", "searchResult", "");
                writer.WriteStartAttribute("searchEngineId", null);
                writer.WriteString(seid);
                writer.WriteStartAttribute("keyword", null);
                writer.WriteString(kn);
                writer.WriteStartAttribute("date", null);
                //writer.WriteString(myDate);
                writer.WriteString(myDate);
                string c = string.Empty;
                int k;
                string qry = "";

                for (int i = 0; i < seresults.Count; i++)
                {

                    if (seresults[i].ToString().StartsWith("http") || seresults[i].ToString().StartsWith("https"))
                    {
                        k = (i + 1);
                        c = k.ToString();
                        if (seresults[i].ToString().Contains("e100") && seresults[i].ToString().Trim().StartsWith("e100"))
                            continue;
                        writer.WriteStartElement("", "url", "");
                        writer.WriteStartAttribute("position");
                        writer.WriteString(c);
                        writer.WriteEndAttribute();

                        string dURL = seresults[i].ToString();

                        //qry += "insert into [dashboard_data4_ResultApi](date,name,seid,rank,url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        //qry += "insert into dashboard_japan(date, name, seid, position, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        qry += "insert into dashboard_data4(date, name, seid, rank, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        //qry += "insert into dashboard_Yandex(date, name, seid, position, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        //qry += "insert into dashboard_baidu(date, name, seid, position, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        //qry += "insert into dashboard_Naver(date, name, seid, position, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        writer.WriteString(dURL);
                        writer.WriteEndElement();
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(writer);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
                if (myDate != string.Empty)
                {
                    if (seresults.Count > 50)
                    {
                        sendDatatoURL(path);
                        string strInsert = "insert into [dashboard_data](date,name,seid,url,count)values(Convert(varchar(10),'" + myDate + "',103),N'" + kn.Replace("'", "''") + "'," + seid + ",N'" + seresults[0].ToString().Replace("'", "''") + "','" + seresults.Count.ToString() + "')";
                        SqlConnection objCon = null;
                        try
                        {
                            objCon = new SqlConnection(Common.ReadConnection());
                            objCon.Open();

                            SqlCommand objCmd = new SqlCommand();
                            objCmd.Connection = objCon;
                            objCmd.CommandTimeout = 0;

                            objCmd.CommandText = strInsert;
                            objCmd.ExecuteNonQuery();

                            objCmd.CommandText = qry;
                            objCmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            string errMsg = "Database Connection is temporarily not working\n" + ex.ToString();
                            //MessageBox.Show(errMsg);
                            errorList.Invoke((MethodInvoker)(delegate ()
                            {
                                errorList.Items.Add(errMsg);
                            }));
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.ToString());
                            errorList.Invoke((MethodInvoker)(delegate ()
                            {
                                errorList.Items.Add(ex.ToString());
                            }));
                        }
                        finally
                        {
                            objCon.Dispose();
                            objCon.Close();
                        }
                    }
                }
            }
        }

        void sendDatatoURL(string xmlPath)
        {
            string submitURL = Common.readAPI();

            string user = "pisoftware";
            string pwd = "r00t123456";
            try
            {
                Uri uri = new Uri(submitURL);
                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(uri);
                httpWReq.UseDefaultCredentials = true;
                httpWReq.PreAuthenticate = true;
                httpWReq.Credentials = CredentialCache.DefaultCredentials;

                Encoding encoding = new UTF8Encoding();
                string postData = GetTextFromXMLFile(xmlPath);
                byte[] data = encoding.GetBytes(postData);

                httpWReq.ProtocolVersion = HttpVersion.Version11;
                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/x-www-form-urlencoded"; //charset=UTF-8";  


                string auth = string.Format("{0}:{1}", user, pwd);
                string enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
                string cred = string.Format("{0} {1}", "Basic", enc);


                httpWReq.Headers[HttpRequestHeader.Authorization] = cred;
                httpWReq.ContentLength = data.Length;

                Stream stream = httpWReq.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                string s = response.ToString();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                //System.Threading.Thread.Sleep(2000);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    reader.Close();
                    response.Close();
                    throw new Exception(response.StatusCode + ": " + response.StatusDescription);
                }
                String xmlResponse = "";
                String temp = null;
                while ((temp = reader.ReadLine()) != null)
                {
                    xmlResponse += temp;
                }
                //MessageBox.Show(xmlResponse);
                reader.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                string error = "";
                string message = "";

                using (WebResponse response = ex.Response)
                {
                    if (response != null)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        error = string.Format("Error:{0}", httpResponse.StatusCode);

                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            message = reader.ReadToEnd();
                        }
                    }
                }

                errorList.Invoke((MethodInvoker)(delegate ()
                {
                    errorList.Items.Add(ex.ToString());
                }));

                //throw new Exception(ex.Message);

            }
            catch (Exception ex)
            {

                errorList.Invoke((MethodInvoker)(delegate ()
                {
                    errorList.Items.Add(ex.ToString());
                }));
            }
        }

        private string GetTextFromXMLFile(string file)
        {
            StreamReader reader = new StreamReader(file);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        void InsertDashBoardData(string ddate, string kwd, string seid, string url)
        {
            string strInsert = "insert into dashboard_data(date,name,seid,url)values(Convert(varchar(10),'" + ddate + "',103),N'" + kwd.Replace("'", "''") + "'," + seid + ",N'" + url.Replace("'", "''") + "')";
            SqlConnection objCon = null;
            SqlDataReader objData = null;
            try
            {
                objCon = new SqlConnection(Common.strConn());
                objCon.Open();
                SqlCommand objCmd = new SqlCommand(strInsert, objCon);
                objCmd.CommandTimeout = 0;
                objData = objCmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (SqlException e)
            {
                string errMsg = "Database Connection is temporarily not working\n" + e.ToString();
                //MessageBox.Show(errMsg);
                errorList.Invoke((MethodInvoker)(delegate ()
                {
                    errorList.Items.Add(errMsg);
                }));
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                errorList.Invoke((MethodInvoker)(delegate ()
                {
                    errorList.Items.Add(ex.ToString());
                }));
            }
            finally
            {
                objCon.Dispose();
                objCon.Close();
            }
        }

    }
}
