using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xenqu;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Xenqu.Configuration config = new Xenqu.Configuration();

                config.Initialize( 
                    "<WILL_BE_PROVIDED>",     // Client Id
                    "<WILL_BE_PROVIDED>",     // Client Secret
                    "<WILL_BE_PROVIDED>",     // Subscriber
                    "<PATH_TO_PRIVATE_KEY>"   // Key File
                );

                Xenqu.OAuth2.Authorize();
                
                var user = new User();
                var info = user.info();
                
                Console.WriteLine(info["contact"]["display_name"]);
                Console.WriteLine(info["login"]["user_name"]);
                Console.WriteLine(info["account"]["account_name"]);
                
                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }



    }
}
