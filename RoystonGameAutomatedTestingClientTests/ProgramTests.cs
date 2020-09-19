using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoystonGameAutomatedTestingClient.cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoystonGameAutomatedTestingClient.cs.Tests
{
    [TestClass()]
    public class ProgramTests
    {

        [TestMethod()]
        public void CollectParamsTest()
        {
            string[] arguments = new string[] { "-parallel", "-users", "4", "Imposter Syndrome" };
            Assert.Fail();
        }

    }
}