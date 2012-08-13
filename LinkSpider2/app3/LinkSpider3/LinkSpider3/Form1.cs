using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using LinkSpider3.Process;


namespace LinkSpider3
{
    public partial class Form1 : Form
    {

        CollectorManager CM;

        public Form1()
        {
            InitializeComponent();
            
            this.CM = new CollectorManager();
        }

        #region Invokes
        delegate void RefreshListCurrentlyProcessingHandler();
        void RefreshListCurrentlyProcessing()
        {
            if (this.InvokeRequired)
                this.Invoke(new RefreshListCurrentlyProcessingHandler(RefreshListCurrentlyProcessing));
            else
            {
                this.lstLinksCurrentlyProcessing.DataSource = null;
                this.lstLinksCurrentlyProcessing.DataSource =
                    this.CM.LinksCurrentlyProcessing;
            }
        }

        delegate void RefreshListAccessedHandler();
        void RefreshListAccessed()
        {
            if (this.InvokeRequired)
                this.Invoke(new RefreshListAccessedHandler(RefreshListAccessed));
            else
            {
                //string[] links = new string[this.CM.LinksAccessing.Count];
                //this.CM.LinksAccessing.CopyTo(links);

                this.lstLinksAccessed.DataSource = null;
                this.lstLinksAccessed.DataSource = this.CM.LinksAccessing.ToArray();
            }
        }

        delegate void ChangeStatusLabelHandler(string text);
        void ChangeStatusLabel(string text)
        {
            if (this.InvokeRequired)
                this.Invoke(new ChangeStatusLabelHandler(ChangeStatusLabel), text);
            else
                this.toolStripStatusLabel1.Text = text;
        }
        #endregion

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            // Save the configuration first 
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!config.AppSettings.Settings.AllKeys.Contains("DB"))
                config.AppSettings.Settings.Add("DB", string.Empty);
            config.AppSettings.Settings["DB"].Value =
                (string.IsNullOrEmpty(this.txtRedisServer.Text) ? "127.0.0.1" : this.txtRedisServer.Text);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            
            // Start the collector manager
            if (this.btnStartStop.Text == "Start")
            {
                this.btnStartStop.Text = "Close";
                CM.Start(ConfigurationManager.AppSettings["DB"]);
                this.timer1.Enabled = true;
            }
            else
            {
                this.btnStartStop.Text = "Start";
                this.timer1.Enabled = false;
                CM.Stop();
                this.Close();
            }
        }

        private void btnAddUrlToPool_Click(object sender, EventArgs e)
        {
            CM.AddUrl(this.textBox1.Text);
        }

        private void btnLoadCsvToPool_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV files (*.csv)|*.csv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(ofd.OpenFile()))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (IsFormClosing)
                            break;

                        string[] csvTokens = line.Split(',');
                        //if (Convert.ToInt32(csvTokens[0]) > 30000)
                        //{
                            CM.AddUrl("http://" + csvTokens[1]);
                            Application.DoEvents();
                            toolStripStatusLabel2.Text =
                                string.Format("Loading alexa url: {0}", csvTokens[0]);
                        //}
                    }

                    toolStripStatusLabel2.Text = string.Empty;
                    sr.Close();
                }
            }
        }

        bool IsFormClosing = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsFormClosing = true;
        }

        private void txtWorkerCount_TextChanged(object sender, EventArgs e)
        {
            this.CM.SetWorkerCount(int.Parse(this.txtWorkerCount.Text));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshListCurrentlyProcessing();
            RefreshListAccessed();
            ChangeStatusLabel(string.Format("Collectors executed: {0}",
                this.CM.Collectors.Count));
        }

        private void btnIndexCrawlDate_Click(object sender, EventArgs e)
        {
            CM.ReIndex();
            MessageBox.Show("Indexing done!");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.txtRedisServer.Text = ConfigurationManager.AppSettings["DB"];
            if (string.IsNullOrEmpty(this.txtRedisServer.Text))
            {
                this.txtRedisServer.Text = "127.0.0.1";
            }
        }
    }
}
