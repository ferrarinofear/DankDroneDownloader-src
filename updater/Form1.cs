using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace updater
{
	public partial class Form1 : Form
	{
		private string _source;
		private string _target;
		private string _tempFileName;
		private readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
		private WebClient _webClient;

		public Form1(string[] args)
		{
			InitializeComponent();
			if (args.Count() != 2)
			{
				Application.Exit();
			}
			_source = args[0];
			_target = args[1];
		}

		/// <summary>
		/// Format output
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private string SizeSuffix(long value)
		{
			if (value < 0) { return "-" + SizeSuffix(-value); }
			if (value == 0) { return "0 bytes"; }
			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));
			return $"{adjustedSize:n1} {_sizeSuffixes[mag]}";
		}

		/// <summary>
		/// Download updated file
		/// </summary>
		private async void DoUpdate()
		{
			try
			{
				_webClient = new WebClient();
				_webClient.DownloadProgressChanged += Client_DownloadProgressChanged;
				_webClient.DownloadFileCompleted += Client_DownloadFileCompleted;
				_tempFileName = Path.GetTempFileName();
				await _webClient.DownloadFileTaskAsync(_source, _tempFileName);
			}
			catch
			{
				//Do nothing
			}
		}

		/// <summary>
		/// Delete temp file
		/// </summary>
		private void DeleteTempFile()
		{
			if (File.Exists(_tempFileName))
			{
				try
				{
					File.Delete(_tempFileName);
				}
				catch
				{

				}
			}
		}

		/// <summary>
		/// Download complete
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				this.Invoke((MethodInvoker)(() =>
				{
					MessageBox.Show(this, $"Unable to download update from \"{_source}\". Error: {e.Error.Message}\r\nTry again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					DeleteTempFile();
					Application.Exit();
				}));
			}
			
			try
			{
				if (File.Exists(_target))
				{
					File.Delete(_target);
				}
				File.Move(_tempFileName, _target);
				this.Invoke((MethodInvoker)(() =>
				{
					if (MessageBox.Show(this, $"Update done. Do you want to run updated application?.", "Update done", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						Process.Start(_target);
					}
					Application.Exit();
				}));
			}
			catch (Exception ex)
			{
				this.Invoke((MethodInvoker)(() =>
				{
					MessageBox.Show(this, $"Unable to write updated file as \"{_target}\". Error: {ex.Message}\r\nTry again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}));
				DeleteTempFile();
				Application.Exit();
			}
		}

		/// <summary>
		/// Update progress bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			if (e.TotalBytesToReceive == -1)
			{
				Download_progressBar.Invoke((MethodInvoker)(() => { Download_progressBar.Style = ProgressBarStyle.Marquee; }));
				Status_label.Invoke((MethodInvoker)(() => { Status_label.Text = $"Downloading {SizeSuffix(e.BytesReceived)}..."; }));
			}
			else
			{
				Download_progressBar.Invoke((MethodInvoker)(() => 
				{
					Download_progressBar.Style = ProgressBarStyle.Blocks;
					Download_progressBar.Value = e.ProgressPercentage;
				}));
				Status_label.Invoke((MethodInvoker)(() => { Status_label.Text = $"Downloading {SizeSuffix(e.BytesReceived)} from {SizeSuffix(e.TotalBytesToReceive)}..."; }));
			}
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			//Used for debug
			//MessageBox.Show(this, "Wait for update");
			Task.Run(() => { DoUpdate(); });
		}

		private void StopUpdate_button_Click(object sender, EventArgs e)
		{
			_webClient.CancelAsync();
		}
	}
}
