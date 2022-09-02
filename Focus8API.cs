using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrjNMC_Inbound
{
    public class Focus8API
    {
        public static string Post(string url, string data, string sessionId, ref string err)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("fSessionId", sessionId);
                    client.Headers.Add("Content-Type", "application/json");
                    var response = client.UploadString(url, data);

                    return response;
                }
            }
            catch (Exception e)
            {

                err = e.Message;
                return null;
            }

        }
    }
    public class APIResponse
    {
        public class Data
        {
            public List<Hashtable> Body { get; set; }
            public Hashtable Header { get; set; }
            public List<Hashtable> Footer { get; set; }
        }

        public class Response
        {
            public string url { get; set; }
            public List<Data> data { get; set; }
            public int result { get; set; }
            public string message { get; set; }
        }
        public class PostResponse
        {
            public string url { get; set; }
            public List<Hashtable> data { get; set; }
            public int result { get; set; }
            public string message { get; set; }
        }
    }
}