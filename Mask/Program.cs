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
using System.Collections.Generic;
using Util;
using Library;
using Parser;

namespace Mask
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Mask <backgroundColor> <imagefilepath>");
                return;
            }

            SxzColor backgroundColor = SxzColor.Hex(args[0]);
            string filename = args[1];

            MaskRegion maskRegion = new MaskRegion();
            using (System.Drawing.Bitmap bitMap = new System.Drawing.Bitmap(filename))
            {
                int width = bitMap.Width;
                int height = bitMap.Height;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        System.Drawing.Color pixel = bitMap.GetPixel(x, y);

                        //Don't put transparent colors into the histogram
                        if (pixel.A == 0)
                        {
                            continue;
                        }

                        SxzColor color = new SxzColor(pixel.R, pixel.G, pixel.B);

                        if (color.Equals(backgroundColor))
                        {
                            continue;
                        }

                        Location location = new Location();
                        location.Color = color;
                        location.Point.X = x;
                        location.Point.Y = y;

                        maskRegion.Add(location);
                    }
                }
            }

            Container container = new Container();
            Frame frame = new Frame();
            container.Frames.Add(frame);

            HashSet<ChunkContainer> containers = maskRegion.GetChunks(null);
            foreach (ChunkContainer chunkContainer in containers)
            {
                frame.Chunks.Add(chunkContainer.Chunk);
            }

            File.WriteAllText(Helper.GetPrefix(filename) + ".sxz.txt", Print.GetString(container));
            byte[] output = container.GetData();
            Console.WriteLine("Output byte total is " + output.Length);
            Helper.WriteBytesToFile(Helper.GetPrefix(filename) + ".sxz", output);
            Helper.WriteBytesToZipFile(Helper.GetPrefix(filename) + ".sxz.gz", output);
        }
    }
}
