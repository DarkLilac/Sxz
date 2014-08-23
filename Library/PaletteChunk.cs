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
using System.Text;

namespace Library
{
    /// <summary>
    /// Holds up to 256 colors.  Doesn't draw anything on the canvas itself, can be repeated multiple times in a frame.
    /// </summary>
    public class PaletteChunk : Chunk
    {
        public List<SxzColor> Colors { get; set; }
        public static string Label { get { return "PB"; } }

        public PaletteChunk()
        {
            //Don't auto-sort - since we have the unsorted default palette to think about
            Colors = new List<SxzColor>();
        }

        public SxzPoint Origin { get; set; }

        public virtual List<byte> GetData()
        {
            List<byte> result = new List<byte>();
            byte[] byteLabel = Encoding.ASCII.GetBytes(Label);
            Writer.AddBytes(result, byteLabel);
            result.Add((byte)(Colors.Count - 1));
            SxzColor previousColor = new SxzColor(0, 0, 0);
            foreach (SxzColor color in Colors)
            {
                byte red = (byte)((color.Red - previousColor.Red) & 0xff);
                result.Add(red);
                byte green = (byte)((color.Green - previousColor.Green) & 0xff);
                result.Add(green);
                byte blue = (byte)((color.Blue - previousColor.Blue) & 0xff);
                result.Add(blue);
                previousColor = color;
                //Writer.AddBytes(result, color);
            }

            return result;
        }

        public virtual void SetData(byte[] data)
        {
            int index = 2;
            int count = data[index] + 1;
            index++;
            SxzColor previousColor = new SxzColor(0, 0, 0);
            for (int i = 0; i < count; i++)
            {
                byte red = (byte)((data[index] + previousColor.Red) & 0xff);
                index++;
                byte green = (byte)((data[index] + previousColor.Green) & 0xff);
                index++;
                byte blue = (byte)((data[index] + previousColor.Blue) & 0xff);
                index++;
                previousColor = new SxzColor(red, green, blue);
                Colors.Add(previousColor);
            }
        }

        public bool IsBackground()
        {
            return false;
        }

        public bool IsPalette()
        {
            return true;
        }

        public bool IsTransparent()
        {
            return false;
        }

        public PaletteChunk Palette { get; set; }

        public SxzColor GetColor(int x, int y)
        {
            return null;
        }

        public void EnsureDimensions(SxzPoint boundingBox)
        {
        }

        public SxzPoint GetDimensions()
        {
            return null;
        }

        public class ColorSorter : IComparer<SxzColor>
        {
            private static SxzColor Black = new SxzColor(0, 0, 0);

            public int Compare(SxzColor x, SxzColor y)
            {
                double xDistance = SxzColor.GetColorDistance(Black, x);
                double yDistance = SxzColor.GetColorDistance(Black, y);

                return (int)Math.Round(xDistance - yDistance, MidpointRounding.AwayFromZero);
            }
        }
    }
}
