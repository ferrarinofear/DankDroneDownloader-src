namespace updater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.StopUpdate_button = new System.Windows.Forms.Button();
            this.Download_progressBar = new System.Windows.Forms.ProgressBar();
            this.Status_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StopUpdate_button
            // 
            this.StopUpdate_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopUpdate_button.Location = new System.Drawing.Point(153, 85);
            this.StopUpdate_button.Name = "StopUpdate_button";
            this.StopUpdate_button.Size = new System.Drawing.Size(132, 32);
            this.StopUpdate_button.TabIndex = 0;
            this.StopUpdate_button.Text = "Cancel Update";
            this.StopUpdate_button.UseVisualStyleBackColor = true;
            this.StopUpdate_button.Click += new System.EventHandler(this.StopUpdate_button_Click);
            // 
            // Download_progressBar
            // 
            this.Download_progressBar.Location = new System.Drawing.Point(12, 34);
            this.Download_progressBar.Name = "Download_progressBar";
            this.Download_progressBar.Size = new System.Drawing.Size(415, 23);
            this.Download_progressBar.TabIndex = 1;
            // 
            // Status_label
            // 
            this.Status_label.AutoSize = true;
            this.Status_label.Location = new System.Drawing.Point(12, 18);
            this.Status_label.Name = "Status_label";
            this.Status_label.Size = new System.Drawing.Size(78, 13);
            this.Status_label.TabIndex = 2;
            this.Status_label.Text = "Downloading...";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 129);
            this.Controls.Add(this.Status_label);
            this.Controls.Add(this.Download_progressBar);
            this.Controls.Add(this.StopUpdate_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Update DankDroneDownloader";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button StopUpdate_button;
		private System.Windows.Forms.ProgressBar Download_progressBar;
		private System.Windows.Forms.Label Status_label;
	}
}

