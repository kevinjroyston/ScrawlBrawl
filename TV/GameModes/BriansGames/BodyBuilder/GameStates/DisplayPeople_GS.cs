using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
{
    public class DisplayPeople_GS : GameState
    {
        public DisplayPeople_GS(Lobby lobby, RoundTracker roundTracker, BodyBuilderConstants.DisplayTypes displayType, List<Setup_Person> peopleList, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            var unityImages = new List<UnityImage>();
            string title = null;
            if (displayType == BodyBuilderConstants.DisplayTypes.PlayerHands)
            {
                title = "Here's What Everyone Made";
                foreach (User user in roundTracker.OrderedUsers)
                {          
                    unityImages.Add(new UnityImage
                    {
                        Base64Pngs = new StaticAccessor<IReadOnlyList<string>> {Value = roundTracker.AssignedPeople[user].GetOrderedDrawings() },
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                        SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                        SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                        Header = new StaticAccessor<string> { Value = user.DisplayName }
                    });       
                }
            }
            else if (displayType == BodyBuilderConstants.DisplayTypes.OriginalPeople)
            {
                title = "And Here's The Original People";
                foreach (Setup_Person person in peopleList)
                {
                    unityImages.Add(new UnityImage
                    {
                        Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string> 
                        {
                            person.UserSubmittedDrawingsByDrawingType[Setup_Person.DrawingType.Head].Drawing,
                            person.UserSubmittedDrawingsByDrawingType[Setup_Person.DrawingType.Body].Drawing,
                            person.UserSubmittedDrawingsByDrawingType[Setup_Person.DrawingType.Legs].Drawing
                        }},
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                        SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                        SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                        Header = new StaticAccessor<string> { Value = person.Prompt }
                    });
                }
            }
            else
            {
                throw new Exception("Something Went Wrong While Trying To Display People");
            }

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = title },
            };
        }
    }
}
