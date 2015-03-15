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
using Library;

namespace Parser
{
    public class MaskRegion : Region
    {
        public MaskRegion()
            : base()
        {
        }

        public override HashSet<SxzColor> GetColors()
        {
            HashSet<SxzColor> result = new HashSet<SxzColor>();
            return result;
        }

        public override bool IsValid(Location location)
        {
            return true;
        }

        public override void Add(Location location)
        {
            base.Add(location);
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public BoundingBox GetBoundingBox()
        {
            BoundingBox boundingBox = new BoundingBox();
            foreach (Location location in Locations)
            {
                boundingBox.Add(location.Point);
            }

            return boundingBox;
        }

        /// <summary>
        /// To return a RectangleChunk we will need the bounding box and a check for missing locations
        /// </summary>
        /// <returns></returns>
        public override HashSet<ChunkContainer> GetChunks(LocationPool locationPool)
        {
            //BoundingBox boundingBox = new BoundingBox();
            //foreach (Location location in Locations)
            //{
            //    boundingBox.Add(location.Point);
            //}

            BlackWhiteBitPlaneChunk blackWhiteBitPlaneChunk = new BlackWhiteBitPlaneChunk();
            //blackWhiteBitPlaneChunk.Width = boundingBox.Width();
            //blackWhiteBitPlaneChunk.Height = boundingBox.Height();
            blackWhiteBitPlaneChunk.Width = Width;
            blackWhiteBitPlaneChunk.Height = Height;
            blackWhiteBitPlaneChunk.Initialize();
            //blackWhiteBitPlaneChunk.Origin = boundingBox.UpperLeft;
            blackWhiteBitPlaneChunk.Origin = Origin;

            foreach (Location location in Locations)
            {
                blackWhiteBitPlaneChunk.SetColor(location.Point.X, location.Point.Y);
            }

            BlackWhiteChunkContainer blackWhiteChunkContainer = new BlackWhiteChunkContainer();
            blackWhiteChunkContainer.Chunk = blackWhiteBitPlaneChunk;
            blackWhiteChunkContainer.Locations = Locations;

            HashSet<ChunkContainer> result = new HashSet<ChunkContainer>();
            result.Add(blackWhiteChunkContainer);
            return result;
        }
    }
}
