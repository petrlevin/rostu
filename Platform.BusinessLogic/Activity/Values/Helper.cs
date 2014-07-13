using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Values
{
    class Helper
    {
        static public bool IsScalar(Type t)
        {
            return t == typeof(bool) ||
                   t == typeof(byte) ||
                   t == typeof(char) ||
                   t == typeof(decimal) ||
                   t == typeof(double) ||
                   t == typeof(float) ||
                   t == typeof(int) ||
                   t == typeof(long) ||
                   t == typeof(sbyte) ||
                   t == typeof(short) ||
                   t == typeof(string) ||
                   t == typeof(uint) ||
                   t == typeof(ulong) ||
                   t == typeof(ushort) ||
                   t == typeof(DateTime) ||
                   t == typeof(TimeSpan) ||
                   t == typeof(bool?) ||
                   t == typeof(byte?) ||
                   t == typeof(char?) ||
                   t == typeof(decimal?) ||
                   t == typeof(double?) ||
                   t == typeof(float?) ||
                   t == typeof(int?) ||
                   t == typeof(long?) ||
                   t == typeof(sbyte?) ||
                   t == typeof(short?) ||
                   t == typeof(uint?) ||
                   t == typeof(ulong?) ||
                   t == typeof(ushort?) ||
                   t == typeof(DateTime?);


        }

    }
}
