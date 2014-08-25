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
    /// Similar to MonoColorChunk except nothing is drawn on an irregular area.  Useful for painterly rendering.  Bit plane is stored in bool[] data.
    /// </summary>
    public class TransparentBitPlaneChunk : Chunk
    {
        private static SxzColor Color = new SxzColor();

        public TransparentBitPlaneChunk()
        {
        }

        public BitPlane BitPlane { get; set; }
        public SxzPoint Origin { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public static string Label { get { return "TB"; } }

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
                return Color;
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
            return true;
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

            //now write the bitmask
            result.AddRange(BitPlane.GetData());

            int size = result.Count - 2;

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

            //read bitmask bitplane
            byte[] bitplane = new byte[Frame.SizeOfBitPlaneInBytes(Width * Height)];
            Array.Copy(data, index, bitplane, 0, bitplane.Length);

            BitPlane.SetData(bitplane, Width * Height);
        }
    }
}
