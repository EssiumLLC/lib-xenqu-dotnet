using System;
using System.Data;
using System.IO;
using CsvHelper;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xenqu
{
    public class Reports
    {
        public string ResultsCSV( string jobId ) 
        {
            return jsonToCSV( ResultsJSON( jobId ), "," );
        }
        
        public List<Dictionary<string, string>> ResultsDataTable( string jobId ) 
        {
            return jsonStringToTable( ResultsJSON( jobId ) );
        }
        
        public string ResultsJSON( string jobId ) 
        {
            string serviceName = "";
            dynamic results;          
            ServiceProvider provider = ServiceProvider.Instance;

            serviceName = "/reporting/results/?job_id=" + jobId + "&status=3&count=1&offset=0&sortby=run_date%3Aasc";  
            results = JObject.Parse( provider.GetData( serviceName ) );
            
            serviceName = "/reporting/results/" + results.data[0]._id;
            results = JObject.Parse( provider.GetData( serviceName ) );

            //return JsonConvert.SerializeObject( results.result_data.records );
            return ((JArray) results.result_data.records ).ToString();
        }     

        private static List<Dictionary<string, string>> jsonStringToTable( string jsonContent )
        {            
            //DataTable dt = JsonConvert.DeserializeObject<DataTable>( jsonContent );
            //return JsonConvert.DeserializeObject<List<Dictionary<string, string>>( jsonContent );
            return JArray.Parse(jsonContent).ToObject<List<Dictionary<string, string>>>();
        }
        
        private static string jsonToCSV( string jsonContent, string delimiter )
        {
            StringWriter csvString = new StringWriter();
            List<Dictionary<string, string>> dt = jsonStringToTable(jsonContent);
            bool firstRow = true;
            
            using (var csv = new CsvWriter(csvString))
            {
                csv.Configuration.Delimiter = delimiter;

                foreach (var row in dt)
                {
                    if ( firstRow )
                    {
                        foreach (var col in row)
                        {
                            csv.WriteField(col.Key);
                        }
                        csv.NextRecord();   
                        firstRow = false;
                    }
                    
                    foreach (var col in row)
                    {
                        csv.WriteField(col.Value);
                    }
                    csv.NextRecord();
                }                    
            }
/*              
                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
*/            
            return csvString.ToString();
        }        
    }
}
