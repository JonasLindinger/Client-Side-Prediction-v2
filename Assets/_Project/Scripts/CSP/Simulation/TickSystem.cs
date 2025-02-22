using UnityEngine;

namespace _Project.Scripts.CSP.Simulation
{
    public abstract class TickSystem : MonoBehaviour
    {
        public uint CurrentTick { get; protected set; }
        public int TickRate { get; protected set; }
        public float TimeBetweenTicks { get; protected set; }

        private float _time;
        private int _ticksToSkip;
        private bool _started;
        
        /// <summary>
        /// Starts the Tick System
        /// </summary>
        /// <param name="tickRate">Amount of ticks per second</param>
        /// <param name="startingTickOffset"></param>
        public void Run(uint tickRate, uint startingTickOffset = 0)
        {
            Debug.Log("Starting tick system");
            // Setting the TickRate and calculating the Time Between Ticks.
            TickRate = (int) tickRate;
            TimeBetweenTicks = 1f / tickRate;
            
            // Setting the starting Tick (default 0)
            CurrentTick = startingTickOffset;
            
            _started = true;
        }
        
        public abstract void OnTick(uint tick);

        private protected void Update()
        {
            if (!_started) return;
            
            // Increase the Time between last Tick and now
            _time += Time.deltaTime;

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
            OnTick(CurrentTick);
        }
        
        /// <summary>
        /// Set the Amount of ticks we should skip
        /// </summary>
        /// <param name="amount"></param>
        public void SkipTick(int amount)
        {
            // Todo: Check if the amount is too high, so we set the CurrentTick or run slower over a longer period
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
                OnTick(CurrentTick);
            }
        }
    }
}