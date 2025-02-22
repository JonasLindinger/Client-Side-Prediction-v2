using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Utility.SceneManagement
{
    public class SceneLoader : MonoBehaviourSingleton<SceneLoader>
    {
        [SerializeField] private Image loadingBar;
        [SerializeField] private float fillSpeed = 0.5f;
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private Camera loadingCamera;
        [SerializeField] private SceneGroup[] sceneGroups;
        
        private float _targetProgress;
        private bool _isLoading;

        public readonly SceneGroupManager Manager = new SceneGroupManager();

        private void Start()
        {
            ShowLoadingCanvas(false);
            Manager.OnSceneLoaded += (sceneName) => { Debug.Log("Loaded: " + sceneName); };
            Manager.OnSceneUnloaded += (sceneName) => { Debug.Log("Unloaded: " + sceneName); };
            Manager.OnSceneGroupLoaded += () => { Debug.Log("Scene group loaded"); };
        }

        private void Update()
        {
            if (!_isLoading) return;

            float currentFillAmount = loadingBar.fillAmount;
            float progressDifference = Mathf.Abs(currentFillAmount - _targetProgress);
            
            float dynamicFillSpeed = progressDifference * fillSpeed;
            
            loadingBar.fillAmount = Mathf.Lerp(currentFillAmount, _targetProgress, Time.deltaTime * dynamicFillSpeed);
        }

        public async Task LoadSceneGroup(int index)
        {
            loadingBar.fillAmount = 0;
            _targetProgress = 1f;

            // Just a safety check for the index
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            LoadingProgress progress = new LoadingProgress();
            progress.Progressed += (target) =>
            {
                _targetProgress = Mathf.Max(target, _targetProgress);
            };

            ShowLoadingCanvas();
            await Manager.LoadScenes(sceneGroups[index], progress);
            ShowLoadingCanvas(false);
        }

        private void ShowLoadingCanvas(bool show = true)
        {
            _isLoading = show;
            loadingCanvas.gameObject.SetActive(show);
            loadingCamera.gameObject.SetActive(show);
        }
    }
}