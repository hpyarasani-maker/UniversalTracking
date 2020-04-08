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
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace UniversalTracking
{
    public class HaoSou360
    {
        public string seid;
        public string kw;

        public async Task<ArrayList> GetTop100HaoSou360(string seid, string keyword)
        {
            ArrayList myArrayList = new ArrayList();
            switch (seid)
            {
                case "175":
                    {
                        myArrayList = getTop100HaoSou360Desktop(seid, keyword).Result;
                        break;
                    }
                case "193":
                    {
                        myArrayList = getTop100HaoSou360Mobile(seid, keyword).Result;
                        break;
                    }
            }
            return await Task.FromResult<ArrayList>(myArrayList);
        }

        public async Task<ArrayList> getTop100HaoSou360Desktop(string seid, string kw)
        {
            ArrayList alresult = null;
            ArrayList arRes = new ArrayList();
            string device = "";
            string jobid = "";
            string ul = "";
            string html = "";
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();

                for (int i = 1; i <= 10; i++)
                {
                    ul = "https://www.so.com/s?ie=utf-8&shb=1&src=360sou_newhome&q=" + kw + "&pn=" + i;

                    REPEAT:
                    alresult = await GetOxylabsWebDataSources(ul, "desktop");

                    if (alresult.Count == 0)
                    {
                        goto REPEAT;
                    }

                    try
                    {
                        foreach (string[] src in alresult)
                        {
                            if (String.IsNullOrEmpty(src[1]))
                            {
                                //System.IO.File.WriteAllText(@"D:\24-03-2020\175\" + kw + "_" + i + "_" + jobid + ".html", src[1]);
                                goto REPEAT;
                            }

                            JObject obj = JObject.Parse(src[1]);
                            string pagehtml = obj["results"][0]["content"].Value<string>();
                            jobid = src[2];
                            device = src[3];

                            //System.IO.File.WriteAllText(@"D:\24-03-2020\175\" + kw + "_" + i + "_" + jobid + ".html", pagehtml);
                            //File.WriteAllText(@"C:\inetpub\wwwroot\html\"+jobid+"_withOut filter_"+".html", html, Encoding.UTF8);
                            if (pagehtml.Contains("No results found for") || String.IsNullOrEmpty(pagehtml) || pagehtml.Contains("無法找到符合") || pagehtml.Contains("Sorry! Not found") || pagehtml.Contains("對不起！找不到") || pagehtml.Contains("Sorry") || pagehtml.Contains("シルクエピル") || pagehtml.Contains("Dear, the system has detected that you operate too frequently") || pagehtml.Contains("亲，系统检测到您操作过于频繁。"))
                            {
                                Thread.Sleep(5000);
                                //System.IO.File.WriteAllText(@"D:\24-03-2020\175\" + kw + "_" + i + "_" + jid + ".html", pagehtml);
                                goto REPEAT;
                            }

                            html += pagehtml;
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                    }
                }

                ArrayList addURLs = HaoSou360DesktopPattern(html).Result;
                foreach (string str in addURLs)
                {
                    if (!arRes.Contains(str))
                        arRes.Add(str);
                }
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult<ArrayList>(arRes);
        }

        public async Task<ArrayList> getTop100HaoSou360Mobile(string seid, string kw)
        {
            ArrayList alresult = null;
            ArrayList arRes = new ArrayList();
            string html = "";
            string device = "";
            string jobid = "";
            string ul = "";
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                for (int i = 1; i <= 10; i++)
                {
                    ul = "https://m.so.com/nextpage?q= " + kw + " &pn=" + i + "&ajax=1";

                    REPEAT:
                    alresult = await GetOxylabsWebDataSources(ul, "mobile");

                    if (alresult.Count == 0)
                    {
                        goto REPEAT;
                    }

                    try
                    {
                        foreach (string[] src in alresult)
                        {
                            JObject obj = JObject.Parse(src[1]);
                            string pagehtml = obj["results"][0]["content"].Value<string>();
                            jobid = src[2];
                            device = src[3];

                            //System.IO.File.WriteAllText(@"D:\source\newresults\" + kw + "_" + i + "_" + jobid + ".html", pagehtml);
                            //File.WriteAllText(@"C:\inetpub\wwwroot\"+jobid+"_withOut filter_"+".html", html, Encoding.UTF8);
                            if (pagehtml.Contains("に一致する情報は見つかりませんでした。") || pagehtml.Contains("男の子リュックサック」に一致する情報は見つかりませんでした。"))
                            {
                                goto REPEAT;
                            }

                            html += pagehtml;
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message.ToString();
                    }
                }

                ArrayList addURLs = HaoSou360MobilePattern(html).Result;
                foreach (string str in addURLs)
                {
                    if (!arRes.Contains(str))
                        arRes.Add(str);
                }
            }
            catch (Exception ex) { throw new ArgumentException(ex.Message.ToString()); }

            return await Task.FromResult<ArrayList>(arRes);
        }


        public async Task<ArrayList> HaoSou360DesktopPattern(string html)
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

        public async Task<ArrayList> HaoSou360MobilePattern(string html)
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
                user_agent_type = type,
                render = "html"
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
            var links = from p in jo["_links"] select p;
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
                    //Uri uri = new Uri("https://data.oxylabs.io/v1/queries/6643418210903262209/results");
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
                        reslt[0] = cbUrl[0];
                        reslt[1] = "";
                        reslt[2] = cbUrl[4];
                        reslt[3] = cbUrl[5];
                        alResult.Add(reslt);
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
                    Task.Delay(400).Wait();
                }
                if (lst.Count == cnt) break;

            } while (true);

            return await Task.FromResult<ArrayList>(alResult);
        }

    }
}
