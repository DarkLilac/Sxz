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
using System.IO;
using Library;
using Util;

namespace SxzAppend
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SxzAppend <sxzfilepath1> <sxzfilepath2>");
                return;
            }

            string filename1 = args[0];

            if (!File.Exists(filename1))
            {
                Console.WriteLine("Failed to find file " + filename1);
                return;
            }

            byte[] byteData = Helper.ReadBytesFromFile(filename1);
            Console.WriteLine("Read in size for file1 is " + byteData.Length);

            Container container = new Container();
            container.SetData(byteData);

            SxzPoint dimensions = new SxzPoint();
            container.EnsureDimensions(dimensions);
            dimensions.X = (int)(dimensions.X / 2);
            dimensions.Y = (int)(dimensions.Y / 2);

            string filename2 = args[1];

            if (!File.Exists(filename2))
            {
                Console.WriteLine("Failed to find file " + filename2);
                return;
            }

            byteData = Helper.ReadBytesFromFile(filename2);
            Console.WriteLine("Read in size for file2 is " + byteData.Length);
            Container container2 = new Container();
            container2.SetData(byteData);
            SxzPoint dimensions2 = new SxzPoint();
            container2.EnsureDimensions(dimensions2);
            dimensions.X = dimensions.X - (int)(dimensions2.X / 2);
            dimensions.Y = dimensions.Y - (int)(dimensions2.Y / 2);

            foreach (Frame frame in container2.Frames)
            {
                foreach (Chunk chunk in frame.Chunks)
                {
                    if (chunk.Origin == null)
                    {
                        continue;
                    }

                    chunk.Origin.X = chunk.Origin.X + dimensions.X;
                    chunk.Origin.Y = chunk.Origin.Y + dimensions.Y;
                }
            }

            container.Frames.AddRange(container2.Frames);

            byte[] output = container.GetData();
            Console.WriteLine("Output byte total is " + output.Length);
            Helper.WriteBytesToFile("appendoutput.sxz", output);
            File.WriteAllText("appendoutput.sxz.txt", Print.GetString(container));

            Helper.DrawContainer("appendoutput.png", container);
        }
    }
}
