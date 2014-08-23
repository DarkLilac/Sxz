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
    public class PaletteContainer
    {
        public PaletteContainer()
        {
            ChunkContainers = new HashSet<ChunkContainer>();
            Initial = false;
        }

        public Palette Palette { get; set; }
        public HashSet<ChunkContainer> ChunkContainers { get; set; }
        public bool Initial { get; set; }

        public bool Contains(SxzColor color)
        {
            return Palette.Contains(color);
        }

        public bool Contains(HashSet<SxzColor> colors)
        {
            foreach (SxzColor color in colors)
            {
                if (!Palette.Contains(color))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasRoom(SxzColor color)
        {
            return Palette.HasRoom(color);
        }

        /// <summary>
        /// This is wrong currently.  Need to test if the colors will fit if they are all added to the palette...
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        public bool HasRoom(HashSet<SxzColor> colors)
        {
            Palette testPalette = Palette.Clone();
            return testPalette.Add(colors);
        }

        public void Add(ChunkContainer chunkContainer)
        {
            ChunkContainers.Add(chunkContainer);
        }

        public void AddColors(HashSet<SxzColor> colors)
        {
            foreach (SxzColor color in colors)
            {
                if (!Palette.Contains(color))
                {
                    Palette.Add(color);
                }
            }
        }
    }
}
