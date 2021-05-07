using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace EPAM.Deltix.Utilities.ResourceLoaderUtils
{
	public static class UtilHelper
	{
		public static String GetTags(String str, Dictionary<String, String>  tags)
		{
			tags.Clear();
			return Util.GetTags(str, tags);
		}
	}
}

namespace FunctionsTests
{
	using static EPAM.Deltix.Utilities.ResourceLoaderUtils.UtilHelper;
	[TestFixture]
	class UtilsTest
    {
		[Test]
	    public void TestTags()
	    {
			Dictionary<String, String> tags = new Dictionary<String, String>();
			
			Assert.AreEqual("12345", GetTags("12[@]345", tags));
			Assert.AreEqual(1, tags.Count);
		    Assert.AreEqual("", tags[""]);
			Assert.AreEqual("12345", GetTags("12[i@141]345", tags));
		    Assert.AreEqual(1, tags.Count);
		    Assert.AreEqual("141", tags["i"]);
			Assert.AreEqual("kernel32_dll_zst", GetTags("kerne[i@141]l32_d[foo@b[*~ar]ll_zst", tags));
		    Assert.AreEqual(2, tags.Count);
		    Assert.AreEqual("141", tags["i"]);
		    Assert.AreEqual("b[*~ar", tags["foo"]);
			Assert.AreEqual("fo[[]]obar[]].so.1", GetTags("fo[[]]ob[\\@454564523463&%^&$%!#$!$]ar[]].so.1", tags));
		    Assert.AreEqual(1, tags.Count);
		    Assert.AreEqual("454564523463&%^&$%!#$!$", tags["\\"]);
		}
    }
}
