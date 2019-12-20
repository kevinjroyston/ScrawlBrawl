﻿using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

using static System.FormattableString;

namespace RoystonGame.TV.DataModels
{
    /// <summary>
    /// A state has an inlet and outlet.
    /// </summary>
    public abstract class State : StateOutlet, StateInlet
    {
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }

    public interface StateInlet
    {
        /// <summary>
        /// The inlet to the transition.
        /// </summary>
        /// <param name="user">The user to move into the transition.</param>
        /// <param name="stateResult">The state result of the last node (this transition doesnt care).</param>
        /// <param name="formSubmission">The user input of the last node (this transition doesnt care).</param>
        public abstract void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission);
    }

    public abstract class StateOutlet
    {
        /// <summary>
        /// Useful for tracking when an outlet is set.
        /// </summary>
        public Guid StateId { get; } = Guid.NewGuid();

        /// <summary>
        /// A bool per user indicating this user has already called CompletedActionCallback
        /// </summary>
        private Dictionary<User, bool> HaveAlreadyCalledOutlet { get; set; } = new Dictionary<User, bool>();

        /// <summary>
        /// A set of overrides per user for outlet.
        /// </summary>
        private Dictionary<User, Connector> UserOutletOverrides { get; set; } = new Dictionary<User, Connector>();

        private Action StateEndingCallback { get; set; }
        private Connector InternalOutlet { get; set; }
        bool FirstUser = true;
        // This needs to be wrapped so that "Outlet" can be passed as an action prior to InternalOutlet being defined
        protected void Outlet(User user, UserStateResult result, UserFormSubmission input)
        {
            if (this.FirstUser)
            {
                this.FirstUser = false;
                this.StateEndingCallback?.Invoke();
            }

            if (this.UserOutletOverrides.ContainsKey(user))
            {
                this.UserOutletOverrides[user](user, result, input);
            }
            else
            {
                this.InternalOutlet(user, result, input);
            }
        }

        /// <summary>
        /// Sets up <paramref name="stateEnding"/> to be called just prior to the first user leaving this state.
        /// </summary>
        /// <param name="stateEnding">The action to call immediately prior to leaving this state.</param>
        public void SetStateEndingCallback(Action stateEnding)
        {
            this.StateEndingCallback = stateEnding;
        }

        /// <summary>
        /// Sets the state completed callback. This should be called before state is entered!
        /// </summary>
        /// <param name="outlet">The callback to use.</param>
        public void SetOutlet(Connector outlet, List<User> specificUsers = null)
        {
            if (outlet == null)
            {
                throw new ArgumentNullException("Outlet cannot be null");
            }

            Debug.WriteLine(Invariant($"|||STATE SETUP|||{this.StateId}|{this.GetType()}|{(specificUsers == null ? "all users" : string.Join(", ", specificUsers.Select(user=>user.DisplayName)))}"));

            Action<User,UserStateResult,UserFormSubmission> internalOutlet = (User user, UserStateResult result, UserFormSubmission input) =>
            {
                // An outlet should only ever be called once per user. Ignore extra calls (most likely a timeout thread).
                if (this.HaveAlreadyCalledOutlet.ContainsKey(user) && this.HaveAlreadyCalledOutlet[user] == true)
                {
                    return;
                }

                this.HaveAlreadyCalledOutlet[user] = true;

                if(outlet == null)
                {
                    throw new Exception(Invariant($"Outlet not defined for User '{user.DisplayName}' who is currently in state type '{user.UserState.GetType()}'"));
                }
                outlet(user, result, input);
            };

            if (specificUsers == null)
            {
                this.InternalOutlet = internalOutlet;
            }
            else
            {
                foreach(User user in specificUsers)
                {
                    this.UserOutletOverrides[user] = internalOutlet;
                }
            }
        }
    }
}