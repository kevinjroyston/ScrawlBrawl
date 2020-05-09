﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.Extensions
{
    public static class StringExtentions
    {
        public static bool FuzzyEquals(this string a, string b)
        {
            if(a==null || b ==null)
            {
                return a == b;
            }
            a = a.Trim();
            b = b.Trim();
            return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
