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
    /// Holds all the data for all the frames of an image.  Top level outermost container of data.
    /// </summary>
    public class Container
    {
        public Container()
        {
            Frames = new List<Frame>();
        }

        public static string Label { get { return "SXZ"; } }

        public List<Frame> Frames { get; private set; }

        public void EnsureDimensions(SxzPoint boundingBox)
        {
            foreach (Frame frame in Frames)
            {
                frame.EnsureDimensions(boundingBox);
            }
        }

        public Chunk GetSelected(int x, int y)
        {
            for (int i = this.Frames.Count - 1; i >= 0; i--)
            {
                Frame frame = this.Frames[i];

                var boundingBox = new SxzPoint(0, 0);
                frame.EnsureDimensions(boundingBox);
                Chunk chunk = frame.GetSelected(x, y);
                if (chunk != null)
                {
                    return chunk;
                }
            }

            return null;
        }

        public byte[] GetData()
        {
            List<byte> result = new List<byte>();
            byte[] byteLabel = Encoding.ASCII.GetBytes(Label);
            Writer.AddBytes(result, byteLabel);

            foreach (Frame frame in Frames)
            {
                result.AddRange(frame.GetData());
            }

            return result.ToArray();
        }

        public void SetData(byte[] data)
        {
            int index = 3;
            while (index < data.Length)
            {
                string label = Encoding.ASCII.GetString(data, index, 2);
                index += 2;
                if (label.Equals(Frame.Label))
                {
                    int size = BitConverter.ToInt32(data, index);
                    index += 4;
                    byte[] frameData = new byte[size];
                    Array.Copy(data, index, frameData, 0, frameData.Length);
                    Frame frame = new Frame();
                    frame.SetData(frameData);
                    this.Frames.Add(frame);
                    index += size;
                }
            }
        }
    }
}
