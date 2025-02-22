using _Project.Scripts.CSP.Connection.Approval;
using _Project.Scripts.CSP.Connection.Data;
using _Project.Scripts.CSP.Connection.Listener;
using _Project.Scripts.CSP.Simulation;
using _Project.Scripts.Utility;
using _Project.Scripts.Utility.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using NetworkSettings = _Project.Scripts.CSP.ScriptableObjects.NetworkSettings;

namespace _Project.Scripts.CSP
{
    [RequireComponent(typeof(TickSystemManager))]
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class NetworkRunner : MonoBehaviourSingleton<NetworkRunner>
    {
        [Header("References")]
        [SerializeField] private NetworkSettings networkSettings;
        [SerializeField] private ConnectionApproval connectionApproval;
        [SerializeField] private ConnectionListener connectionListener;
        [Space(5)]
        [Header("Settings")] 
        [SerializeField] private bool autoStartServer = true;
        
        // References
        private TickSystemManager _tickSystemManager;
        private NetworkManager _networkManager;
        private UnityTransport _unityTransport;
        
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

        #region References

        /// <summary>
        /// Set's our References (uses GetComponent())
        /// </summary>
        private void Reference()
        {
            _networkManager = GetComponent<NetworkManager>();
            _unityTransport = GetComponent<UnityTransport>();
            _tickSystemManager = GetComponent<TickSystemManager>();
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
            _networkManager.NetworkConfig.TickRate = networkSettings.networkTickRate;
            
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
            Application.targetFrameRate = (int) networkSettings.physicsTickRate;
            #endif
        }
        
        /// <summary>
        /// Connects the Unity Transport to the Network Manager
        /// </summary>
        private void LinkTransport()
        {
            _networkManager.NetworkConfig.NetworkTransport = _unityTransport;
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
                Debug.Log("Server started");
                _tickSystemManager.StartTickSystems(
                    networkSettings.physicsTickRate, 
                    networkSettings.networkTickRate,
                    1
                );
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
        
        #endif

        /// <summary>
        /// Set's connection data on the unity transport
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        private void SetConnectionData(string ipAddress, ushort port)
        {
            _unityTransport.ConnectionData.Address = string.IsNullOrEmpty(ipAddress) ? networkSettings.defaultIp : ipAddress;
            _unityTransport.ConnectionData.Port = port == 0 ? networkSettings.defaultPort : port;
        }

        /// <summary>
        /// Set's the connectionPayload of the NetworkManager to the payload you give the method
        /// </summary>
        /// <param name="payload"></param>
        private void SendPayload(ConnectionPayload payload)
        {
            _networkManager.NetworkConfig.ConnectionData = payload.GetAsPayload();
        }
        
        #endregion
        
        #endregion
    }
}