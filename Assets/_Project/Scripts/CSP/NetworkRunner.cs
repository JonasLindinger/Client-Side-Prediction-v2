using System.Threading.Tasks;
using _Project.Scripts.CSP.Connection.Approval;
using _Project.Scripts.CSP.Connection.Data;
using _Project.Scripts.Utility;
using _Project.Scripts.Utility.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using NetworkSettings = _Project.Scripts.CSP.ScriptableObjects.NetworkSettings;

namespace _Project.Scripts.CSP
{
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class NetworkRunner : MonoBehaviourSingleton<NetworkRunner>
    {
        [Header("References")]
        [SerializeField] private NetworkSettings networkSettings;
        [SerializeField] private ConnectionApproval connectionApproval;
        [Space(5)] 
        [Header("Settings")] 
        [SerializeField] private bool autoStartServer = true;
        
        // References
        private NetworkManager _networkManager;
        private UnityTransport _unityTransport;
        
        private async void Start()
        {
            Reference();
            
            // Setting up Network Components
            SetUpNetworkManager();
            LinkTransport();
            ConnectConnectionApproval();
            
            await LoadIntoMainMenu();
            
            #if Server
            
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
        }

        #endregion
        
        #region SceneLoader

        /// <summary>
        /// Loads the scene Group of MainMenu
        /// </summary>
        private async Task LoadIntoMainMenu()
        {
            await SceneLoader.GetInstance().LoadSceneGroup(1);
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
        /// Connects the Unity Transport to the Network Manager
        /// </summary>
        private void LinkTransport()
        {
            _networkManager.NetworkConfig.NetworkTransport = _unityTransport;
        }

        /// <summary>
        /// Subscribes a IConnectionApproval.cs Interface to the connection approval callback
        /// </summary>
        private void ConnectConnectionApproval()
        {
            _networkManager.ConnectionApprovalCallback += connectionApproval.OnConnectionRequest;
        }
        
        #endregion
        
        #region Run
        
        #if Server

        /// <summary>
        /// Start's the Server and set's connection data
        /// </summary>
        private void Run()
        {
            SetConnectionData(networkSettings.defaultIp, networkSettings.defaultPort);

            if (_networkManager.StartServer())
                Debug.Log("Server started");
            else
                Debug.LogError("Couldn't start server");
        }
        
        #elif Client
        
        /// <summary>
        /// Start's the Client, set's connection data and send's payload
        /// </summary>
        public void Run(string ipAddress, ushort port, ConnectionPayload payload)
        {
            SetConnectionData(ipAddress, port);

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
            _unityTransport.ConnectionData.Address = ipAddress;
            _unityTransport.ConnectionData.Port = port;
        }
        
        #endregion
        
        #endregion
    }
}