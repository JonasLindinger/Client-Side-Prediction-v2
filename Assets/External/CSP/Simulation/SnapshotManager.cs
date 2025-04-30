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
            
            // 2. Save the current State
            TakeSnapshot(tick);
            
            #if Client
            // 2.5 Collect Client Data and Input
            if (PlayerInputNetworkBehaviour.LocalPlayer != null)
            {
                IData data = PlayerInputNetworkBehaviour.LocalPlayer.GetPlayerData();
            
                // Collect input
                ClientInputState clientInputState = GetInputState(tick, data);
            }
            #endif
            
            // 3. Update all Players (Server moves everyone, Client predicts his own player)
            PlayerInputNetworkBehaviour.UpdatePlayersWithAuthority(tick, false);
        }

        public static void RecalculatePhysicsTick(uint tick)
        {
            // 1. Simulate Physics
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);
            
            // 2. Save the current State
            TakeSnapshot(tick);
            
            // 3. Update all Players (Server moves everyone, Client predicts his own player)
            PlayerInputNetworkBehaviour.UpdatePlayersWithAuthority(tick, true);
        }
        
        #if Client
        public static ClientInputState GetInputState(uint tick, IData data)
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();
            
            // If we already collected input for this tick, we reuse it.
            if (_inputStates[tick % _inputStates.Length] != null)
                if (_inputStates[tick % _inputStates.Length].Tick == tick)
                    return _inputStates[tick % _inputStates.Length];
            
            ClientInputState clientInputState = _inputCollector.GetClientInputState(tick);
            clientInputState.Data = data;
            _inputStates[tick % _inputStates.Length] = clientInputState;

            return clientInputState;
        }
        
        /// <summary>
        /// Apply's the state on the object with the corresponding network Id
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <returns>Return's if the prediction was wrong</returns>
        public static void ApplyState(ulong networkId, IState state)
        {
            if (!_networkedObjects.ContainsKey(networkId)) return;
            NetworkedObject networkedObject = _networkedObjects[networkId];

            // Check for null reference
            if (networkedObject == null)
            {
                Debug.LogWarning("Something went wrong!");
                return;
            }
            
            networkedObject.ApplyState(state);
        }
        #elif Server
        public static GameState GetLatestGameState()
        {
            return _gameStates[(int)_latestGameStateTick % _gameStates.Length];
        }
        #endif
        
        public static GameState GetGameState(uint tick)
        {
            return _gameStates[(int)tick % _gameStates.Length];
        }
        
        public static void SaveGameState(GameState gameState)
        {
            _gameStates[(int) gameState.Tick % _gameStates.Length] = gameState;
        }
    }
}