using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PostSharp.Patterns.Model;
using System.ComponentModel;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.APIs.DataModels.UnityObjects
{
    [NotifyPropertyChanged]
    public class UnityView : OptionsInterface<UnityViewOptions>
    {
        // TODO: standardize on one json framework.
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Lobby Lobby { get; }

        public UnityField<IReadOnlyList<UnityObject>> UnityObjects { get; set; } = null;
        public TVScreenId? ScreenId { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public IReadOnlyList<UnityUser> Users { get; set; } = new List<UnityUser>().AsReadOnly();
        public UnityField<string> Title { get; set; }
        public UnityField<string> Instructions { get; set; }
        public DateTime? ServerTime { get { return DateTime.UtcNow; } }
        public DateTime? StateEndTime { get; set; }
        public bool IsRevealing { get; set; } = false;
        public Dictionary<UnityViewOptions, object> Options { get; set; } = new Dictionary<UnityViewOptions, object>();

        public UnityView(Lobby lobby)
        {
            this.Lobby = lobby;
            if (lobby != null)
            {
                this.Users = Lobby.GetAllUsers().Select(user => new UnityUser(user)).ToList().AsReadOnly();
            }
        }

        public UnityView(Legacy_UnityView legacy)
        {
            if (legacy._UnityImages?.Count > 0 && legacy._UnityImages?[0]?._Base64Pngs?.Count > 0)
            {
                this.UnityObjects = new UnityField<IReadOnlyList<UnityObject>> 
                {
                    Value = legacy._UnityImages.Select(image => new UnityImage(image)).ToList().AsReadOnly()
                };
            }
            else if (legacy._UnityImages?.Count > 0)
            {
                this.UnityObjects = new UnityField<IReadOnlyList<UnityObject>>
                {
                    Value = legacy._UnityImages.Select(image => new UnityText(image)).ToList().AsReadOnly()
                };
            }
            this.Id = legacy._Id ?? Guid.NewGuid();
            this.ScreenId = legacy._ScreenId;
            this.Users = legacy._Users?.Select(user => new UnityUser(user)).ToList().AsReadOnly();
            this.Title = new UnityField<string> { Value = legacy._Title };
            this.Instructions = new UnityField<string> { Value = legacy._Instructions };
            this.StateEndTime = legacy._StateEndTime;
            this.IsRevealing = legacy._VoteRevealUsers?.Count > 0;
            this.Options = new Dictionary<UnityViewOptions, object>()
            {
                {UnityViewOptions.PrimaryAxis, legacy._Options?._PrimaryAxis },
                {UnityViewOptions.PrimaryAxisMaxCount, legacy._Options?._PrimaryAxisMaxCount },
                {UnityViewOptions.BlurAnimate, new UnityField<float?>
                {
                    StartTime = legacy._Options?._BlurAnimate?._StartTime,
                    EndTime = legacy._Options?._BlurAnimate?._EndTime,
                    StartValue = legacy._Options?._BlurAnimate?._StartValue,
                    EndValue = legacy._Options?._BlurAnimate?._EndValue,
                }}
            };
        }
    }
}
