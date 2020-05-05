using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Threading;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Receiving
{
    public class HTMLParserNewTask
    {
        string myDate = DateTime.Today.ToString("yyyy-MM-dd");
        public event KeywordDone OnKeywordDone;
        public delegate void KeywordDone(string value);
        //public HTMLParserNewTask()
        //{
        //    Thread t1 = new Thread(new ThreadStart(StartProcess))
        //    {
        //        Name = "YahooHK_Desktop"
        //    };
        //    t1.Start();
        //}

        

        public string Gethtml(int seid, string kw, ArrayList al)
        {
            string response = "";
            string html = "";
            string status = "done";            
            string resultsurl = "";
            string jobid = "";
            for (int i = 0; i < al.Count; i++)
            {
                jobid = al[i].ToString().Replace("http://data.oxylabs.io/v1/queries/", "").Replace("/results", "");
                resultsurl = al[i].ToString();
                

                try
            {
                string username = "gpidatametrics";
                string password = "sdV5X3fcX6";

                if (status == "done")
                {
                    string resURL = resultsurl;
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(resURL);
                    string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));
                    httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
                    HttpWebResponse res = (HttpWebResponse)httpWebRequest.GetResponse();
                    Stream resStream = res.GetResponseStream();
                    StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                    response = reader.ReadToEnd();
                    resStream.Close();
                    res.Close();
                    ArrayList arRes = new ArrayList();
                    try
                    {
                        
                        JObject obj = JObject.Parse(response);
                        var cont = obj["results"];
                        foreach (JObject jo in cont)
                        {
                            ArrayList addURLs = new ArrayList();
                            html+= jo["content"].Value<string>();
                            //System.IO.File.WriteAllText(@"c:\inetpub\wwwroot\html\" + kw + ".html", response, Encoding.UTF8);

                        }
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                    
                }
            }
                
            catch (Exception ex)
            {
                Console.WriteLine("Result Request: " + ex.Message);
                if (!string.IsNullOrEmpty(jobid))
                {
                    try
                    {
                        ProcessError(kw, seid, jobid);
                    }
                    finally { }
                }
                OnKeywordDone.Invoke("Error:  seid: " + seid + ",  keyword: " + kw + ",  jobid: " + jobid + "\r\n\t" + ex.Message);
            }
            }
            return html;
        }

        private void ProcessError(string kw, int seid, string jobid)
        {
            string qry = "insert into dashboard_dataerrors (date, name, seid, jobid) values(Convert(varchar(10),'" + myDate + "',103), N'" +
                kw.Replace("'", "''") + "', " + seid + ", '" + jobid + "' )";

            using (SqlConnection con = new SqlConnection(strConn()))
            {
                try
                {
                    con.Open();
                    using (SqlCommand comm = new SqlCommand(qry, con))
                    {
                        comm.CommandTimeout = 0;
                        comm.ExecuteNonQuery();
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
        }

        public string strConn()
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                string fileName = @"C:\Inetpub\wwwroot\Callback_TrackingTrending.xml";
                // You'll need to put the correct path to your xml file here
                xml.Load(fileName);

                // Select a specific node
                XmlNode node = xml.SelectSingleNode("ConnectionString/con");
                // Get its value
                string name = node.InnerText;

                return name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void ProcessResults(ArrayList alRes, string kw, int seid, string jobid)
        {
            string qry = "";

            if (alRes.Count < 1)
            {
                Console.WriteLine("There is no result for " + kw);
                throw new Exception("No Results");
            }
            else
            {
                Console.WriteLine("Processing the keyword: " + kw);

                string tname = Thread.CurrentThread.Name;

                string path = @"C:\Inetpub\wwwroot\oxycallback" + tname + ".xml";

                string result = string.Empty;
                try
                {
                    MemoryStream stream = new MemoryStream();
                    using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                    {
                        writer.Formatting = System.Xml.Formatting.Indented;
                        writer.Indentation = 2;
                        writer.WriteStartDocument();

                        writer.WriteStartElement("", "searchResults", "");

                        writer.WriteStartElement("", "searchResult", "");
                        writer.WriteStartAttribute("searchEngineId");
                        writer.WriteString(seid.ToString());
                        writer.WriteStartAttribute("keyword");
                        writer.WriteString(kw);
                        writer.WriteStartAttribute("date");
                        writer.WriteString(myDate);
                        string c = string.Empty;
                        int k;

                        for (int i = 0; i < alRes.Count; i++)
                        {
                            k = i + 1;
                            c = k.ToString();
                            if (alRes[i].ToString().Contains("e100") && alRes[i].ToString().Trim().StartsWith("e100"))
                                continue;
                            writer.WriteStartElement("", "url", "");
                            writer.WriteStartAttribute("position");
                            writer.WriteString(c);
                            writer.WriteEndAttribute();
                            string dURL = alRes[i].ToString();
                            writer.WriteString(dURL);
                            writer.WriteEndElement();
                            //Insert100DashBoardData(myDate, kw, seid.ToString(),k, alRes[i].ToString());
                            qry += "insert into dashboard_data4(date,name,seid,position,url)values(Convert(varchar(10),'" + myDate + "',103),N'" + kw.Replace("'", "''") + "'," + seid + ",'" + c + "',N'" + dURL.ToString().Replace("'", "''") + "')";
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndDocument();

                        writer.Flush();
                        writer.Flush();
                        //writer.Close();
                        Encoding utf = Encoding.UTF8;
                        result = utf.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                        stream.Close();
                    }

                    if (!string.IsNullOrEmpty(result))
                    {
                        StreamWriter sw = new StreamWriter(path, false);
                        sw.Write(result);
                        sw.Close();
                    }
                    if (!string.IsNullOrEmpty(result))
                    {
                        try
                        {
                            if (alRes.Count > 50)
                            {
                                SendXmlToAPI(path);
                                Insert100DashBoardData(qry); 
                                InsertDashBoardData(kw, seid, alRes.Count, alRes[0].ToString(), jobid);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void SendXmlToAPI(string path)
        {
            string submitURL = readAPI();

            //string user = "pi-tracking";
            //string pwd = "ipseo2001";

            string user = "pisoftware";
            string pwd = "r00t123456";

            try
            {
                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(submitURL);
                //httpWReq = (HttpWebRequest)WebRequest.Create(submitURL);
                httpWReq.UseDefaultCredentials = true;
                httpWReq.PreAuthenticate = true;
                httpWReq.Credentials = CredentialCache.DefaultCredentials;

                Encoding encoding = new UTF8Encoding();
                string postData = GetTextFromXMLFile(path);
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

                String xmlResponse = "";
                String temp = null;
                while ((temp = reader.ReadLine()) != null)
                {
                    xmlResponse += temp;
                }
                reader.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                string errorMsg = string.Empty;
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMsg = string.Format("API Error: StatusCode {0}", httpResponse.StatusCode);

                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        errorMsg += "\r\n" + reader.ReadToEnd();
                    }
                }
                throw new Exception(errorMsg);
            }
        }

        private string GetTextFromXMLFile(string file)
        {
            StreamReader reader = new StreamReader(file);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        public string readAPI()
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

        void Insert100DashBoardData(string qry)
        {
            //string strInsert = "insert into dashboard_yandex(date,name,seid,position,url)values(Convert(varchar(10),'" + ddate + "',103),N'" + kwd.Replace("'", "''") + "'," + seid + ",'"+ position +"',N'" + url.Replace("'", "''") + "')";

            try
            {
                using (SqlConnection con = new SqlConnection(strConn()))
                {
                    con.Open();
                    using (SqlCommand comm = new SqlCommand(qry, con))
                    {
                        comm.CommandTimeout = 0;
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

        private void InsertDashBoardData(string kw, int seid, int cnt, string alRes, string jobid)
        {
            string qry = "insert into dashboard_data(date, name, seid, jobid, count, url) " +
                    "values(Convert(varchar(10),'" + myDate + "',103),N'" + kw.Replace("'", "''") + "', " + seid + ", '" + jobid + "', " + cnt + ", N'" + alRes.Replace("'", "''") + "')";

            try
            {
                using (SqlConnection con = new SqlConnection(strConn()))
                {
                    con.Open();
                    using (SqlCommand comm = new SqlCommand(qry, con))
                    {
                        comm.CommandTimeout = 0;
                        comm.ExecuteNonQuery();



                        //if (lt20)
                        //{
                        //    string url = alRes.Count > 0 ? alRes[0].ToString() : "";
                        //    qry = "exec [InsertLessthan20] '" + myDate + "',N'" + kw.Replace("'", "''") + "'," + seid + ",N'" + url.Replace("'", "''") + "'," + alRes.Count + ",'" + jobid + "'";
                        //    comm.CommandText = qry;
                        //    comm.CommandType = CommandType.Text;
                        //    comm.ExecuteNonQuery();
                        //}
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

        public async Task<ArrayList> getTop100YahooHKDesktopPattern(string html)
        {
            ArrayList al = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='compTitle options-toggle']/h3");

            foreach (var links in hn)
            {
                try
                {
                    HtmlNode a = links.SelectSingleNode(".//a");
                    string urls = a.Attributes["href"].Value;
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        if (urls.Contains("/RK=2"))
                        {


                            indx = urls.LastIndexOf("/RK=2");
                            int length = urls.Length;
                            urls = urls.Remove(indx);
                        }
                        //urls = urls.Remove(indx, length);
                        al.Add(HttpUtility.HtmlDecode(urls));

                    }
                }
                catch { continue; }

            }
            return await Task.FromResult<ArrayList>(al);
        }

        public async Task<ArrayList> getTop100YahooHKMobilePattern(string html)
        {
            ArrayList al = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='compTitle options-toggle']/h3");

            foreach (var links in hn)
            {
                try
                {
                    HtmlNode a = links.SelectSingleNode(".//a");
                    string urls = a.Attributes["href"].Value;
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        if (urls.Contains("/RK=2"))
                        {


                            indx = urls.LastIndexOf("/RK=2");
                            int length = urls.Length;
                            urls = urls.Remove(indx);
                        }
                        //urls = urls.Remove(indx, length);
                        al.Add(HttpUtility.HtmlDecode(urls));

                    }
                }
                catch { continue; }

            }
            return await Task.FromResult<ArrayList>(al);
        }

        public async Task<ArrayList> getTop100YahooJapanDesktopPattern(string html)
        {
            ArrayList yahoojapan = new ArrayList();
            ArrayList aldup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='sw-Card__section sw-Card__section--header']/div/div/a[1]");

            try
            {
                foreach (var links in hn)
                {
                    string urls = links.Attributes[0].Value;
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);

                        //urls = urls.Remove(indx, length);
                        //al.Add(HttpUtility.HtmlDecode(urls));
                        aldup.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
                foreach (string s in aldup)
                {
                    string s1 = s;
                    //s1 = s1.Remove(s1.Length - 1);
                    if (yahoojapan.Contains(s1) || string.IsNullOrEmpty(s1)) continue;
                    if (s1.Contains("search.yahoo.co.jp") || s1.Contains("news.yahoo.co.jp") || s1.Contains("topics.shopping") || s1.Contains("paypayfleamarket.yahoo.co.jp") || s1.Contains("app.adjust.com")) continue;
                    yahoojapan.Add(s1);
                }
                if (yahoojapan.Count > 100)
                {
                    yahoojapan.RemoveRange(100, yahoojapan.Count - 100);
                }
            }
            catch (Exception ex) { throw ex; }


            return await Task.FromResult<ArrayList>(yahoojapan);
        }
        public async Task<ArrayList> getTop100YahooJapanMobilePattern(string html)
        {
            ArrayList yahoojapan = new ArrayList();
            ArrayList aldup = new ArrayList();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);//sw-Card__section
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='sw-Card__section']/a");

            try
            {
                foreach (var links in hn)
                {
                    //string urls = links.Attributes[0].Value;
                    string urls = links.Attributes["href"].Value;
                    urls = HttpUtility.UrlDecode(urls);

                    ///amp/s/amp.olhardigital.com.br/dicas_e_tutoriais/noticia/como-ativar-o-modo-escuro-do-iphone/90652%3Fusqp%3Dmq331AQQKAGYAb37k5Gc1bbAKbABIA%253D%253D
                    if (urls.StartsWith("http") || urls.StartsWith("https") || urls.StartsWith("/amp/s/"))
                    {
                        if (urls.StartsWith("/amp/s/"))
                        {
                            urls = urls.Replace("/amp/s/", "https://");
                        }

                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        //urls = urls.Remove(indx, length);
                        //al.Add(HttpUtility.HtmlDecode(urls));
                        aldup.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
                foreach (string s in aldup)
                {
                    string s1 = s;
                    //s1 = s1.Remove(s1.Length - 1);
                    if (yahoojapan.Contains(s1) || string.IsNullOrEmpty(s1)) continue;
                    if (s1.Contains("search.yahoo.co.jp") || s1.Contains("news.yahoo.co.jp") || s1.Contains("topics.shopping") || s1.Contains("paypayfleamarket.yahoo.co.jp") || s1.Contains("app.adjust.com")) continue;
                    yahoojapan.Add(s1);
                }
                if (yahoojapan.Count > 100)
                {
                    yahoojapan.RemoveRange(100, yahoojapan.Count - 100);
                }

            }
            catch (Exception ex) { }

            return await Task.FromResult<ArrayList>(yahoojapan);
        }

        public async Task<ArrayList> getTop100SogouDesktopPattern(string html)
        {
            ArrayList sogouDesktop = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='fb']/a");

            foreach (var links in hn)
            {
                try
                {
                    //HtmlNode a = links.SelectSingleNode(".//a");
                    string urls = links.Attributes["href"].Value;
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        if (urls.Contains("http://snapshot.sogoucdn.com/websnapshot"))
                        {
                            Regex r = new Regex("&url=(.*)&did=", RegexOptions.IgnoreCase);
                            Match m1 = r.Match(urls);
                            if (m1.Success)
                                urls = HttpUtility.UrlDecode(m1.Groups[1].Value);
                        }
                        if (urls.LastIndexOf("http://") > 0)
                        {
                            urls = urls.Remove(0, urls.LastIndexOf("http://"));
                        }
                        else if (urls.LastIndexOf("https://") > 0)
                        {
                            urls = urls.Remove(0, urls.LastIndexOf("https://"));
                        }

                        if (!urls.Contains("www.sogou.com"))
                            sogouDesktop.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
                catch { continue; }
            }
            return await Task.FromResult<ArrayList>(sogouDesktop); ;
        }

        public async Task<ArrayList> getTop100SogouMobilePattern(string html)
        {
            ArrayList sogouMobile = new ArrayList();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//a[@class='resultLink  ']|.//a[@class='resultLink']|.//a[@class='resultLink ellipsis']|.//a[@class='resultLink clamp2']");

            Regex r1 = null;
            Match m2 = null;

            foreach (var links in hn)
            {
                try
                {

                    string urls = links.Attributes["href"].Value;
                    urls = HttpUtility.UrlDecode(urls);

                    if (urls.Contains("&url") || urls.Contains("&url=") || urls.Contains("url="))
                    {

                        Regex r = new Regex("url=(.*)&amp;dp=1&amp;", RegexOptions.IgnoreCase);
                        if (r == null)
                        {
                            r1 = new Regex("url=(.*)&vrid=", RegexOptions.IgnoreCase);//&amp;vrid=   &vrid= &amp;vrid=
                            m2 = r1.Match(urls);
                            if (m2.Success)
                                urls = HttpUtility.UrlDecode(m2.Groups[1].Value);
                        }
                        if (r != null)
                        {
                            Match m1 = r.Match(urls);
                            if (m1.Success)
                                urls = HttpUtility.UrlDecode(m1.Groups[1].Value);
                        }
                    }
                    if (urls.LastIndexOf("http://") > 0)
                    {
                        urls = urls.Remove(0, urls.LastIndexOf("http://"));
                    }
                    else if (urls.LastIndexOf("https://") > 0)
                    {
                        urls = urls.Remove(0, urls.LastIndexOf("https://"));
                    }
                    if (!urls.Contains("tv.sogou.com"))

                        if (urls.Contains("&vrid=") && urls.Contains("&wml"))
                        {
                            int index = urls.IndexOf("&vrid");
                            urls = urls.Remove(index);
                        }
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        if (urls.Contains("&amp;vrid") && urls.Contains("&amp;wml"))
                        {
                            int index = urls.IndexOf("&amp;vrid");
                            urls = urls.Remove(index);
                        }
                        sogouMobile.Add(HttpUtility.HtmlDecode(urls));
                    }

                }
                catch { continue; }
            }
            return await Task.FromResult<ArrayList>(sogouMobile);
        }

        public async Task<ArrayList> getTop100SmCnMobilePattern(string html)
        {
            ArrayList SmcnMobile = new ArrayList();
            ArrayList alDup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//a[@class='c-header-inner c-flex-1']|.//div[@class='c-nature--v1_0_0']|.//div[@class='c-source--v1_0_0 c-source-l c-margin-top-s']");

            foreach (var links in hn)
            {
                try
                {
                    //HtmlNode a = links.SelectSingleNode(".//a");
                    string urls = links.Attributes["href"].Value.Trim();
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        alDup.Add(HttpUtility.HtmlDecode(urls));
                    }
                    foreach (string s in alDup)
                    {
                        if (SmcnMobile.Contains(s) || string.IsNullOrEmpty(s)) continue;
                        if (s.Contains("mparticle") || s.Contains("zm.sm-tc.cn")) continue;
                        SmcnMobile.Add(s);
                    }
                    if (SmcnMobile.Count > 100)
                    {
                        SmcnMobile.RemoveRange(100, SmcnMobile.Count - 100);
                    }
                }
                catch { continue; }
            }
            return await Task.FromResult<ArrayList>(SmcnMobile); ;
        }

        public async Task<ArrayList> getTop100RamblerDesktopPattern(string html)
        {
            ArrayList top100Rambler = new ArrayList();
            ArrayList alDup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//h2[@class='Serp__item__title--2KnDi']/a");

            try
            {
                foreach (var links in hn)
                {
                    string urls = links.Attributes["href"].Value.Trim();
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        alDup.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
                foreach (string s in alDup)
                {
                    if (top100Rambler.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    top100Rambler.Add(s);
                }
                if (top100Rambler.Count > 100)
                {
                    top100Rambler.RemoveRange(100, top100Rambler.Count - 100);
                }
            }

            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message.ToString());
            }
            return await Task.FromResult<ArrayList>(top100Rambler);
        }


        public async Task<ArrayList> getTop100PriceSearcherPattern(string html)
        {
            ArrayList top100PriceSearcherUK = new ArrayList();
            ArrayList alDup = new ArrayList();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='w-product-title']/a");
            try
            {
                foreach (var links in hn)
                {
                    string urls = string.Empty;
                    urls = links.Attributes["href"].Value.Trim();
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        alDup.Add(HttpUtility.HtmlDecode(urls));
                    }
                    else if (urls.StartsWith("/gb/product"))
                    {
                        urls = "https://www.pricesearcher.com" + urls;
                        alDup.Add(HttpUtility.HtmlDecode(urls));
                        if (alDup.Count >= 100) break;
                    }
                }
                foreach (string s in alDup)
                {
                    if (top100PriceSearcherUK.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    top100PriceSearcherUK.Add(s);
                }
                if (top100PriceSearcherUK.Count > 100)
                {
                    top100PriceSearcherUK.RemoveRange(100, top100PriceSearcherUK.Count - 100);
                }
            }
            catch (Exception ex) { throw ex; }

            return await Task.FromResult<ArrayList>(top100PriceSearcherUK);
        }


        public async Task<ArrayList> getTop100NaverDesktopPattern(string html)
        {
            ArrayList top100NaverDesktop = new ArrayList();
            ArrayList alDup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='web_url']/a");

            try
            {
                foreach (var links in hn)
                {
                    string urls = links.Attributes["href"].Value.Trim();
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        alDup.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
                foreach (string s in alDup)
                {
                    if (top100NaverDesktop.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    top100NaverDesktop.Add(s);
                }
                if (top100NaverDesktop.Count > 100)
                {
                    top100NaverDesktop.RemoveRange(100, top100NaverDesktop.Count - 100);
                }
            }

            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult<ArrayList>(top100NaverDesktop);
        }

        public async Task<ArrayList> getTop100NaverMobilePattern(string html)
        {
            ArrayList top100NaverMobile = new ArrayList();
            ArrayList alDup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='elss web_url']/a");

            try
            {
                foreach (var links in hn)
                {
                    string urls = links.Attributes["href"].Value.Trim();
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        alDup.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
                foreach (string s in alDup)
                {
                    if (top100NaverMobile.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    top100NaverMobile.Add(s);
                }
                if (top100NaverMobile.Count > 100)
                {
                    top100NaverMobile.RemoveRange(100, top100NaverMobile.Count - 100);
                }
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult<ArrayList>(top100NaverMobile);
        }


        public async Task<ArrayList> getTop100HaoSou360DesktopPattern(string html)
        {
            ArrayList top100HaoSou360Desktop = new ArrayList();
            ArrayList alDup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            string urls = string.Empty;
            doc.LoadHtml(html);
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//h3[@class='res-title']|//h3[@class='res-title ']/a[1]");

            try
            {
                foreach (var links in hn)
                {
                    HtmlNode h3 = links.SelectSingleNode(".//a");
                    if (h3 == null)
                    {
                        if (links.OuterHtml.Contains("data-mdurl"))
                        {
                            urls = links.Attributes[1].Value;
                        }
                        else
                        {
                            urls = links.Attributes["href"].Value;
                        }
                    }
                    if (h3 != null)
                    {
                        if (h3.OuterHtml.Contains("data-mdurl"))
                        {
                            urls = h3.Attributes[1].Value;
                        }
                        else
                        {
                            urls = h3.Attributes["href"].Value;
                        }
                    }
                    urls = HttpUtility.UrlDecode(urls);
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        int indx = urls.LastIndexOf("http://");
                        if (indx < 0)
                        {
                            indx = urls.LastIndexOf("https://");
                        }
                        urls = urls.Remove(0, indx);
                        top100HaoSou360Desktop.Add(HttpUtility.HtmlDecode(urls));
                    }
                }
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult(top100HaoSou360Desktop); ;
        }

        public async Task<ArrayList> getTop100HaoSou360MobilePattern(string html)
        {
            ArrayList top100HaoSou360Mobile = new ArrayList();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='feedback-btn']");

            foreach (var links in hn)
            {
                try
                {
                    string urls = links.Attributes["data-url"].Value.Trim();
                    urls = HttpUtility.HtmlDecode(urls);
                    if (urls.Contains("m.so.com") || urls.Contains("&m="))
                    {
                        Regex r = new Regex("u=(.*)&m=", RegexOptions.IgnoreCase);
                        Regex r1 = new Regex("(.*)&m=", RegexOptions.IgnoreCase);
                        Match m1 = r.Match(urls);
                        Match m2 = r1.Match(urls);
                        if (m1.Success)
                            urls = HttpUtility.UrlDecode(m1.Groups[1].Value);
                        if (m2.Success)
                            urls = HttpUtility.UrlDecode(m1.Groups[1].Value);
                    }
                    if (urls.LastIndexOf("http://") > 0)
                    {
                        urls = urls.Remove(0, urls.LastIndexOf("http://"));
                    }
                    else if (urls.LastIndexOf("https://") > 0)
                    {
                        urls = urls.Remove(0, urls.LastIndexOf("https://"));
                    }
                    if (urls.StartsWith("http") || urls.StartsWith("https"))
                    {
                        if (!urls.Contains("www.so.com"))
                            top100HaoSou360Mobile.Add(urls);
                    }
                }
                catch { continue; }
            }
            return await Task.FromResult<ArrayList>(top100HaoSou360Mobile);
        }


    }
}

