using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Gllo.Sys
{
    public class HttpWebClient
    {
        private CookieContainer cookie;
        private HttpWebResponse response;
        private HttpWebRequest request;

        public string Referer
        {
            get;
            set;
        }

        public Encoding HttpEncoding
        {
            get;
            set;
        }

        public HttpWebClient()
        {
            cookie = new CookieContainer();
            HttpEncoding = System.Text.Encoding.UTF8;
        }

        private void Post(IDictionary<string, string> postData)
        {
            if (postData != null && postData.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var kv in postData)
                {
                    sb.Append(kv.Key)
                        .Append("=")
                        .Append(System.Web.HttpUtility.UrlEncode(kv.Value, this.HttpEncoding))
                        .Append("&");
                }

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] buffer = this.HttpEncoding.GetBytes(sb.ToString());
                request.ContentLength = buffer.Length;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public string GetString(string url, IDictionary<string, string> postData)
        {
            request = (HttpWebRequest)WebRequest.Create(url);

            request.CookieContainer = cookie;
            request.Referer = this.Referer;

            this.Post(postData);

            try
            {
                response = (HttpWebResponse)request.GetResponse();

                string result;
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), this.HttpEncoding))
                {
                    result = sr.ReadToEnd();
                }

                response.Close();

                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public IDictionary<string, object> GetJsonData(string url, IDictionary<string, string> postData)
        {
            string res = GetString(url, postData);
            if (res != null)
            {
                try
                {
                    return new JavaScriptSerializer().Deserialize<IDictionary<string, object>>(res);
                }
                catch
                {
                }
            }
            return null;
        }

        public byte[] GetImage(string url)
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Referer = this.Referer;
            request.CookieContainer = cookie;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                return null;
            }

            System.Drawing.Bitmap image;
            using (Stream stream = response.GetResponseStream())
            {
                image = new System.Drawing.Bitmap(stream);
            }

            response.Close();
            //输出到浏览器
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                image.Dispose();
                return ms.ToArray();
            }
        }
    }

}
