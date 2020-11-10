using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Interfaces
{
    public interface IMember : Identifiable
    {
        /// <summary>
        /// A list of Tags associated with this member.
        /// ex: Team Id
        /// </summary>
        public IReadOnlyList<Guid> Tags { get; }

        /// <summary>
        /// An Id representing the source of this member.
        /// ex: The UserId who created this object.
        /// </summary>
        public Guid Source { get; }
    }
}
