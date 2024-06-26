﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Responses.Gameplay
{
    public class PromptHeaderMetadata
    {
        public int MaxProgress { get; set; } = 0;
        public int CurrentProgress { get; set; } = 0;
        public TimeSpan? ExpectedTimePerPrompt { private get; set; }

        public int ExpectedTimePerPromptInSec => (int)(ExpectedTimePerPrompt?.TotalSeconds ?? -1.0);
        public string PromptLabel { get; set; } = "";
    }
}
