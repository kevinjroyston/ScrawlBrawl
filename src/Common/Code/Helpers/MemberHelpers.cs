using Common.Code.Extensions;
using Common.Code.Validation;
using Common.DataModels.Interfaces;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MiscUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Common.Code.Helpers
{
    public class MemberHelpers<M> where M:IMember
    {
        /// <summary>
        /// Selects an equal number from each source wherever possible. The items are prioritized in the order they are provided.
        /// </summary>
        /// <param name="members">List of members to pull from. They will be grouped by source.</param>
        /// <param name="count">Total members to take.</param>
        /// <returns>A list of length <paramref name="count"/> members evenly taken based on source.</returns>
        public static List<M> Select_Ordered(IReadOnlyList<M> members, int count)
        {
            Arg.GreaterThan(count, 0, nameof(count));
            Arg.NotNullOrEmpty(members, nameof(members));

            List<M> memberList = members.ToList();
            List<M> output = new List<M>();
            if (memberList.Count <= count)
            {
                output.AddRange(memberList);
                return output;
            }



            // Group input members by their sources, randomly ordering the group members
            Dictionary<Guid, Queue<M>> sourceToMemberMap = memberList
                .GroupBy((member) => member.Source)
                .ToDictionary(
                    group => group.Key,
                    group => new Queue<M>(group));
            List<Queue<M>> orderedSources = sourceToMemberMap.Values.OrderBy((Queue<M> val) => StaticRandom.Next()).ToList();
            int currentSource = 0;
            while (output.Count < count)
            {
                if (orderedSources[currentSource].TryDequeue(out M val))
                {
                    output.Add(val);
                }
                currentSource = (currentSource + 1) % orderedSources.Count;
            }

            return output;
        }

        /// <summary>
        /// Selects items from an input list using a dynamic weighted random strategy. The weights are per SOURCE (IMember.Source).
        /// If an item from a given source has not been selected recently, that source gets a higher weight.
        /// </summary>
        /// <param name="members">A list of members to be selected from.</param>
        /// <param name="count">The total number to select.</param>
        /// <param name="dynamicWeightMultiplier">(0.0,infinity). Use 1 for unweighted random, larger values result in more even distribution, lower values result in a spikier normal distribution.</param>
        /// <returns>The output list containing the selected members.</returns>
        public static List<M> Select_DynamicWeightedRandom(
            IReadOnlyList<M> members,
            int count,
            double dynamicWeightMultiplier = 1.0)
        {
            Arg.GreaterThan(dynamicWeightMultiplier, 0.0, nameof(dynamicWeightMultiplier));
            Arg.GreaterThan(count, 0, nameof(count));
            Arg.NotNullOrEmpty(members, nameof(members));

            List<M> memberList = members.ToList();
            List<M> output = new List<M>();
            if (memberList.Count <= count)
            {
                output.AddRange(memberList);
                return output;
            }

            // Group input members by their sources, randomly ordering the group members
            Dictionary<Guid, Queue<M>> sourceToMemberMap = memberList
                .GroupBy((member) => member.Source)
                .ToDictionary(
                    group => group.Key,
                    group => new Queue<M>(group.OrderBy((M val) => StaticRandom.Next())));

            // Instantiate all source weights to 1.0.
            Dictionary<Guid, double> sourceToDynamicWeight = sourceToMemberMap.ToDictionary(kvp => kvp.Key, kvp => 1.0);

            for (int i = 0; i < count; i++)
            {
                // Select random source based on weights.
                Guid selectedSource = sourceToDynamicWeight.GetWeightedRandomKey();

                // Increase weights of non-selected items.
                foreach ((Guid source, double weight) in sourceToDynamicWeight.ToList())
                {
                    if (source != selectedSource)
                    {
                        sourceToDynamicWeight[source] = weight * dynamicWeightMultiplier;
                    }
                }

                // Take first member from selected source
                output.Add(sourceToMemberMap[selectedSource].Dequeue());

                // Check if that source is empty and set weight accordingly.
                if (!sourceToMemberMap[selectedSource].TryPeek(out _))
                {
                    sourceToDynamicWeight[selectedSource] = 0.0;
                }
            }

            return output;
        }

        /*
        /// <summary>
        /// Assigns a list of members to a set of nested groups (groups within groups).
        /// Constraints of the subgroups take precedence where possible.
        /// </summary>
        /// <typeparam name="T">Defines the constraints for a group as well as contains a group of constraints for its' subgroup.</typeparam>
        /// <typeparam name="U">The member type being grouped.</typeparam>
        /// <param name="constraints">List of constraints containing member list of subgroup constraints.</param>
        /// <param name="members">List of members to be assigned to groups</param>
        /// <returns>The assigned list of groups of groups of members.</returns>
        public static List<IGroup<IGroup<U>>> Assign<T,U>(List<T> constraints, List<U> members)
            where T : IGroup<IConstraints<U>>, IConstraints<U>
            where U : IMember
        {

        }*/
        private class Group : IGroup<M>
        {
            public IEnumerable<M> Members { get; set; }
        }

        private enum Operations
        {
            Unassign,
            Assign,
            Swap,
            Move
        }

        private const int unassignedMemberPenalty = 1;
        private const int iterations = 1000000;
        private const int maxIterationsWithoutImprovement = 15000;
        private const int minIterations = 10000;

        /// <summary>
        /// Assigns members to a list of groups.
        /// </summary>
        /// <typeparam name="M">The type of member being assigned to groups.</typeparam>
        /// <param name="constraints">The constraints for each group.</param>
        /// <param name="members">The members to be assigned.</param>
        /// <param name="duplicateMembers">The number of times a single member will be assigned.</param>
        /// <returns>The list of groups of members.</returns>
        public static List<IGroup<M>> Assign(List<IConstraints<M>> constraints, IEnumerable<M> members, int duplicateMembers = 1)
        {
            Dictionary<Operations, Func<int, double>> operatorWeightings = new Dictionary<Operations, Func<int, double>>
            {
                { Operations.Assign, (int timestep) => 10.0 },
                { Operations.Unassign, (int timestep) => MathHelpers.ClampedLerp(10.0, 2.0, timestep * 1.0/minIterations) },
                { Operations.Swap, (int timestep) => 5.0 },
                { Operations.Move, (int timestep) => 10.0 }
            };


            // TODO: Use something with named fields instead of a tuple.
            List<(IConstraints<M>, List<M>)> assignments = constraints.Select(constraints => (constraints,new List<M>())).ToList();
            List<M> unassignedMembers = new List<M>();

            List<M> originalMembers = members.ToList();
            List<M> duplicatedMembers = new List<M>();
            for(int i = 0; i < duplicateMembers; i++)
            {
                duplicatedMembers.AddRange(originalMembers);
            }

            // If all constraints have a max limit, trim the inputs such that source objects are roughly even in duplicate quantity.
            if (constraints.All(constraint => constraint.MaxMemberCount.HasValue))
            {
                duplicatedMembers = Select_Ordered(duplicatedMembers, constraints.Sum(constraints => constraints.MaxMemberCount.Value));
            }
            bool optimal = false;

            // Try assigning randomly without violating any constraints!
            int currentAssignmentIndex = -1;
            foreach (M member in duplicatedMembers.OrderBy(_=>StaticRandom.Next()).ToList())
            {
                bool assigned = false;

                // Try assigning to every group
                for (int i = 0; i < assignments.Count; i++)
                {
                    currentAssignmentIndex = (currentAssignmentIndex + 1) % assignments.Count;
                    var assignment = assignments[currentAssignmentIndex];

                    bool validAssignment = assignment.Item1.SatisfiesConstraints(assignment.Item2, member, out int assignmentPenalty);

                    // No violations, successful assignment.
                    if (assignmentPenalty == 0 && validAssignment)
                    {
                        assignment.Item2.Add(member);
                        assigned = true;
                        break;
                    }
                }

                if (!assigned)
                {
                    // Could not find any groups to assign member to :(
                    unassignedMembers.Add(member);
                }
            }
            int constraintViolations = unassignedMemberPenalty * unassignedMembers.Count;
            int bestConstraintViolations = constraintViolations;
            int bestIter = 0;

            if (unassignedMembers.Count <= 0)
            {
                // Package the return list into group list.
                return assignments.Select(grp => (IGroup<M>)(new Group { Members = grp.Item2 })).ToList();
            }

            for (int i = 0; i < iterations; i++)
            {
                if ((i > minIterations) && ((i-bestIter)>maxIterationsWithoutImprovement))
                {
                    break;
                }

                List<Operations> skipOperations = new List<Operations>();
                if (unassignedMembers.Count <= 0)
                {
                    skipOperations.Add(Operations.Assign);
                }

                Operations operation = GetWeightedOperation(i, operatorWeightings, skipOperations);
                int group1Index = StaticRandom.Next(assignments.Count);
                var group1 = assignments[group1Index];
                M member1;

                int group2Index = StaticRandom.Next(assignments.Count);
                var group2 = assignments[group2Index];
                M member2;

                switch (operation)
                {
                    case Operations.Swap:
                        // Can't swap within a group.
                        if (group1Index == group2Index)
                        {
                            continue;
                        }

                        // Can't swap if a group is empty.
                        if (group1.Item2.Count <= 0 || group2.Item2.Count <= 0)
                        {
                            continue;
                        }

                        int item1Index = StaticRandom.Next(group1.Item2.Count);
                        member1 = group1.Item2.ElementAt(item1Index);
                        group1.Item2.RemoveAt(item1Index);

                        int item2Index = StaticRandom.Next(group2.Item2.Count);
                        member2 = group2.Item2.ElementAt(item2Index);
                        group2.Item2.RemoveAt(item2Index);

                        bool unassign1Valid = group1.Item1.SatisfiesConstraints(group1.Item2, member1, out int unassignment1Penalty);
                        bool unassign2Valid = group2.Item1.SatisfiesConstraints(group2.Item2, member2, out int unassignment2Penalty);
                        bool assign1Valid = group2.Item1.SatisfiesConstraints(group2.Item2, member1, out int assignment1Penalty);
                        bool assign2Valid = group1.Item1.SatisfiesConstraints(group1.Item2, member2, out int assignment2Penalty);

                        // Prior state was invalid
                        if (!unassign1Valid || !unassign2Valid)
                        {
                            throw new Exception("Unexpected result, state somehow invalidated constraints.");
                        }
                        
                        // Swap would result in an invalid state, shouldn't be possible currently.
                        if (!assign1Valid || !assign2Valid)
                        {
                            throw new Exception("Unexpected result with current implementation. This may be expected if more constraint validations have been added.");
                        }
                        int swapPenalty = (-1)*unassignment1Penalty + (-1)*unassignment2Penalty + assignment1Penalty + assignment2Penalty;

                        // No violations, successful assignment. Or won coin flip on violations
                        if (swapPenalty <= 0
                            || AcceptPenaltiesCoinFlip(swapPenalty, i))
                        {
                            group1.Item2.Add(member2);
                            group2.Item2.Add(member1);
                            constraintViolations += swapPenalty;
                        }
                        else
                        {
                            group1.Item2.Add(member1);
                            group2.Item2.Add(member2);
                        }

                        break;
                    case Operations.Move:
                        // Can't move within a group.
                        if (group1Index == group2Index)
                        {
                            continue;
                        }

                        // Can't move if starting group is empty.
                        if (group1.Item2.Count <= 0)
                        {
                            continue;
                        }

                        item1Index = StaticRandom.Next(group1.Item2.Count);
                        member1 = group1.Item2.ElementAt(item1Index);
                        group1.Item2.RemoveAt(item1Index);

                        unassign1Valid = group1.Item1.SatisfiesConstraints(group1.Item2, member1, out unassignment1Penalty);
                        assign1Valid = group2.Item1.SatisfiesConstraints(group2.Item2, member1, out assignment1Penalty);

                        // Prior state was invalid
                        if (!unassign1Valid)
                        {
                            throw new Exception("Unexpected result, state somehow invalidated constraints.");
                        }

                        // Move would result in an invalid state.
                        if (!assign1Valid)
                        {
                            group1.Item2.Add(member1);
                            continue;
                        }
                        int movePenalty = (-1) * unassignment1Penalty+ assignment1Penalty ;

                        // No violations, successful assignment. Or won coin flip on violations
                        if (movePenalty <= 0
                            || AcceptPenaltiesCoinFlip(movePenalty, i))
                        {
                            group2.Item2.Add(member1);
                            constraintViolations += movePenalty;
                        }
                        else
                        {
                            group1.Item2.Add(member1);
                        }

                        break;
                    case Operations.Assign:
                        member1 = unassignedMembers.First();
                        unassignedMembers.RemoveAt(0);

                        bool valid = group1.Item1.SatisfiesConstraints(group1.Item2, member1, out int assignmentPenalty);
                        if (!valid)
                        {
                            unassignedMembers.Add(member1);
                            continue;
                        }
                        assignmentPenalty -= unassignedMemberPenalty;
                        // No violations, successful assignment. Or won coin flip on violations
                        if (assignmentPenalty <= 0
                            || AcceptPenaltiesCoinFlip(assignmentPenalty, i))
                        {
                            group1.Item2.Add(member1);
                            constraintViolations += assignmentPenalty;
                        }
                        else
                        {
                            unassignedMembers.Add(member1);
                        }
                        break;
                    case Operations.Unassign:
                        // nothing to Unassign in target group.
                        if (group1.Item2.Count <= 0)
                        {
                            continue;
                        }

                        int itemIndex = StaticRandom.Next(group1.Item2.Count);
                        member1 = group1.Item2.ElementAt(itemIndex);
                        group1.Item2.RemoveAt(itemIndex);

                        bool isValid = group1.Item1.SatisfiesConstraints(group1.Item2, member1, out int unassignmentPenalty);
                        unassignmentPenalty = (-1) * unassignmentPenalty + unassignedMemberPenalty;

                        if (!isValid)
                        {
                            throw new Exception("Should not be possible with current validations");
                        }

                        // No violations, successful assignment. Or won coin flip on violations
                        if (unassignmentPenalty <= 0
                            || AcceptPenaltiesCoinFlip(unassignmentPenalty, i))
                        {
                            unassignedMembers.Add(member1);
                            constraintViolations += unassignmentPenalty;
                        }
                        else
                        {
                            group1.Item2.Add(member1);
                        }
                        break;
                    default:
                        throw new Exception($"Unknown operation: '{operation}'");
                }

                if (constraintViolations < bestConstraintViolations)
                {
                    bestConstraintViolations = constraintViolations;
                    bestIter = i;
                }

                // Can't assign any more items and no invalid constraints
                if ((assignments.All((tup) => tup.Item1.MaxMemberCount.HasValue && (tup.Item2.Count == tup.Item1.MaxMemberCount.Value))) && (unassignedMemberPenalty * unassignedMembers.Count >= constraintViolations))
                {
                    optimal = true;
                    break;
                }

                // No invalid constraints
                if (unassignedMembers.Count <= 0 && constraintViolations <= 0)
                {
                    optimal = true;
                    break;
                }
            }

            if (!optimal)
            {
                throw new NotImplementedException("Suboptimal assignment resolution not yet implemented.");
            }

            // Package the return list into group list.
            return assignments.Select(grp => (IGroup<M>)(new Group { Members = grp.Item2 })).ToList();
        }

        private static bool AcceptPenaltiesCoinFlip(int violations, int step)
        {
            return false;
            // Seems to work fine even with just only ever accepting improvements, but I will keep temp for now.
            return MathF.Min(.95f, MathF.Exp(violations * -1.0f / TempSchedule(step))) >= StaticRandom.NextDouble();
        }

        private static float TempSchedule(int step)
        {
            const float startingTemp = 2.0f;
            const float temperatureDecayRate = startingTemp/minIterations*4;
            return Math.Clamp(startingTemp - temperatureDecayRate * step, 0.0005f, startingTemp);
        }

        private static Operations GetWeightedOperation(int step, Dictionary<Operations, Func<int, double>> weightProviders, List<Operations> skipOperations)
        {
            List<(Operations, double)> weights = weightProviders.Where(kvp=> !skipOperations.Contains(kvp.Key)).Select(kvp => (kvp.Key, kvp.Value(step))).ToList();
            double totalWeight = weights.Sum(kvp => kvp.Item2);
            double selectedWeight = StaticRandom.NextDouble() * totalWeight;

            foreach((Operations operation, double weight) in weights)
            {
                selectedWeight -= weight;
                if (selectedWeight <= 0.0)
                {
                    return operation;
                }
            }

            //Failsafe.
            return Operations.Swap;
        }

    }
}
