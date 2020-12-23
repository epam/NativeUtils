using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Deltix
{
	class FunctionsDllClient
	{
		public static string ThisPath = Path.GetDirectoryName(typeof(FunctionsDllClient).Assembly.Location);
		public static string ThisConfiguration = Path.GetFileName(Path.GetDirectoryName(ThisPath));
		public static string ThisFilename = Path.GetFileName(ThisPath);
		private static bool Verbose = false;

		static public void InvokeDll(string version)
		{
			string framework = ThisFilename == "netcoreapp2.0" ? "netstandard2.0" : ThisFilename;
			string Location = Path.GetFullPath(Path.Combine(ThisPath, "..", "..", "..", "..", "Functions.Dll", "bin", version,
				"Release"/* ThisConfiguration*/, framework, "FunctionsDll.dll"));

			Assembly AssemblyObject = Assembly.LoadFile(Location);
			Type FunctionsType = AssemblyObject.GetType("Functions.FunctionsDll");
			MethodInfo TestFunction = FunctionsType.GetMethod("TestFunction");

			double x = (double)TestFunction.Invoke(null, new object[] { 1, 2 });
			double x1 = (double)TestFunction.Invoke(null, new object[] { 10, 20 });
			if (Verbose)
			{
				Console.WriteLine($"f(1, 2) = {x}");
				Console.WriteLine($"f(10, 20) = {x1}");
			}
		}

		// Console version of the same stuff
		static void Main(string[] args)
		{
			var startTime = DateTime.Parse(args.Length > 0 && args[0].Length > 10 ? args[0] : DateTime.Now.AddSeconds(2).ToString());
			Console.WriteLine("Will start execution on: " + startTime);
			Thread.Sleep((int)Math.Max((startTime - DateTime.Now).TotalMilliseconds, 100.0));
			bool failed = false;
			bool wait = args.Any(x => (x.Contains("-w") || x.Equals("wait")));

			try
			{
				InvokeVersion(1);
			} catch(Exception e)
			{
				Console.WriteLine(e);
				failed = true;
			}

			try
			{
				InvokeVersion(2);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				failed = true;
			}

			// On Linux, you can truncate or modify already loaded native library and crash the application.
			// Apparently, nothing will protect from this behavior if you have the necessary access rights.
			//try
			//{
			//	new FileStream("/tmp/_/1.0.0.0/64/libFunctionsNative1-0-0.so", FileMode.Create, FileAccess.ReadWrite).Dispose();
			//}
			//catch {}

			//Thread.Sleep(4000);
			Console.WriteLine($"{Process.GetCurrentProcess().Id}: Again.. ");

			try
			{
				InvokeVersion(1);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				failed = true;
			}

			try
			{
				InvokeVersion(2);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				failed = true;
			}

			if (failed || wait)
				Console.ReadKey(true);
		}

		private static void InvokeVersion(int v)
		{
			Console.Write($"{Process.GetCurrentProcess().Id}: Invoke V{v}.. ");
			if (Verbose)
				Console.WriteLine();

			InvokeDll($"{v}-0-0");
			Console.WriteLine($"{Process.GetCurrentProcess().Id}: V{v} OK!");
		}
	}
}
