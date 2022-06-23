using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.Extensions
{
    public static class LobbyExtensions
    {
        public static void ResetScores(this Lobby lobby, Score.Scope? scope = null)
        {
            foreach (User user in lobby.GetAllUsers())
            {
                user.ScoreHolder.ResetScore(scope);
            }
        }
        public static void ResetScoreBreakdownsOnly(this Lobby lobby, Score.Scope? scope = null)
        {
            foreach (User user in lobby.GetAllUsers())
            {
                user.ScoreHolder.ResetScoreBreakdownsOnly(scope);
            }
        }
        public static List<User> GetUsersSortedByTopScores(this Lobby lobby, int maxUsers, Score.Scope scope = Score.Scope.Total, bool sortDescending = true)
        {
            if (sortDescending)
            {
                return lobby.GetAllUsers().OrderByDescending((user) => user.ScoreHolder.ScoreAggregates[scope]).Take(maxUsers).ToList();
            }
            else
            {
                return lobby.GetAllUsers().OrderBy((user) => user.ScoreHolder.ScoreAggregates[scope]).Take(maxUsers).ToList();
            }
        }

        public static int GetUserScorePosition(this Lobby lobby, User user, Score.Scope scope = Score.Scope.Total)
        {
            var scores = lobby.GetAllUsers().OrderByDescending((user) => user.ScoreHolder.ScoreAggregates[scope]).ToList();
            return scores.IndexOf(user) + 1;
        }

    }
}
