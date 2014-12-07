using RaikesSimplexService.Implementation;
using RaikesSimplexService.Implementation.Extensions;
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
    public class SolverTest
    {
        private TestContext testContextInstance;
        private Model simpleModel, impossibleModel, unboundedModel, twoPhaseModel, ashuModel;

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
            ashuModel = ModelGenerator.GetAshuModel();
            simpleModel = ModelGenerator.GetSimpleModel();
            impossibleModel = ModelGenerator.GetImpossibleModel();
            unboundedModel = ModelGenerator.GetUnboundedModel();
            twoPhaseModel = ModelGenerator.GetTwoPhaseModel();
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
        public void SolveSimpleModelTest()
        {
            Solution expectedSolution = SolutionGenerator.GetSimpleSolution();
            SolveModelTest(simpleModel, expectedSolution);
        }

        [TestMethod()]
        public void SolveImpossibleModelTest()
        {
            SolveModelTest(impossibleModel, SolutionQuality.Infeasible);
        }

        [TestMethod()]
        public void SolveUnboundedModelTest()
        {
            SolveModelTest(unboundedModel, SolutionQuality.Unbounded);
        }

        [TestMethod()]
        public void SolveTwoPhaseModelTest()
        {
            SolveModelTest(twoPhaseModel, SolutionGenerator.GetTwoPhaseSolution());
        }

        [TestMethod()]
        public void SolveAshuModelTest()
        {
            Console.WriteLine(StandardModel.FromModel(ashuModel).ToString(StandardModel.OutputFormat.Expression));
            SolveModelTest(ashuModel, SolutionGenerator.GetAshuSolution());
        }

        [TestMethod()]
        public void NearlyZeroPositiveTest()
        {
            double zero = -0.0000000001;
            Assert.IsTrue(Solver.NearlyZero(zero));
        }


        [TestMethod()]
        public void NearlyZeroNegativeTest()
        {
            double zero = -0.0000000001;
            Assert.IsTrue(Solver.NearlyZero(zero));
        }

        [TestMethod()]
        public void NearlyZeroFalseTest()
        {
            double nonZero = 0.01;
            Assert.IsFalse(Solver.NearlyZero(nonZero));
        }


        public void SolveModelTest(Model m, SolutionQuality expectedQuality)
        {
            Solver solver = new Solver();
            Solution actualSolution = solver.Solve(m);
            Assert.AreEqual(expectedQuality, actualSolution.Quality);
        }

        public void SolveModelTest(Model m, Solution expectedSolution)
        {
            Solver solver = new Solver();
            Solution actualSolution = solver.Solve(m);
            Assert.IsTrue(expectedSolution.EqualValues(actualSolution));
        }


    }
}

