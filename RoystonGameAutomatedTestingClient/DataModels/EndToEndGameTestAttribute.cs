using System;
using System.Collections.Generic;
using System.Text;

namespace RoystonGameAutomatedTestingClient.DataModels
{
    public class EndToEndGameTestAttribute : Attribute
    {
        public string TestName { get; private set; }
        public string TestDescription { get; private set; }

        /// <summary>
        /// Indicates that this specific class contains a test.
        /// </summary>
        /// <param name="testName">Name of test when asking to run from command line.</param>
        /// <param name="description">The test description.</param>
        public EndToEndGameTestAttribute(string testName, string description = null)
        {
            TestName = testName;
            TestDescription = description;
        }
    }
}