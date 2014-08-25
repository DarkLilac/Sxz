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
using System.Drawing;
using System.IO;
using Library;
using Parser;
using Util;

namespace Test
{
    class Program
    {
        private static string[] Filenames = { "img_160x120_3x8bit_RGB_color_bars_CMYKWRGB.png",
                                                "img_160x120_3x8bit_RGB_color_bars_CMYKWRGB_gradient.png",
                                                "img_160x120_3x8bit_RGB_color_rainbow.png",
                                                "img_160x120_3x8bit_RGB_color_rainbow_gradient.png",
                                                "img_160x120_3x8bit_RGB_color_SMPTE_RP_219_2002.png",
                                                "sxz.png"
                                            };
        static void Main(string[] args)
        {
            if (!TestFilters())
            {
                return;
            }

            if (!TestBinary())
            {
                return;
            }

            if (!TestBoolsToBytes())
            {
                return;
            }

            foreach (string filename in Filenames)
            {
                ParseImage parseImage = new ParseImage();
                Container container = parseImage.Parse(filename, new SxzColor(255, 255, 255), false, false, (BackgroundType)0, 2, 32);
                string outputFilename = Helper.GetPrefix(filename) + ".2.png";
                Helper.DrawContainer(outputFilename, container);
                if (!CompareTwoPngs(filename, outputFilename))
                {
                    Console.WriteLine("Failed on " + filename);
                    return;
                }
            }
        }

        private static bool CompareTwoPngs(string filename1, string filename2)
        {
            using (System.Drawing.Bitmap bitMap1 = new System.Drawing.Bitmap(filename1))
            {
                int width1 = bitMap1.Width;
                int height1 = bitMap1.Height;
                using (System.Drawing.Bitmap bitMap2 = new System.Drawing.Bitmap(filename2))
                {
                    int width2 = bitMap1.Width;
                    int height2 = bitMap1.Height;
                    if (width1 != width2 || height1 != height2)
                    {
                        return false;
                    }

                    for (int x = 0; x < width1; x++)
                    {
                        for (int y = 0; y < height1; y++)
                        {
                            System.Drawing.Color pixel1 = bitMap1.GetPixel(x, y);
                            System.Drawing.Color pixel2 = bitMap2.GetPixel(x, y);
                            if (pixel1.R != pixel2.R || pixel1.G != pixel2.G || pixel1.B != pixel2.B)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static bool TestBoolsToBytes()
        {
            bool[] input = { false, false, true, true, true, true, false, false,
                               true, true, true, true, true, true, true, true,
                               true, true, true, true, false, false, true, true };
            Console.WriteLine("Have input length " + input.Length);

            List<byte> output = BitPlane.ConvertBoolsToBytes(input);
            Console.WriteLine("Have output length " + output.Count);
            return true;
        }

        private static bool TestFilters()
        {
            List<byte> values = new List<byte>() { 1, 2, 3, 5 };
            List<byte> original = new List<byte>(values);
            ColorBitPlaneChunk.Filter(values);
             Console.Write("Have filtered values");
            foreach (byte b in values)
            {
                Console.Write(" " + b);
            }

            Console.WriteLine(string.Empty);

            byte[] array = values.ToArray();
            ColorBitPlaneChunk.UnFilter(array);
            int index = 0;
            foreach (byte b in original)
            {
                if (array[index] != b)
                {
                    Console.WriteLine("Failed match in index " + index + " have original " + b + " and got instead " + array[index]);
                    return false;
                }

                index++;
            }

            return true;
        }

        private static bool TestBinary()
        {
            int depth = 5;

            byte inputValue = 23;

            bool[] bools = new bool[5];
            int counter = 0;
            for (int j = 0; j < depth; j++)
            {
                bools[counter] = (inputValue & Writer.Masks[j]) != 0;
                counter++;
            }
            Console.Write("Input bool array is ");
            foreach (bool b in bools)
            {
                Console.Write(b + " ");
            }

            Console.Write("\n");

            byte colorIndex = 0;
            int index = 0;
            for (int j = 0; j < depth; j++)
            {
                //populate indexColor
                if (bools[index])
                {
                    colorIndex |= Writer.Masks[j];
                    Console.WriteLine("ColorIndex is " + colorIndex);
                }

                index++;
            }

            Console.WriteLine("TestBinary output is " + colorIndex);
            return true;
        }
    }
}
