using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class VersionTests
    {
        [TestMethod]
        public void Version_Parsing_Valid()
        {
            RunParsingValidTest(0u, 0u);
            RunParsingValidTest(1u, 0u);
            RunParsingValidTest(1u, 1u);
            RunParsingValidTest(1u, 5u);
            RunParsingValidTest(1u, 15u);
            RunParsingValidTest(15u, 15u);
        }

        [TestMethod]
        public void Version_Parsing_Invalid()
        {
            RunParsingInvalidTest("1");
            RunParsingInvalidTest("1.");
            RunParsingInvalidTest(".");
            RunParsingInvalidTest(".1");
            RunParsingInvalidTest("1.1.1");
            RunParsingInvalidTest("1..1");
            RunParsingInvalidTest("..");
            RunParsingInvalidTest("a.b");
            RunParsingInvalidTest("abc");
            RunParsingInvalidTest("");
            RunParsingInvalidTest(null);
        }


        private static void RunParsingValidTest(uint major, uint minor)
        {
            var versionStr = new Version(major, minor).ToString();

            var version = new Version(versionStr);

            Assert.AreEqual(major, version.MajorVersion);
            Assert.AreEqual(minor, version.MinorVersion);

            Assert.IsTrue(Version.TryParse(versionStr, out version));
            Assert.AreEqual(major, version.MajorVersion);
            Assert.AreEqual(minor, version.MinorVersion);

        }

        
        public static void RunParsingInvalidTest(string invalidVersion)
        {
            Assert.IsFalse(Version.TryParse(invalidVersion, out Version version));
            Assert.IsNull(version);

            try
            {
                new Version(invalidVersion);
                Assert.Fail("An exception should have been thrown");
            }
            catch (ArgumentNullException ane)
            {
                Assert.IsTrue(ane.Message.Contains("Parameter name: version"));
            }
            catch (ArgumentException ae)
            {
                Assert.IsTrue(ae.Message.Contains("Parameter name: version"));
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
            }
        }


    }
}
