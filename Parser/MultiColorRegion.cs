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
using System.Linq;
using Library;

namespace Parser
{
    public class MultiColorRegion : Region
    {
        public MultiColorRegion(double maximumDistance)
        {
            Palette = new Palette();
            Palette.Distance = maximumDistance;
        }

        public Palette Palette { get; set; }

        public override bool IsValid(Location location)
        {
            //is there room?
            if (!Palette.HasRoom(location.Color))
            {
                return false;
            }

            if (!base.Inside(location))
            {
                return false;
            }

            return true;
        }

        public override HashSet<SxzColor> GetColors()
        {
            HashSet<SxzColor> result = new HashSet<SxzColor>();
            foreach (SxzColor color in Palette.Colors)
            {
                result.Add(color);
            }

            return result;
        }

        public override void Add(Location location)
        {
            base.Add(location);

            if (!Palette.Contains(location.Color))
            {
                Palette.Add(location.Color);
            }
        }

        public override HashSet<ChunkContainer> GetChunks(LocationPool locationPool)
        {
            HashSet<ChunkContainer> result = new HashSet<ChunkContainer>();

            if (Locations.Count == 0)
            {
                return result;
            }

            if (Palette.Count() == 1)
            {
                if (IsRectangle())
                {
                    MonoRectangleChunk rectangleChunk = new MonoRectangleChunk();
                    rectangleChunk.Origin = BoundingBox.UpperLeft;
                    rectangleChunk.Width = BoundingBox.Width();
                    rectangleChunk.Height = BoundingBox.Height();
                    MonoRectangleChunkContainer container = new MonoRectangleChunkContainer();
                    container.Chunk = rectangleChunk;
                    container.Locations = GetLocations(BoundingBox);
                    container.Color = Palette.Colors.First();
                    result.Add(container);
                    return result;
                }

                //output a monochunk?
                MonoBitPlaneChunk monoColorChunk = new MonoBitPlaneChunk();
                monoColorChunk.Width = BoundingBox.Width();
                monoColorChunk.Height = BoundingBox.Height();
                monoColorChunk.Initialize();
                monoColorChunk.Origin = BoundingBox.UpperLeft;

                foreach (Location location in Locations)
                {
                    monoColorChunk.SetColor(location.Point.X, location.Point.Y);
                }

                MonoBitPlaneChunkContainer monoContainer = new MonoBitPlaneChunkContainer();
                monoContainer.Chunk = monoColorChunk;
                monoContainer.Locations = Locations;
                monoContainer.Color = Palette.Colors.First();
                result.Add(monoContainer);

                return result;

            }

            if (IsRectangle())
            {
                MultiColorRectangleChunkContainer container = new MultiColorRectangleChunkContainer();
                ColorRectangleChunk rectangleChunk = new ColorRectangleChunk();
                rectangleChunk.Origin = BoundingBox.UpperLeft;
                rectangleChunk.Width = BoundingBox.Width();
                rectangleChunk.Height = BoundingBox.Height();
                //rectangleChunk.Direction = Direction.Left;
                rectangleChunk.Initialize();
                container.Chunk = rectangleChunk;
                container.Locations = Locations;
                rectangleChunk.Palette = Palette.GetPaletteChunk();

                foreach (Location location in Locations)
                {
                    rectangleChunk.SetColor(location.Color, location.Point.X, location.Point.Y);
                }

                result.Add(container);
                return result;
            }

            //Don't break it into further rectangles right now... just output a bitplane
            MultiColorBitPlaneChunkContainer chunkContainer = new MultiColorBitPlaneChunkContainer();
            ColorBitPlaneChunk colorBitPlaneChunk = new ColorBitPlaneChunk();
            colorBitPlaneChunk.Origin = BoundingBox.UpperLeft;
            colorBitPlaneChunk.Height = BoundingBox.Height();
            colorBitPlaneChunk.Width = BoundingBox.Width();
            colorBitPlaneChunk.Palette = Palette.GetPaletteChunk();
            colorBitPlaneChunk.Initialize();
            chunkContainer.Chunk = colorBitPlaneChunk;
            chunkContainer.Locations = Locations;

            foreach (Location location in Locations)
            {
                //bitmask must use local coordinates
                colorBitPlaneChunk.SetColor(location.Color, location.Point.X, location.Point.Y);
            }

            result.Add(chunkContainer);

            return result;
        }
    }
}
