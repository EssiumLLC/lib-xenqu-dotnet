using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TinyOAuth1;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Runtime.Serialization;

namespace Xenqu
{
    public static class Authorization
    {
        public static string serviceProviderUrl;
        public static string consumerKey;
        public static string consumerSecret;
        public static string tokenKey;
        public static string tokenSecret;

    }
    
    public class Configuration
    {
        public void Initialize( string consumerKey, string consumerSecret, string tokenKey, string tokenSecret )
        {
            Authorization.serviceProviderUrl = "https://xenqu.com/api/";
            Authorization.consumerKey = consumerKey;
            Authorization.consumerSecret = consumerSecret;
            Authorization.tokenKey = tokenKey;
            Authorization.tokenSecret = tokenSecret;              
        } 
    }
    
    public class ServiceProvider
    {
        private const int RequestTimeOut = 1000 * 60 * 10;

        private static volatile ServiceProvider instance;
        private static object syncLock = new Object();

        private ServiceProvider()
        {
            
        }

        public static ServiceProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new ServiceProvider();
                    }
                }
                return instance;
            }
        }

        private HttpWebRequest GenerateRequest(string serviceName, string contentType, System.Net.Http.HttpMethod requestMethod, string Referer = "")
        {
            Console.WriteLine( Authorization.consumerKey );
            Console.WriteLine( Authorization.consumerSecret );
            Console.WriteLine( Authorization.tokenKey );
            Console.WriteLine( Authorization.tokenSecret );
            Console.WriteLine( GetFullServiceName(serviceName) );
            
            var config = new TinyOAuthConfig
            {
                ConsumerKey = Authorization.consumerKey,
                ConsumerSecret = Authorization.consumerSecret
            };
            
            var tinyOAuth = new TinyOAuth(config);
            
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(GetFullServiceName(serviceName));
            httpWebRequest.Method = requestMethod.ToString();
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Timeout = RequestTimeOut;
            httpWebRequest.Headers.Add( "Authorization", tinyOAuth.GetAuthorizationHeader(Authorization.tokenKey, Authorization.tokenSecret, Regex.Replace(GetFullServiceName(serviceName), "(.*)\\/api(.*)", "$1$2"), requestMethod).ToString() );
            httpWebRequest.Referer = Referer;
            return httpWebRequest;
        }

        private string GetRequestResponse(HttpWebRequest httpWebRequest)
        {
            if (httpWebRequest == null) throw new ArgumentNullException("httpWebRequest");
            string responseString = null;
            try
            {
                using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            var reader = new StreamReader(responseStream);
                            responseString =  reader.ReadToEnd();
                            reader.Close();
                            responseStream.Close();
                        }
                    }
                    response.Close();
                }
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Unhandled exception while reading response - {0}", exception.Message));
            }
            return responseString;
        }
        
         public string GetData(string serviceName)
        {
            return  GetData(serviceName, null);
        }

         public string PostData(string serviceName, string contentType, string data)
        {
            return PostData(serviceName, contentType, data, null);
        }

         private string GetData(string serviceName, string Referer = "")
        {
            var request = GenerateRequest(serviceName, string.Empty, HttpMethod.Get, Referer);
            return  GetRequestResponse(request);
        }

         private string PostData(string serviceName, string contentType, string data, string Referer = "")
        {
            var request = GenerateRequest(serviceName, contentType, HttpMethod.Post, Referer);
            var bytes = Encoding.ASCII.GetBytes(data);
            request.ContentLength = bytes.Length;
            if (bytes.Length > 0)
            {
                using (var requestStream = request.GetRequestStream())
                {
                    if (!requestStream.CanWrite) throw new Exception("The data cannot be written to request stream");
                    try
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(string.Format("Error while writing data to request stream - {0}", exception.Message));
                    }
                }
            }
            return  GetRequestResponse(request);
        }

        private string GetFullServiceName(string serviceName)
        {
            string slash = Authorization.serviceProviderUrl.EndsWith("/") ? "" : "/";
            string service = serviceName.StartsWith("/") ? serviceName.Substring(1, serviceName.Length -1) : serviceName;
            return Authorization.serviceProviderUrl + slash + service;
        }

    }


}