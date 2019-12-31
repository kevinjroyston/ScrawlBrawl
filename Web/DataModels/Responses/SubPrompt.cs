using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.Responses
{
    public class SubPrompt
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The prompt to answer.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// A list of strings w/o radio buttons.
        /// </summary>
        public string[] StringList { get; set; }

        /// <summary>
        /// The radio answers to choose from, if applicable
        /// </summary>
        public string[] Answers { get; set; }

        /// <summary>
        /// Indicates a color picker tool should be rendered.
        /// </summary>
        public bool ColorPicker { get; set; }

        /// <summary>
        /// Indicates a text box should be rendered.
        /// </summary>
        public bool ShortAnswer { get; set; }

        /// <summary>
        /// If this is not null a drawing will be prompted with the specified metadata.
        /// </summary>
        public DrawingPromptMetadata Drawing { get; set; }


    }
}
