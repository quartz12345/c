namespace LinkSpider3
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstLinksCurrentlyProcessing = new System.Windows.Forms.ListBox();
            this.bsLinksCurrentlyProcessing = new System.Windows.Forms.BindingSource(this.components);
            this.btnStartStop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnAddUrlToPool = new System.Windows.Forms.Button();
            this.btnLoadCsvToPool = new System.Windows.Forms.Button();
            this.lstLinksAccessed = new System.Windows.Forms.ListBox();
            this.txtWorkerCount = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnIndexCrawlDate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRedisServer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkCrawlNew = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.bsLinksCurrentlyProcessing)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstLinksCurrentlyProcessing
            // 
            this.lstLinksCurrentlyProcessing.FormattingEnabled = true;
            this.lstLinksCurrentlyProcessing.ItemHeight = 14;
            this.lstLinksCurrentlyProcessing.Location = new System.Drawing.Point(10, 63);
            this.lstLinksCurrentlyProcessing.Name = "lstLinksCurrentlyProcessing";
            this.lstLinksCurrentlyProcessing.Size = new System.Drawing.Size(691, 256);
            this.lstLinksCurrentlyProcessing.TabIndex = 0;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(272, 32);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(64, 23);
            this.btnStartStop.TabIndex = 1;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 663);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(713, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel1.Text = "Ready";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel3.Text = "toolStripStatusLabel3";
            // 
            // textBox1
            // 
            this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.textBox1.Location = new System.Drawing.Point(272, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(223, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "http://jubacs.somee.com/";
            // 
            // btnAddUrlToPool
            // 
            this.btnAddUrlToPool.Location = new System.Drawing.Point(501, 9);
            this.btnAddUrlToPool.Name = "btnAddUrlToPool";
            this.btnAddUrlToPool.Size = new System.Drawing.Size(109, 23);
            this.btnAddUrlToPool.TabIndex = 4;
            this.btnAddUrlToPool.Text = "Add Url to Pool";
            this.btnAddUrlToPool.UseVisualStyleBackColor = true;
            this.btnAddUrlToPool.Click += new System.EventHandler(this.btnAddUrlToPool_Click);
            // 
            // btnLoadCsvToPool
            // 
            this.btnLoadCsvToPool.Location = new System.Drawing.Point(501, 32);
            this.btnLoadCsvToPool.Name = "btnLoadCsvToPool";
            this.btnLoadCsvToPool.Size = new System.Drawing.Size(109, 23);
            this.btnLoadCsvToPool.TabIndex = 5;
            this.btnLoadCsvToPool.Text = "Load CSV to Pool";
            this.btnLoadCsvToPool.UseVisualStyleBackColor = true;
            this.btnLoadCsvToPool.Click += new System.EventHandler(this.btnLoadCsvToPool_Click);
            // 
            // lstLinksAccessed
            // 
            this.lstLinksAccessed.FormattingEnabled = true;
            this.lstLinksAccessed.ItemHeight = 14;
            this.lstLinksAccessed.Location = new System.Drawing.Point(10, 325);
            this.lstLinksAccessed.Name = "lstLinksAccessed";
            this.lstLinksAccessed.Size = new System.Drawing.Size(691, 326);
            this.lstLinksAccessed.Sorted = true;
            this.lstLinksAccessed.TabIndex = 6;
            // 
            // txtWorkerCount
            // 
            this.txtWorkerCount.Location = new System.Drawing.Point(123, 33);
            this.txtWorkerCount.Name = "txtWorkerCount";
            this.txtWorkerCount.Size = new System.Drawing.Size(26, 20);
            this.txtWorkerCount.TabIndex = 7;
            this.txtWorkerCount.Text = "2";
            this.txtWorkerCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtWorkerCount.TextChanged += new System.EventHandler(this.txtWorkerCount_TextChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnIndexCrawlDate
            // 
            this.btnIndexCrawlDate.Location = new System.Drawing.Point(391, 33);
            this.btnIndexCrawlDate.Name = "btnIndexCrawlDate";
            this.btnIndexCrawlDate.Size = new System.Drawing.Size(104, 23);
            this.btnIndexCrawlDate.TabIndex = 8;
            this.btnIndexCrawlDate.Text = "Reindex";
            this.btnIndexCrawlDate.UseVisualStyleBackColor = true;
            this.btnIndexCrawlDate.Visible = false;
            this.btnIndexCrawlDate.Click += new System.EventHandler(this.btnIndexCrawlDate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 14);
            this.label1.TabIndex = 9;
            this.label1.Text = "Redis Server";
            // 
            // txtRedisServer
            // 
            this.txtRedisServer.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtRedisServer.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.HistoryList;
            this.txtRedisServer.Location = new System.Drawing.Point(86, 10);
            this.txtRedisServer.Name = "txtRedisServer";
            this.txtRedisServer.Size = new System.Drawing.Size(147, 20);
            this.txtRedisServer.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(239, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 14);
            this.label2.TabIndex = 11;
            this.label2.Text = "URL";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 14);
            this.label3.TabIndex = 12;
            this.label3.Text = "Collectors to spawn";
            // 
            // chkCrawlNew
            // 
            this.chkCrawlNew.AutoSize = true;
            this.chkCrawlNew.Location = new System.Drawing.Point(155, 35);
            this.chkCrawlNew.Name = "chkCrawlNew";
            this.chkCrawlNew.Size = new System.Drawing.Size(81, 18);
            this.chkCrawlNew.TabIndex = 13;
            this.chkCrawlNew.Text = "Crawl New";
            this.chkCrawlNew.UseVisualStyleBackColor = true;
            this.chkCrawlNew.Visible = false;
            this.chkCrawlNew.CheckedChanged += new System.EventHandler(this.chkCrawlNew_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 685);
            this.Controls.Add(this.chkCrawlNew);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtRedisServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnIndexCrawlDate);
            this.Controls.Add(this.txtWorkerCount);
            this.Controls.Add(this.lstLinksAccessed);
            this.Controls.Add(this.btnLoadCsvToPool);
            this.Controls.Add(this.btnAddUrlToPool);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.lstLinksCurrentlyProcessing);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "LinkSpider3";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bsLinksCurrentlyProcessing)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstLinksCurrentlyProcessing;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.BindingSource bsLinksCurrentlyProcessing;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnAddUrlToPool;
        private System.Windows.Forms.Button btnLoadCsvToPool;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ListBox lstLinksAccessed;
        private System.Windows.Forms.TextBox txtWorkerCount;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnIndexCrawlDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRedisServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkCrawlNew;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
    }
}

