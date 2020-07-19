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
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xenqu
{
    public static class Authorization
    {
        public static string serviceProviderUrl;
        public static string clientId;
        public static string clientSecret;
        public static string subscriber;
        public static string privateKey;
        public static string tokenKey;
        public static string tokenSecret;
        public static double tokenExpires;

    }
    
    public class Configuration
    {
        public void Initialize( string clientId, string clientSecret, string subscriber, string keyFile )
        {
            Authorization.serviceProviderUrl = "https://xenqu.com/api/";
            Authorization.clientId = clientId;
            Authorization.clientSecret = clientSecret;     
            Authorization.subscriber = subscriber;     
            Authorization.privateKey = File.ReadAllText( keyFile );      
        } 
    }
    
    public static class OAuth2 {
        
        public static void Authorize() {

            var txtreader = new StringReader( Authorization.privateKey );
            var rsaParams = DotNetUtilities.ToRSAParameters( (RsaPrivateCrtKeyParameters)new PemReader( txtreader ).ReadObject() );

            var jwtsigner = new JsonWebTokenHandler();
            var now = DateTime.UtcNow;

            var descriptor = new SecurityTokenDescriptor
                {
                    Issuer = Authorization.clientId,
                    Audience = "https://xenqu.com",
                    Expires = now.AddMinutes(5),
                    Claims = new Dictionary<string,object>{ { "sub", Authorization.subscriber } },
                    SigningCredentials = new SigningCredentials(new RsaSecurityKey( rsaParams ), "RS256" )
                };
            
            var data = "grant_type=" + Uri.EscapeDataString( "urn:ietf:params:oauth:grant-type:jwt-bearer" ) +
                       "&assertion=" + Uri.EscapeDataString( jwtsigner.CreateToken( descriptor ) );            
            
            var provider = ServiceProvider.Instance;
            var results = JObject.Parse( provider.PostData( "/oauth2/token", "application/x-www-form-urlencoded", data ) );
            
            Authorization.tokenKey = (string)results["token"];
            Authorization.tokenSecret = (string)results["token_secret"];
            Authorization.tokenExpires = Convert.ToDouble((string)results["expires"]);
            
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
            var bytes = Encoding.UTF8.GetBytes(data);
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

        private HttpWebRequest GenerateRequest(string serviceName, string contentType, System.Net.Http.HttpMethod requestMethod, string Referer = "")
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(GetFullServiceName(serviceName));
            httpWebRequest.Method = requestMethod.ToString();
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Timeout = RequestTimeOut;
            
            var config = new TinyOAuthConfig
            {
                ConsumerKey = Authorization.clientId,
                ConsumerSecret = Authorization.clientSecret
            };
            
            var tinyOAuth = new TinyOAuth(config);
            
            if ( serviceName == "/oauth2/token" ) {
                httpWebRequest.Headers.Add( "Authorization", "Basic " + System.Convert.ToBase64String( Encoding.UTF8.GetBytes( Authorization.clientId + ":" + Authorization.clientSecret )) );
            } else {
                httpWebRequest.Headers.Add( "Authorization", tinyOAuth.GetAuthorizationHeader(Authorization.tokenKey, Authorization.tokenSecret, Regex.Replace(GetFullServiceName(serviceName), "(.*)\\/api(.*)", "$1$2"), requestMethod).ToString() );
            }

            httpWebRequest.Referer = Referer;
            httpWebRequest.UserAgent = "essium-dotnet-connector";
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
            catch (WebException exception)
            {
                var stream = exception.Response.GetResponseStream();
                var reader = new StreamReader( stream );
              
                throw new Exception(string.Format("{0}: {1}", exception.Message, reader.ReadToEnd()));
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Unhandled exception while reading response - {0}", exception.Message));
            }
            return responseString;
        }
        
        private string GetFullServiceName(string serviceName)
        {
            string slash = Authorization.serviceProviderUrl.EndsWith("/") ? "" : "/";
            string service = serviceName.StartsWith("/") ? serviceName.Substring(1, serviceName.Length -1) : serviceName;
            return Authorization.serviceProviderUrl + slash + service;
        }

    }
}