using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Data
{
    public class ClientInputState : INetworkSerializable
    {
        public uint Tick;
        public Vector2[] DirectionalInputs;
        public bool[] InputFlags;
        
        private byte[] _data;

        private void Serialize()
        {
            // Calculate how many bytes are needed for the Vector2 array and bool array
            int movementCount = DirectionalInputs.Length;
            int boolCount = InputFlags.Length;

            // Calculate the number of bytes required for the Vector2s
            int vector2Bytes = Mathf.CeilToInt(movementCount * 2 * 2f / 8);  // 2 bits for each axis per Vector2
            // Calculate the number of bytes required for the bools
            int boolBytes = Mathf.CeilToInt(boolCount / 8f);  // 1 byte for up to 8 bools

            // The total size of the byte array will include space for the tick (4 bytes)
            _data = new byte[vector2Bytes + boolBytes + 4];  // Add 4 bytes for the tick

            int dataIndex = 0;

            // Serialize Vector2s (each using 2 bits for x and y, 4 bits total per Vector2)
            for (int i = 0; i < movementCount; i++)
            {
                Vector2 input = DirectionalInputs[i];
                byte x = (byte)(input.x == 1 ? 2 : (input.x == -1 ? 1 : 0));
                byte y = (byte)(input.y == 1 ? 2 : (input.y == -1 ? 1 : 0));
    
                if (i % 2 == 0)
                {
                    // Pack the first Vector2 (x and y) into the first half of the byte
                    _data[dataIndex] |= (byte)(x << 0);
                    _data[dataIndex] |= (byte)(y << 2);
                }
                else
                {
                    // Pack the second Vector2 (x and y) into the second half of the byte
                    _data[dataIndex] |= (byte)(x << 4);
                    _data[dataIndex] |= (byte)(y << 6);
                    dataIndex++;
                }
            }

            // Serialize bools into the byte array
            int boolByteIndex = dataIndex;
            for (int i = 0; i < boolCount; i++)
            {
                int byteIndex = boolByteIndex + (i / 8);
                _data[byteIndex] |= (byte)((InputFlags[i] ? 1 : 0) << (i % 8));
            }

            // Serialize the tick (4 bytes)
            byte[] tickBytes = System.BitConverter.GetBytes(Tick);
            System.Array.Copy(tickBytes, 0, _data, dataIndex, tickBytes.Length);
        }
        private void Deserialize()
        {
            int movementCount = (_data.Length - 4) / 2 / 2;  // 2 bits for each axis per Vector2
            int boolCount = (_data.Length - 4) / 8;  // 1 byte for up to 8 bools

            DirectionalInputs = new Vector2[movementCount];
            InputFlags = new bool[boolCount];

            int dataIndex = 0;

            // Deserialize Vector2s
            for (int i = 0; i < movementCount; i++)
            {
                byte byte1 = _data[dataIndex++];
                byte x = (byte)((byte1 >> 0) & 0x03);  // Extract x component
                byte y = (byte)((byte1 >> 2) & 0x03);  // Extract y component
                DirectionalInputs[i] = new Vector2(x == 2 ? 1f : (x == 1 ? -1f : 0f), y == 2 ? 1f : (y == 1 ? -1f : 0f));
            }

            // Deserialize bools
            int boolByteIndex = dataIndex;
            for (int i = 0; i < InputFlags.Length; i++)
            {
                int byteIndex = boolByteIndex + (i / 8);
                InputFlags[i] = ((_data[byteIndex] >> (i % 8)) & 1) != 0;
            }

            // Deserialize the tick (4 bytes)
            Tick = System.BitConverter.ToUInt32(_data, dataIndex);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Serialize the entire byte array directly (including tick)
            int dataSize = _data.Length;
            serializer.SerializeValue(ref dataSize);
            
            if (serializer.IsWriter)
            {
                Serialize();

                // Write the byte array to the serializer
                for (int i = 0; i < dataSize; i++)
                {
                    serializer.SerializeValue(ref _data[i]);
                }
            }
            else
            {
                // Read the byte array from the serializer
                _data = new byte[dataSize];
                for (int i = 0; i < dataSize; i++)
                {
                    serializer.SerializeValue(ref _data[i]);
                }
                
                Deserialize();
            }
        }
    }
}