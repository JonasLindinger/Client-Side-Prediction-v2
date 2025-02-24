using System;
using System.Collections.Generic;

namespace CSP.Simulation.State
{
    public class StateFactory
    {
        private static readonly Dictionary<int, Func<IState>> Registry = new Dictionary<int, Func<IState>>();

        public static void Register(int type, Func<IState> creator)
        {
            Registry[type] = creator;
        }

        public static IState Create(int type)
        {
            if (Registry.TryGetValue(type, out var creator))
                return creator();
            throw new ArgumentException($"No IState registered for type {type}");
        }
    }
}