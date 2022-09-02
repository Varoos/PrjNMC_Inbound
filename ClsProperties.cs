using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrjNMC_Inbound
{
    public class ClsProperties
    {

       
        public class LogingResult
        {
            public int iStatus { get; set; }
            public string Auth_Token { get; set; }
            public string sMessage { get; set; }
            public object obj { get; set; }
        }
        public class Result
        {
            public int iStatus { get; set; }

            public string Voucher_No { get; set; }
            public string sMessage { get; set; }
            public object obj { get; set; }
        }

        public class JVResult
        {
            public string iStatus { get; set; }
            public string sMessage { get; set; }
            public object obj { get; set; }
        }

        public class Datum
        {
            public string Username { get; set; }
            public string password { get; set; }
            public string CompanyCode { get; set; }
        }
       
        public class Lolgin
        {
            public List<Datum> data { get; set; }
        }

        public class Datumresult
        {
            public string fSessionId { get; set; }
        }

        public class Resultlogin
        {
            public List<Datumresult> data { get; set; }
            public string url { get; set; }
            public int result { get; set; }
            public string message { get; set; }
        }

        public class PostingData2
        {
            public PostingData2()
            {
                data = new List<Hashtable>();
            }
            public List<Hashtable> data { get; set; }
        }

        public class HashData
        {
            public string url { get; set; }
            public List<Hashtable> data { get; set; }
            public int result { get; set; }
            public string message { get; set; }

        }
        public class Data
        {
            public List<Hashtable> Body { get; set; }
            public Hashtable Header { get; set; }
            public List<Hashtable> Footer { get; set; }
        }
        public class PostingData
        {
            public PostingData()
            {
                data = new List<Hashtable>();
            }
            public List<Hashtable> data { get; set; }

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