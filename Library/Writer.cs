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

namespace Library
{
    public class Writer
    {
        public static byte[] Masks = { 1, 2, 4, 8, 16, 32, 64, 128 };

        public Writer()
        {
        }

        public byte[] Write(Frame frame)
        {
            List<byte> result = new List<byte>();
            foreach (Chunk chunk in frame.Chunks)
            {
            }

            return result.ToArray();
        }

        public static void AddBytes(List<byte> list, byte[] array)
        {
            foreach (byte b in array)
            {
                list.Add(b);
            }
        }

        public static void AddBytes(List<byte> list, int value)
        {
            byte b0 = (byte)value;
            byte b1 = (byte)(value >> 8);

            list.Add(b0);
            list.Add(b1);
        }

        public static void AddBytes32(List<byte> list, int value)
        {
            byte[] byteArray = BitConverter.GetBytes(value);

            list.Add(byteArray[0]);
            list.Add(byteArray[1]);
            list.Add(byteArray[2]);
            list.Add(byteArray[3]);
        }

        public static void AddBytes(List<byte> list, SxzColor color)
        {
            list.Add(color.Red);
            list.Add(color.Green);
            list.Add(color.Blue);
        }

        /// <summary>
        /// Add two bytes after the header label for the size
        /// </summary>
        /// <param name="list"></param>
        public static void WriteSizeShort(List<byte> list)
        {
            int value = list.Count - 2;
            byte b0 = (byte)value;
            byte b1 = (byte)(value >> 8);

            list.Insert(2, b0);
            list.Insert(3, b1);
        }

        public static void WriteSizeInt(List<byte> list)
        {
            int value = list.Count - 2;
            byte[] byteArray = BitConverter.GetBytes(value);

            list.Insert(2, byteArray[0]);
            list.Insert(3, byteArray[1]);
            list.Insert(4, byteArray[2]);
            list.Insert(5, byteArray[3]);
        }
    }
}
