using System;
using Eflatun.SceneReference;

namespace _Project.Scripts.Utility.SceneManagement
{
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public string Name => reference.Name;
        public SceneType sceneType;
    }
}