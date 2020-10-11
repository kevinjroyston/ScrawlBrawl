using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class UnityViewAnimationOptions<T>
{
    public DateTime? _StartTime { get; set; }
    public DateTime? _EndTime { get; set; }
    public T _StartValue { get; set; }
    public T _EndValue { get; set; }
}

