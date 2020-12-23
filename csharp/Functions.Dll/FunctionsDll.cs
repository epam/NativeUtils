using System;
using System.Collections.Generic;
using System.Text;

namespace Functions
{
	public static class FunctionsDll
	{
		// It is enough to wrap native methods into a dummy object in order to avoid Mono premature DllImport resolution problem
		private static readonly FunctionsInterfaceImpl _impl = new FunctionsInterfaceImpl();
		private static readonly FunctionsInterface _interface = _impl;

		// Seem to work the same as direct static method call except object reference will be loaded (but not used)
		public static double TestFunction(double a, double b) => _impl.TestFunction(a, b);

		// Calling via interface seems to be as fast despite generated code actually using vtable call
		public static double TestFunction2(double a, double b) => _interface.TestFunction(a, b);

		// Should not reference DllImport class directly (breaks Mono compatibility)
		// Calling this method will fail if it is the first call made
		public static double TestFunction3(double a, double b) => FunctionsImport.TestFunction(a, b);
	}
}
