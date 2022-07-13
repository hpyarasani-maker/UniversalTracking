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
    public class Rambler
    {
        public string seid;
        public string kw;
        public async Task<ArrayList> GetTop100Rambler(string seid, string keyword)
        {
            ArrayList myArrayList = new ArrayList();
            switch (seid)
            {
                case "19":
                    {
                        myArrayList = getTop100RamblerDesktop(seid, keyword).Result;
                        break;
                    }
            }
            return await Task.FromResult<ArrayList>(myArrayList);
        }


        public async Task<ArrayList> getTop100RamblerDesktop(string seid, string kw)
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
                int i = 1;
                //for (int i = 1; i <= 10; i++)
                //{
                //ul = "http://nova.rambler.ru/search?pagelen=100&query=" + kw + "&page=" + i;
                ul = "http://nova.rambler.ru/search?pagelen=100&query=" + kw;

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
                        JObject obj = JObject.Parse(src[1]);
                        string pagehtml = obj["results"][0]["content"].Value<string>();
                        jobid = src[2];
                        device = src[3];
                        //System.IO.File.WriteAllText(@"C:\inetpub\wwwroot\html\" + jobid + "_" + kw + ".html", html, Encoding.UTF8);
                        //System.IO.File.WriteAllText(@"D:\source\newresults\" + kw + "_" + i + "_" + jobid + ".html", pagehtml, Encoding.UTF8);

                        if (pagehtml.Contains("Sorry, no results were found for"))
                        {
                            goto REPEAT;
                        }

                        html = pagehtml;
                        //System.IO.File.WriteAllText(@"C:\inetpub\wwwroot\html\" + jobid + "_" + kw + ".html", html, Encoding.UTF8);

                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message.ToString();
                }
                //}

                ArrayList addURLs = RamblerDesktopPattern(html).Result;
                foreach (string str in addURLs)
                {
                    if (!arRes.Contains(str))
                        arRes.Add(str);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message.ToString());
            }

            return await Task.FromResult<ArrayList>(arRes);
        }

        public async Task<ArrayList> RamblerDesktopPattern(string html)
        {
            ArrayList top100Rambler = new ArrayList();
            ArrayList alDup = new ArrayList();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            //HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//h2[@class='Serp__item__title--2KnDi']/a");
            HtmlNodeCollection hn = doc.DocumentNode.SelectNodes(".//h2[@class='Serp__title--3i6Ro']/a");


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

        async Task<ArrayList> GetOxylabsWebDataSources(string ul, string type)
        {
            //Uri queryUri = new Uri("https://data.oxylabs.io/v1/queries/batch");//io/v1/stats
            Uri queryUri = new Uri("https://data.oxylabs.io/v1/queries");//io/v1/stats

            string username = "gpidatametrics";
            string password = "sdV5X3fcX6";
            string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));

            //string[] kwd = { sp.query };
            RamblerOxyParams op = new RamblerOxyParams()
            {
                source = "universal",
                url = ul,
                //limit = 20,
                //pages = 1,
                //start_page = 1,
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
