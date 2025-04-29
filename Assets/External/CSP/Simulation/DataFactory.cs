using System;
using System.Collections.Generic;

namespace CSP.Simulation
{
    public class DataFactory
    {
        private static readonly Dictionary<int, Func<IData>> Registry = new Dictionary<int, Func<IData>>();

        public static void Register(int type, Func<IData> creator)
        {
            Registry[type] = creator;
        }

        public static IData Create(int type)
        {
            if (Registry.TryGetValue(type, out var creator))
                return creator();
            return null;
        }
    }
}