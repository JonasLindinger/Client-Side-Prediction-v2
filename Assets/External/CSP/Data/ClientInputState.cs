using System;
using System.Collections.Generic;
using CSP.Input;
using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace CSP.Data
{
    public class ClientInputState : INetworkSerializable
    {
        public uint Tick;
        public IData Data;
        
        private bool[] _inputFlags;
        private Vector2[] _directionalInputs;
        
        public Dictionary<string, bool> InputFlags = new Dictionary<string, bool>();
        public Dictionary<string, Vector2> DirectionalInputs = new Dictionary<string, Vector2>();
    
        private byte[] _inputFlagsByte;
        private byte[] _directionalInputsByte;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            try
            {
                serializer.SerializeValue(ref Tick);

                if (serializer.IsWriter)
                {
                    SerializeFlags();
                    SerializeDirectionals();

                    serializer.SerializeValue(ref _inputFlagsByte);
                    serializer.SerializeValue(ref _directionalInputsByte);

                    if (Data == null) return;

                    // Serialize the data type
                    int dataType = Data.GetDataType();
                    serializer.SerializeValue(ref dataType);

                    Data.NetworkSerialize(serializer);
                }
                else
                {
                    serializer.SerializeValue(ref _inputFlagsByte);
                    serializer.SerializeValue(ref _directionalInputsByte);

                    DeserializeFlags();
                    DeserializeDirectionals();

                    try
                    {
                        int dataType = 0;
                        serializer.SerializeValue(ref dataType); // Read the data type

                        IData data = DataFactory.Create(dataType);
                        if (data == null)
                            return;
                        data.NetworkSerialize(serializer);
                        Data = data;
                    }
                    catch (Exception e)
                    {
                        // Ignore
                    }
                }
            }
            catch (Exception e)
            {
                // Ignore i guess
            }
        }
        
        private void SerializeFlags()
        {
            int i = 0;
            _inputFlags = new bool[InputFlags.Count];  // Initialize this only once

            foreach (var kvp in InputFlags)
            {
                _inputFlags[i] = kvp.Value;
                i++;
            }

            int byteAmount = Mathf.CeilToInt(_inputFlags.Length / 8f);
            byte[] byteArray = new byte[byteAmount];

            for (i = 0; i < _inputFlags.Length; i++)
            {
                if (_inputFlags[i]) // Only set bits for true values
                {
                    byteArray[i / 8] |= (byte)(1 << (i % 8));
                }
            }

            _inputFlagsByte = byteArray;
        }
        private void DeserializeFlags()
        {
            int boolCount = InputCollector.InputFlagNames.Count;
            
            bool[] boolArray = new bool[boolCount];

            int i = 0;
            for (i = 0; i < boolCount; i++)
            {
                boolArray[i] = (_inputFlagsByte[i / 8] & (1 << (i % 8))) != 0;
            }

            _inputFlags = boolArray;

            i = 0;
            foreach (var input in _inputFlags)
            {
                InputFlags.Add(InputCollector.InputFlagNames[i], input);
                i++;
            }
        }

        private void SerializeDirectionals()
        {
            Vector2[] vectors = new Vector2[DirectionalInputs.Count];
            int i = 0;
            foreach (var kvp in DirectionalInputs)
            {
                vectors[i] = kvp.Value;
                vectors[i].x = ClampValue(vectors[i].x);
                vectors[i].y = ClampValue(vectors[i].y);
                i++;
            }
            
            int byteCount = Mathf.CeilToInt(vectors.Length / 2f);
            byte[] byteArray = new byte[byteCount];

            for (i = 0; i < vectors.Length; i++)
            {
                byte packedValue = 0;

                int xBits = EncodeVectorComponent(vectors[i].x);
                int yBits = EncodeVectorComponent(vectors[i].y);

                int shift = (i % 2) * 4; // Shift by 0 or 4 bits

                packedValue = (byte)((xBits << 2) | yBits); // Encode a single Vector2
                byteArray[i / 2] |= (byte)(packedValue << shift); // Store two Vector2 values per byte
            }

            _directionalInputsByte =  byteArray;
        }
        private void DeserializeDirectionals()
        {
            int vectorCount = InputCollector.DirectionalInputNames.Count;
            
            Vector2[] vectors = new Vector2[vectorCount];
            
            int i = 0;
            for (i = 0; i < vectorCount; i++)
            {
                int shift = (i % 2) * 4; // Shift by 0 or 4 bits
                byte packedValue = (byte)((_directionalInputsByte[i / 2] >> shift) & 0b1111); // Extract 4 bits

                float x = DecodeVectorComponent((packedValue >> 2) & 0b11);
                float y = DecodeVectorComponent(packedValue & 0b11);

                vectors[i] = new Vector2(x, y);
                vectors[i].Normalize();
            }

            _directionalInputs = vectors;

            i = 0;
            foreach (var input in _directionalInputs)
            {
                DirectionalInputs.Add(InputCollector.DirectionalInputNames[i], input);
                i++;
            }
        }
        
        private static int EncodeVectorComponent(float value)
        {
            if (value == -1) return 0b00;
            if (value == 0) return 0b01;
            if (value == 1) return 0b10;
            throw new ArgumentException("Invalid Vector2 component value. Must be -1, 0, or 1.");
        }
        private static float DecodeVectorComponent(int bits)
        {
            return bits switch
            {
                0b00 => -1f,
                0b01 => 0f,
                0b10 => 1f,
                _ => throw new ArgumentException("Invalid encoded bits in byte array.")
            };
        }
        
        private static float ClampValue(float value)
        {
            if (value < -0.5f)
                return -1f;
            else if (value > 0.5f)
                return 1f;
            else
                return 0f;
        }
    }
}