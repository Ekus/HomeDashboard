using System;
using System.IO;
using System.Web;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace AspNetWebProxy
{
    /// <summary>
    /// Receives a web request with target URL, invokes another web request to the target URL with added NTLM credentials, and returns the response to the original caller.
    /// </summary>
    public class MiniProxy : IHttpHandler
    {
        Logger log = LogManager.GetCurrentClassLogger();

        public void ProcessRequest(HttpContext context)
        {
            var url = context.Request.QueryString["url"];
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.StartsWith("http")) url = "http://" + url;

            var targetUri = new Uri(url);

            var whitelistedDomains = new List<string>() {
                "www.duolingo.com",
                "192.168.29.2"
            };
            if (!whitelistedDomains.Contains(targetUri.Host, StringComparer.InvariantCultureIgnoreCase)) throw new ApplicationException("Invalid host in URL");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            //if (proxy != null) req.Proxy = new WebProxy(proxy, false);

            var origHeaders = context.Request.Headers;

            // copy the original request's headers to our internal request, with some special cases where applicable
            foreach (string hKey in origHeaders.Keys)
            {
                switch (hKey.ToLowerInvariant())
                {
                    case "connection": if (origHeaders[hKey].ToLowerInvariant() == "keep-alive") req.KeepAlive = true; break;
                    case "accept": req.Accept = origHeaders[hKey]; break;
                    case "accept-encoding": break; // if (origHeaders[hKey].Split(',').Contains("gzip")) req.AutomaticDecompression = DecompressionMethods.None; break;
                    //case "accept-encoding" : req.enc
                    case "host": break;
                    case "referer": req.UserAgent = origHeaders[hKey]; break;
                    case "user-agent": req.UserAgent = origHeaders[hKey]; break;
                    //case "xxxxx": req.Connection = origHeaders[hKey]; break;
                    default: req.Headers.Add(hKey, origHeaders[hKey]); break;
                }
            }

            context.Response.BufferOutput = true;
            //HttpCookieCollection origCookies = context.Request.Cookies;

            //req.CookieContainer = new CookieContainer();
            //foreach (Cookie c in origCookies.) {
            //    req.CookieContainer.Add(c);
            //}


            // if SOAPAction header is required, add it here...
            //req.Headers.Add("SOAPAction", context.Request.Headers["SOAPAction"]);
            //req.ContentType = "text/xml;charset=\"utf-8\"";
            //req.Accept = "text/xml";
            //req.Method = context.Request.HttpMethod; // "POST";
            //req.Credentials = new NetworkCredential(

            //req.PreAuthenticate = true;
            //req.KeepAlive = false;
            //req.Timeout = -1;


            if (req.Method == "POST")
            {
                // copy original request "body" to the new request
                string input = new StreamReader(context.Request.InputStream).ReadToEnd();

                // encode it using the predefined encoding (see above, req.ContentType)
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] bytesToSend = encoding.GetBytes(input);

                // Set the content length of the string being posted.
                req.ContentLength = bytesToSend.Length;

                Stream newStream = req.GetRequestStream(); // This method has the side effect of initiating delivery of the request in its current state to the server. Any properties like the request method, content type or content length as well as any custom headers need to be assigned before calling the GetRequestStream() method.
                newStream.Write(bytesToSend, 0, bytesToSend.Length);

                // Close the Stream object.
                newStream.Close();
            } // else GET, no body to send. Other verbs are not supported at the moment.

            HttpWebResponse resp = null;
            string output = null;
            string status = null;
            Stream respStream = null;
            MemoryStream temp = new MemoryStream();
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                log.Debug("Status: {0}", ex.Status);
                log.Debug("Exception: {0}", ex);
                throw;

                if (ex.Response != null)
                {
                    resp = (HttpWebResponse)ex.Response;
                }
                else { output = ex.ToString(); }
            }

            if (null != resp)
            {
                byte[] data;
                using (respStream = resp.GetResponseStream())
                {

                    //if (resp.ContentType.StartsWith("text"))
                    //{
                    //    StreamReader r = new StreamReader(respStream);
                    //    // process SOAP return doc here. For now, we'll just send the XML out to the browser ...
                    //    output = r.ReadToEnd();
                    //}
                    //else
                    //{

                    //    //int length= (int)respStream.Length;
                    //    //log.Debug(length);
                    //    //binOutput = r.ReadBytes(length);
                    //    //log.Debug(binOutput.Length);
                    //}
                    status = string.Format("{0} {1}", (int)((HttpWebResponse)resp).StatusCode, ((HttpWebResponse)resp).StatusDescription);
                    log.Debug("Remote server returned status: {0}", status);

                    log.Debug("Remote server returned headers:");
                    foreach (string h in resp.Headers.Keys) { log.Debug("{0}: {1}", h, resp.Headers[h]); }

                    log.Debug("Proxy scrypt is returning headers:");
                    foreach (string h in context.Response.Headers.Keys) { log.Debug("{0}: {1}", h, resp.Headers[h]); }

                    //context.Response.Headers.Add(resp.Headers);
                    context.Response.ContentType = resp.ContentType;
                    //                context.Response.ContentEncoding = System.Text.Encoding.HttpResponse.Conte resp.ContentEncoding;
                    //context.Response.ContentType = resp.ContentType;
                    //context.Response.ContentType = resp.ContentType;



                    log.Debug("Proxy response status: {0}", context.Response.Status);
                    log.Debug("Proxy response status code: {0}", context.Response.StatusCode);

                    log.Debug(output);
                    context.Response.Status = status;
                    log.Debug("Proxy response status: {0}", context.Response.Status);
                    log.Debug("Proxy response status code: {0}", context.Response.StatusCode);


                    //            if (resp.ContentType.ToLowerInvariant().StartsWith("text"))
                    //            { 
                    //                StreamReader r = new StreamReader(respStream);
                    //                // process SOAP return doc here. For now, we'll just send the XML out to the browser ...
                    //                output = r.ReadToEnd();
                    //                StreamWriter sw = new StreamWriter(context.Response.OutputStream);
                    //                sw.Write(output);
                    ////                context.Response.Write(output);
                    //            }
                    //            else
                    //            {



                    data = GetDataFromStream(respStream);
                }
                //    MemoryStream ms = new MemoryStream(data);
                context.Response.AddHeader("Content-Length", data.Length.ToString());

                //BinaryWriter bw = new BinaryWriter(context.Response.OutputStream);
                //    context.Response.ClearHeaders();
                context.Response.ContentType = resp.ContentType;
                context.Response.BinaryWrite(data); // bw.Write();
                //}
            }
            try
            {
                context.Response.Flush();
                context.Response.Close();
            }
            catch (Exception ex) { log.Error(ex); }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public byte[] GetDataFromStream(Stream inputStream)
        {
            byte[] result;
            byte[] buffer = new byte[4096];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = inputStream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, count);
                } while (count != 0);

                result = memoryStream.ToArray();
            }
            return result;
        }
    }
}