using System.Collections.Generic;
using CSP.Data;
using CSP.Simulation.State;
using LindoNoxStudio.Network.Simulation;
using UnityEngine;

namespace CSP.Simulation
{
    public static class SnapshotManager
    {
        #if Client
        // Local Client Input Saving
        private static ClientInputState[] _inputStates;
        #endif
        
        private static Dictionary<ulong, NetworkedObject> _networkedObjects  = new Dictionary<ulong, NetworkedObject>(); // NetworkObjectId | PredictionObject

        private static GameState[] _gameStates;

        #if Server
        private static uint _latestGameStateTick;
        #endif
        
        public static void Initialize()
        {
            _gameStates = new GameState[NetworkRunner.NetworkSettings.stateBufferSize];
            #if Client
            // Local Client Input Saving
            _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            #endif
        }

        #region Register

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

        #endregion
        
        #region Core State Methods
        
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

        public static GameState GetGameState(uint tick)
        {
            return _gameStates[(int)tick % _gameStates.Length];
        }

        public static void SaveGameState(GameState gameState)
        {
            _gameStates[(int) gameState.Tick % _gameStates.Length] = gameState;
        }
        
        #endregion
        
        #region Side State Methods
        #if Client

        /// <summary>
        /// Applys the state on the object with the corresponding network Id
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <returns>Return's if the prediction was wrong</returns>
        public static void ApplyState(ulong networkId, IState state)
        {
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
        /// <summary>
        /// Returns the newest GameState
        /// </summary>
        public static GameState GetLatestGameState()
        {
            return _gameStates[(int)_latestGameStateTick % _gameStates.Length];
        }
        #endif
        #endregion
        
        #region ClientInputSaving
        
        #if Client
        
        public static void RegisterInputState(ClientInputState input)
        {
            _inputStates[input.Tick % _inputStates.Length] = input;
        }

        public static ClientInputState GetInputState(uint tick)
        {
            if (_inputStates[tick % _inputStates.Length].Tick != tick)
            {
                Debug.LogError("USING WRONG INPUT STATE!!!!!!!!!!!");
            }
            return _inputStates[tick % _inputStates.Length];
        }
        #endif
        
        #endregion
    }
}