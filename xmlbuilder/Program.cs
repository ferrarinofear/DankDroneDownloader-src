using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UpdateInfo;

namespace xmlbuilder
{
	class Program
	{
		private static string xmlInstallerContent =
@"<?xml version=""1.0"" encoding=""utf-8"" ?> 
<update>
	<!--Current version-->
	<version>{0}</version>
	<!--Update source.-->
	<sources>
		<!--Remote servers-->
		<source>{1}</source>
	</sources>
	<!--Description for this update. This text display without leading tabs and leading and trailing spaces-->
	<description>
		Added link to DUMLdore, an easy way for Windows users to flash these FW files..
	</description>
</update>";

		static void DoWork(string[] args)
		{
			Console.WriteLine("xmlbuilder start");
			if (args.Length == 0 || !File.Exists(args[0]))
			{
				Console.WriteLine("Need specify executable name");
				return;
			}
			Assembly assembly = Assembly.LoadFrom(args[0]);
			string version = assembly.GetName().Version.ToString();
			string xmlContent = String.Format(xmlInstallerContent, version, UpdateSources.ExecutableSource);
			string xmlFileName = Path.Combine(Path.GetDirectoryName(args[0]), "DankDroneDownloader.xml");
			File.WriteAllText(xmlFileName, xmlContent);
			Console.WriteLine($"Version info and update source configuration written in {xmlFileName}");
		}

		static void Main(string[] args)
		{
			LoadResolver();
			DoWork(args);
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
