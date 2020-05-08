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


        public string Gethtml(int seid, string kw, ArrayList al)
        {
            string response = "";
            string html = "";
            string status = "done";
            string resultsurl = "";
            string jobid = "";
            try
            {
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
                                    html += jo["content"].Value<string>();
                                    //System.IO.File.WriteAllText(@"c:\inetpub\wwwroot\html\" + kw + ".html", response, Encoding.UTF8);
                                }
                            }
                            catch (Exception ex)
                            {
                                string error = ex.Message.ToString();
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
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }

            return html;
        }

        private void ProcessError(string kw, int seid, string jobid)
        {
            string qry = "insert into dashboard_dataerrors (date, name, seid, jobid) values(Convert(varchar(10),'" + myDate + "',103), N'" +
                kw.Replace("'", "''") + "', " + seid + ", '" + jobid + "' )";

            using (SqlConnection con = new SqlConnection(Common.strConn()))
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
            ArrayList top100YahooJapan = new ArrayList();
            ArrayList alDup = new ArrayList();

            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='sw-Card__section sw-Card__section--header']/div/div/a[1]");

                foreach (var links in hn)
                {
                    try
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
                            alDup.Add(HttpUtility.HtmlDecode(urls));
                        }
                    }
                    catch { continue; }
                }
                foreach (string s in alDup)
                {
                    string s1 = s;
                    //s1 = s1.Remove(s1.Length - 1);
                    if (top100YahooJapan.Contains(s1) || string.IsNullOrEmpty(s1)) continue;
                    if (top100YahooJapan.Contains(s1) || string.IsNullOrEmpty(s1)) continue;
                    if (s1.Contains("search.yahoo.co.jp") || s1.Contains("news.yahoo.co.jp") || s1.Contains("topics.shopping") || s1.Contains("paypayfleamarket.yahoo.co.jp") || s1.Contains("app.adjust.com")) continue;
                    top100YahooJapan.Add(s1);
                }
                if (top100YahooJapan.Count > 100)
                {
                    top100YahooJapan.RemoveRange(100, top100YahooJapan.Count - 100);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No pattern match" + ex.Message);
            }
            return await Task.FromResult<ArrayList>(top100YahooJapan); ;
        }

        public async Task<ArrayList> getTop100YahooJapanMobilePattern(string html)
        {
            ArrayList top100YahooJapanMobile = new ArrayList();
            ArrayList alDup = new ArrayList();

            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//div[@class='sw-Card__section']/a");

                foreach (var links in hn)
                {
                    try
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
                            alDup.Add(HttpUtility.HtmlDecode(urls));
                        }
                    }
                    catch { continue; }
                }
                foreach (string s in alDup)
                {
                    string s1 = s;
                    //s1 = s1.Remove(s1.Length - 1);
                    if (top100YahooJapanMobile.Contains(s1) || string.IsNullOrEmpty(s1)) continue;
                    if (s1.Contains("shopping.yahoo.co.jp/search?rkf=2")
                        || s1.Contains("auctions.yahoo.co.jp/search/search?rkf=2")
                        || s1.Contains("news.yahoo.co.jp/search/?rkf=2")
                        || s1.Contains("topics.shopping")
                        || s1.Contains("paypayfleamarket.yahoo.co.jp/search/")
                        || s1.Contains("app.adjust.com")
                        || s1.Contains("chiebukuro.yahoo.co.jp/search/?rkf=1")
                        || s1.Contains("search.yahoo.co.jp/video/search?rkf=2")
                        || s1.Contains("search.yahoo.co.jp/image/search?rkf=2")
                        || s1.Contains("loco.yahoo.co.jp/search/?ei=utf-8&rkf=2")
                        || s1.Contains("zozo.jp/search/?p_keyv=")
                        || s1.Contains("rd.listing.yahoo.co.jp/o/search/GU=")
                        || s1.Contains("isi.edu.pa/maps/place/")
                        || s1.Contains("www.ombudsman.gov.ua/ua/all-news")
                        || s1.Contains("www.navitime.co.jp/taxi/result/?")
                        || s1.Contains("www.ezimport.co.jp/search.php?id=")
                        || s1.Contains("www.facebook.com/yasuhiko.tsuchida.coboking/posts/")) continue;
                    top100YahooJapanMobile.Add(s1);
                }
                if (top100YahooJapanMobile.Count > 100)
                {
                    top100YahooJapanMobile.RemoveRange(100, top100YahooJapanMobile.Count - 100);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No pattern match" + ex.Message);
            }
            return await Task.FromResult<ArrayList>(top100YahooJapanMobile); ;
        }

        public async Task<ArrayList> getTop100SogouDesktopPattern(string html)
        {
            ArrayList top100sogouDesktop = new ArrayList();
            ArrayList alDup = new ArrayList();

            try
            {
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
                                alDup.Add(HttpUtility.HtmlDecode(urls));
                        }
                    }
                    catch { continue; }
                }
                foreach (string s in alDup) //07-05-2020
                {
                    if (top100sogouDesktop.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    top100sogouDesktop.Add(s);
                }

                if (top100sogouDesktop.Count > 100) //07-05-2020
                {
                    top100sogouDesktop.RemoveRange(100, top100sogouDesktop.Count - 100);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No pattern match" + ex.Message);
            }

            return await Task.FromResult<ArrayList>(top100sogouDesktop);
        }

        public async Task<ArrayList> getTop100SogouMobilePattern(string html)
        {
            ArrayList top100sogouMobile = new ArrayList();
            ArrayList alDup = new ArrayList();

            try
            {
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
                            alDup.Add(HttpUtility.HtmlDecode(urls));
                        }

                    }
                    catch { continue; }
                }
                foreach (string s in alDup) //07-05-2020
                {
                    if (top100sogouMobile.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    top100sogouMobile.Add(s);
                }

                if (top100sogouMobile.Count > 100) //07-05-2020
                {
                    top100sogouMobile.RemoveRange(100, top100sogouMobile.Count - 100);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No pattern match" + ex.Message);
            }
            return await Task.FromResult<ArrayList>(top100sogouMobile);
        }

        public async Task<ArrayList> getTop100SmCnMobilePattern(string html)
        {
            ArrayList top100SmcnMobile = new ArrayList();
            ArrayList alDup = new ArrayList();

            try
            {
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
                    }
                    catch { continue; }
                }
                foreach (string s in alDup)   //07-05-2020
                {
                    if (top100SmcnMobile.Contains(s) || string.IsNullOrEmpty(s)) continue;
                    if (s.Contains("mparticle") || s.Contains("zm.sm-tc.cn")) continue;
                    top100SmcnMobile.Add(s);
                }
                if (top100SmcnMobile.Count > 100)  //07-05-2020
                {
                    top100SmcnMobile.RemoveRange(100, top100SmcnMobile.Count - 100);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No pattern match" + ex.Message);

            }

            return await Task.FromResult<ArrayList>(top100SmcnMobile); ;
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

