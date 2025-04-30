using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using CSP.Data;
using CSP.Input;
using CSP.Object;
using CSP.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CSP.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public abstract class PlayerInputNetworkBehaviour : NetworkedObject
    {
        #if Client
        public static PlayerInputNetworkBehaviour LocalPlayer;
        #endif
        
        private static List<PlayerInputNetworkBehaviour> _playersWithAuthority = new List<PlayerInputNetworkBehaviour>();

        private NetworkClient _networkClient;
        private PlayerInput _playerInput;
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner || IsServer)
                _playersWithAuthority.Add(this);

            #if Client
            _networkClient = NetworkClient.LocalClient;
            _playerInput = GetComponent<PlayerInput>();
            if (IsOwner)
                LocalPlayer = this;
            #elif Server
            _networkClient = NetworkClient.ClientsByOwnerId[OwnerClientId];
            #endif
            
            OnSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner || IsServer)
                _playersWithAuthority.Remove(this);

            OnDespawn();
        }

        protected void Update()
        {
            #if Client
            InputUpdate(_playerInput);
            #endif
        }

        public static void UpdatePlayersWithAuthority(uint tick, bool isReconciliation)
        {
            foreach (PlayerInputNetworkBehaviour player in _playersWithAuthority)
            {
                #if Client
                player.OnTick(tick, SnapshotManager.GetInputState(tick, null), isReconciliation);
                #elif Server
                if (!player._networkClient.sentInput) continue;
                player.OnTick(tick, player._networkClient.GetInputState(tick), isReconciliation);
                #endif
            }
        }

        public abstract void OnSpawn();
        public abstract void OnDespawn();
        public abstract void InputUpdate(PlayerInput playerInput);
        public abstract void OnTick(uint tick, ClientInputState input, bool isReconciliation);

        public abstract IData GetPlayerData();
    }
}