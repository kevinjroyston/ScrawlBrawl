using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Responses
{
    public class UserRecordType
    {
        public string PlayerName { get; set; }
    }
    public class UserListMetadata
    {
            public int UserCount { get; set; } = 0;

            public UserRecordType[] UserRecords { get; set; }

    }
}
