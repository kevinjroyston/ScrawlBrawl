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
        // TODO: do we need this if the type is Guid??
        [RegexSanitizer(Constants.RegexStrings.Guid)]
        public Guid Id { get; set; }

        public List<UserSubForm> SubForms { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }

        public static UserFormSubmission WithDefaults(UserFormSubmission partialSubmission, UserPrompt prompt)
        {
            partialSubmission ??= new UserFormSubmission();
            partialSubmission.SubForms ??= prompt.SubPrompts?.Select<SubPrompt, UserSubForm>(_ => null).ToList();
            return new UserFormSubmission()
            {
                Id = prompt.Id,
                SubForms = (prompt.SubPrompts == null) ? null : partialSubmission.SubForms.Zip(prompt.SubPrompts)
                    .Select<(UserSubForm, SubPrompt), UserSubForm>(
                        (tuple) => UserSubForm.WithDefaults(tuple.Item1, tuple.Item2)).ToList()
            };
        }
        public static UserFormSubmission WithNulls(UserPrompt prompt)
        {
            var submission = new UserFormSubmission();
            if (prompt == null)
            {
                return submission;
            }

            submission.SubForms = prompt.SubPrompts?.Select<SubPrompt, UserSubForm>(_ => null).ToList();
            return new UserFormSubmission()
            {
                Id = prompt.Id,
                SubForms = (prompt.SubPrompts == null) ? null : submission.SubForms.Zip(prompt.SubPrompts)
                    .Select<(UserSubForm, SubPrompt), UserSubForm>(
                        (tuple) => new UserSubForm()
                        {
                            Id = prompt.Id
                        }).ToList()
            };
        }
    }
}
