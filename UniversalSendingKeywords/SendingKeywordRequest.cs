using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Web;

namespace UniversalSendingKeywords
{
    class SendingKeywordRequest
    {
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

        private void SendToDb(int seid, string response)
        {
            string date = DateTime.Today.ToString("yyyy-MM-dd");
            StringBuilder sb = new StringBuilder();

            JObject jo = JObject.Parse(response);
            var links = from p in jo["queries"] select p;

            foreach (JToken link in links)
            {
                string kw = link["query"].Value<string>();
                string jobid = link["id"].Value<string>();

                string qry = "insert into dashboard_data_sending (date, name, seid, jobid) values(Convert(varchar(10),'" + date + "',103), N'" + kw.Replace("'", "''") + "', " + seid + ", '" + jobid + "'); ";
                sb.Append(qry);
            }

            try
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    using (SqlConnection con = new SqlConnection(strConn()))
                    {
                        con.Open();
                        using (SqlCommand comm = new SqlCommand(sb.ToString(), con))
                        {
                            comm.CommandTimeout = 0;
                            comm.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetOxylabsWebDataSources(string[] arr, string type, int seid)
        {
            Uri queryUri = new Uri("https://data.oxylabs.io/v1/queries/batch");
            string username = "gpidatametrics";
            string password = "sdV5X3fcX6";
            string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));

            string callbackURL = "http://previous.azurewebsites.net/api/callbackrapidtrackingmobile/";

            string[] keyword = arr;

            OxyParams op = new OxyParams()
            {
                source = "universal",
                query = keyword,
                url = keyword,
                //url = ul,
                limit = 10,
                pages = 1,
                start_page = 1,
                callback_url = callbackURL,
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
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
                res.Close();

                SendToDb(seid, response);
            }
            catch (WebException ex)
            {
                string errorMsg = string.Empty;
                using (WebResponse res = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)res;
                    errorMsg = string.Format("API Error: StatusCode {0}", httpResponse.StatusCode);

                    using (Stream data = res.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        errorMsg += "\r\n" + reader.ReadToEnd();
                    }
                }
                throw new Exception(errorMsg);
            }
        }


