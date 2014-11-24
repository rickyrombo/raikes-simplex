using RaikesSimplexService.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using RaikesSimplexService.DataModel;
using RaikesSimplexService.Implementation.Extensions;
using UnitTests.Helpers;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for DataModelStringify and is intended
    ///to contain all DataModelStringify Unit Tests
    ///</summary>
    [TestClass()]
    public class DataModelStringify
    {
        private TestContext testContextInstance;
        private StandardModel simpleModel;

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
        [TestInitialize()]
        public void MyTestInitialize()
        {
            simpleModel = StandardModelGenerator.getSimpleModel();
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void SimpleToStringOriginalTest()
        {
            String expectedStringification = "Constraints:\n1 X1\t+ 3 X2\t+ 4 X3 \t<= 10\n4 X1\t+ 3 X2\t+ 2 X3 \t<= 50\nObjective: Maximize\n-2 X1\t+ -6 X2\t+ -4 X3 \t= Z";
            ToStringOriginalTest(simpleModel, expectedStringification);
        }

        public void ToStringOriginalTest(StandardModel modelToStringify, String expectedStringification)
        {
            String actualStringification = modelToStringify.ToString(StandardModel.OutputFormat.Original);
            Assert.AreEqual(expectedStringification, actualStringification);
        }

        [TestMethod()]
        public void SimpleToStringExpressionTest()
        {
            String expectedStringification = "Constraints:\n1 X1\t+ 3 X2\t+ 4 X3\t+ 1 S1\t+ 0 S2\t= 10\n4 X1\t+ 3 X2\t+ 2 X3\t+ 0 S1\t+ 1 S2\t= 50\nObjective: Maximize\nZ\t+ -2 X1\t+ -6 X2\t+ -4 X3\t+ 0 S1\t+ 0 S2 \t= 0";
            ToStringExpressionTest(simpleModel, expectedStringification);
        }

        public void ToStringExpressionTest(StandardModel modelToStringify, String expectedStringification)
        {
            String actualStringification = modelToStringify.ToString(StandardModel.OutputFormat.Expression);
            Assert.AreEqual(expectedStringification, actualStringification);
        }

    }
}
