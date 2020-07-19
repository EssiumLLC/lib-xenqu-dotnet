using System;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xenqu
{
    public class User
    {
        public dynamic info() 
        {    
            ServiceProvider provider = ServiceProvider.Instance;
            return JObject.Parse( provider.GetData( "/user/info" ) );
        }
        
    }
}
        