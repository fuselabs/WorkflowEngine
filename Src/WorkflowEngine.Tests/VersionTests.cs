using System;
using Xunit;
using Assert = Xunit.Assert;

namespace WorkflowEngine.Tests
{
    public class VersionTests
    {
        [Fact]
        public void Parsing_Valid()
        {
            RunParsingValidTest(0u, 0u);
            RunParsingValidTest(1u, 0u);
            RunParsingValidTest(1u, 1u);
            RunParsingValidTest(1u, 5u);
            RunParsingValidTest(1u, 15u);
            RunParsingValidTest(15u, 15u);
        }

        [Fact]
        public void Parsing_Invalid()
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

            Assert.Equal(major, version.MajorVersion);
            Assert.Equal(minor, version.MinorVersion);

            Assert.True(Version.TryParse(versionStr, out version));
            Assert.Equal(major, version.MajorVersion);
            Assert.Equal(minor, version.MinorVersion);

        }

        
        public static void RunParsingInvalidTest(string invalidVersion)
        {
            Assert.False(Version.TryParse(invalidVersion, out Version version));
            Assert.Null(version);

            try
            {
                new Version(invalidVersion);
                Assert.True(false, "An exception should have been thrown");
            }
            catch (ArgumentNullException ane)
            {
                Assert.True(ane.Message.Contains("Parameter name: version"));
            }
            catch (ArgumentException ae)
            {
                Assert.True(ae.Message.Contains("Parameter name: version"));
            }
            catch (Exception e)
            {
                Assert.True(false, $"Unexpected exception of type {e.GetType()} caught: {e.Message}");
            }
        }


    }
}
