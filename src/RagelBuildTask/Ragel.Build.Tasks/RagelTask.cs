 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Policy;
namespace Ragel.Build.Tasks
{
	public class RagelTask : Task
	{
		private const string DefaultGeneratedSourceExtension = "g.cs";
		private static AppDomain _sharedAppDomain;
		private List<ITaskItem> _generatedCodeFiles = new List<ITaskItem>();
		[Required]
		public string ToolPath
		{
			get;
			set;
		}
		[Required]
		public string OutputPath
		{
			get;
			set;
		}
		public string TargetLanguage
		{
			get;
			set;
		}
		public string TargetFrameworkVersion
		{
			get;
			set;
		}
		public string BuildTaskPath
		{
			get;
			set;
		}
		public ITaskItem[] SourceCodeFiles
		{
			get;
			set;
		}
		public string GeneratedSourceExtension
		{
			get;
			set;
		}
		public string TargetNamespace
		{
			get;
			set;
		}
		public string[] LanguageSourceExtensions
		{
			get;
			set;
		}
		public bool GenerateDot
		{
			get;
			set;
		}
		[Output]
		public ITaskItem[] GeneratedCodeFiles
		{
			get
			{
				return this._generatedCodeFiles.ToArray();
			}
			set
			{
				this._generatedCodeFiles = new List<ITaskItem>(value);
			}
		}
		public AppDomain GetAntlrTaskAppDomain()
		{
			AppDomain sharedAppDomain;
			if (RagelTask._sharedAppDomain != null)
			{
				sharedAppDomain = RagelTask._sharedAppDomain;
			}
			else
			{
				AppDomainSetup info = new AppDomainSetup
				{
					ApplicationBase = this.BuildTaskPath,
					LoaderOptimization = LoaderOptimization.MultiDomainHost,
					ShadowCopyFiles = "true"
				};
				string friendlyName = "RagelDomain_" + Guid.NewGuid();
				RagelTask._sharedAppDomain = AppDomain.CreateDomain(friendlyName, AppDomain.CurrentDomain.Evidence, info, new NamedPermissionSet("FullTrust"), new StrongName[0]);
				sharedAppDomain = RagelTask._sharedAppDomain;
			}
			return sharedAppDomain;
		}
		public override bool Execute()
		{
			Debug.WriteLine("Execute");
			bool result;
			if (this.GenerateDot)
			{
				if (!this.Execute(true))
				{
					result = false;
					return result;
				}
			}
			result = this.Execute(false);
			return result;
		}
		public bool Execute(bool dot)
		{
			string ragel = Path.Combine(this.ToolPath, "ragel.exe");
			bool result;
			foreach (string file in 
				from n in this.SourceCodeFiles
				select n.ItemSpec)
			{
				List<string> arguments = new List<string>();
				arguments.Add("-A");
				arguments.Add("-o");
				string outfile = Path.GetFileNameWithoutExtension(file) + ".g.cs";
				arguments.Add(Path.Combine(this.OutputPath, outfile));
				if (dot)
				{
					arguments.Add("-V");
					arguments.Add("-p");
					arguments.Add("-s");
				}
				arguments.Add(file);
				ProcessStartInfo startInfo = new ProcessStartInfo(ragel, string.Join(" ", arguments))
				{
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};
				Process process = new Process();
				process.StartInfo = startInfo;
				Debug.WriteLine("CMD: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
				Console.WriteLine("CMD: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
				process.Start();
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();
				process.StandardInput.Close();
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}
	}
}
