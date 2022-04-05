using BatchRDLReportDeploy.Data;
using BatchRDLReportDeploy.ReportService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BatchRDLReportDeploy.Services
{
    public class DeployManager
    {
        /// <summary>
        /// Get All Reports files From Reports Directory
        /// </summary>
        /// <param name="ReportsFolderPath"></param>
        public FileInfo[] GetReportsFiles(string ReportsFolderPath)
        {
            if (ReportsFolderPath == string.Empty) return null;
            DirectoryInfo d = new DirectoryInfo(ReportsFolderPath);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.rdl"); //Getting Text files
            return Files;
        }

        public DeployResult PublishReports(FileInfo[] reports,  ReportingService2010  rsc,string serverURL)
        {
           
            if (reports == null) return null;

            var deployResult = new DeployResult()
            {
                ServerName = serverURL,
                SingleReportResults = new List<DeployResult.SingleReportResult>()
            };
          
            Warning[] Warnings = null;
            foreach (FileInfo ReportFile in reports)
            {
                Byte[] definition = null;
                Warning[] warnings = null;

                try
                {
                    FileStream stream = ReportFile.OpenRead();
                    definition = new Byte[stream.Length];
                    stream.Read(definition, 0, (int)stream.Length);
                    stream.Close();
                }

                catch (IOException e)
                {

                    deployResult.SingleReportResults.Add(new DeployResult.SingleReportResult()
                    {
                        ErrorMessage = e.Message,
                        Status = 2,
                        ReportName = ReportFile.Name,                  
                    });

                    continue;

                }

                try
                {
                    ///Creating Catalog of type Report in report server
                   CatalogItem report = rsc.CreateCatalogItem("Report",
                    ReportFile.Name, @"/Report", true, definition, null, out Warnings);

                    var singleResult = new DeployResult.SingleReportResult()
                    {
                        ReportName = ReportFile.Name,
                    };

                    if (report != null)
                    {
                        singleResult.Status = 0;
                    }
                    if (warnings != null)
                    {
                        singleResult.Status = 1;

                        singleResult.Warnings = warnings.Select(ws => ws.Message).ToArray();

                        singleResult.ErrorMessage = string.Empty;

                    }
                    else
                        singleResult.ErrorMessage = string.Empty;

                    deployResult.SingleReportResults.Add(singleResult);
                }

                catch (Exception e)
                {
                    deployResult.SingleReportResults.Add(new DeployResult.SingleReportResult()
                    {
                        ErrorMessage = e.Message,
                        Status = 2,
                        ReportName = ReportFile.Name,
                    });

                    continue;
                }
            }

            return deployResult;
        }
    }
}
