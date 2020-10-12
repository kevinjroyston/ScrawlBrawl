using System.Collections.Generic;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;

namespace BackendAutomatedTestingClient.DataModels
{
    public interface IStructuredTest
    {
        public TestOptions TestOptions { get; }

        public IReadOnlyList<GameStep> UserPromptIdValidations { get; }
    }
}
