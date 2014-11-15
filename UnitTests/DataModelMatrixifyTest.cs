using RaikesSimplexService.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using RaikesSimplexService.DataModel;
using RaikesSimplexService.Implementation.Extensions;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for DataModelMatrixify and is intended
    ///to contain all DataModelMatrixify Unit Tests
    ///</summary>
    [TestClass()]
    public class DataModelMatrixifyTest
    {
        private TestContext testContextInstance;
        private Model testModel;

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
            testModel = new Model
            {
                Constraints = new List<LinearConstraint>
                { 
                    new LinearConstraint{
                        Coefficients = new double[]{1, 3, 4},
                        Relationship = Relationship.LessThanOrEquals,
                        Value = 10,
                    },
                    new LinearConstraint{
                        Coefficients = new double[]{4, 3, 2},
                        Relationship = Relationship.GreaterThanOrEquals,
                        Value = 50,
                    }
                },
                Goal = new Goal
                {
                    Coefficients = new double[] { 2, 6, 4 },
                    ConstantTerm = 20
                },
                GoalKind = GoalKind.Minimize,
            };
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
        public void MatrixifyTest()
        {
            var solver = new Solver();
            var model = solver.Standardize(testModel);
            Console.WriteLine(model.Matrixify());
        }
    }
}
