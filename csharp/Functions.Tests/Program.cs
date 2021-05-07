using System;
using System.IO;
using System.Threading;

namespace FunctionsTests
{
	class Program
	{
		static string DirParent(string dir, int n)
		{
			for (int i = 0; i < n; ++i)
			{
				dir = Directory.GetParent(dir).ToString();
			}

			return dir;
		}

		// Console version of the same stuff
		static void Main(string[] args)
		{

			//try
			//{
			//	Console.WriteLine("-------------VersionsTestCase-------------");
			//	new VersionsTest().VersionsTestCase();
			//	Console.WriteLine("OK!");
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine(e);
			//}

			//try
			//{
			//	Console.WriteLine("-------------TestMultithreading-------------");
			//	new VersionsTest().TestMultithreading();
			//	Console.WriteLine("OK!");
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine(e);
			//}
			Thread.Sleep(1000);
			try
			{
				Console.WriteLine("-------------Test Many processes-------------");
				new VersionsTest().TestManyProcesses(args.Length > 0 ? args[args.Length - 1] : "");
				Console.WriteLine("OK!");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			//Util.CleanPath(Path.GetTempPath());
			//Util.CleanPath(DirParent(typeof(Program).Assembly.Location, 5));

			//Console.WriteLine(typeof(Program).Assembly.Location);
			//Console.WriteLine(DirParent(typeof(Program).Assembly.Location, 5));
			//Console.WriteLine("Old files deleted, press Enter");
			//Console.ReadLine();
			//Console.WriteLine("Running test");

			//Util.RunManyClients(3);
		}
	}
}
