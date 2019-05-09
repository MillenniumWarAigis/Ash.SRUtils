using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ash.SRUtils.ResourceEncryption
{
	public class Application
	{
		public class OptionsDefinition
		{
			public bool WaitForUserInput = true;
			public int VerboseLevel = 3;
			// it's more common/standard to have those set to "-", "--", and true respectively.
			// feel free to change it to whatever you prefer.
			public string ShortOptionPrefix = "/";
			public string LongOptionPrefix = "/";
			public bool IsOptionCaseSensitive = false;

			public string InputPath = "";
			public string OutputPath = "out";
			public string FilePattern = "*.*";
			public string DirectoryPattern = "*";
			public bool PreserveDirectoryStructure = true;
		}

		private OptionsDefinition options;
		protected virtual OptionsDefinition Options
		{
			get
			{
				if (options == null)
				{
					options = new OptionsDefinition();
				}
				return options;
			}
		}

		public virtual void Run(string[] args)
		{
			WriteProgramHeader();

			ProcessProgramArguments(args);

			if ((args.Length == 0) || (args.Length == args.Count(x => x.StartsWith(Options.ShortOptionPrefix) || x.StartsWith(Options.LongOptionPrefix))))
			{
				ProcessDirectUserInput();
			}

			if (Options.WaitForUserInput)
			{
				WaitForUserInput();
			}
		}

		protected virtual void WriteProgramHeader()
		{
			FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

			Console.Out.WriteLine("{0} v{1}", fileVersionInfo.ProductName, fileVersionInfo.FileVersion);
			Console.Out.WriteLine();
		}

		protected virtual void WaitForUserInput()
		{
			Console.Out.WriteLine();
			Console.Out.Write("Done. Press any key to exit...");
			Console.ReadKey(intercept: true);
		}

		protected virtual void ProcessDirectUserInput()
		{
			Console.Out.WriteLine("Type /help for usage or press Enter to quit the program.");
			Console.Out.WriteLine();

			do
			{
				Console.Out.Write("Enter either nothing, an option, a file or directory name: ");

				string line = Console.ReadLine();

				if (string.IsNullOrEmpty(line))
				{
					break;
				}
				else
				{
					// TODO ideally, we'd want to split <line> (a string) into <args> (an array of strings).
					ProcessProgramArguments(new string[] { line });
				}
			} while (true);
		}

		protected virtual void ProcessProgramArguments(string[] args)
		{
			for (int i = 0; i < args.Length; ++i)
			{
				string arg = args[i];

				try
				{
					if (arg.StartsWith(Options.ShortOptionPrefix) || arg.StartsWith(Options.LongOptionPrefix))
					{
						ProcessOption(args, ref i);
					}
					else
					{
						// dropping a file with spaces inserts a filename with double quotes.
						arg = arg.StripDoubleQuotes();

						ProcessDirectoryOrFile(arg, ref i);
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("*** exception: {0}", ex.Message);

					if (Options.VerboseLevel >= 4)
					{
						Console.Error.WriteLine();
						Console.Error.WriteLine(ex.StackTrace);
						Console.Error.WriteLine();
					}
				}
			}
		}

		protected virtual bool ProcessOption(string[] args, ref int argIndex)
		{
			string arg = args[argIndex];
			int length = 0;

			if (TryMatchArgument(arg, "h", "help", out length))
			{
				PrintHelp();
			}
			else if (TryMatchArgument(arg, "w", "waitForUserInput", out length))
			{
				ProcessArgument(arg, length, ref Options.WaitForUserInput);
			}
			else if (TryMatchArgument(arg, "q", "quiet", out length))
			{
				Options.VerboseLevel = 0;
			}
			else if (TryMatchArgument(arg, "v", "verbose", out length))
			{
				ProcessArgument(arg, length, ref Options.VerboseLevel);
			}
			else if (TryMatchArgument(arg, "i", "inputPath", out length))
			{
				ProcessArgument(arg, length, ref Options.InputPath);
				Options.InputPath = Options.InputPath.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "o", "outputPath", out length))
			{
				ProcessArgument(arg, length, ref Options.OutputPath);
				Options.OutputPath = Options.OutputPath.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "f", "filePattern", out length))
			{
				ProcessArgument(arg, length, ref Options.FilePattern);
				Options.FilePattern = Options.FilePattern.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "d", "directoryPattern", out length))
			{
				ProcessArgument(arg, length, ref Options.DirectoryPattern);
				Options.DirectoryPattern = Options.DirectoryPattern.StripDoubleQuotes();
			}
			else if (TryMatchArgument(arg, "t", "preserveDirectoryStructure", out length))
			{
				ProcessArgument(arg, length, ref Options.PreserveDirectoryStructure);
			}
			else
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: unrecognized option `{0}`", arg);
				}
				return false;
			}

			return true;
		}

		protected virtual bool TryMatchArgument(string arg, string shortName, string longName, out int length)
		{
			string prefixedShortName = Options.ShortOptionPrefix + shortName;
			string prefixedLongName = Options.LongOptionPrefix + longName;
			StringComparison stringComparison = Options.IsOptionCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

			if (!string.IsNullOrEmpty(prefixedShortName) && (arg.Equals(prefixedShortName, stringComparison) || arg.StartsWith(prefixedShortName + "=", stringComparison)))
			{
				length = prefixedShortName.Length;
			}
			else if (!string.IsNullOrEmpty(prefixedLongName) && (arg.Equals(prefixedLongName, stringComparison) || arg.StartsWith(prefixedLongName + "=", stringComparison)))
			{
				length = prefixedLongName.Length;
			}
			else
			{
				length = 0;
				return false;
			}

			return true;
		}

		protected virtual void ProcessArgument(string arg, int startIndex, ref string destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => x);
		}

		protected virtual void ProcessArgument(string arg, int startIndex, ref int destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => int.Parse(x));
		}

		protected virtual void ProcessArgument(string arg, int startIndex, ref bool destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => x.ToBoolean());
		}

		/// <example>
		/// I prefer the "/verb=argument" syntax; but to support the more common "-verb argument" syntax,
		/// you can just replace the parameters:
		/// <code>string arg, int startIndex</code> with <code>string[] args, ref int argIndex</code>
		/// and the argument extraction:
		/// <code>arg.Substring(startIndex + 1)</code> with <code>args[++argIndex]</code>.
		/// </example>
		protected virtual void ProcessArgument<TValue>(string arg, int startIndex, ref TValue destination, Func<string, TValue> converter)
		{
			if ((startIndex + 1) <= arg.Length)
			{
				destination = converter.Invoke(arg.Substring(startIndex + 1));
			}
			else
			{
				Console.Out.WriteLine(destination);
			}
		}

		protected virtual void PrintHelpUsage()
		{
			Console.Out.WriteLine("  [options...] <[file|directory]...>");
			Console.Out.WriteLine("");
		}

		protected virtual void PrintHelpOptions()
		{
			//                     ....|....1....|....2....|....3....|....4....|....5....|....6....|....7....|....8
			Console.Out.WriteLine("  /i or /inputPath=<pathname:string>");
			Console.Out.WriteLine("     Override input path (default: ``)");
			Console.Out.WriteLine();
			Console.Out.WriteLine("  /o or /outputPath=<pathname:string>");
			Console.Out.WriteLine("     Override output path (default: `out`)");
			Console.Out.WriteLine();
			Console.Out.WriteLine("  /f or /filePattern=<pattern:string>");
			Console.Out.WriteLine("     Override file pattern (default: `*.*`)");
			Console.Out.WriteLine("     Use | to split multiple patterns");
			Console.Out.WriteLine();
			Console.Out.WriteLine("  /d or /directoryPattern=<pattern:string>");
			Console.Out.WriteLine("     Override directory pattern (default: `*`)");
			Console.Out.WriteLine("     Use | to split multiple patterns");
			Console.Out.WriteLine();
			Console.Out.WriteLine("  /t or /preserveDirectoryStructure=<enable:bool>");
			Console.Out.WriteLine("     Preserve directory structure between input and output paths (default: 1)");
			Console.Out.WriteLine();

			Console.Out.WriteLine("  /v or /verbose=<level:int>");
			Console.Out.WriteLine("     Set the level of verbosity in message output (default: 3)");
			Console.Out.WriteLine("     Where level is 0=none, 1=errors, 2=warnings, 3=traces, 4=debugging info");
			Console.Out.WriteLine();
			Console.Out.WriteLine("  /q or /quiet");
			Console.Out.WriteLine("     Suppress all message output (default: 0)");
			Console.Out.WriteLine();
			Console.Out.WriteLine("  /w or /waitForUserInput=<enable:bool>");
			Console.Out.WriteLine("     Whether to wait for user confirmation to exit the program (default: 1)");
			Console.Out.WriteLine("");
		}

		protected virtual void PrintHelpExamples()
		{
			Console.Out.WriteLine("  /q=true /v=0");
			Console.Out.WriteLine("");
		}

		protected virtual void PrintHelp()
		{
			Console.Out.WriteLine("");

			Console.Out.WriteLine("usage:");
			Console.Out.WriteLine("");
			PrintHelpUsage();

			Console.Out.WriteLine("options:");
			Console.Out.WriteLine("");
			PrintHelpOptions();

			Console.Out.WriteLine("examples:");
			Console.Out.WriteLine("");
			PrintHelpExamples();
		}

		protected virtual void ProcessDirectoryOrFile(string arg, ref int argIndex)
		{
			string sourcePath = BuildSourcePath(arg);
			bool isFile = File.Exists(sourcePath);
			bool isDirectory = Directory.Exists(sourcePath);

			if (!isFile && !isDirectory)
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: `{0}` is not a valid file or directory.", sourcePath);
				}
				return;
			}

			Stopwatch timer = null;

			if (Options.VerboseLevel >= 4)
			{
				timer = Stopwatch.StartNew();
			}

			if (isDirectory)
			{
				ProcessDirectory(sourcePath, sourcePath);
			}
			else
			{
				ProcessFile(sourcePath, Path.GetDirectoryName(sourcePath));
			}

			if (Options.VerboseLevel >= 4)
			{
				timer.Stop();

				Console.Out.WriteLine("Completed in {0}.", timer.Elapsed);
				Console.Out.WriteLine();
			}
		}

		protected virtual string BuildSourcePath(string path)
		{
			string sourcePath;

			if (!string.IsNullOrEmpty(Options.InputPath))
			{
				sourcePath = Path.Combine(Options.InputPath, path);
			}
			else
			{
				sourcePath = path;
			}

			return sourcePath;
		}

		protected virtual void ProcessDirectory(string path, string rootPath)
		{
			if (Options.VerboseLevel >= 3)
			{
				Console.Out.WriteLine("Processing directory `{0}`...", path.Replace(AppDomain.CurrentDomain.BaseDirectory, ""));
			}

			if (!Directory.Exists(path))
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: the directory does not exist.");
				}
				return;
			}

			foreach (string filePattern in Options.FilePattern.Split(new char[] { '|' }))
			{
				IEnumerable<string> files = Directory.EnumerateFiles(path, filePattern, SearchOption.TopDirectoryOnly);

				foreach (string fileName in files)
				{
					ProcessFile(fileName, rootPath);
				}
			}

			foreach (string directoryPattern in Options.DirectoryPattern.Split(new char[] { '|' }))
			{
				IEnumerable<string> directories = Directory.EnumerateDirectories(path, directoryPattern, SearchOption.TopDirectoryOnly);

				foreach (string directoryName in directories)
				{
					ProcessDirectory(directoryName, rootPath);
				}
			}
		}

		protected virtual bool ProcessFile(string path, string rootPath)
		{
			if (Options.VerboseLevel >= 3)
			{
				Console.Out.WriteLine("Processing file `{0}`...", Path.GetFileName(path));
			}

			if (!File.Exists(path))
			{
				if (Options.VerboseLevel >= 1)
				{
					Console.Error.WriteLine("*** error: the file `{0}` does not exist.", path);
				}
				return false;
			}

			// NOTE: you probably want to override this.

			return true;
		}

		protected virtual string BuildDestinationPath(string path, string rootPath, string outputFileName)
		{
			string destinationPath;

			if (!string.IsNullOrEmpty(Options.OutputPath))
			{
				if (Options.PreserveDirectoryStructure)
				{
					string subPath = Path.GetDirectoryName(path);
					subPath = subPath.Replace(rootPath, "");
					subPath = subPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
					if (subPath.Any() && (subPath.First() == Path.DirectorySeparatorChar || subPath.First() == Path.AltDirectorySeparatorChar))
					{
						subPath = subPath.Substring(1);
					}

					destinationPath = Path.Combine(Options.OutputPath, subPath, outputFileName);
				}
				else
				{
					destinationPath = Path.Combine(Options.OutputPath, outputFileName);
				}
			}
			else if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)))
			{
				destinationPath = Path.Combine(Path.GetDirectoryName(path), outputFileName);
			}
			else
			{
				destinationPath = outputFileName;
			}

			return destinationPath;
		}
	}

	public static class StringExtensionMethods
	{
		public static readonly string[] trueValues = new string[] { "1", "true"/*, "on", "yes",*/ };
		public static readonly string[] falseValues = new string[] { "0", "false"/*, "off", "no",*/ };

		public static bool ToBoolean(this string value)
		{
			if (falseValues.Contains(value, StringComparer.InvariantCultureIgnoreCase))
			{
				return false;
			}
			else if (trueValues.Contains(value, StringComparer.InvariantCultureIgnoreCase))
			{
				return true;
			}
			else
			{
				throw new ArgumentException("Cannot convert boolean from value string.", "value");
			}
		}

		public static string StripDoubleQuotes(this string value)
		{
			return value.StripDelimiters('\"', '\"');
		}

		public static string StripDelimiters(this string value, char startDelimiterChar, char endDelimiterChar)
		{
			if (value.Length >= 2 && value.First() == startDelimiterChar && value.Last() == endDelimiterChar)
			{
				return value.Substring(1, value.Length - 2);
			}
			else
			{
				return value;
			}
		}
	}
}
