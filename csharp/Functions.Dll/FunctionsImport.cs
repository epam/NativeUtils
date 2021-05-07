using System;
using System.Runtime.InteropServices;
using EPAM.Deltix.Utilities;


namespace Functions
{
	static class Const
	{
		// Debug logging flag
		internal static bool Verbose = false;
	}

	internal interface FunctionsInterface
	{
		double TestFunction(double a, double b);
	}

	internal class FunctionsInterfaceImpl : FunctionsInterface
	{
		static FunctionsInterfaceImpl()
		{
			Init();
		}

		public static void Init()
		{
			// Also possible to just move ResourceLoader call into this constructor
			if (Const.Verbose)
				Console.WriteLine("Native Init...");

			if (_initialized)
				return;

			if (Const.Verbose)
				Console.WriteLine("Will load native lib");

			var rl =
				ResourceLoader.From("Functions.$(OS).x$(ARCH).*")
					.To(".deltix/Functions/DotNet/$(VERSION)/$(ARCH)").Load();
				//.To("c:/Users/ChuprinB/Dropbox/Projects/decimal.net/NativeUtils/csharp/Functions/DotNet/$(VERSION)/$(ARCH)").Load();
				//.To("Functions/DotNet/Debug").Load();

				// If we don't want automatic target root path selection and fallback path features:

				//.To("$(TEMP)/Functions/DotNet/$(VERSION)/$(ARCH)")/*.RetryTimeout(2000)*/.Load();
				//.To("$(TEMP)/_/$(RANDOM)").Load();
				//.To("$(TEMP)/_/$(VERSION)/$(ARCH)").AlwaysOverwite(true).ReusePartiallyDeployed(false).TryRandomFallbackSubDirectory(true).Load();

			if (Const.Verbose)
				Console.WriteLine("Loaded native lib at: {0}", rl.ActualDeploymentPath);

			_initialized = true;
		}

		public double TestFunction(double a, double b) => FunctionsImport.TestFunction(a, b);
		private static bool _initialized;
	}

	internal class FunctionsImport
	{
		internal const string NativeDllName = "FunctionsNative" + Version.versionDashed;

		static FunctionsImport()
		{
			FunctionsInterfaceImpl.Init();

			// Assume we need to call a native initialization function once
			var rv = TestFunction(0, 0);

			if (Const.Verbose)
				Console.WriteLine($"Some init method returned: {rv}");
		}

		[DllImport(NativeDllName, EntryPoint="testFunction", CallingConvention = CallingConvention.Cdecl)]
		internal static extern double TestFunction(double a, double b);
	}
}
