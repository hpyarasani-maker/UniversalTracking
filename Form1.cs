using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace UniversalTracking
{
    public partial class Form1 : Form
    {        
        
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private int noResult;
        //YahooClass WOWS = new YahooClass();
        //Baidu WOWS = new Baidu();
        //Sogou WOWS = new Sogou();
        //SmCnMobile_278 WOWS = new SmCnMobile_278();
        //YahooJapan WOWS = new YahooJapan();
        //YahooHK WOWS = new YahooHK();
        HaoSou360 WOWS = new HaoSou360();
        //SmCnMobile_278 WOWS = new SmCnMobile_278();
        //Sogou WOWS = new Sogou();
        //Naver WOWS = new Naver();
        //PriceSearcher WOWS = new PriceSearcher();

        //int count; 

        public Form1()
        {
            InitializeComponent();
            //count = 0;   // Common.GetOxylabsCount();          
            timerExit();
        }
        void timerExit()
        {
            timer.Interval = 720 * 60000;
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
            this.Text = "YahooHK_256_6_WC_70_Universal";//changes
            Thread t = new Thread(new ThreadStart (StartProcess));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            noResult = 0;
            
        }

        public void  StartProcess()
        {
            Task<ArrayList> ta = null;
            while (true)
            {
                string myDate = DateTime.Today.ToString("yyyy-MM-dd");             
                string kwQry = "[GetKeywords_48_D] '" + myDate + "'";//changes                
                GetKeywords(kwQry);
                if (lstKWs.Items.Count <= 0)
                    Environment.Exit(Environment.ExitCode);

                foreach (string s in lstKWs.Items)
                {
                    string seid = s.Split(':')[0];
                    string kw = s.Split(':')[1];
                    //ta = WOWS.getTop100YahooHK(seid, kw);
                    //ta = WOWS.getTop100YahooJapan(seid, kw);
                    //ta = WOWS.GetTop100Sogou(seid, kw);
                    //ta = WOWS.getTop100SmCnMobile(seid, kw);
                    ta = WOWS.GetTop100HaoSou360(seid, kw);
                    //ta = WOWS.GetTop100SmCnMobile(seid,kw);
                    //ta = WOWS.GetTop100Sogou(seid, kw);z
                    //ta = WOWS.GetBaidu(seid, kw);
                    //ta = WOWS.GetTop100Naver(seid, kw);
                    //ta = WOWS.GetTop100PriceSearcher(seid, kw);

                    if (ta.Result.Count >= 0)
                    {
                        sendtoAPI(ta, seid, kw);
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
                //lstKWs.Items.Add("138:2금융권대출");
                //lstKWs.Items.Add("175:seo multiple domains");
                //lstKWs.Items.Add("175:macbook pro");
            });
           // return;

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
                                    lstKWs.Items.Add(dr[0].ToString() + ":" + dr[1].ToString());
                                });
                            }
                        }
                    }
                }
                //this.Invoke((MethodInvoker)delegate ()
                //{
                //    lstKWs.Refresh();
                //});
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

        private string GetTextFromXMLFile(string file)
        {
            StreamReader reader = new StreamReader(file);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        public string ReadAPI()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                string fileName = @"C:\Inetpub\wwwroot\Callback_TrackingTrending.xml";

                // You'll need to put the correct path to your xml file here
                xml.Load(fileName);

                // Select a specific node
                XmlNode node = xml.SelectSingleNode("ConnectionString/apiSubmit");

                // Get its value
                string name = node.InnerText;

                return name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        private void SendToDB(string seid, string keyword, string xml, string jobid, int urlcount)
        {
            try
            {
                string myDate = DateTime.Today.ToString("yyyy-MM-dd");
                using (SqlConnection con = new SqlConnection(Common.ReadConnection()))
                {
                    con.Open();

                    using (SqlCommand comm = con.CreateCommand())
                    {
                        comm.CommandTimeout = 0;
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.CommandText = "Insert_dashboard_data";
                        comm.Parameters.Add("Date", SqlDbType.DateTime).Value = myDate;
                        comm.Parameters.Add("Name", SqlDbType.NVarChar).Value = keyword; //.Replace("'", "''");
                        comm.Parameters.Add("Seid", SqlDbType.Int).Value = seid;
                        comm.Parameters.Add("JobId", SqlDbType.NVarChar).Value = jobid;
                        comm.Parameters.Add("Count", SqlDbType.Int).Value = urlcount;
                        comm.Parameters.Add("XmlData", SqlDbType.Xml).Value = xml.Replace("'", "''");

                        comm.ExecuteNonQuery();
                    }
                }

            }
            catch (SqlException ex)
            {
                string errorMessage = "Database Error: \r\n";
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessage += "Index #" + i + "\n" +
                                     "Message: " + ex.Errors[i].Message + "\n" +
                                     "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                     "Source: " + ex.Errors[i].Source + "\n" +
                                     "Procedure: " + ex.Errors[i].Procedure + "\n" +
                                     "Server: " + ex.Errors[i].Server + "\n";
                }

                throw new Exception(errorMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void sendtoAPI(Task<ArrayList> ta,string seid, string kn)
        {          

            const string path = @"C:\Inetpub\wwwroot\data_256_6_GT70_Universal.xml";//changes 
            
            string myDate = DateTime.Today.ToString("yyyy-MM-dd");
            ArrayList seresults = ta.Result;                     
            if (seresults.Count < 1)
            {
                noResult++;
                results.Invoke((MethodInvoker)(delegate ()
                {
                    results.Text = "no results";
                    label1.Text = "completed " + noResult.ToString() + " of " + lstKWs.Items.Count;
                    results.Refresh();
                }));
                

            }
            else if (seresults[0].ToString().Contains("e100") && seresults[0].ToString().Trim().StartsWith("e100"))
            {
                noResult++;
                results.Invoke((MethodInvoker)(delegate ()
                {
                    results.Text = "e100: no results";
                     label1.Text = "completed " + noResult.ToString() + " of " + lstKWs.Items.Count;
                    results.Refresh();
                }));

               
            }
            else
            {
                noResult++;
                
                results.Invoke((MethodInvoker)(delegate ()
                {
                    lblCount.Text = "No. of Urls : " + seresults.Count;
                    label1.Text = "completed "+ noResult.ToString()+" of " + lstKWs.Items.Count;
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
                        qry += "insert into dashboard_japan(date, name, seid, position, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
                        //qry += "insert into dashboard_data4(date, name, seid, rank, url) values(Convert(varchar(10), '" + myDate + "',103), N'" + kn.Replace("'", "''") + "', " + seid + ", " + k + ", N'" + dURL.ToString().Replace("'", "''") + "'); ";
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
                        //sendDatatoURL(path);
                        string strInsert = "insert into dashboard_data(date,name,seid,url,count)values(Convert(varchar(10),'" + myDate + "',103),N'" + kn.Replace("'", "''") + "'," + seid + ",N'" + seresults[0].ToString().Replace("'", "''") + "','" + seresults.Count.ToString() + "')";
                        SqlConnection objCon = null;
                        try
                        {
                            objCon = new SqlConnection(strConn());
                            objCon.Open();

                            SqlCommand objCmd = new SqlCommand();
                            objCmd.Connection = objCon;
                            objCmd.CommandTimeout = 0;

                            objCmd.CommandText = strInsert;
                            objCmd.ExecuteNonQuery();

                            objCmd.CommandText = qry;
                            objCmd.ExecuteNonQuery();
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
        }

        void sendDatatoURL(string xmlPath)
        {
            string submitURL = readAPI();

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

        public string readAPI()
        {
            XmlDocument xml = new XmlDocument();
            string fileName = @"C:\Inetpub\wwwroot\ServerIP1.xml";
            //string fileName = @"C:\Inetpub\wwwroot\ServerIP_Callback.xml";
            // You'll need to put the correct path to your xml file here
            xml.Load(fileName);
            // Select a specific node
            XmlNode node = xml.SelectSingleNode("ConnectionString/apiNew");
            // Get its value
            string name = node.InnerText;
            return name;
        }
        public string strConn()
        {
            XmlDocument xml = new XmlDocument();
            string fileName = @"C:\Inetpub\wwwroot\ServerIP1.xml";
            // You'll need to put the correct path to your xml file here
            xml.Load(fileName);

            // Select a specific node
            XmlNode node = xml.SelectSingleNode("ConnectionString/con");
            // Get its value
            string name = node.InnerText;
            return name;
        }
    }
}
