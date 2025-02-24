using System;
using Eflatun.SceneReference;

namespace SceneManagement
{
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public string Name => reference.Name;
        public SceneType sceneType;
    }
}