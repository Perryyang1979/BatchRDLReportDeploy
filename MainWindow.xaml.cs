using BatchRDLReportDeploy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BatchRDLReportDeploy.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using BatchRDLReportDeploy.ReportService;
using BatchRDLReportDeploy.Services;
using SystemTask = System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace BatchRDLReportDeploy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string ServerConfigListFileName = "ServerConfig.xml";

        private ObservableCollection<ReportServer> _reportServers = new ObservableCollection<ReportServer>();
        private ObservableCollection<DisplayLog> _displayLogs = new ObservableCollection<DisplayLog>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class DisplayLog
        {
            public string Description { get; set; }
            public Brush BackgroundBrush { get; set; }
        }

        public ObservableCollection<ReportServer> ReportServers
        {
            get
            {
                return _reportServers;
            }
            set
            {
                _reportServers = value;
                OnPropertyChanged(nameof(ReportServers));
            }
        }

        public ReportServer SelectedReportServer { get; set; }

        public ObservableCollection<DisplayLog> DisplayLogs
        {
            get { return _displayLogs; }
            set
            {
                _displayLogs = value;
                OnPropertyChanged(nameof(DisplayLogs));
            }
        }

        private bool defaultCredential = true;

        private string username = string.Empty;

        private string password = string.Empty;

        private string localPath = string.Empty;
           

        public MainWindow()
        {
            InitializeComponent();

            var reportListFile = $"{System.IO.Directory.GetCurrentDirectory()}\\{ServerConfigListFileName}";

            if (File.Exists(reportListFile))
            {
                var reportServerList = Utils.DeSerializeObject<ReportServerListDetail>(reportListFile);
                ReportServers = new ObservableCollection<ReportServer>(reportServerList.ReportServerDetails);
            }

            cbCredintial.IsChecked = true;
            gridCredential.Visibility = Visibility.Hidden;
        }


        private void OpenPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = @"c:\";
            if (dlg.ShowDialog() == true)
            {
                tbLocalPath.Text = dlg.ResultPath;
            }
        }

        private void AddLogMessage(string message, Color rowColor)
        {
            Dispatcher.Invoke((() =>
            {
                var displayLog = new DisplayLog
                {
                    Description = message,
                    BackgroundBrush = new SolidColorBrush(rowColor)
                };

                _displayLogs.Add(displayLog);
                DisplayLogs = _displayLogs;
            }));
        }


        private async void  btDeploy_Click(object sender, RoutedEventArgs e)
        {

            defaultCredential = cbCredintial.IsChecked ?? true;
            username = tbUserName.Text.Trim();
            password = tbPassword.Text.Trim();
            localPath = tbLocalPath.Text.Trim();

            DisplayLogs.Clear();

            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
 
            await DeployReportParalle();
            stopWatch.Stop();

            lbDeployResult.Text = $"Total Deploy time:{stopWatch.ElapsedMilliseconds}";


            var path = $"{System.IO.Directory.GetCurrentDirectory()}\\{ServerConfigListFileName}";

            Utils.SerializeObject(new ReportServerListDetail()
            {
                ReportServerDetails = ReportServers.ToList(),
                LastUpdateTime = DateTime.Now
            }, path
             );
        }


        private async SystemTask.Task DeployReportParalle()
        {
            List<SystemTask.Task<DeployResult>> tasks = new List<SystemTask.Task<DeployResult>>();

            foreach(var reportServer in ReportServers)
            {
                if (reportServer.EnableDeploy)
                {
                    AddLogMessage($"{reportServer.ServerUrl} is Start to deploy.", Colors.Orange);
                    tasks.Add(SystemTask.Task.Run(() => DepolyReports(reportServer)));
                }
                else
                    AddLogMessage($"{reportServer.ServerUrl} is ignore to deploy.", Colors.Orange);
            }

            var results = await SystemTask.Task.WhenAll(tasks);

            foreach(var result in results)
            {
                DisplayResult(result);
            }
        }

        private void DisplayResult(DeployResult result)
        {
           foreach(var singleResult in result.SingleReportResults)
            {
                var displaymessage = string.Empty;
                if(singleResult.Status ==0)
                {
                    displaymessage = $"{singleResult.ReportName} is deployed on {result.ServerName} succesfully without any warnings";

                    AddLogMessage(displaymessage, Colors.Green);
                }
                else if(singleResult.Status ==1)
                {

                    displaymessage = $"{singleResult.ReportName} is deployed on {result.ServerName} succesfully with warnings {singleResult.Warnings.ToString()} ";
                    AddLogMessage(displaymessage, Colors.Yellow);
                }
                else
                {
                    displaymessage = $"{singleResult.ReportName} is deployed on {result.ServerName} Failed and Error is {singleResult.ErrorMessage} ";
                    AddLogMessage(displaymessage, Colors.Yellow);
                }
            }
        }


        private DeployResult DepolyReports(ReportServer reportserver)
        {
            ReportingService2010 rs = new ReportingService2010();
            var deployManager = new DeployManager();
            var Url = reportserver.ServerUrl;
            if (!Url.ToUpper().Contains("REPORTSERVICE2010.ASMX"))
            {
                Url += @"/reportservice2010.asmx";
            }

            rs.Url = Url;

            if (defaultCredential)
                rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            else
                rs.Credentials = new System.Net.NetworkCredential(username,password, string.Empty);

            var ReportFiles = deployManager.GetReportsFiles(localPath);

            ///Publish all files to Reporting Server
            return deployManager.PublishReports(ReportFiles, rs, reportserver.ServerUrl);
        }


        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            var path = $"{System.IO.Directory.GetCurrentDirectory()}\\{ServerConfigListFileName}";

            Utils.SerializeObject(new ReportServerListDetail()
            {
                ReportServerDetails = ReportServers.ToList(),
                LastUpdateTime = DateTime.Now
            }, path
             );
            this.Close();
        }


        private void cbCredintial_Checked(object sender, RoutedEventArgs e)
        {
                gridCredential.Visibility = Visibility.Hidden;
        }

        private void cbCredintial_Unchecked(object sender, RoutedEventArgs e)
        {
                gridCredential.Visibility = Visibility.Visible;
        }
    }
}
