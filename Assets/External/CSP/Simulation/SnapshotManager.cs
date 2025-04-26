using System.Collections.Generic;
using CSP.Data;
using CSP.Input;
using CSP.Object;
using CSP.Player;
using UnityEngine;

namespace CSP.Simulation
{
    public static class SnapshotManager
    {
        public static uint TickRate => PhysicsTickSystem.TickRate;
        public static uint CurrentTick => PhysicsTickSystem.CurrentTick;
            
        public static TickSystem PhysicsTickSystem;

        private static Dictionary<ulong, NetworkedObject> _networkedObjects  = new Dictionary<ulong, NetworkedObject>(); // NetworkObjectId | PredictionObject
        
        private static GameState[] _gameStates;
        
        #if Client
        // Local Client Input Saving
        private static InputCollector _inputCollector;
        private static ClientInputState[] _inputStates;
        #elif Server
        private static uint _latestGameStateTick;
        #endif
        
        public static void KeepTrack(uint tickRate, uint startingTickOffset = 0)
        {
            Physics.simulationMode = SimulationMode.Script;
            
            _gameStates = new GameState[NetworkRunner.NetworkSettings.stateBufferSize];
            
            #if Client
            _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            #endif
            
            PhysicsTickSystem = new TickSystem(tickRate, startingTickOffset);
            
            PhysicsTickSystem.OnTick += OnTick;
        }

        public static void StopTracking()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
            
            PhysicsTickSystem.OnTick -= OnTick;
            
            PhysicsTickSystem = null;
        }
        
        /// <summary>
        /// Every Networked Object registered will be included in the GameState.
        /// It wouldn't make to Unregister Objects so we don't have a method for that!
        /// </summary>
        /// <param name="id">NetworkId</param>
        /// <param name="networkedObject"></param>
        public static void RegisterNetworkedObject(ulong id, NetworkedObject networkedObject)
        {
            _networkedObjects.Add(id, networkedObject);
        }
        
        /// <summary>
        /// Saves the current GameState.
        /// </summary>
        /// <param name="tick">Current Tick</param>
        public static void TakeSnapshot(uint tick)
        {
            GameState currentGameState = GetCurrentState(tick);

            _gameStates[(int)tick % _gameStates.Length] = currentGameState;
            #if Server
            _latestGameStateTick = tick;
            #endif
        }
        
        /// <summary>
        /// Returns the current GameState.
        /// </summary>
        /// <param name="tick">Current Tick</param>
        public static GameState GetCurrentState(uint tick)
        {
            GameState currentGameState = new GameState();
            currentGameState.Tick = tick;
            
            foreach (var kvp in _networkedObjects)
            {
                ulong networkId = kvp.Key;
                NetworkedObject networkedObject = kvp.Value;

                IState state = networkedObject.GetCurrentState();
                
                currentGameState.States.Add(networkId, state);
            }
            
            return currentGameState;
        }

        public static void Update(float deltaTime) => PhysicsTickSystem?.Update(deltaTime);
        
        private static void OnTick(uint tick)
        {
            // 1. Simulate Physics
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);
            
            #if Client
            // 1.5 Collect Client Input
            ClientInputState clientInputState = GetInputState(tick);
            #endif
            
            // 2. Update all Players (Server moves everyone, Client predicts his own player)
            PlayerInputNetworkBehaviour.UpdatePlayersWithAuthority(tick, false);
        }
        
        #if Client
        public static ClientInputState GetInputState(uint tick)
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();
            
            // If we already collected input for this tick, we reuse it.
            if (_inputStates[tick % _inputStates.Length] != null)
                if (_inputStates[tick % _inputStates.Length].Tick == tick)
                    return _inputStates[tick % _inputStates.Length];
            
            ClientInputState clientInputState = _inputCollector.GetClientInputState(tick);
            _inputStates[tick % _inputStates.Length] = clientInputState;

            return clientInputState;
        }
        #endif
    }
}