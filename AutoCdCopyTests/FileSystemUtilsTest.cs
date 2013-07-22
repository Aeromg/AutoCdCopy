using ADev.FileSystemTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace AutoCdCopyTests
{
    
    
    /// <summary>
    ///This is a test class for FileSystemUtilsTest and is intended
    ///to contain all FileSystemUtilsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileSystemUtilsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetRelativePath
        ///</summary>
        [TestMethod()]
        public void GetRelativePathTest()
        {
            // case inbound
            DirectoryInfo baseDirectory = new DirectoryInfo("c:\\test");
            DirectoryInfo subDirectory = new DirectoryInfo("c:\\test\\subtest\\subtest2");
            string expected = "subtest\\subtest2";
            string actual;
            actual = FileSystemUtils.GetRelativePath(baseDirectory, subDirectory);
            Assert.AreEqual(expected, actual);

            // case outbound
            DirectoryInfo outDirectory = new DirectoryInfo("d:\\outtest\\subtest\\subtest2");
            expected = "outtest\\subtest\\subtest2";
            actual = FileSystemUtils.GetRelativePath(baseDirectory, outDirectory);
            Assert.AreEqual(expected, actual);

            // case rooted
            baseDirectory = new DirectoryInfo("c:\\");
            expected = "test\\subtest\\subtest2";
            actual = FileSystemUtils.GetRelativePath(baseDirectory, subDirectory);
            Assert.AreEqual(expected, actual);

            // case fail outbound
            baseDirectory = new DirectoryInfo("c:\\test\\subdirectory");
            outDirectory = new DirectoryInfo("c:\\outbound\\sub");
            expected = "outbound\\sub";
            actual = FileSystemUtils.GetRelativePath(baseDirectory, outDirectory);
            Assert.AreEqual(expected, actual);
        }
    }
}
