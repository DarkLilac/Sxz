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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using Library;

namespace Util
{
    public static class Helper
    {
        public static void WriteBytesToFile(string filename, byte[] output)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(output, 0, output.Length);
                fileStream.Flush();
                fileStream.Close();
            }
        }

        public static void WriteBytesToZipFile(string filename, byte[] output)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (GZipStream zipStream = new GZipStream(fileStream, CompressionMode.Compress, false))
                {
                    zipStream.Write(output, 0, output.Length);
                    zipStream.Flush();
                    zipStream.Close();
                }
            }
        }

        public static byte[] ReadBytesFromFile(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        public static string GetPrefix(string filename)
        {
            int index = filename.IndexOf(".");
            if (index < 0)
            {
                throw new Exception("File lacks dot?");
            }

            return filename.Substring(0, index);
        }

        public static void DrawContainer(string filename, Container container)
        {
            SxzPoint boundingBox = new SxzPoint();

            container.EnsureDimensions(boundingBox);
            DrawContainer(filename, container, boundingBox);
        }

        public static void DrawContainer(string filename, Container container, SxzPoint boundingBox)
        {
            bool[,] drawable = new bool[boundingBox.X, boundingBox.Y];
            Color transparent = Color.Transparent;

            using (Bitmap drawableSurface = new Bitmap(boundingBox.X, boundingBox.Y, PixelFormat.Format32bppArgb))
            {
                drawableSurface.MakeTransparent(transparent);
                using (Graphics graphics = Graphics.FromImage(drawableSurface))
                {
                    graphics.Clear(transparent);
                    //debug color of green for now
                    SolidBrush brush = new SolidBrush(Color.Green);
                    //graphics.FillRectangle(brush, 0, 0, boundingBox.X, boundingBox.Y);
                    foreach (Frame frame in container.Frames)
                    {
                        foreach (Chunk chunk in frame.Chunks)
                        {
                            if (chunk.IsPalette())
                            {
                                continue;
                            }

                            if (chunk.IsBackground())
                            {
                                brush.Color = GetColor(chunk.GetColor(0, 0));
                                graphics.FillRectangle(brush, 0, 0, boundingBox.X, boundingBox.Y);
                                continue;
                            }

                            SxzPoint dimensions = chunk.GetDimensions();
                            SxzPoint origin = chunk.Origin;
                            for (int y = origin.Y; y < origin.Y + dimensions.Y; y++)
                            {
                                for (int x = origin.X; x < origin.X + dimensions.X; x++)
                                {
                                    SxzColor sxzColor = chunk.GetColor(x, y);

                                    if (sxzColor == null)
                                    {
                                        //For chunks with bitplanes, this means don't drawn on this pixel even though it is within
                                        //the region of the chunk
                                        continue;
                                    }

                                    if (chunk.IsTransparent())
                                    {
                                        drawableSurface.SetPixel(x, y, transparent);
                                        continue;
                                    }

                                    Color color = GetColor(sxzColor);
                                    if (color == null)
                                    {
                                        continue;
                                    }

                                    drawable[x, y] = true;
                                    brush.Color = color;
                                    graphics.FillRectangle(brush, x, y, 1, 1);
                                }
                            }
                        }
                    }

                    Image outputImage = (Image)drawableSurface;
                    outputImage.Save(filename, ImageFormat.Png);
                }
            }
        }

        /// <summary>
        /// Drawing function for rasterization
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="frame"></param>
        public static void DrawFrame(string filename, Frame frame)
        {
            SxzPoint boundingBox = new SxzPoint();
            //Why did I write this code?
            //if (boundingBox.X == 0 || boundingBox.Y == 0)
            //{
            //    return;
            //}

            frame.EnsureDimensions(boundingBox);

            bool[,] drawable = new bool[boundingBox.X, boundingBox.Y];
            Color transparent = Color.Transparent;

            using (Bitmap drawableSurface = new Bitmap(boundingBox.X, boundingBox.Y, PixelFormat.Format32bppArgb))
            {
                drawableSurface.MakeTransparent(transparent);
                using (Graphics graphics = Graphics.FromImage(drawableSurface))
                {
                    graphics.Clear(transparent);
                    //debug color of green for now
                    SolidBrush brush = new SolidBrush(Color.Green);
                    //graphics.FillRectangle(brush, 0, 0, boundingBox.X, boundingBox.Y);

                    foreach (Chunk chunk in frame.Chunks)
                    {
                        if (chunk.IsPalette())
                        {
                            continue;
                        }

                        if (chunk.IsBackground())
                        {
                            brush.Color = GetColor(chunk.GetColor(0, 0));
                            graphics.FillRectangle(brush, 0, 0, boundingBox.X, boundingBox.Y);
                            continue;
                        }

                        SxzPoint dimensions = chunk.GetDimensions();
                        SxzPoint origin = chunk.Origin;
                        for (int y = origin.Y; y < origin.Y + dimensions.Y; y++)
                        {
                            for (int x = origin.X; x < origin.X + dimensions.X; x++)
                            {
                                SxzColor sxzColor = chunk.GetColor(x, y);

                                if (sxzColor == null)
                                {
                                    //For chunks with bitplanes, this means don't drawn on this pixel even though it is within
                                    //the region of the chunk
                                    continue;
                                }

                                if (chunk.IsTransparent())
                                {
                                    drawableSurface.SetPixel(x, y, transparent);
                                    continue;
                                }

                                Color color = GetColor(sxzColor);
                                if (color == null)
                                {
                                    continue;
                                }

                                drawable[x, y] = true;
                                brush.Color = color;
                                graphics.FillRectangle(brush, x, y, 1, 1);
                            }
                        }
                    }
                }

                Image outputImage = (Image)drawableSurface;
                outputImage.Save(filename, ImageFormat.Png);
            }
        }

        //private static int GetDrawables(bool[,] drawable, SxzPoint origin, SxzPoint dimensions)
        //{
        //    int result = 0;
        //    for (int x = origin.X; x < origin.X + dimensions.X; x++)
        //    {
        //        for (int y = origin.Y; y < origin.Y + dimensions.Y; y++)
        //        {
        //            if (drawable[x, y])
        //            {
        //                result++;
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static void SaveImageData(string filename, Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                byte[] data = ms.ToArray();
                File.WriteAllBytes(filename, data);
            }
        }

        public static SxzColor GetSxzColor(Color color)
        {
            return new SxzColor(color.R, color.G, color.B);
        }

        public static Color GetColor(SxzColor color)
        {
            return Color.FromArgb(color.Red, color.Green, color.Blue);
        }
    }
}
