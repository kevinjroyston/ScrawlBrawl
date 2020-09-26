using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.Games;
using System;
using System.Collections.Generic;
using System.Text;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<RoystonGame.Web.DataModels.Enums.UserPromptId, int>;

namespace RoystonGameAutomatedTestingClient.DataModels
{
    public interface IStructuredTest
    {
        public TestOptions TestOptions { get; }

        public IReadOnlyList<GameStep> UserPromptIdValidations { get; }
    }
}
