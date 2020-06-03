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
                    "<WILL_BE_PROVIDED>",  // Consumer Key
                    "<WILL_BE_PROVIDED>",  // Consumer Secret
                    "<WILL_BE_PROVIDED>",  // Token Key
                    "<WILL_BE_PROVIDED>"   // Token Secret
                );
                
                Reports dsr = new Reports();
                string str = dsr.ResultsCSV( "<SEE_DOCUMENTATION>" );
                
                Console.WriteLine(str);
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
