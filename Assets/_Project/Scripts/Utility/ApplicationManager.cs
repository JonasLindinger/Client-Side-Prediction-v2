using UnityEngine;

namespace _Project.Scripts.Utility
{
    public class ApplicationManager : MonoBehaviourSingleton<ApplicationManager>
    {
        private void Start()
        {
            #if Client
            LimitFPS();
            #endif
        }

        #if Client
        private void LimitFPS()
        {
            // Todo: Make this an option in the Settings
            Application.targetFrameRate = 240;
        }
        #endif
    }
}