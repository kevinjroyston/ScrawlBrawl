using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.QueryAndReveal
{
    public interface IQueryable<T>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<QueryInfo<T>> UserAnswers { get; set; }

        public abstract UnityObject QueryUnityObjectGenerator(int numericId);
        public abstract UnityObject RevealUnityObjectGenerator(int numericId);
    }
}
