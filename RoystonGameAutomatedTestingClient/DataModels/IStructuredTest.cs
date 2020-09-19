using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.Games;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoystonGameAutomatedTestingClient.DataModels
{
    public interface IStructuredTest
    {
        public TestOptions TestOptions { get; }

        public IReadOnlyList<IReadOnlyDictionary<UserPromptId, int>> UserPromptIdValidations { get; }
    }
}
