using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DankDroneDownloader
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			LoadResolver();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		/// <summary>
		/// Embed library resolves
		/// </summary>;
		private static void LoadResolver()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolver);
		}

		/// <summary>
		/// Resolve embed DLL's
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static Assembly CurrentDomain_AssemblyResolver(object sender, ResolveEventArgs args)
		{
			String resourceName = Assembly.GetExecutingAssembly().FullName.Split(',').First() + "." + new AssemblyName(args.Name).Name + ".dll";
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				if (stream != null)
				{
					Byte[] assemblyData = new Byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			}
			return null;
		}

	}
}
