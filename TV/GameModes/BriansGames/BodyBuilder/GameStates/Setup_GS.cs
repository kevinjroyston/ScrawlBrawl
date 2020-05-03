using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels.Setup_Person;
using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
{
    public class Setup_GS : GameState
    {

        public Dictionary<DrawingType, int> widths = new Dictionary<DrawingType, int>()
        {
            {DrawingType.Head, 240},
            {DrawingType.Body, 240},
            {DrawingType.Legs, 240},
        };
        public Dictionary<DrawingType, int> heights = new Dictionary<DrawingType, int>()
        {
            {DrawingType.Head, 120},
            {DrawingType.Body, 240},
            {DrawingType.Legs, 240},
        };
        
        public Dictionary<DrawingType, string> backgrounds = new Dictionary<DrawingType, string>()
        {
            #region HeadBG
             {DrawingType.Head, "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPAAAAB4CAYAAADMtn8nAAACHUlEQVR4nO3ZwWoTYRSA0ZumIkU0BTe+/4O5FMFNI0IraTpltItmFzfGT85Zziz+y9x8DEk2y7J8npnbAWru1oAfZuat1UHOzzXg48xc2R3kPK3h7u0NkvbevBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ9i15f19x8eZx/uZ4+H06O2bmeubme0/upXq3P8zj/wC1gi+f52538/M8nL+ZuZmN/Ph08z2vbk5j4AvYH2DPexnfnw7DWEzM+8+mpvzCfgC1s/+sswsT6ch/Lpmbv6AgC9gs5m52v7+7vg6hPXaes/cnMuv0BAmYAgTMIQJGMIEDGFrwDsLhKTdGvDB7iDpsP4P/GVmbu0PYmbungEVxFF+WFuqEgAAAABJRU5ErkJggg=="},
            #endregion

            #region BodyBG
            {DrawingType.Body, "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPAAAADwCAYAAAA+VemSAAAEKUlEQVR4nO3a22ocRxRA0dMamUQYWwl+8f9/XDAYksgxyEGXDi0PiSYgUF4y2pq1HksPOkzV1qWrl3Vdf52Zn4f/zZ9/zHz5NHN9NTPr/rsuMxeXM+8/zvzw7mXuRXXuV+y381P/BI7h7mbm29XM18+HISwz8/aDuXk+AR/BdvbXdWa9PwzhYc3c/AcCPoJlmTnbzezeHIawrW1fMzfPdeaTgi4BQ5iAIUzAECZgCPMU+gi2a5f7u+/3qo+f5m5r6wu+j6nO/ZoJ+AiW/ZXMcvavFyL2L0WYm+cS8BFs96g/Xu4beBTCtvZwx2punsm70Edwdztze73/U/SRLYLzi5ndC/2xWp37FfMu9DFsB30XfPG/Ovdr5ik0hAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ9j5KW/e3e3M7fXM3c3h+u7NzPnFzO6kP52Xwz497aSP6HYovnyaub6amXW/uMxcXM68/zize3fkAXlgn5522r+Bb2a+Xc18/Xx4MJaZefvhyMPxN/v0tJMOeDsL6zqz3h8ejIe1I8/GP+zT00464GWZOdt9/1/q8cHY1rav8TLYp6d5Cg1hAoYwAUOYgCFMwBC2BXxpAyHpcgv4xt5B0s12D/zLzPxk/yBm5ve/AINppA0rSIxSAAAAAElFTkSuQmCC"},
            #endregion

            #region LegsBG
            {DrawingType.Legs, "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAPAAAADwCAYAAAA+VemSAAADhElEQVR4nO3aTYvTUBSA4ZNORUT8ws38/x8ngqCOCiqljVQGYRZdqn0nz7PM6ibnvuTSdFnX9Tgzu9mgn19nvryf+X43M+v9/S8zz17NvLydefpii0/l+pjTRaf9VuM9Ox5mftzNfPvwcGMsM/P87X9eHH+Y00W7/ZUu7J8474V1nVlPDzfG72uP/N5LzOmy/f0jWa51gX/TsszsbmZunjzcGOdryyafyHUyp4vWzR6f4TEQMIQJGMIEDGEChrDdVn+Bhkdg8QaGsPN34NNWj9K+LzaY00Wn83+hP87MmytdIHDZJ0doCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQJmAIEzCECRjCBAxhAoYwAUOYgCFMwBAmYAgTMIQJGMIEDGEChjABQ5iAIUzAECZgCBMwhAkYwgQMYQKGMAFDmIAhTMAQtqzrehQyJJ3O4R7MDpIO+5l5NzOvzQ9iZj7/AsBRSqqzpLyAAAAAAElFTkSuQmCC"}
            #endregion
        };

        private UserState GetPeoplePrompts_State(Connector outlet = null)
        {
            return new SimplePromptUserState((User user) => new UserPrompt()
            {
                Title = "Game setup",
                Description = "In the boxes below, enter drawing prompts which will be given to different players.",
                RefreshTimeInMs = 1000,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt="Person#1",
                        ShortAnswer=true
                    },
                    new SubPrompt
                    {
                        Prompt="Person#2",
                        ShortAnswer=true
                    },
                },
                SubmitButton = true
            },
            outlet,
            formSubmitListener: (User user, UserFormSubmission input) =>
            {
                if(input.SubForms[0].ShortAnswer.Equals(input.SubForms[1].ShortAnswer, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (false, "Please enter 2 distinct people");
                }
                this.PeopleList.Add(new Setup_Person
                {
                    Owner = user,
                    Prompt = input.SubForms[0].ShortAnswer
                });
                this.PeopleList.Add(new Setup_Person
                {
                    Owner = user,
                    Prompt = input.SubForms[1].ShortAnswer
                });
                return (true, String.Empty);
            });
        }

        private List<Setup_Person> PeopleList { get; set; }

        private Random rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <param name="outlet">The state to link the end of the chain to.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<UserState> GetDrawingsUserStateChain(User user, Connector outlet)
        {
            List<UserState> stateChain = new List<UserState>();
            List<Setup_Person> people = this.PeopleList.OrderBy(_ => rand.Next()).ToList();
            foreach (Setup_Person person in people)
            {
                if (!person.UserSubmittedDrawingsByUser.ContainsKey(user))
                {
                    continue;
                }

                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    Title = "Time to draw!",
                    Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"You are drawing the \"{person.UserSubmittedDrawingsByUser[user].Type}\" of \"{person.Prompt}\""),
                            Drawing = new DrawingPromptMetadata()
                            {
                                WidthInPx = widths[person.UserSubmittedDrawingsByUser[user].Type],
                                HeightInPx = heights[person.UserSubmittedDrawingsByUser[user].Type],
                                CanvasBackground = backgrounds[person.UserSubmittedDrawingsByUser[user].Type],
                            },//{} add in width, height, background
                        },
                    },
                    SubmitButton = true
                },
                formSubmitListener: (User user, UserFormSubmission input) =>
                {
                    person.UserSubmittedDrawingsByUser[user].Drawing = input.SubForms[0].Drawing;
                    return (true, String.Empty);
                }));
            }

            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }

        public Setup_GS(Lobby lobby, List<Setup_Person> peopleList, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, Connector outlet = null) : base(lobby, outlet)
        {
            this.PeopleList = peopleList;

            UserStateTransition waitForAllDrawings = new WaitForAllPlayers(lobby: lobby, outlet: this.Outlet);
            UserStateTransition waitForAllPrompts = new WaitForAllPlayers(lobby: lobby, outlet:(User user, UserStateResult result, UserFormSubmission input) =>
            {
                // This call doesn't actually happen until after all prompts are submitted
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });
            // Just before users call the line above, call AssignPrompts
            waitForAllPrompts.AddStateEndingListener(() => this.AssignPrompts());

            this.Entrance = GetPeoplePrompts_State(waitForAllPrompts.Inlet);

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete all the prompts on your devices." },
            };
        }

        /// <summary>
        /// This method of assigning prompts is not the most efficient but the next best algorithm I could find for getting a 
        /// good solution was even more inefficient. All the efficient algorithms gave suboptimal solutions.
        /// </summary>
        private void AssignPrompts()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Setup_Person> randomlyOrderedPeople = this.PeopleList.OrderBy(_ => rand.Next()).ToList();
            IReadOnlyList<User> users = this.Lobby.GetActiveUsers();
            for (int i = 0; i < randomlyOrderedPeople.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    UserDrawing temp = new UserDrawing
                    {
                        Type = (Setup_Person.DrawingType)j,
                        Owner = users[(i + j + 1) % users.Count],
                        Id = randomlyOrderedPeople[i].Id
                    };

                    randomlyOrderedPeople[i].UserSubmittedDrawingsByUser.Add(
                        temp.Owner, temp);
                    randomlyOrderedPeople[i].UserSubmittedDrawingsByDrawingType.Add(
                        temp.Type, temp);
                }
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
