using RaikesSimplexService.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using RaikesSimplexService.DataModel;
using UnitTests.Helpers;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for SolverTest and is intended
    ///to contain all SolverTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StandardModelTest
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
        public void SimpleToStringMatrixTest()
        {
            String expectedMatrixification = "X0\tX1\tX2\tS0\tS1\t|\tRHS\r\n1\t3\t4\t1\t0\t|\t10\r\n4\t3\t2\t0\t1\t|\t50\r\nObjective\r\n-2\t-6\t-4\t0\t0\r\n";
            ToStringMatrixTest(simpleModel, expectedMatrixification);
        }

        public void ToStringMatrixTest(StandardModel modelToMatrixify, String expectedMatrixification)
        {
            String actualMatrixification = simpleModel.ToString(StandardModel.OutputFormat.Matrix);
            Assert.AreEqual(expectedMatrixification, actualMatrixification);
        }

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
