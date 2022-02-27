using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Concurrent;

namespace Backend.Games.KevinsGames.LateToArtClass.DataModels
{
    public class ArtClass : Constraints<User>
    {
        /// <summary>
        /// The person who came up with the assignment
        /// </summary>
        public User Teacher { get; set; }
        public string ArtAssignment { get; set; }
        public ConcurrentDictionary<User, UserDrawing> UsersToDrawings { get; set; } = new ConcurrentDictionary<User, UserDrawing>();

        /// <summary>
        /// The student doing the copying
        /// </summary>
        public User LateStudent { get; set; }

        /// <summary>
        /// This will be determined at runtime / will be biased towards those who finish faster (to avoid delays)
        /// </summary>
        public User CopiedFrom { get; set; }

        public override bool? AllowDuplicateIds { get; set; } = false;


        #region stupid locking shenanigans
        /// <summary>
        /// Sigh... Use this lock to make sure the HaveArtToCopyOffOfCallback is called ONLY ONCE!!!. BUT DO NOT CALL IT FROM WITHIN A LOCK!!!! -_-
        /// </summary>
        public object ArtClassLock { get; set; } = new object();

        /// <summary>
        /// Sigh... Use this to track who is responsible for calling the HaveArtToCopyOffOfCallback. DO NOT CALL IT FROM WITHIN A LOCK!!!! -_-
        /// </summary>
        public User ArtClassLockWinner { get; set; }

        /// <summary>
        /// This is used to coordinate the hopefully rare occurance where the late student arrives and there is nobody ready to copy off of.
        /// </summary>
        public Action HaveArtToCopyOffOfCallback { get; set; } = () =>
        {
            // By default do nothing
        };
        #endregion
    }
}
