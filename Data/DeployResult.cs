using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRDLReportDeploy.Data
{
    public class DeployResult
    {  
        public string ServerName { get; set; }
       public  List<SingleReportResult> SingleReportResults { get; set; }
 

        public class SingleReportResult
        {
            public string ReportName { get; set; }

            public int Status { get; set; }  // 0 successfull without warnings 1 successful with warnings 2 failed

            public string ErrorMessage { get; set; }

            public string[] Warnings { get; set; }

        }
    }
}
