using System;
using UnityEngine;

namespace CSP.Simulation
{
    public class TickSystem
    {
        public event Action<uint> OnTick = delegate { };
        
        public uint CurrentTick { get; protected set; }
        public uint TickRate { get; protected set; }
        public float TimeBetweenTicks { get; protected set; }

        private float _time;
        private int _ticksToSkip;
        
        /// <summary>
        /// Starts the Tick System
        /// </summary>
        /// <param name="tickRate">Amount of ticks per second</param>
        /// <param name="startingTickOffset"></param>
        public TickSystem(uint tickRate, uint startingTickOffset = 0)
        {
            // Setting the TickRate and calculating the Time Between Ticks.
            TickRate = tickRate;
            TimeBetweenTicks = 1f / tickRate;
            
            // Setting the starting Tick (default 0)
            CurrentTick = startingTickOffset;
        }
        
        public void Update(float deltaTime)
        {
            // Increase the Time between last Tick and now
            _time += deltaTime;

            // If the Time between last Tick and now is greater than the Time Between Ticks,
            // we should run a tick and decrease the time between the last Tick by the Time Between Ticks.
            if (!(_time >= TimeBetweenTicks)) return;
            _time -= TimeBetweenTicks;

            // Check if we should skip ticks
            if (_ticksToSkip > 0)
            {
                _ticksToSkip--;
                return;
            }
                
            // Increase tick and run the tick
            CurrentTick++;
            OnTick?.Invoke(CurrentTick);
        }
        
        /// <summary>
        /// Set the Amount of ticks we should skip
        /// </summary>
        /// <param name="amount"></param>
        public void SkipTick(int amount)
        {
            _ticksToSkip = amount;
        }

        /// <summary>
        /// When running this, we instantly run the next few ticks and set the ticks to skip to zero.
        /// </summary>
        /// <param name="amount"></param>
        public void CalculateExtraTicks(int amount)
        {
            _ticksToSkip = 0;

            for (int i = 0; i < amount; i++)
            {
                CurrentTick++;
                OnTick?.Invoke(CurrentTick);
            }
        }

        /// <summary>
        /// Sets the tick to the given tick
        /// </summary>
        /// <param name="tick"></param>
        public void SetTick(uint tick) => CurrentTick = tick;
    }
}