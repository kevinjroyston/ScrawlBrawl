using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Responses
{
    public class UserRecordType
    {
        public string PlayerName { get; set; }
    }
    public class UserListPromptMetadata
    {
            public int UserCount { get; set; } = 0;

            public string Description { get; set; } = "";

            public UserRecordType[] UserRecords { get; set; }

    }
}
