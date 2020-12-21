using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
    public class TimerHolder
    {
        public DateTime? ServerTime { get; set; }
        public DateTime? StateEndTime { get; set; }
    }
    public interface Timer_HandlerInterface : HandlerInterface<TimerHolder>
    {
        void UpdateValue(TimerHolder value);
    }
}
