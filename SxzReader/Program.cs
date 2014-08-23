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
using Util;
using Library;
using Parser;

namespace SxzReader
{

    //Encoder to convert from other formats to .sxz
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SxzReader <options> <imagefilepath>");
                return;
            }

            SxzColor backgroundColor = null;
            bool parseBackground = true;
            int backgroundType = 0;
            bool mostCommonColor = false;
            //Maximum number of bytes per pixel allowed for filtering fat chunks
            //Ideally this would be about 1 byte/pixel as is with indexed png images approximately (pre-compressed)
            double bitsPerPixel = 2;
            double colorDistance = 32;

            for (int argIndex = 0; argIndex < args.Length - 1; argIndex++)
            {
                if (args[argIndex].Equals("-bg"))
                {
                    argIndex++;
                    string colorInput = args[argIndex];
                    if (colorInput.Equals("none"))
                    {
                        parseBackground = false;
                        
                    }
                    else if (colorInput.Equals("mostcommon"))
                    {
                        //removes most common color
                        mostCommonColor = true;
                    }
                    else
                    {
                        backgroundColor = SxzColor.Hex(colorInput);
                    }
                }
                else if (args[argIndex].Equals("-backgroundtype"))
                {
                    argIndex++;
                    if (!int.TryParse(args[argIndex], out backgroundType))
                    {
                        Console.WriteLine("Invalid background type " + args[argIndex]);
                        return;
                    }
                }
                else if (args[argIndex].Equals("-bpp"))
                {
                    argIndex++;
                    if (!double.TryParse(args[argIndex], out bitsPerPixel))
                    {
                        Console.WriteLine("Invalid Bits Per Pixel argument " + args[argIndex]);
                        return;
                    }
                }
                else if (args[argIndex].Equals("-distance"))
                {
                    argIndex++;
                    if (!double.TryParse(args[argIndex], out colorDistance))
                    {
                        Console.WriteLine("Invalid Color Distance argument " + args[argIndex]);
                        return;
                    }
                }
            }

            string filename = args[args.Length - 1];

            if (!File.Exists(filename))
            {
                Console.WriteLine("Failed to find file " + filename);
                return;
            }

            ParseImage parseImage = new ParseImage();
            Container container = parseImage.Parse(filename, backgroundColor, mostCommonColor, parseBackground, (BackgroundType)backgroundType, bitsPerPixel, colorDistance);

            File.WriteAllText(Helper.GetPrefix(filename) + ".sxz.txt", Print.GetString(container));
            byte[] output = container.GetData();
            Console.WriteLine("Output byte total is " + output.Length);
            Helper.WriteBytesToFile(Helper.GetPrefix(filename) + ".sxz", output);
            Helper.WriteBytesToZipFile(Helper.GetPrefix(filename) + ".sxz.gz", output);

            byte[] byteData = Helper.ReadBytesFromFile(Helper.GetPrefix(filename) + ".sxz");

            Container otherContainer = new Container();
            otherContainer.SetData(byteData);

            Helper.DrawContainer(Helper.GetPrefix(filename) + ".2.png", otherContainer);
        }
    }
}
