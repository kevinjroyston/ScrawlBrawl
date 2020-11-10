using Common.DataModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Code.Extensions
{
    public static class IConstraintExtensions
    {
        public static bool SatisfiesConstraints<M>(this IConstraints<M> constraints, IEnumerable<M> members, M member, out int constraintViolations)
            where M : IMember
        {
            bool isValid = true;
            // TODO: rewrite this code to be more efficient, using hashsets instead of iterating the lists.
            constraintViolations = 0;
            if (constraints.AllowDuplicateFromSource.HasValue
                && !constraints.AllowDuplicateFromSource.Value)
            {
                constraintViolations += members.Any(mem => mem.Source == member.Source) ? 1 : 0;
            }

            if (constraints.AllowDuplicateIds.HasValue
                && !constraints.AllowDuplicateIds.Value)
            {
                constraintViolations += members.Any(mem => mem.Id == member.Id) ? 1 : 0;
            }

            if (constraints.AllowDuplicateTags.HasValue
                && !constraints.AllowDuplicateTags.Value)
            {
                constraintViolations += members.Where(mem => mem.Tags.Intersect(member.Tags).Count() > 0).Count();
            }

            if (constraints.AllowedMemberTags != null)
            {
                constraintViolations += (member.Tags.Intersect(constraints.AllowedMemberTags).Count() <= 0) ? 1 : 0;
            }

            if (constraints.BannedMemberIds != null)
            {
                constraintViolations += constraints.BannedMemberIds.Contains(member.Id) ? 1 : 0;
            }

            if (constraints.BannedMemberTags != null)
            {
                constraintViolations += (member.Tags.Intersect(constraints.BannedMemberTags).Count() > 0) ? 1 : 0;
            }
            /* // This doesn't work / needs to be re-thought
            if (constraints.MinMemberCount.HasValue
                && ((members.Count() + 1) < constraints.MinMemberCount.Value))
            {
                constraintViolations += 1;
            }*/

            if (constraints.MaxMemberCount.HasValue
                && ((members.Count() + 1) > constraints.MaxMemberCount.Value))
            {
                isValid = false;
                //constraintViolations += 1;
            }

            return isValid;
        }
        public static bool SatisfiesConstraints<M>(IConstraints<M> groupConstraints, IEnumerable<IConstraints<M>> subGroupConstraints, IEnumerable<IEnumerable<M>> members, M member, out int constraintViolations)
            where M : IMember
        {
            constraintViolations = 0;
            bool valid = true;
            foreach((IConstraints<M> constraints, IEnumerable<M> group) in subGroupConstraints.Zip(members))
            {
                valid &= constraints.SatisfiesConstraints(group, member, out int violations);
                constraintViolations += violations;
            }
            valid &= groupConstraints.SatisfiesConstraints(members.SelectMany(grp => grp).ToList(), member, out int violations2);
            constraintViolations += violations2;
            return valid;
        }
    }
}
