using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CSP.TextDebug
{
    public static class TextWriter
    {
        private static Dictionary<ulong, string> _playerInputs = new Dictionary<ulong, string>();

        private static void Init(ulong player)
        {
            string fileName = "Player " + player + ".txt";
            string path = Application.dataPath + "/" + fileName;
            
            // Delete file if it exists
            if (File.Exists(path))
                File.Delete(path);
            
            // Create file if not existing
            if (!_playerInputs.ContainsKey(player))
            {
                _playerInputs.Add(player, path);
            }
        }
        
        public static void Update(ulong player, uint tick, Vector2 input)
        {
            // Check if we have to init this player
            if (!_playerInputs.ContainsKey(player))
                Init(player);
            
            string message = tick.ToString("00000") + " -- " + input;
            
            // Check if this file exists. If it doesn't just set the input of the file to the input
            if (!File.Exists(_playerInputs[player]))
                File.WriteAllText(_playerInputs[player], message);
            // If the file exists, append the new input int a new Line
            else
            {
                using (var writer = new StreamWriter(_playerInputs[player], true))
                {
                    writer.WriteLine(message);
                }
            }
        }
    }
}