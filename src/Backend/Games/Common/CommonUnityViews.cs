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
        public static UnityView GenerateInvalidVersionView(double serverVersion, double? unityVersion = null) //atm version numbers unused
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
                    Value = "Go to https://www.scrawlbrawl.tv/lobby to update it"
                },
            };
        }
    }
}
