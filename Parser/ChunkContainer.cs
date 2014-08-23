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
    public abstract class ChunkContainer
    {
        public ChunkContainer()
        {
            Locations = new HashSet<Location>();
            AdditionalSize = 0;
        }

        public abstract Chunk Chunk { get; set; }
        public abstract void SetIndex(PaletteChunk paletteChunk);

        /// <summary>
        /// Additional size is for overhead of putting bytes into the palette
        /// </summary>
        public int AdditionalSize { get; set; }
        public HashSet<Location> Locations { get; set; }

        /// <summary>
        /// Size here is the number of uncompressed bytes of the data structure
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return Chunk.GetData().Count + AdditionalSize;
        }

        /// <summary>
        /// Validity is measurement of bytes per pixel.
        /// </summary>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public bool IsValid(double multiplier)
        {
            return (Size() * 8) < (Locations.Count * multiplier);
        }
    }
}
