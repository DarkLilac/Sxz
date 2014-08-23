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
    /// Draws nothing on a rectangular area with no missing pixels.  Useful for painterly rendering.
    /// </summary>
    public class TransparentRectangleChunk : Chunk
    {
        private static SxzColor Color = new SxzColor();

        public TransparentRectangleChunk()
        {
        }

        public static string Label { get { return "TR"; } }

        public SxzPoint Origin { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<byte> GetData()
        {
            List<byte> result = new List<byte>();
            byte[] byteLabel = Encoding.ASCII.GetBytes(Label);
            Writer.AddBytes(result, byteLabel);
            Writer.AddBytes(result, Width);
            Writer.AddBytes(result, Height);
            Writer.AddBytes(result, Origin.X);
            Writer.AddBytes(result, Origin.Y);

            return result;
        }

        public void SetData(byte[] data)
        {
            int index = 2;
            this.Width = BitConverter.ToInt16(data, index);
            index += 2;
            this.Height = BitConverter.ToInt16(data, index);
            index += 2;
            int x = BitConverter.ToInt16(data, index);
            index += 2;
            int y = BitConverter.ToInt16(data, index);
            index += 2;
            this.Origin = new SxzPoint(x, y);
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

        public SxzColor GetColor(int x, int y)
        {
            return Color;
        }

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

        public SxzPoint GetDimensions()
        {
            return new SxzPoint(Width, Height);
        }
    }
}
