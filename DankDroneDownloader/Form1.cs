using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UpdateInfo;

namespace DankDroneDownloader
{
	public partial class Form1 : Form
	{
		private DataStorage _dataStorage;
		private SimplyDownloader _downloader;

		public Form1()
		{
			InitializeComponent();
			//Initialize data storage
			_dataStorage = new DataStorage();
			_dataStorage.OnNewModelDownloading += _dataStorage_OnNewModelDownloading;
			//Initialize downloader
			_downloader = new SimplyDownloader();
			_downloader.OnDownloadComplete += _downloader_OnDownloadComplete;
			_downloader.OnReportProgress += _downloader_OnReportProgress;
		}

		/// <summary>
		/// Report about progress
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _downloader_OnReportProgress(object sender, DownloadProgressChangedEventArgs e)
		{
			Download_progressBar.Invoke((MethodInvoker)(() => 
			{
				if (e.TotalBytesToReceive != -1)
				{
					//Update progress bar
					Download_progressBar.Value = e.ProgressPercentage;
					Download_progressBar.Style = ProgressBarStyle.Blocks;
				}
				else
				{
					//If we not get size of file we switch to marquee mode
					Download_progressBar.Style = ProgressBarStyle.Marquee;
				}
			}));
		}

		/// <summary>
		/// Save file when complete
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _downloader_OnDownloadComplete(object sender, AsyncCompletedEventArgs e)
		{
			Download_button.Text = "Download Firmware";
			Download_progressBar.Visible = false;
			//Just care about "Download firmware" button
			Versions_listView_SelectedIndexChanged(null, null);
			//Save result file
			if (!e.Cancelled && e.Error == null)
			{
				saveFileDialog.FileName = Path.GetFileName(_downloader.DownloadedURL);
				if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						if (File.Exists(saveFileDialog.FileName))
						{
							File.Delete(saveFileDialog.FileName);
						}
						File.Move(_downloader.GetDownloadedFileName, saveFileDialog.FileName);
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, $"Unable to save file \"{saveFileDialog.FileName}\". Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			//User cancel download
			if (e.Cancelled)
			{
				//Try delete temporary file
				try
				{
					File.Delete(_downloader.GetDownloadedFileName);
				}
				catch
				{
					//Do nothing
				}
				MessageBox.Show(this, "Download canceled.", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			//Error while downloading
			if (e.Error != null && !e.Cancelled)
			{
				//Try delete temporary file
				try
				{
					File.Delete(_downloader.GetDownloadedFileName);
				}
				catch
				{
					//Do nothing
				}
				MessageBox.Show(this, $"Unable to download file \"{_downloader.DownloadedURL}\". Error: {e.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Update status label when start downloading new drone model
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _dataStorage_OnNewModelDownloading(object sender, string e)
		{
			WaitStatus_label.Invoke((MethodInvoker)(() => { WaitStatus_label.Text = $"Updating \"{e}\"..."; }));
		}

		/// <summary>
		/// Refresh list of firmwares
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Refresh_button_ClickAsync(object sender, EventArgs e)
		{
			multiPanel.SelectedPage = WaitPage;
			try
			{
				//Update list of firmware
				await _dataStorage.Update();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, $"Unable to update list of firmwares. Error: {ex.Message}\r\nPlease check you Internet connection, and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.Close();
				return;
			}
			Model_comboBox.Items.Clear();
			Model_comboBox.Items.AddRange(_dataStorage.Firmwares.Select(item => item.Model).Distinct().ToArray());
			Model_comboBox.SelectedIndex = 0;
			ACandRC_radioButton.Checked = true;
			multiPanel.SelectedPage = DataPage;
		}

		/// <summary>
		/// Update list of firmwares
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Model_comboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Versions_listView.Items.Clear();
			Notes_textBox.Text = "";
			List<SingleFirmware> firmWares = null;
			//Get list according to selected filter
			if (ACandRC_radioButton.Checked)
			{
				firmWares = new List<SingleFirmware>(_dataStorage.Firmwares.Where(item => item.Model == Model_comboBox.SelectedItem.ToString()));
			}
			else if (ACOnly_radioButton.Checked)
			{
				firmWares = new List<SingleFirmware>(_dataStorage.Firmwares.Where(item => item.Model == Model_comboBox.SelectedItem.ToString() && item.Type == FirmwareType.AC));
			}
			else if (RCOnly_radioButton.Checked)
			{
				firmWares = new List<SingleFirmware>(_dataStorage.Firmwares.Where(item => item.Model == Model_comboBox.SelectedItem.ToString() && item.Type == FirmwareType.RC));
			}
			//Update list
			foreach (SingleFirmware singleFirmware in firmWares)
			{
				ListViewItem listViewItem = new ListViewItem(new string[] { singleFirmware.Version, singleFirmware.Type.ToString(), String.IsNullOrEmpty(singleFirmware.ReleaseNotes) ? "No" : "Yes", singleFirmware.Size });
				Versions_listView.Items.Add(listViewItem);
			}
			Versions_listView_SelectedIndexChanged(null, null);
		}

		/// <summary>
		/// Refresh list when form show first time
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_Shown(object sender, EventArgs e)
		{
			Refresh_button_ClickAsync(null, null);
			//Start update thread
			Task.Run(() => { CheckUpdate(null); });
		}

		/// <summary>
		/// Check for update
		/// </summary>
		/// <param name="state"></param>
		private async void CheckUpdate(object state)
		{
			List<string> updateSources = new List<string>();
			string update = "";
			string version = "";
			string description = "";
			try
			{
				//Download and parse configuration XML
				using (WebClient client = new WebClient())
				{
					update = await client.DownloadStringTaskAsync(UpdateSources.UpdateConfigurationSource);
				}
				XmlDocument doc = new XmlDocument()
				{
					PreserveWhitespace = false
				};
				doc.LoadXml(update);
				version = doc["update"]?["version"]?.InnerText;
				description = doc["update"]?["description"]?.InnerText;
				XmlNodeList downloadUpdateSources = doc["update"]?["sources"]?.ChildNodes;
				foreach (XmlNode node in downloadUpdateSources)
				{
					if (node.NodeType != XmlNodeType.Comment)
					{
						updateSources.Add(node.InnerText);
					}
				}
			}
			catch
			{
				return;
			}
			if (!String.IsNullOrEmpty(version) && updateSources.Count > 0)
			{
				Version serverVersion = new Version(version);
				Version currentVerstion = Assembly.GetEntryAssembly().GetName().Version;
				//Extract and start update process
				if (currentVerstion < serverVersion)
				{
					this.Invoke((MethodInvoker)(() =>
					{
						if (MessageBox.Show(this, $"Update available. Current version: \"{currentVerstion}\"\r\nVersion on server: {serverVersion}.\r\nUpdate information: \"{description.Trim()}\"\r\nDo you want close this application and install update?", "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
						{
							//Actual update
							using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DankDroneDownloader.updater.exe"))
							{
								if (stream != null)
								{
									Byte[] assemblyData = new Byte[stream.Length];
									stream.Read(assemblyData, 0, assemblyData.Length);
									string updatePath = Path.Combine(Path.GetTempPath(), "DankDroneDownloader.updater.exe");
									try
									{
										if (File.Exists(updatePath))
										{
											File.Delete(updatePath);
										}
										File.WriteAllBytes(updatePath, assemblyData);
										//First parameter is source for download new executable, second target executable
										Process.Start(updatePath, $"\"{updateSources[0]}\" \"{Application.ExecutablePath}\"");
										Application.Exit();
									}
									catch
									{
										MessageBox.Show(this, "Unable to start update process. Reboot PC and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
									}
								}
							}
						}
					}));
				}
			}
		}

		/// <summary>
		/// Update notes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Versions_listView_SelectedIndexChanged(object sender, EventArgs e)
		{
			Notes_textBox.Text = "";
			fileName_label.Text = "File name: <Select firmware>";
			Download_button.Enabled = Versions_listView.SelectedItems.Count > 0 || _downloader.IsBusy;
			if (Versions_listView.SelectedItems.Count == 0) { return; }
			ListViewItem selectedItem = Versions_listView.SelectedItems[0];
			int firmwareIndex = _dataStorage.Firmwares.FindIndex(item => item.Model == Model_comboBox.SelectedItem.ToString() && item.Version == selectedItem.SubItems[0].Text && item.Type == (FirmwareType)Enum.Parse(typeof(FirmwareType), selectedItem.SubItems[1].Text));
			Notes_textBox.Text = _dataStorage.Firmwares[firmwareIndex].NotesContent;
			fileName_label.Text = $"File name: {Path.GetFileName(_dataStorage.Firmwares[firmwareIndex].Binary)}";

		}

		/// <summary>
		/// Start download firmware
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Download_button_Click(object sender, EventArgs e)
		{
			if (_downloader.IsBusy)
			{
				_downloader.StopDownload();
				return;
			}
			ListViewItem selectedItem = Versions_listView.SelectedItems[0];
			int firmwareIndex = _dataStorage.Firmwares.FindIndex(item => item.Model == Model_comboBox.SelectedItem.ToString() && item.Version == selectedItem.SubItems[0].Text && item.Type == (FirmwareType)Enum.Parse(typeof(FirmwareType), selectedItem.SubItems[1].Text));
			_downloader.DownloadFile(_dataStorage.Firmwares[firmwareIndex].Binary);
			Download_button.Text = "Stop Download";
			Download_progressBar.Value = 0;
			Download_progressBar.Visible = true;
		}

		private void GitVisit(object sender, EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("https://github.com/cs2000/DankDroneDownloader");
			}
			catch { }
		}

		/// <summary>
		/// Update window title
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_Load(object sender, EventArgs e)
		{
			this.Text = $"DankDroneDownloader v{Assembly.GetEntryAssembly().GetName().Version}";
			//Check and delete old update file if exists
			string updatePath = Path.Combine(Path.GetTempPath(), "DankDroneDownloader.updater.exe");
			if (File.Exists(updatePath))
			{
				try
				{
					File.Delete(updatePath);
				}
				catch
				{

				}
			}
		}

        private void DumlVisit(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/jezzab/DUMLdore");
            }
            catch { }
        }
    }
}
