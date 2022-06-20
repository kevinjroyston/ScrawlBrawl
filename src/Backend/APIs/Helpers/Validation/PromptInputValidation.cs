using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System.Linq;

namespace Backend.APIs.Helpers.Validation
{
    public static class PromptInputValidation
    {
        public static bool Validate_Drawing(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForDrawing = prompt.Drawing != null;
            bool providedDrawing = !string.IsNullOrWhiteSpace(subForm.DrawingObject?.DrawingStr);
            return promptedForDrawing == providedDrawing;
        }
        public static bool Validate_ShortAnswer(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForShortAnswer = prompt.ShortAnswer;
            bool providedShortAnswer = !string.IsNullOrWhiteSpace(subForm.ShortAnswer);
            return promptedForShortAnswer == providedShortAnswer;
        }
        public static bool Validate_ColorPicker(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForColorPicker = prompt.ColorPicker;
            bool providedColorPicker = !string.IsNullOrWhiteSpace(subForm.Color);
            return promptedForColorPicker == providedColorPicker;
        }
        public static bool Validate_Answers(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForAnswers = prompt.Answers != null;
            bool providedAnswers = subForm.RadioAnswer.HasValue;
            bool submissionValid = !providedAnswers || (subForm.RadioAnswer.Value >= 0 && subForm.RadioAnswer.Value < prompt.Answers.Length);
            return (promptedForAnswers == providedAnswers) && submissionValid;
        }

        public static bool Validate_Selector(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForSelector = prompt.Selector != null;
            bool providedSelector = subForm.Selector.HasValue;
            bool submissionValid = !providedSelector || (subForm.Selector.Value >= 0 && subForm.Selector.Value < (prompt.Selector?.ImageList?.Length ?? 0));
            return (promptedForSelector == providedSelector) && submissionValid;
        }

        public static bool Validate_Dropdown(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForDropdown = prompt.Dropdown != null;
            bool providedDropdown = subForm.DropdownChoice.HasValue;
            bool validSubmission = !providedDropdown || (subForm.DropdownChoice.Value >= 0 && subForm.DropdownChoice.Value < prompt.Dropdown.Length);
            return (promptedForDropdown == providedDropdown) && validSubmission;
        }

        public static bool Validate_Slider(SubPrompt prompt, UserSubForm subForm)
        {
            bool promptedForSlider = prompt.Slider != null;
            bool providedSlider = subForm.Slider != null;

            if (!promptedForSlider || !providedSlider)
            {
                return promptedForSlider == providedSlider;
            }

            bool validSubmission = true;

            // Range sliders should have a list of length 2 (min, max). Nonrange sliders should only have one value.
            validSubmission &= (subForm.Slider.Count == (prompt.Slider.Range ? 2 : 1));
            validSubmission &= subForm.Slider.All(val => (val >= prompt.Slider.Min) && (val <= prompt.Slider.Max));
            validSubmission &= subForm.Slider.Count == 1 || (subForm.Slider[0] <= subForm.Slider[1]);

            return validSubmission;
        }
    }
}
