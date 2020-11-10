using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Common.DataModels.Interfaces
{
    public class Constraints<M> : IConstraints<M> where M : IMember
    {
        public virtual ImmutableHashSet<Guid> BannedMemberIds { get; set; } = null;

        public virtual int? MaxMemberCount { get; set; } = null;

        public virtual ImmutableHashSet<Guid> BannedMemberTags { get; set; } = null;

        public virtual ImmutableHashSet<Guid> AllowedMemberTags { get; set; } = null;

        public virtual bool? AllowDuplicateIds { get; set; } = null;

        public virtual bool? AllowDuplicateTags { get; set; } = null;

        public virtual bool? AllowDuplicateFromSource { get; set; } = null;
    }
}
