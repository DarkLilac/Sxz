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
using System.Text;

namespace Library
{
    public static class Print
    {
        public static string GetString(Container container)
        {
            StringBuilder result = new StringBuilder();
            List<byte> bytes = new List<byte>();
            byte[] containerBytes = container.GetData().ToArray();
            for (int i = 0; i < 3; i++)
            {
                PrintLine(result, containerBytes[i]);
            }

            foreach (Frame frame in container.Frames)
            {
                result.AppendLine(string.Empty);
                result.Append(GetString(frame));

                foreach (Chunk chunk in frame.Chunks)
                {
                    result.AppendLine(string.Empty);
                    byte[] output = chunk.GetData().ToArray();
                    foreach (byte b in output)
                    {
                        PrintLine(result, b);
                    }
                }
            }

            return result.ToString();
        }

        public static string GetString(Frame frame)
        {
            StringBuilder result = new StringBuilder();
            byte[] output = frame.GetData().ToArray();
            for (int i = 0; i < 6; i++)
            {
                PrintLine(result, output[i]);
            }

            return result.ToString();
        }

        private static void PrintLine(StringBuilder result, byte b)
        {
            char c = ' ';
            if (b > 47 && b < 91)
            {
                c = (char)b;
            }

            result.AppendLine(ConvertByteToBitString(b) + " " + c + " " + (int)b);
        }

        private static string ConvertByteToBitString(byte b)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                bool aByte = (b & Writer.Masks[i]) != 0;
                if (aByte)
                {
                    result.Append("1");
                }
                else
                {
                    result.Append("0");
                }
            }

            return result.ToString();
        }
    }
}
