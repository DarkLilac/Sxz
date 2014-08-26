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
    public class TransparentRegion : Region
    {
        public TransparentRegion()
            : base()
        {
        }

        public override bool IsValid(Location location)
        {
            return true;
        }

        public override HashSet<SxzColor> GetColors()
        {
            HashSet<SxzColor> result = new HashSet<SxzColor>();
            return result;
        }

        public override HashSet<ChunkContainer> GetChunks(LocationPool locationPool)
        {
            HashSet<ChunkContainer> result = new HashSet<ChunkContainer>();

            if (Locations.Count == 0)
            {
                return result;
            }

            if (IsRectangle())
            {
                TransparentRectangleChunk rectangleChunk = new TransparentRectangleChunk();
                rectangleChunk.Origin = BoundingBox.UpperLeft;
                rectangleChunk.Width = BoundingBox.Width();
                rectangleChunk.Height = BoundingBox.Height();
                TransparentRectangleChunkContainer container = new TransparentRectangleChunkContainer();
                container.Chunk = rectangleChunk;
                container.Locations = Locations;
                result.Add(container);
                return result;
            }

            TransparentBitPlaneChunk transparentChunk = new TransparentBitPlaneChunk();
            transparentChunk.Origin = BoundingBox.UpperLeft;
            transparentChunk.Width = BoundingBox.Width();
            transparentChunk.Height = BoundingBox.Height();
            transparentChunk.Initialize();
            TransparentBitPlaneChunkContainer transparentChunkContainer = new TransparentBitPlaneChunkContainer();
            transparentChunkContainer.Chunk = transparentChunk;
            transparentChunkContainer.Locations = Locations;
            foreach (Location location in Locations)
            {
                transparentChunk.SetColor(location.Point.X, location.Point.Y);
            }

            result.Add(transparentChunkContainer);
            return result;
        }
    }
}
