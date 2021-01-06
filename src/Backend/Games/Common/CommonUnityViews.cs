using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common
{
    public class CommonUnityViews
    {
        public static UnityView GenerateInvalidVersionView()
        {
            return new UnityView(lobby: null)
            {
                ScreenId = TVScreenId.ShowDrawings,//TODO
                Title = new UnityField<string>()
                {
                    Value = "Your Unity Client Needs To Be Updated"
                },
                Instructions = new UnityField<string>()
                {
                    Value = "Go to scrawlbrawl.tv/lobby to update it"
                },
            };
        }
        public static Legacy_UnityView GenerateInvalidVersionLegacyView()
        {
            return new Legacy_UnityView(lobby: null)
            {
                ScreenId = new StaticAccessor<TVScreenId>()
                {
                    Value = TVScreenId.ShowDrawings
                },
                Title = new StaticAccessor<string>()
                {
                    Value = "Your Unity Client Needs To Be Updated"
                },
                Instructions = new StaticAccessor<string>()
                {
                    Value = "Go to scrawlbrawl.tv/lobby to update it"
                },
            };
        }
    }
}
