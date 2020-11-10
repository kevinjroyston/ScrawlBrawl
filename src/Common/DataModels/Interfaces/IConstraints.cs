using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Common.DataModels.Interfaces
{
    public interface IConstraints<T> where T : IMember
    {
        /// <summary>
        /// A list of members <typeparamref name="T"/>.Id that cannot be added to this group.
        /// </summary>
        public ImmutableHashSet<Guid> BannedMemberIds { get; }

        /// <summary>
        /// The maximum number of members that can be added to this group.
        /// </summary>
        public int? MaxMemberCount { get; }

        /// <summary>
        /// The minimum number of members that must be added to this group.
        /// </summary>
        //public int? MinMemberCount { get; }

        /// <summary>
        /// A list of members <typeparamref name="T"/>.Tags that cannot be added to this group.
        /// </summary>
        public ImmutableHashSet<Guid> BannedMemberTags { get; }

        /// <summary>
        /// A list of members <typeparamref name="T"/>.Tags, any of which must be present to be added to this group.
        /// </summary>
        public ImmutableHashSet<Guid> AllowedMemberTags { get; }

        /// <summary>
        /// Indicates duplicates Ids are allowed. Default: false
        /// </summary>
        public bool? AllowDuplicateIds { get; }

        /// <summary>
        /// Indicates duplicates tags are allowed. Default: true
        /// </summary>
        public bool? AllowDuplicateTags { get; }

        /// <summary>
        /// Indicates duplicates tags are allowed. Default: true
        /// </summary>
        public bool? AllowDuplicateFromSource { get; }
    }
}
