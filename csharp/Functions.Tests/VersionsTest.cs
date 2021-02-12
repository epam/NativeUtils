using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using EPAM.Deltix.Utilities;

namespace FunctionsTests
{
	class Util {
		public static string ThisPath = Path.GetDirectoryName(typeof(VersionsTest).Assembly.Location);
		public static string ThisConfiguration = Path.GetFileName(Path.GetDirectoryName(ThisPath));
		public static string ThisFilename = Path.GetFileName(ThisPath);

		private const string randomDirRegEx = "^[0-9a-fA-F]{4,8}$";

		public static void CleanPath(string basePath)
		{
			string[] versionDirNames = { "0.0.0.0", "1.0.0.0", "2.0.0.0" };
			foreach (var version in versionDirNames)
			{
				for (int archBits = 32; archBits <= 64; archBits += archBits)
				{
					FileJanitor.TryCleanup(Path.Combine(basePath, $".deltix/Functions/DotNet/{version}/{archBits}"), true,
						".*");
					FileJanitor.TryCleanup(Path.Combine(basePath, $".deltix/Functions/DotNet/{version}"), true, ".*");
					FileJanitor.TryCleanup(Path.Combine(basePath, ".deltix/Functions/DotNet"), false, ".*");
					FileJanitor.TryCleanup(Path.Combine(basePath, ".deltix/Functions"), false, ".*");
				}
			}
		}

		public static void CleanPaths()
		{
			CleanPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
			CleanPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
			CleanPath(Path.GetTempPath());
		}

		public static void RunManyClients(int n, string extraArgs = "")
		{
			var assembly = typeof(Program).Assembly;
			var dir = Path.GetDirectoryName(assembly.Location);

			var startTime = DateTime.Now + TimeSpan.FromSeconds((n + 3) / 4) + TimeSpan.FromSeconds(2);
			var startTimeStr = startTime.ToString("s");
			string dir2 = Path.Combine(dir, "..", "..", "..", "..", "Functions.Client", "bin",
				"Release" /*Path.GetFileName(Path.GetDirectoryName(dir))*/, Path.GetFileName(dir));
			Console.WriteLine("Dst Path: " + dir2);
			Console.WriteLine($"Will run {n} child processes");

#if NETCOREAPP2_0
			for (int p = 0; p < n; ++p)
				Process.Start("dotnet", $"{dir2}/Functions.Client.dll {startTimeStr} {extraArgs}");
#else
			for (int p = 0; p < n; ++p)
				Process.Start($"{dir2}/Functions.Client.exe", $"{startTimeStr} {extraArgs}");
#endif

		}
	}

	class TestAssembly
	{
		public String Version { get; private set; }
		public String Location { get; private set; }
		public Assembly AssemblyObject { get; private set; }
		public Type FunctionsType { get; private set; }
		public MethodInfo TestFunction { get; private set; }

		public TestAssembly(string thisPath, string version, string configuration, string framework)
		{
			Version = version;
			Location = Path.GetFullPath(Path.Combine(thisPath, "..", "..", "..", "..", "Functions.Dll", "bin", version, configuration, framework, "FunctionsDll.dll"));

			AssemblyObject = Assembly.LoadFile(Location);
			FunctionsType = AssemblyObject.GetType("Functions.FunctionsDll");
			TestFunction = FunctionsType.GetMethod("TestFunction");
		}

		public double Invoke(double a, double b)
		{
			return (double)TestFunction.Invoke(null, new object[] { a, b });
		}
	}

	[TestFixture]
	class VersionsTest
	{
		string ThisPath = Util.ThisPath;
		string ThisFramework = Util.ThisFilename;
		string TargetFramework = Util.ThisFilename == "netcoreapp2.0" ? "netstandard2.0" : Util.ThisFilename;
		string ThisConfiguration = Util.ThisConfiguration;


		[Test]
		public void VersionsTestCase()
		{
			ThisConfiguration = "Release";
			//Console.WriteLine($"Test {thisConfiguration} configuration on the {thisFramework} framework.");

			// Delete previously existing files
			Util.CleanPaths();
			// Many iterations of assembly loading
			for (int i = 0; i < 10; ++i)
			{
				var assembly1 = new TestAssembly(ThisPath, "1-0-0", ThisConfiguration, TargetFramework);
				var assembly2 = new TestAssembly(ThisPath, "2-0-0", ThisConfiguration, TargetFramework);

				var r1 = assembly1.Invoke(6, 7);
				var r2 = assembly2.Invoke(6, 7);

				Assert.AreEqual(13, r1, $"Assembly V{assembly1.Version} result mismatch");
				Assert.AreEqual(42, r2, $"Assembly V{assembly2.Version} result mismatch");
			}
		}

		private const int msPerThread = 2000;
		private const int numThreads = 20;

		[Test, MaxTime((msPerThread + 1) * numThreads)]
		public void TestMultithreading()
		{
			int remaining = numThreads, unsuccesful = numThreads;
			Util.CleanPaths();
			for (int i = 0; i < numThreads; ++i)
			{
				(new Thread((x) =>
				{
					int j = (int)x;
					try
					{
						Thread.Sleep(500);
						var assembly1 = new TestAssembly(ThisPath, 0 == (j & 1) ? "1-0-0" : "2-0-0", ThisConfiguration, TargetFramework);
						assembly1.Invoke(123, 12345);

						Console.WriteLine($"Thread {j} done");
						Interlocked.Decrement(ref unsuccesful);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.StackTrace);
					}

					Interlocked.Decrement(ref remaining);
				})).Start(i);
			}

			for (int i = 0; i < (numThreads * msPerThread / 20); ++i)
			{
				if (0 == Interlocked.Add(ref remaining, 0))
				{
					Assert.AreEqual(0, Interlocked.Add(ref unsuccesful, 0), $"{unsuccesful} threads threw an exception");
					return; // Ok
				}

				Thread.Sleep(20);
			}

			Assert.Fail($"{remaining} threads failed to finish before timeout, {unsuccesful} threw exception");
		}


		//[Test, MaxTime(60000)]
		public void TestManyProcesses(string extraArgs = "")
		{
			Util.CleanPaths();
			Util.RunManyClients(5, extraArgs);
		}
	}
}