        string[] urlbatch;
        char ch = ',';
        string urls = "";
        public void getTop100(string keyword, int seid)
        {
            try
            {
                string[] kwdsarray = keyword.Split(',');
                string type = "";
                for (int j = 0; j < kwdsarray.Length; j++)
                {
                    string kw = kwdsarray[j].ToString();
                    switch (seid)
                    {
                        //case 19:
                        //    {
                        //        type = "desktop";
                        //        for (int i = 1; i <= 10; i++)
                        //        {
                        //            urls += "http://nova.rambler.ru/search?pagelen=10&query=" + kw + "&page=" + i + ch;
                        //        }
                        //        break;
                        //    }
                        case 38:
                            {
                                type = "desktop";
                                for (int i = 0; i < 2; i++)
                                {
                                    int st = (50 * i) + 1;
                                    urls += "https://hk.search.yahoo.com/search?p=" + kw + "&ei=UTF-8&b=" + st + "&pz=100" + ch;

                                }

                                break;
                            }
                        case 48:
                            {
                                type = "desktop";
                                for (int i = 0; i < 10; i++)
                                {
                                    int st = (10 * i) + 1;
                                    urls += "https://search.yahoo.co.jp/search?p=" + kw + "&ei=UTF-8&fr=top_ga1&n=10&fl=0&x=wrt&b=" + st + "&pz=10" + ch;
                                }

                                break;
                            }
                        case 138:
                            {
                                type = "desktop";
                                for (int i = 0; i < 10; i++)
                                {
                                    int n = (i * 10) + 1;
                                    urls += "http://web.search.naver.com/search.naver?where=webkr&query=" + HttpUtility.UrlEncode(HttpUtility.HtmlDecode(kw)) + "&start=" + n + "&display=10&ie=utf8" + ch;
                                }
                                    break;
                            }
                        case 175:
                            {
                                type = "desktop";
                                for (int i = 1; i <= 10; i++)
                                {
                                    urls += "https://www.so.com/s?ie=utf-8&shb=1&src=360sou_newhome&q=" + kw + "&pn=" + i + ch;
                                }

                                    break;
                            }
                        case 193:
                            {
                                type = "mobile";
                                for (int i = 1; i <= 10; i++)
                                {
                                    urls += "https://m.so.com/nextpage?q= " + kw + " &pn=" + i + "&ajax=1" + ch;
                                }

                                    break;
                            }
                        case 194:
                            {
                                type = "mobile";
                                for (int i = 0; i < 10; i++)
                                {
                                    int st = (10 * i) + 1;

                                    urls += "https://search.yahoo.co.jp/search?p=" + kw + "&x=wrt&ei=UTF-8&fr=top_ga1&n=10&fl=0&b=" + st + "&pz=10" + ch;
                                }

                                break;
                            }
                        case 256:
                            {
                                type = "mobile";
                                for (int i = 0; i < 2; i++)
                                {
                                    int st = (50 * i) + 1;
                                    urls += "https://hk.search.yahoo.com/search?p=" + kw + "&ei=UTF-8&.tsrc=yfp-hrmob-sb&b=" + st + "&pz=100" + ch;

                                }

                                break;
                            }
                        case 276:
                            {
                                type = "desktop";
                                for (int i = 1; i <= 10; i++)
                                {
                                    urls += "https://www.sogou.com/web?query=" + kw + "&ie=utf8&page=" + i + ch;
                                }
                                break;
                            }
                        case 277:
                            {
                                type = "mobile";
                                for (int i = 1; i <= 10; i++)
                                {
                                    urls += "https://m.sogou.com/web/searchList.jsp?keyword=" + kw + "&p=" + i + "&s_from=pagenext&showextquery=1" + ch;
                                }
                                break;
                            }
                        case 278:
                            {
                                type = "mobile";
                                for (int i = 1; i <= 10; i++)
                                {
                                    urls += "https://m.sm.cn/s?q=" + kw + "&from=smor&safe=1&page=" + i + ch;
                                }
                                break;
                            }
                        case 340:
                            {
                                type = "desktop";
                                for (int i = 1; i <= 5; i++)
                                {
                                    urls += "https://www.pricesearcher.com/gb/search/?p=" + i + "&q=" + kw + ch;
                                }
                                break;
                            }
                        case 440:
                            {
                                type = "mobile";
                                for (int i = 0; i < 7; i++)
                                {
                                    int n = (i * 15) + 1;

                                    urls += "https://m.search.naver.com/search.naver?where=m&sm=mtb_pge&query=" + HttpUtility.UrlEncode(HttpUtility.HtmlDecode(kw)) + "&start=" + n + "&page=" + (i + 2) + "&display=15" + ch;
                                }
                                    break;
                            }
                    }
                    urls = urls.Remove(urls.Length - 1);
                    urls = HttpUtility.UrlDecode(urls);
                    //urlbatch = new[] { urls };
                    urlbatch = urls.Split(',');
                    GetOxylabsWebDataSources(urlbatch, type, seid);
                }
                //urls = urls.Remove(urls.Length - 1);
                //urls = HttpUtility.UrlDecode(urls);
                //urlbatch = new[] { urls };
                //GetOxylabsWebDataSources(urlbatch, type, seid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public string[] GetURLBatch(string kw, string type)
        //{
        //    char ch = ',';
        //    string urls = "";
        //    if (type == "desktop")
        //    {
        //        for (int i = 1; i <= 2; i++)
        //        {
        //            int st = (50 * i) + 1;
        //            urls += "https://hk.search.yahoo.com/search?p=" + kw + "&ei=UTF-8&b=" + st + "&pz=100" + ch;

        //        }
        //    }
        //    if (type == "mobile")
        //    {
        //        for (int i = 0; i < 2; i++)
        //        {
        //            int st = (50 * i) + 1;
        //            urls += "https://hk.search.yahoo.com/search?p=" + kw + "&ei=UTF-8&.tsrc=yfp-hrmob-sb&b=" + st + "&pz=100" + ch;

        //        }
        //    }

        //    urls = urls.Remove(urls.Length - 1);
        //    urls = HttpUtility.UrlDecode(urls);
        //    return new[] { urls };

        //}

    }
}


