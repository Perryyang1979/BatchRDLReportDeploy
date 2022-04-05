using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace BatchRDLReportDeploy.Data
{
    [XmlRoot("ReportServerList")]
    public class ReportServerListDetail
    {
        public DateTime LastUpdateTime { get; set; }

        [XmlArray("ReportServerDetail")]
        [XmlArrayItem(typeof(ReportServer))]
        public List<ReportServer> ReportServerDetails  { get; set; }
    }
}




