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
    public class YahooHK
    {     
       public async Task<ArrayList> getTop100YahooHK(string seid, string keyword)
        {
            ArrayList myArrayList = new ArrayList();
            switch (seid)
            {
                case "38":
                    {
                        myArrayList = GetTop100YahaooHKDesktop(seid, keyword).Result;
                        break;
                    }
                case "256":
                    {
                        myArrayList = GetTop100YahaooHKMobile(seid, keyword).Result;
                        break;
                    }
            }
            return await Task.FromResult<ArrayList>(myArrayList);
        }
        public async Task<ArrayList> GetTop100YahaooHKDesktop(string seid, string kw)
        {
            ArrayList arRes = new ArrayList();
            string html = null;
            string device = "";
            string jobid = "";
            string keyword = "";
            string ul = "";
            string pagehtml = "";
            Task<ArrayList> alresult = null;
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                for (int i = 0; i < 2; i++)
                {
                    string jid = "";
                    int st = (50 * i) + 1;                    
                    ul = "https://hk.search.yahoo.com/search?p=" + kw + "&ei=UTF-8&b=" + st + "&pz=100";              
                    Repeat:                    
                    //alresult = GetHTML(kw, Convert.ToInt32(seid), ul);                    
                    alresult = GetOxylabsWebDataSources(ul, "desktop");
                    foreach (string[] src in alresult.Result)
                    {
                        keyword = src[0];
                        jid = src[2];
                        JObject obj = JObject.Parse(src[1]);
                        pagehtml = obj["results"][0]["content"].Value<string>();
                        //System.IO.File.WriteAllText(@"D:\source\38\" + kw + "_" + i + 1 + "_" + jid + ".html", pagehtml);
                        if (pagehtml.Contains("consent-semi-transparent") || pagehtml.Contains("consent-overlay") || pagehtml.Contains("無法找到符合"))//38 選擇較常用的字, 或減省符號如「，」 或「@」。 
                        {
                            goto Repeat;
                        }
                        html += obj["results"][0]["content"].Value<string>();
                        jobid = src[2];
                        device = src[3];
                    }
                }                                
                        ArrayList addURLs = getTop100YahooHKDesktopPattern(html).Result;                        
                        foreach (string str in addURLs)
                        {
                            if (!arRes.Contains(str))
                                arRes.Add(str);
                        }               
                  
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }
            return await Task.FromResult<ArrayList>(arRes);
        }
        public async Task<ArrayList> GetTop100YahaooHKMobile(string seid, string kw)
        {
            ArrayList arRes = new ArrayList();
            string html = null;
            string device = "";
            string jobid = "";
            string keyword = "";
            string ul = "";
            string pagehtml = "";
            Task<ArrayList> alresult = null;
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                for (int i = 0; i < 2; i++)
                {
                    string jid = "";
                    int st = (50 * i) + 1;                   
                    ul = "https://hk.search.yahoo.com/search?p=" + kw + "&ei=UTF-8&.tsrc=yfp-hrmob-sb&b=" + st + "&pz=100";                   
                    Repeat:                   
                    alresult = GetOxylabsWebDataSources(ul, "mobile");
                    foreach (string[] src in alresult.Result)
                    {
                        keyword = src[0];
                        jid = src[2];
                        JObject obj = JObject.Parse(src[1]);
                        pagehtml = obj["results"][0]["content"].Value<string>();
                        //System.IO.File.WriteAllText(@"D:\source\38\" + kw + "_" + i + 1 + "_" + jid + ".html", pagehtml);
                        if (pagehtml.Contains("consent-semi-transparent") || pagehtml.Contains("consent-overlay") || pagehtml.Contains("無法找到符合"))//38 選擇較常用的字, 或減省符號如「，」 或「@」。 
                        {
                            goto Repeat;
                        }
                        html += obj["results"][0]["content"].Value<string>();
                        jobid = src[2];
                        device = src[3];
                    }
                }                             
                   
                        ArrayList addURLs = getTop100YahooHKMobilePattern(html).Result;
                        foreach (string str in addURLs)
                        {
                            if (!arRes.Contains(str))
                                arRes.Add(str);
                        }
                                    
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }
            return await Task.FromResult<ArrayList>(arRes);
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
                       
                        al.Add(HttpUtility.HtmlDecode(urls));

                    }
                }
                catch { continue; }

            }
            return await Task.FromResult<ArrayList>(al);
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
                user_agent_type = type,
                render= "html"
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
            string jobid = jo["id"].Value<string>();//6638477297173144577
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
