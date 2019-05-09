using System;
using System.IO;

namespace Ash.SRUtils.ResourceEncryption
{
	public enum EncryptionMethod
	{
		None,
		Xor,
		CXor
	}

	public class ResourceEncryptionApp : Application
	{
		public new class OptionsDefinition : Application.OptionsDefinition
		{
			public EncryptionMethod EncryptionMethod = EncryptionMethod.Xor;
		}

		private OptionsDefinition options;
		protected new OptionsDefinition Options
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

		protected override void PrintHelpOptions()
		{
			base.PrintHelpOptions();

			Console.Out.WriteLine("  /e or /encryptionMethod=<none|xor|cxor>");
			Console.Out.WriteLine("     Set the encryption method (default: xor)");
			Console.Out.WriteLine("");
		}

		protected override void PrintHelpExamples()
		{
			base.PrintHelpExamples();

			Console.Out.WriteLine("  \"image.png\"");
			Console.Out.WriteLine("");
			Console.Out.WriteLine("  \"image.png\" \"background.jpg\" \"spritesheet.anm\" \"archive.dat\"");
			Console.Out.WriteLine("");
			Console.Out.WriteLine("  /filePattern=\"*.png|*.jpg\" /verbose=4 \"images\"");
			Console.Out.WriteLine("");
			Console.Out.WriteLine("  /outputPath=\"C:\\Users\\USER\\Pictures\\Albums\" /quiet \"Downloads\"");
			Console.Out.WriteLine("");
		}

		protected override bool ProcessOption(string[] args, ref int argIndex)
		{
			string arg = args[argIndex];
			int length = 0;

			if (TryMatchArgument(arg, "e", "encryptionMethod", out length))
			{
				ProcessArgument(arg, length, ref Options.EncryptionMethod);
			}
			else
			{
				return base.ProcessOption(args, ref argIndex);
			}

			return true;
		}

		protected virtual void ProcessArgument(string arg, int startIndex, ref EncryptionMethod destination)
		{
			ProcessArgument(arg, startIndex, ref destination, x => (EncryptionMethod)Enum.Parse(typeof(EncryptionMethod), x, true));
		}

		protected override bool ProcessFile(string path, string rootPath)
		{
			if (!base.ProcessFile(path, rootPath))
			{
				return false;
			}

			string destinationPath = BuildDestinationPath(path, rootPath, Path.GetFileName(path));

			Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

			try
			{
				byte[] input = File.ReadAllBytes(path);

				if (Options.EncryptionMethod == EncryptionMethod.CXor)
				{
					SRResourceEncryption.CXor(input);
				}
				else if (Options.EncryptionMethod == EncryptionMethod.Xor)
				{
					SRResourceEncryption.Xor(input);
				}

				Array.Resize(ref input, input.Length - 4);

				File.WriteAllBytes(destinationPath, input);

				if (Options.VerboseLevel >= 4)
				{
					Console.Out.WriteLine("  ->  {0}", destinationPath);
					Console.Out.WriteLine();
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

			return true;
		}
	}

	public static class SRResourceEncryption
	{
		public static class Constants
		{
			public const byte XorKey = 204;
			public const byte CXorKey = 15;
		}

		public static int Xor(byte[] input)
		{
			int length = GetDecryptionLength(input);

			for (int i = 0; i < length; ++i)
			{
				input[i] = (byte)(input[i] ^ Constants.XorKey);
			}

			return length;
		}

		public static int CXor(byte[] input)
		{
			int length = GetDecryptionLength(input);
			byte key = Constants.CXorKey;

			for (int i = 0; i < length; ++i)
			{
				byte value = (byte)(input[i] ^ key);

				input[i] = value;
				key = value;
			}

			return length;
		}

		public static int GetDecryptionLength(byte[] input)
		{
			int realInputLength = input.Length - 4;
			int length = ((input[realInputLength + 0]) << 24 |
					(input[realInputLength + 1]) << 16 |
					(input[realInputLength + 2]) << 8 |
					(input[realInputLength + 3]));

			// TODO byteswap <length> if support for big-endian platforms is desired

			if (length == -1 || length >= realInputLength)
			{
				return realInputLength;
			}

			return length;
		}
	}
}
