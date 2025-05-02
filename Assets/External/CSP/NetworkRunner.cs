using System;
using _Project.Scripts.Items;
using CSP.Connection.Approval;
using CSP.Connection.Listener;
using CSP.Data;
using CSP.Simulation;
using _Project.Scripts.Utility;
using CSP.Connection;
using SceneManagement;
using Singletons;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using NetworkSettings = CSP.ScriptableObjects.NetworkSettings;

namespace CSP
{
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class NetworkRunner : MonoBehaviourSingleton<NetworkRunner>
    {
        public static NetworkSettings NetworkSettings;
        #if Client
        public static ulong ServerClientId => GetInstance()._networkManager.NetworkConfig.NetworkTransport.ServerClientId;
        #endif
        
        [Header("References")]
        [SerializeField] private NetworkSettings networkSettings;
        [SerializeField] private ConnectionApproval connectionApproval;
        [SerializeField] private ConnectionListener connectionListener;
        [Space(5)]
        [Header("Settings")] 
        [SerializeField] private bool autoStartServer = true;
        
        // References
        private NetworkManager _networkManager;
        public UnityTransport UnityTransport { get; private set; }
        
        private async void Start()
        {
            Reference();
            
            // Setting up Network Components
            SetUpNetworkManager();
            LinkTransport();
            ConnectConnectionListener();
            
            #if Client
            await SceneLoader.GetInstance().LoadSceneGroup(0);
            #elif Server
            ConnectConnectionApproval();
            LimitFPS();

            if (autoStartServer)
                Run();
            
            #endif
        }

        private void Update()
        {
            SnapshotManager.Update(Time.deltaTime);
            CommunicationManager.Update(Time.deltaTime);
        }

        #region References

        /// <summary>
        /// Set's our References (uses GetComponent())
        /// </summary>
        private void Reference()
        {
            _networkManager = GetComponent<NetworkManager>();
            UnityTransport = GetComponent<UnityTransport>();
            NetworkSettings = networkSettings;
        }

        #endregion
        
        #region Network

        #region Init
        
        /// <summary>
        /// Sets values on the NetworkManager
        /// </summary>
        private void SetUpNetworkManager()
        {
            // Setting the TickRate to our custom TickRate
            _networkManager.NetworkConfig.TickRate = NetworkSettings.networkTickRate;
            
            // We use our custom scene manager so we don't need Unity to do that for us.
            _networkManager.NetworkConfig.EnableSceneManagement = false;
            
            // Recycling Network Ids could cause errors
            _networkManager.NetworkConfig.RecycleNetworkIds = false;

            // We want to handle connections ourselves for more control
            _networkManager.NetworkConfig.ConnectionApproval = true;
        }

        /// <summary>
        /// Set's the target Frame Rate to the tick system
        /// </summary>
        /// <returns></returns>
        private void LimitFPS()
        {
            #if Server
            Application.targetFrameRate = (int) NetworkSettings.physicsTickRate;
            #endif
        }
        
        /// <summary>
        /// Connects the Unity Transport to the Network Manager
        /// </summary>
        private void LinkTransport()
        {
            _networkManager.NetworkConfig.NetworkTransport = UnityTransport;
        }

        /// <summary>
        /// Subscribes a ConnectionApproval.cs class to the connection approval callback
        /// </summary>
        private void ConnectConnectionApproval()
        {
            _networkManager.ConnectionApprovalCallback += connectionApproval.OnConnectionRequest;
        }

        /// <summary>
        /// Subscribes a ConnectionListener.cs class to the connection event callback
        /// </summary>
        private void ConnectConnectionListener()
        {
            _networkManager.OnConnectionEvent += connectionListener.OnConnectionEvent;
        }
        
        #endregion
        
        #region Run
        
        #if Server

        /// <summary>
        /// Start's the Server and set's connection data
        /// </summary>
        private async void Run()
        {
            await SceneLoader.GetInstance().LoadSceneGroup(1);
            SetConnectionData("", 0);

            if (_networkManager.StartServer()) 
            {
                SnapshotManager.KeepTrack(NetworkSettings.physicsTickRate);
                CommunicationManager.StartCommunication(NetworkSettings.networkTickRate);
                Debug.Log("Server started");
                ItemSpawner.GetInstance().SpawnItems();
            }
            else
                Debug.LogError("Couldn't start server");
        }
        
        #elif Client
        
        /// <summary>
        /// Start's the Client, set's connection data and send's payload
        /// </summary>
        public async void Run(string ipAddress, ushort port, ConnectionPayload payload)
        {
            await SceneLoader.GetInstance().LoadSceneGroup(1);
            
            SetConnectionData(ipAddress, port);
            SendPayload(payload);
            
            if (_networkManager.StartClient())
                Debug.Log("Client started");
            else
                Debug.LogError("Couldn't start client");
        }
        
        /// <summary>
        /// Set's the connectionPayload of the NetworkManager to the payload you give the method
        /// </summary>
        /// <param name="payload"></param>
        private void SendPayload(ConnectionPayload payload)
        {
            _networkManager.NetworkConfig.ConnectionData = payload.GetAsPayload();
        }
        
        #endif

        /// <summary>
        /// Set's connection data on the unity transport
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        private void SetConnectionData(string ipAddress, ushort port)
        {
            UnityTransport.ConnectionData.Address = string.IsNullOrEmpty(ipAddress) ? NetworkSettings.defaultIp : ipAddress;
            UnityTransport.ConnectionData.Port = port == 0 ? NetworkSettings.defaultPort : port;
        }
        
        #endregion
        
        #endregion
    }
}