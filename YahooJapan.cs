using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UniversalTracking
{
   public class YahooJapan
    {
       public async Task<ArrayList> getTop100YahooJapan(string seid, string keyword)
        {
            ArrayList myArrayList = new ArrayList();
            switch (seid)
            {
                case "48":
                    {
                        myArrayList = GetTop100YahaooJapanDesktop(seid,keyword).Result;
                        break;
                    }
                case "194":
                    {
                        myArrayList = GetTop100YahaooJapanMobile(seid,keyword).Result;
                        break;
                    }
            }
            return await Task.FromResult<ArrayList>(myArrayList);
        }
        public async Task<ArrayList> GetTop100YahaooJapanDesktop(string seid, string kw)
        {           
            Task<ArrayList> alresult = null;
            ArrayList arRes = new ArrayList();
            string device = "";
            string jobid = "";
            string keyword = "";
            string ul = "";
            string pagehtml = "";
            int ct;
            int lp2;
            string html = null;
            try
            {
                lp2 = 0;
                var doc = new HtmlAgilityPack.HtmlDocument();
                for (int i = 0; i < 10; i++)
                {
                    string jid = "";
                    int st = (10 * i) + 1;                    
                    ul = "https://search.yahoo.co.jp/search?p=" + kw + "&ei=UTF-8&fr=top_ga1&n=10&fl=0&x=wrt&b=" + st + "&pz=10";                    
                    ct = 0;
                    Repeat:
                    if (ct >= 2)
                    {
                        break;
                    }
                    if (lp2 >= 2)
                    {
                        break;
                    }
                    //alresult = GetHTML(kw, Convert.ToInt32(seid), ul);
                    alresult = GetOxylabsWebDataSources(ul, "desktop");

                    foreach (string[] src in alresult.Result)
                    {
                        keyword = src[0];
                        jid = src[2];
                        JObject obj = JObject.Parse(src[1]);
                        pagehtml = obj["results"][0]["content"].Value<string>();
                        //System.IO.File.WriteAllText(@"D:\source\48\" + kw + "_" + i + 1 + "_" + jid + ".html", pagehtml);                                         
                        if (pagehtml.Contains("に一致する情報は見つかりませんでした。") || pagehtml.Contains("男の子リュックサック」に一致する情報は見つかりませんでした。"))//48
                        {
                            if (ct <= 2)
                            {
                                ct++;
                                if (ct >= 2)
                                {
                                    lp2++;
                                    break;
                                }
                                goto Repeat;
                            }
                        }
                        html += obj["results"][0]["content"].Value<string>();
                        jobid = src[2];
                        device = src[3];
                    }
                }              
                        ArrayList addURLs = getTop100YahooJapanDesktopPattern(html).Result;
                        foreach (string str in addURLs)
                        {
                            if (!arRes.Contains(str))
                                arRes.Add(str);

                        }        
             }
            catch (Exception ex) { throw ex; }

            return await Task.FromResult<ArrayList>(arRes);
        }
        public async Task<ArrayList> GetTop100YahaooJapanMobile(string seid, string kw)
        {
            Task<ArrayList> alresult = null;
            ArrayList arRes = new ArrayList();
            string html = null;
            string device = "";
            string jobid = "";
            string keyword = "";
            string ul = "";
            string pagehtml = "";
            int ct;
            int lp2;
            try
            {
                lp2 = 0;
                var doc = new HtmlAgilityPack.HtmlDocument();
                for (int i = 0; i < 10; i++)
                {
                    string jid = "";
                    int st = (10 * i) + 1;
                   
                        ul = "https://search.yahoo.co.jp/search?p=" + kw + "&x=wrt&ei=UTF-8&fr=top_ga1&n=10&fl=0&b=" + st + "&pz=10";
                    
                    ct = 0;
                    Repeat:
                    if (ct >= 2)
                    {
                        break;
                    }
                    if (lp2 >= 2)
                    {
                        break;
                    }
                    alresult = GetOxylabsWebDataSources(ul, "mobile");
                    foreach (string[] src in alresult.Result)
                    {
                        keyword = src[0];
                        jid = src[2];
                        JObject obj = JObject.Parse(src[1]);
                        pagehtml = obj["results"][0]["content"].Value<string>();
                        System.IO.File.WriteAllText(@"D:\source\194\" + kw + "_" + i + 1 + "_" + jid + ".html", pagehtml);
                        if (pagehtml.Contains("に一致する情報は見つかりませんでした。") || pagehtml.Contains("男の子リュックサック」に一致する情報は見つかりませんでした。"))//48
                        {
                            if (ct <= 2)
                            {
                                ct++;
                                if (ct >= 2)
                                {
                                    lp2++;
                                    break;
                                }
                                goto Repeat;
                            }
                        }
                        html += obj["results"][0]["content"].Value<string>();
                        jobid = src[2];
                        device = src[3];
                    }
                }
                        ArrayList addURLs = getTop100YahooJapanMobilePattern(html).Result;
                        foreach (string str in addURLs)
                        {
                            if (!arRes.Contains(str))
                                arRes.Add(str);
                        }                   
              }
            catch (Exception ex) { throw ex; }

            return await Task.FromResult(arRes);
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
                        if(urls.StartsWith("/amp/s/"))
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

        async Task<ArrayList> GetOxylabsWebDataSources(string ul, string type)
        {
            Uri queryUri = new Uri("https://data.oxylabs.io/v1/queries");//io/v1/stats
            string username = "gpidatametrics";
            string password = "sdV5X3fcX6";

            string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));            
            OxyParams op = new OxyParams()
            {
                source = "universal",
                url = ul,
                limit = 10,
                pages = 1,
                start_page = 1,
                parse = false,
                user_agent_type = type
            };

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(queryUri);
            req.Headers.Clear();

            req.Method = "POST";
            req.ContentType = "application/json";
            req.Headers.Add(HttpRequestHeader.Authorization, "Basic " + authInfo);

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                var json = JsonConvert.SerializeObject(op, new JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                });

                streamWriter.Write(json);
            }

            string response;
            try
            {
                HttpWebResponse res = (HttpWebResponse)await req.GetResponseAsync();
                using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
                res.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            JObject jo = JObject.Parse(response);
            var links = from p in jo["query"] select p;
            ArrayList lst = new ArrayList();
            string kw = jo["query"].Value<string>();
            string href = jo["_links"][1]["href"].Value<string>();
            string status1 = jo["status"].Value<string>();
            string jobid = jo["id"].Value<string>();
            string device = jo["user_agent_type"].Value<string>();
            string[] s = { kw, href, status1, "no", jobid, device };
            lst.Add(s);
            if (lst.Count <= 0) return lst;
            ArrayList alResult = new ArrayList();
            do
            {
                int cnt = 0;
                foreach (string[] cbUrl in lst)
                {
                    string[] reslt = { "", "", "", "" };
                    response = "";
                    Uri uri = new Uri(cbUrl[1]);
                    //Uri uri = new Uri("https://data.oxylabs.io/v1/queries/6605663687883627521/results");
                    if (cbUrl[2] == "done" && cbUrl[3] == "no")
                    {
                        try
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                            httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
                            HttpWebResponse res = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
                            Stream resVal = res.GetResponseStream();
                            StreamReader reader = new StreamReader(resVal, Encoding.UTF8);
                            //** Store all the contents
                            response = reader.ReadToEnd();
                            resVal.Close();
                            res.Close();
                            cbUrl[3] = "yes";
                            cnt++;
                            if (!string.IsNullOrEmpty(response))
                            {
                                reslt[0] = cbUrl[0];
                                reslt[1] = response;
                                reslt[2] = cbUrl[4];
                                reslt[3] = cbUrl[5];
                                alResult.Add(reslt);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Result Request: " + ex.Message);
                        }
                    }
                    else if (cbUrl[2] == "faulted" && cbUrl[3] == "no")
                    {
                        cbUrl[3] = "yes";
                        cnt++;
                    }
                    else if (cbUrl[2] == "pending" && cbUrl[3] == "no")
                    {
                        try
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri.ToString().Replace("/results", ""));
                            httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
                            HttpWebResponse res = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

                            string doneresp = "";
                            using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                            {
                                doneresp = reader.ReadToEnd();
                            }
                            res.Close();

                            JObject job = JObject.Parse(doneresp);
                            string status = job["status"].Value<string>();
                            cbUrl[2] = status;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Status Request: " + ex.Message);
                        }
                    }
                    else
                        cnt++;
                    Task.Delay(200).Wait();
                }
                if (lst.Count == cnt) break;

            } while (true);

            return await Task.FromResult<ArrayList>(alResult);
        }
    }
}
