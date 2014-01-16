﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlight
{
    static class Common
    {
        public static void Error(string message, string file, int line)
        {
            Console.WriteLine("Error " + file + "(" + line + "): " + message);
        }

        public static bool Match<V, T>(this V value, IEnumerable<T> list) where V : IEquatable<T>
        {
            foreach (T v in list)
            {
                if (value.Equals(v))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Match<V>(this V value, V stert, V end) where V : IComparable<V>
        {
            return value.CompareTo(stert) >= 0 && value.CompareTo(end) <= 0;
        }
    }
}
