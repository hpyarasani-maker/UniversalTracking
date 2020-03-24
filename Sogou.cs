using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace UniversalTracking
{
    public class Sogou
    {
        public string seid;
        public string kw;

        public async Task<ArrayList> GetTop100Sogou(string seid, string keyword)
        {
            ArrayList myArrayList = new ArrayList();
            switch (seid)
            {
                case "276":
                    {
                        myArrayList = getTop100SogouDesktop(seid, keyword).Result;
                        break;
                    }
                case "277":
                    {
                        myArrayList = getTop100SogouMobile(seid, keyword).Result;
                        break;
                    }
            }
            return await Task.FromResult<ArrayList>(myArrayList);
        }

        public async Task<ArrayList> getTop100SogouDesktop(string seid, string kw)
        {
            Task<ArrayList> alresult = null;
            ArrayList arRes = new ArrayList();
            string device = "";
            string jobid = "";
            string keyword = "";
            string ul = "";
            string pagehtml = "";
            string html = null;
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();

                for (int i = 1; i <= 10; i++)
                {
                    string jid = "";
                    ul = "https://www.sogou.com/web?query=" + kw + "&ie=utf8&page=" + i;

                    //alresult = GetHTML(kw, Convert.ToInt32(seid), ul);
                    //int ct = 0;
                    REPEAT:
                    alresult = GetOxylabsWebDataSources(ul, "desktop");

                    if(alresult.Result.Count == 0)
                    {
                        goto REPEAT;
                    }

                    try
                    {
                        foreach (string[] src in alresult.Result)
                        {
                            keyword = src[0];
                            jid = src[2];
                            JObject obj = JObject.Parse(src[1]);

                            System.IO.File.WriteAllText(@"D:\source\newresults\" + kw + i + ".html", obj["results"][0]["content"].Value<string>());
                            if (pagehtml.Contains("Sorry, no results were found for") || pagehtml.Contains("抱歉，没有找到与“ipad”相关的结果。"))
                            {
                                goto REPEAT;
                            }
                            html += obj["results"][0]["content"].Value<string>();
                            jobid = src[2];
                            device = src[3];
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorr = ex.Message.ToString();
                    }
                    
                }

                ArrayList addURLs = SogouDesktopPattern(html).Result;
                foreach (string str in addURLs)
                {
                    if (!arRes.Contains(str))
                        arRes.Add(str);
                }
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult(arRes);
        }

        public async Task<ArrayList> getTop100SogouMobile(string seid, string kw)
        {
            Task<ArrayList> alresult = null;
            ArrayList arRes = new ArrayList();
            string html = null;
            string device = "";
            string jobid = "";
            string keyword = "";
            string ul = "";
            string pagehtml = "";
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                for (int i = 1; i <= 10; i++)
                {
                    string jid = "";

                    ul = "https://m.sogou.com/web/searchList.jsp?keyword=" + kw + "&p=" + i + "&s_from=pagenext&showextquery=1";

                    REPEAT:

                    alresult = GetOxylabsWebDataSources(ul, "mobile");

                    if(alresult.Result.Count == 0)
                    {
                        goto REPEAT;
                    }
                    
                    try
                    {
                        foreach (string[] src in alresult.Result)
                        {
                            keyword = src[0];
                            jid = src[2];
                            JObject obj = JObject.Parse(src[1]);
                            pagehtml = obj["results"][0]["content"].Value<string>();
                            System.IO.File.WriteAllText(@"D:\source\278\" + kw + "_" + i + 1 + "_" + jid + ".html", pagehtml);

                            if (pagehtml.Contains("に一致する情報は見つかりませんでした。") || pagehtml.Contains("男の子リュックサック」に一致する情報は見つかりませんでした。"))
                            {
                                goto REPEAT;
                            }
                            html += obj["results"][0]["content"].Value<string>();
                            jobid = src[2];
                            device = src[3];
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                    }
                }
                ArrayList addURLs = SogouMobilePattern(html).Result;
                foreach (string str in addURLs)
                {
                    if (!arRes.Contains(str))
                        arRes.Add(str);
                }
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult<ArrayList>(arRes);
        }


        public async Task<ArrayList> SogouDesktopPattern(string html)
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

        public async Task<ArrayList> SogouMobilePattern(string html)
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


        async Task<ArrayList> GetOxylabsWebDataSources(string ul, string type)
        {
            Uri queryUri = new Uri("https://data.oxylabs.io/v1/queries");//io/v1/stats

            string username = "gpidatametrics";
            string password = "sdV5X3fcX6";
            string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));

            //string[] kwd = { sp.query };
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
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(op, new Newtonsoft.Json.JsonSerializerSettings
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
            //foreach (JToken link in links)
            //{
            string kw = jo["query"].Value<string>();
            string href = jo["_links"][1]["href"].Value<string>();
            string status1 = jo["status"].Value<string>();
            string jobid = jo["id"].Value<string>();
            string device = jo["user_agent_type"].Value<string>();
            string[] s = { kw, href, status1, "no", jobid, device };    // keyword, url, status, isdownloaded, jobid, device.
            lst.Add(s);
            //}

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
                            //if (response.Length < 5000)
                            //{
                            //    goto Repeat;
                            //}
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
