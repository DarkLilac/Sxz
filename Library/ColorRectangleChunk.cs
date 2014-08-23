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
    public class ColorRectangleChunk : Chunk
    {
        public SxzPoint Origin { get; set; }

        private byte[] data;

        public int Width { get; set; }
        public int Height { get; set; }
        public Direction Direction { get; set; }
        public static string Label { get { return "CR"; } }

        public PaletteChunk Palette { get; set; }

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
            data = new byte[Width * Height];
        }

        public SxzPoint GetDimensions()
        {
            return new SxzPoint(Width, Height);
        }

        private byte GetByteIndex(int x, int y)
        {
            int location = y * Width + x;
            return data[location];
        }

        private void SetByteIndex(byte value, int x, int y)
        {
            int location = y * Width + x;
            data[location] = value;
        }

        public SxzColor GetColor(int x, int y)
        {
            return Palette.Colors[GetByteIndex(x - Origin.X, y - Origin.Y)];
        }

        public void SetColor(SxzColor color, int x, int y)
        {
            SetByteIndex((byte)Palette.Colors.IndexOf(color), x - Origin.X, y - Origin.Y);
        }

        public void ResetPalette(PaletteChunk newPalette)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SxzColor color = Palette.Colors[GetByteIndex(x , y)];
                    SetByteIndex((byte)newPalette.Colors.IndexOf(color), x, y);
                }
            }

            this.Palette = newPalette;
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

        private List<byte> GetDataList()
        {
            List<byte> result = new List<byte>();
            switch (Direction)
            {
                case Direction.Left:
                    for (int x = Width - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            result.Add(GetByteIndex(x, y));
                        }
                    }

                    break;
                case Direction.Right:
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            result.Add(GetByteIndex(x, y));
                        }
                    }

                    break;
                case Direction.Up:
                    for (int y = Height - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            result.Add(GetByteIndex(x, y));
                        }
                    }

                    break;
                case Direction.Down:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            result.Add(GetByteIndex(x, y));
                        }
                    }

                    break;
            }

            //RowFilter(result);
            return result;
        }

        private void SetDataList(byte[] values)
        {
            //UnRowFilter(values);
            int index = 0;

            switch (Direction)
            {
                case Direction.Left:
                    for (int x = Width - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            SetByteIndex(values[index++], x, y);
                        }
                    }

                    break;
                case Direction.Right:
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            SetByteIndex(values[index++], x, y);
                        }
                    }

                    break;
                case Direction.Up:
                    for (int y = Height - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            SetByteIndex(values[index++], x, y);
                        }
                    }

                    break;
                case Direction.Down:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            SetByteIndex(values[index++], x, y);
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Taken from png Sub filter, not sure if this is useful with existing ordering
        /// </summary>
        /// <param name="values"></param>
        public static void Filter(List<byte> values)
        {
            List<byte> raw = new List<byte>(values);
            for (int i = 1; i < values.Count; i++)
            {
                values[i] = (byte)((raw[i] - raw[i - 1]) & 0xff);
            }
        }

        public static void UnFilter(byte[] values)
        {
            byte[] raw = new byte[values.Length];
            Array.Copy(values, raw, values.Length);
            for (int i = 1; i < values.Length; i++)
            {
                values[i] = (byte)((raw[i] + values[i - 1]) & 0xff);
            }
        }

        public void RowFilter(List<byte> values)
        {
            List<byte> raw = new List<byte>(values);
            for (int y = 0; y < Height; y++)
            {
                byte first = GetByteIndex(0, y);
                for (int x = 1; x < Width; x++)
                {
                    values[y * Width + x] = (byte)((first - raw[y * Width + x]) & 0xff);
                }
            }
        }

        public void UnRowFilter(byte[] values)
        {
            byte[] raw = new byte[values.Length];
            Array.Copy(values, raw, values.Length);

            for (int y = 0; y < Height; y++)
            {
                byte first = GetByteIndex(0, y);
                for (int x = 1; x < Width; x++)
                {
                    values[y * Width + x] = (byte)((first + raw[y * Width + x]) & 0xff);
                }
            }
        }

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

            result.Add((byte)Direction);
            //then the actual indexes
            result.AddRange(GetDataList());

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
            int width = BitConverter.ToInt16(data, index);
            this.Width = width;
            index += 2;
            int height = BitConverter.ToInt16(data, index);
            this.Height = height;
            index += 2;
            int x = BitConverter.ToInt16(data, index);
            index += 2;
            int y = BitConverter.ToInt16(data, index);
            index += 2;
            this.Origin = new SxzPoint(x, y);

            this.Direction = (Direction)data[index];
            index++;

            this.data = new byte[Width * Height];

            //get color index count
            int numberOfIndexes = Width * Height;
            byte[] indexes = new byte[numberOfIndexes];
            //get indexes
            Array.Copy(data, index, indexes, 0, indexes.Length);
            SetDataList(indexes);
        }
    }
}
