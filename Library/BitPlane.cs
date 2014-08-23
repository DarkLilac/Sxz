/*
* SXZ Reference Implementation
*
* Copyright (c) 2014, Dark Lilac. All rights reserved.
*
* This library is free software; you can redistribute it and/or
* modify it under the terms of the GNU Lesser General Public
* License as published by the Free Software Foundation; either
* version 2.1 of the License, or (at your option) any later version.
*
* This library is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
* Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public
* License along with this library; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
* MA 02110-1301 USA
*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library
{
    /// <summary>
    /// Tracks which pixels get drawn with which color, determined by the Index into the current frame's palette.
    /// </summary>
    public class BitPlane
    {
        private bool[] data;

        public BitPlane(int size, int width)
        {
            data = new bool[size];
            this.Width = width;
        }

        public int Width { get; set; }

        public bool HasColor(int x, int y)
        {
            int location = y * Width + x;
            return data[location];
        }

        public bool HasColor(int location)
        {
            return data[location];
        }

        public void SetData(byte[] bytes, int size)
        {
            this.data = ConvertBytesToBools(bytes, size);
        }

        public void DrawLocation(int x, int y)
        {
            int location = (y * Width) + x;
            data[location] = true;
        }

        public void UnDrawLocation(int x, int y)
        {
            int location = (y * Width) + x;
            data[location] = false;
        }

        public int GetPixelCount()
        {
            int result = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i])
                {
                    result++;
                }
            }

            return result;
        }

        public List<byte> GetData()
        {
            return ConvertBoolsToBytes(data);
        }

        /// <summary>
        /// There's probably a better way to do this
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool[] ConvertBytesToBools(byte[] bytes, int size)
        {
            List<bool> result = new List<bool>();

            int index = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                //read a bytes worth of bits at a time
                for (int j = 0; j < Writer.Masks.Length; j++)
                {
                    byte b = bytes[i];
                    bool boolean = (b & Writer.Masks[j]) != 0;
                    result.Add(boolean);
                    index++;
                    if (index >= size)
                    {
                        return result.ToArray();
                    }
                }
            }

            //oops
            throw new Exception("Failed to convert Bytes to Bools");
        }

        public static List<byte> ConvertBoolsToBytes(bool[] data)
        {
            List<byte> result = new List<byte>();
            //wrap into 8 bit bunches of a byte

            byte eightBits = 0;
            int counter = 0;
            foreach (bool b in data)
            {
                //write to next location on eightBits

                if (b)
                {
                    eightBits |= Writer.Masks[counter];
                }

                counter++;

                if (counter > 7)
                {
                    counter = 0;
                    result.Add(eightBits);
                    eightBits = 0;
                }
            }

            if (counter > 0)
            {
                //pad out the eightBits with zeros then add
                result.Add(eightBits);
            }

            return result;
        }
    }
}
