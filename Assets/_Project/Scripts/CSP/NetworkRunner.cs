using System.Threading.Tasks;
using _Project.Scripts.Utility;
using _Project.Scripts.Utility.SceneManagement;

namespace _Project.Scripts.CSP
{
    public class NetworkRunner : MonoBehaviourSingleton<NetworkRunner>
    {
        private async void Start()
        {
            await LoadIntoMainMenu();
        }
        
        #region SceneLoader

        /// <summary>
        /// Loads the scene Group of MainMenu
        /// </summary>
        private async Task LoadIntoMainMenu()
        {
            await SceneLoader.GetInstance().LoadSceneGroup(0);
        }
        
        #endregion
    }
}