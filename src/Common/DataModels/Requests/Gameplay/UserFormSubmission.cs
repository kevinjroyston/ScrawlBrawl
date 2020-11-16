using Common.DataModels.Responses;
using Common.DataModels.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Common.DataModels.Requests
{
    public class UserFormSubmission
    {
        [RegexSanitizer("^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$")]
        public Guid Id { get; set; }

        public List<UserSubForm> SubForms { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }

        public static UserFormSubmission WithDefaults(UserFormSubmission partialSubmission, UserPrompt prompt)
        {
            return new UserFormSubmission()
            {
                Id = partialSubmission.Id,
                SubForms = (prompt.SubPrompts == null) ? null : partialSubmission.SubForms.Zip(prompt.SubPrompts)
                    .Select<(UserSubForm, SubPrompt), UserSubForm>(
                        (tuple) => UserSubForm.WithDefaults(tuple.Item1, tuple.Item2)).ToList()
            };
        }
    }
}
