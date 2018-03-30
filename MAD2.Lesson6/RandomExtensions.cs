using System;

namespace MAD2.Lesson6
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random r) => r.NextDouble() >= 0.5d;
    }
}
