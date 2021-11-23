﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DataObjects;
using DataAccessInterFaces;
using LogicLayer;
using DataAccessFakes;


namespace LogicLayerTests
{
    [TestClass]
    public class RubricManagerTests
    {
        private IRubricManager<Rubric> rubricManager = null;

        [TestInitialize]
        public void TestSetup()
        {
            rubricManager = new RubricManager(new RubricAccessorFake(), new UserAccessorFake());
        }

        [TestMethod]
        public void TestRetrieveActiveRubricsReturnsCorrectList()
        {
            // arrange

            const int expectedCount = 3;
            int actualCount;

            // act
            actualCount = (rubricManager.RetrieveActiveRubrics().Count);

            // assert
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        public void TestRetrieveRubricByRubricIDReturnsCorrectRubric()
        {
            // arrange
            Rubric rubric = null;
            const int expectedRubricID = 100000;
            int actualRubricID;

            // act
            rubric = rubricManager.RetrieveRubricByRubricID(expectedRubricID);
            actualRubricID = rubric.RubricID;

            // assert
            Assert.AreEqual(expectedRubricID, actualRubricID);

        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void TestRetrieveRubricByBadRubricIDThrowsApplicationException()
        {
            // arrange
            Rubric rubric = null;
            const int expectedRubricID = 100111;
            int actualRubricID;

            // act
            rubric = rubricManager.RetrieveRubricByRubricID(expectedRubricID);
            actualRubricID = rubric.RubricID;

            // assert
            // nothing to assert, exception testing
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void TestRetrieveRubricByDuplicateRubricIDThrowsApplicationException()
        {
            // arrange
            Rubric rubric = null;
            const int expectedRubricID = 100003;
            int actualRubricID;

            // act
            rubric = rubricManager.RetrieveRubricByRubricID(expectedRubricID);
            actualRubricID = rubric.RubricID;
            // assert
            // nothing to assert, exception testing
        }

        [TestMethod]
        public void TestRetrieveRubricReturnsCorrectRubricCreator()
        {
            // arrange
            Rubric rubric = null;
            const string expectedUserID = "tess@company.com";
            string actualUserID;

            // act
            rubric = rubricManager.RetrieveRubricByRubricID(100000);
            actualUserID = rubric.RubricCreator.UserID;

            // assert
            Assert.AreEqual(expectedUserID, actualUserID);

        }



    }
}
