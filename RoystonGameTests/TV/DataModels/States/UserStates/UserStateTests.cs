using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

using ValidationResult = RoystonGame.TV.DataModels.States.UserStates.UserState.CleanUserFormInputResult;
using RoystonGame.Web.Helpers.Validation;

namespace RoystonGameTests.TV.DataModels.States.UserStates
{
    [TestClass]
    public class UserStateTests
    {
        private List<(string, UserPrompt, UserFormSubmission, string, ValidationResult)> CleanUserFormInput_TestCases =
            new List<(string, UserPrompt, UserFormSubmission, string, ValidationResult)>()
            {
                ("1", new UserPrompt {Id = Guid.Empty, SubmitButton = false }, new UserFormSubmission { }, string.Empty, ValidationResult.Valid),
                ("2",null, new UserFormSubmission { }, "Try again or try refreshing the page.", ValidationResult.Invalid),
                ("3",new UserPrompt { Id = Guid.Empty}, null,  "Try again or try refreshing the page.", ValidationResult.Invalid),
                ("4",new UserPrompt { Id = Guid.NewGuid() }, new UserFormSubmission { }, "Outdated form submitted, try again or try refreshing the page.", ValidationResult.Invalid),
                ("5",new UserPrompt { Id = Guid.Empty, SubmitButton = true }, new UserFormSubmission { }, string.Empty, ValidationResult.Valid),
                ("6",new UserPrompt { Id = Guid.Empty, SubmitButton = true, SubPrompts = new SubPrompt[1] }, new UserFormSubmission { SubForms = new List<UserSubForm>() }, "Error in submission, try again or try refreshing the page.", ValidationResult.Invalid),
            };


        [TestMethod]
        public void CleanUserFormInput()
        {
            foreach((string testName,
                UserPrompt prompt,
                UserFormSubmission formSubmission,
                string expectedError,
                ValidationResult expectedResult) in CleanUserFormInput_TestCases)
            {
                ExecuteTestCase_CleanUserFormInput(testName, prompt, formSubmission, expectedError, expectedResult);
            }
        }

        private void ExecuteTestCase_CleanUserFormInput(
           string testName,
           UserPrompt userPrompt,
           UserFormSubmission userInput,
           string expectedError,
           ValidationResult expectedResult)
        {
            ValidationResult result = UserState.CleanUserFormInput(userPrompt, ref userInput, out string error);
            string returnMessage = $"TESTNAME: '{testName}', Prompt: '{JsonConvert.SerializeObject(userPrompt)}', UserInput: '{JsonConvert.SerializeObject(userInput)}'";
            Assert.AreEqual(expectedResult, result, returnMessage);
            Assert.AreEqual(expectedError, error, returnMessage);
        }

        [DataRow(true, "", false)]
        [DataRow(true, null, false)]
        [DataRow(false, null, true)]
        [DataRow(false, "abc", false)]
        [DataRow(true, "abc", true)]
        [DataTestMethod]
        public void Validate_Drawing(bool prompt, string drawing, bool expectedAnswer)
        {
            bool actual = PromptInputValidation.Validate_Drawing(
                new SubPrompt
                {
                    Drawing = prompt ? new DrawingPromptMetadata() : null
                }, new UserSubForm { Drawing = drawing });

            Assert.AreEqual(expectedAnswer, actual);
        }

        // TODO: add more tests.

        [DataRow(true, true, 0, 100, new int[] { 0, 5 }, true)]
        [DataRow(true, true, 0, 100, new int[] { 5, 0 }, false)]
        [DataRow(true, true, 0, 100, new int[] { -1, 5 }, false)]
        [DataRow(true, true, 0, 100, new int[] { 0, 101 }, false)]
        [DataRow(false, false, 0, 0, new int[] { 0, 5 }, false)]
        [DataRow(false, false, 0, 0, null, true)]
        [DataRow(true, false, 100, 200, new int[] { 50 }, false)]
        [DataRow(true, false, 100, 200, new int[] { 150 }, true)]
        [DataRow(true, false, 0, 100, new int[] { 200 }, false)]
        [DataRow(true, false, 0, 10, new int[] { 5 }, true)]
        [DataRow(true, false, 0, 100, new int[] { 0, 5 }, false)]
        [DataTestMethod]
        public void Validate_Slider(bool prompt, bool range, int min, int max, int[] submission, bool expectedAnswer)
        {
            bool actual = PromptInputValidation.Validate_Slider(
                new SubPrompt
                {
                    Slider = prompt ? new SliderPromptMetadata { Range = range, Min = min, Max = max } : null
                }, new UserSubForm { Slider = submission?.ToList() });

            Assert.AreEqual(expectedAnswer, actual);
        }
    }
}
