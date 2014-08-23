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
    /// <summary>
    /// Region with depth of zero, all one color
    /// </summary>
    public class MonoRegion : Region
    {
        public MonoRegion() : base()
        {
        }

        public override bool IsValid(Location location)
        {
            return location.Color.Equals(Color);
        }

        public SxzColor Color { get; set; }

        public override HashSet<SxzColor> GetColors()
        {
            HashSet<SxzColor> result = new HashSet<SxzColor>();
            result.Add(Color);
            return result;
        }

        public override void Add(Location location)
        {
            if (Color == null)
            {
                Color = location.Color;
            }

            base.Add(location);
        }

        /// <summary>
        /// To return a RectangleChunk we will need the bounding box and a check for missing locations
        /// </summary>
        /// <returns></returns>
        public override HashSet<ChunkContainer> GetChunks(LocationPool locationPool)
        {
            HashSet<ChunkContainer> result = new HashSet<ChunkContainer>();

            if (IsRectangle())
            {
                MonoRectangleChunk rectangleChunk = new MonoRectangleChunk();
                rectangleChunk.Origin = BoundingBox.UpperLeft;
                rectangleChunk.Width = BoundingBox.Width();
                rectangleChunk.Height = BoundingBox.Height();
                MonoRectangleChunkContainer container = new MonoRectangleChunkContainer();
                container.Chunk = rectangleChunk;
                container.Locations = GetLocations(BoundingBox);
                //container.AdditionalSize = NewColorCount(locationPool.Palette);
                container.Color = Color;
                result.Add(container);
                return result;
            }

            //break up into many rectangles and return rest here
            HashSet<Location> locations = new HashSet<Location>(Locations);
            HashSet<BoundingBox> rectangles = GetAllRectangles(locations, BoundingBox);

            foreach (BoundingBox rectangle in rectangles)
            {
                MonoRectangleChunk rectangleChunk = new MonoRectangleChunk();
                rectangleChunk.Origin = rectangle.UpperLeft;
                rectangleChunk.Width = rectangle.Width();
                rectangleChunk.Height = rectangle.Height();
                MonoRectangleChunkContainer container = new MonoRectangleChunkContainer();
                container.Chunk = rectangleChunk;
                container.Color = Color;
                //container.AdditionalSize = NewColorCount(locationPool.Palette);
                container.Locations = GetLocations(rectangle);
                result.Add(container);
            }

            //now for the remaining locations, create monocolorchunks
            BoundingBox remaining = new BoundingBox();
            foreach (Location location in locations)
            {
                remaining.Add(location.Point);
            }

            MonoBitPlaneChunk monoColorChunk = new MonoBitPlaneChunk();
            monoColorChunk.Width = remaining.Width();
            monoColorChunk.Height = remaining.Height();
            monoColorChunk.Initialize();
            monoColorChunk.Origin = remaining.UpperLeft;

            foreach (Location location in locations)
            {
                monoColorChunk.SetColor(location.Point.X, location.Point.Y);
            }

            MonoBitPlaneChunkContainer monoContainer = new MonoBitPlaneChunkContainer();
            monoContainer.Chunk = monoColorChunk;
            monoContainer.Locations = GetLocations(remaining);
            monoContainer.Color = Color;
            result.Add(monoContainer);

            return result;
        }
    }
}
