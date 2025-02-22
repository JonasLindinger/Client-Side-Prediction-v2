using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Utility.SceneManagement
{
    public class SceneGroupManager
    {
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };

        private SceneGroup _activeSceneGroup;

        /// <summary>
        /// Returns a task, takes in a SceneGroup, IProgress and a bool that defines weather or not to reload scenes that are already loaded.
        /// Set this to true if you wanted some duplicates.
        /// The IProgress is for updating/reporting the progress.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="progress"></param>
        /// <param name="reloadDupScenes">Indicates</param>
        public async Task LoadScenes(SceneGroup group, IProgress<float> progress, bool reloadDupScenes = false)
        {
            // Defining Variables and unloading current scenes
            _activeSceneGroup = group;
            List<string> loadedScenes = new List<string>();

            // Unload every loaded Scene except the active Scene and of course the Bootstrapper
            Debug.Log("Unloading scenes");
            await UnloadScenes();

            int sceneCount = SceneManager.sceneCount;

            for (int i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            int totalScenesToLoad = _activeSceneGroup.scenes.Count;

            AsyncOperationGroup operationGroup = new AsyncOperationGroup(totalScenesToLoad);

            for (int i = 0; i < totalScenesToLoad; i++)
            {
                SceneData sceneData = group.scenes[i];
                if (!reloadDupScenes && loadedScenes.Contains(sceneData.Name)) continue;

                AsyncOperation operation = SceneManager.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);
                
                operationGroup.Operations.Add(operation);
                
                OnSceneLoaded.Invoke(sceneData.Name);
            }
            
            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsdDone)
            {
                progress?.Report(operationGroup.Progress);
                // Update with delay to avoid tight loop
                await Task.Delay(200);
            }
            
            Scene activeScene = SceneManager.GetSceneByName(_activeSceneGroup.FindSceneNameByType(SceneType.ActiveScene));
            
            if (activeScene.IsValid())
                SceneManager.SetActiveScene(activeScene);
            
            OnSceneGroupLoaded.Invoke();
        }
        
        /// <summary>
        /// Unloads all scenes except the active Scene and Bootstrapper.
        /// </summary>
        public async Task UnloadScenes()
        {
            List<string> scenes = new List<string>();
            string activeScene = SceneManager.GetActiveScene().name;
    
            int sceneCount = SceneManager.sceneCount;

            for (int i = sceneCount - 1; i > 0; i--)
            {
                Scene sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;

                string sceneName = sceneAt.name;
                // Skip Bootstrapper
                if (sceneName == "Bootstrapper") continue;

                scenes.Add(sceneName);
            }
            
            // Create an AsyncOperationGroup
            AsyncOperationGroup operationGroup = new AsyncOperationGroup(scenes.Count);
    
            foreach (string scene in scenes)
            {
                AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null) continue;
        
                operationGroup.Operations.Add(operation);
        
                OnSceneUnloaded.Invoke(scene);
            }
    
            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsdDone)
                // Update with delay to avoid tight loop
                await Task.Delay(200);
        }

    }
}