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
    /// Indexed into current frame palette, single color region with an irregular area.
    /// </summary>
    public class MonoBitPlaneChunk : Chunk
    {

        public MonoBitPlaneChunk()
        {
        }

        public BitPlane BitPlane { get; set; }
        public SxzPoint Origin { get; set; }
        public byte ColorIndex { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public static string Label { get { return "MB"; } }

        public void EnsureDimensions(SxzPoint boundingBox)
        {
            if (Origin.X + Width > boundingBox.X)
            {
                boundingBox.X = Origin.X + Width;
            }

            if (Origin.Y + Height > boundingBox.Y)
            {
                boundingBox.Y = Origin.Y + Height;
            }
        }

        public void Initialize()
        {
            BitPlane = new BitPlane(Width * Height, Width);
        }

        public void SetColor(int x, int y)
        {
            BitPlane.DrawLocation((x - Origin.X), (y - Origin.Y));
        }

        public SxzPoint GetDimensions()
        {
            return new SxzPoint(Width, Height);
        }

        public SxzColor GetColor(int x, int y)
        {
            if (BitPlane.HasColor(x - Origin.X, y - Origin.Y))
            {
                return Palette.Colors[ColorIndex];
            }

            return null;
        }

        public bool IsBackground()
        {
            return false;
        }

        public bool IsPalette()
        {
            return false;
        }

        public bool IsTransparent()
        {
            return false;
        }

        public PaletteChunk Palette { get; set; }

        public List<byte> GetData()
        {
            List<byte> result = new List<byte>();

            byte[] byteLabel = Encoding.ASCII.GetBytes(Label);
            Writer.AddBytes(result, byteLabel);
            //write the size of the rest at the end
            Writer.AddBytes(result, Width);
            Writer.AddBytes(result, Height);
            Writer.AddBytes(result, Origin.X);
            Writer.AddBytes(result, Origin.Y);

            result.Add(ColorIndex);

            //now write the bitmask
            result.AddRange(BitPlane.GetData());

            //now insert the size
            Writer.WriteSizeShort(result);
            return result;
        }

        //put the byte data into this object
        public void SetData(byte[] data)
        {
            //assume the first two bytes of the label are not already skipped
            //and skip the size too
            int index = 4;
            this.Width = BitConverter.ToInt16(data, index);
            index += 2;
            this.Height = BitConverter.ToInt16(data, index);
            index += 2;
            int x = BitConverter.ToInt16(data, index);
            index += 2;
            int y = BitConverter.ToInt16(data, index);
            index += 2;
            this.Origin = new SxzPoint(x, y);

            this.ColorIndex = data[index];
            index++;

            BitPlane = new BitPlane(Width * Height, Width);

            //read bitmask bitplane
            byte[] bitplane = new byte[Frame.SizeOfBitPlaneInBytes(Width * Height)];
            Array.Copy(data, index, bitplane, 0, bitplane.Length);

            BitPlane.SetData(bitplane, Width * Height);
        }
    }
}
