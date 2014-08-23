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
    /// Specialty chunk to hold a background color, no bit plane.
    /// </summary>
    public class BackgroundChunk : Chunk
    {
        public BackgroundChunk()
        {
            Origin = new SxzPoint();
        }

        public static string Label { get { return "BG"; } }
        public byte ColorIndex { get; set; }
        public SxzPoint Origin { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<byte> GetData()
        {
            List<byte> result = new List<byte>();
            byte[] byteLabel = Encoding.ASCII.GetBytes(Label);
            Writer.AddBytes(result, byteLabel);
            result.Add(ColorIndex);

            return result;
        }

        public void SetData(byte[] data)
        {
            int index = 2;
            this.ColorIndex = data[index];
        }

        public bool IsBackground()
        {
            return true;
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

        public SxzColor GetColor(int x, int y)
        {
            return Palette.Colors[ColorIndex];
        }

        public void EnsureDimensions(SxzPoint boundingBox)
        {
        }

        public SxzPoint GetDimensions()
        {
            return new SxzPoint(Width, Height);
        }
    }
}
