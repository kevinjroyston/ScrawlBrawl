﻿using Backend.GameInfrastructure.DataModels;
using Common.DataModels.Responses;
using Common.DataModels.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.DataModels.Requests
{
    public class UserSubForm
    {
        [RegexSanitizer(Constants.RegexStrings.Guid)]
        public Guid Id { get; set; }

        [LengthSanitizer(max: 500)]
        public string ShortAnswer { get; set; }

        [RegexSanitizer(Constants.RegexStrings.ImageDataUrl)]
        [LengthSanitizer(max:10000000)]
        public string Drawing { private get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DrawingObject DrawingObject { get
            {
                if ((_DrawingObject==null) && (!string.IsNullOrEmpty(Drawing)))
                {
                    _DrawingObject = new DrawingObject(Drawing);
                }
                return _DrawingObject;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private DrawingObject _DrawingObject { get; set; }

        public int? Selector { get; set; }
        public IReadOnlyList<int> Slider { get; set; }

        public int? DropdownChoice { get; set; }
        public int? RadioAnswer { get; set; }

        [RegexSanitizer(Constants.RegexStrings.ColorString)]
        public string Color { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }

        public static UserSubForm WithDefaults(UserSubForm partialSubmission, SubPrompt prompt)
        {
            if (prompt == null)
            {
                throw new ArgumentNullException($"prompt was null");
            }
            // TODO: default to selecting random choice.
            return new UserSubForm()
            {
                Id = prompt.Id,
                ShortAnswer = prompt.ShortAnswer ? (partialSubmission?.ShortAnswer ?? "N/A") : null,
                Drawing = (prompt.Drawing != null) ? (partialSubmission?.Drawing ?? Constants.Drawings.DefaultDrawing(prompt.Drawing.DrawingType)) : null,
                Selector= (prompt.Selector != null) ? (partialSubmission?.Selector ?? (int?)0) : null,
                Slider= (prompt.Slider != null) ? (partialSubmission?.Slider ?? (prompt.Slider.Range ? new List<int> { prompt.Slider.Min, prompt.Slider.Max } : new List<int> { prompt.Slider.Min }) ): null,
                DropdownChoice = (prompt.Dropdown != null) ? (partialSubmission?.DropdownChoice ?? (int?)0): null,
                RadioAnswer = (prompt.Answers != null) ? (partialSubmission?.RadioAnswer ?? (int?)0) : null,
                Color = prompt.ColorPicker ? (partialSubmission?.Color ?? Constants.Colors.Black) : null,
            };

        }
    }
}
