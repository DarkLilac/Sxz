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
    public class Palette
    {
        public Palette()
        {
            Colors = new HashSet<SxzColor>();
            Distance = 32;
        }

        public double Distance { get; set; }

        public Palette Clone()
        {
            Palette result = new Palette();
            foreach (SxzColor color in this.Colors)
            {
                result.Add(color);
            }

            return result;
        }

        public Palette(DefaultPaletteChunk defaultPaletteChunk)
        {
            Colors = new HashSet<SxzColor>();
            foreach (SxzColor color in defaultPaletteChunk.Colors)
            {
                Colors.Add(color);
            }

            Default = true;
        }

        public bool Default { get; set; }
        internal HashSet<SxzColor> Colors { get; set; }

        public int Count()
        {
            return Colors.Count;
        }

        public void Add(SxzColor color)
        {
            if (Colors.Contains(color))
            {
                throw new Exception("Duplicate color in palette");
            }

            if (Colors.Count >= 256)
            {
                throw new Exception("Palette too large");
            }

            Colors.Add(color);
        }

        public bool Contains(SxzColor color)
        {
            return Colors.Contains(color);
        }

        public bool Add(HashSet<SxzColor> colors)
        {
            //see if they fit first
            foreach (SxzColor color in colors)
            {
                if (Contains(color))
                {
                    continue;
                }

                if (Colors.Count == 256)
                {
                    return false;
                }

                Colors.Add(color);
            }

            //now look at color distances, the nearest distance must be less than 32
            foreach (SxzColor color in Colors)
            {
                if (FindNearestDistance(color) > Distance)
                {
                    return false;
                }
            }

            return true;
        }

        private double FindNearestDistance(SxzColor target)
        {
            double result = double.MaxValue;
            foreach (SxzColor color in Colors)
            {
                if (target.Equals(color))
                {
                    continue;
                }

                double distance = SxzColor.GetColorDistance(target, color);
                if (distance < result)
                {
                    result = distance;
                }
            }

            return result;
        }

        public bool HasRoom(SxzColor color)
        {
            if (Default)
            {
                return false;
            }

            double nearestDistance = double.MaxValue;
            foreach (SxzColor alreadyMember in Colors)
            {
                double distance = SxzColor.GetColorDistance(color, alreadyMember);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                }
            }

            //TODO: Make the distance configurable
            if (nearestDistance > Distance)
            {
                return false;
            }

            return Colors.Count < 256;
        }

        public PaletteChunk GetPaletteChunk()
        {
            if (Default)
            {
                return null;
            }

            PaletteChunk result = new PaletteChunk();

            List<SxzColor> colorList = Colors.ToList();
            colorList.Sort(new PaletteChunk.ColorSorter());

            foreach (SxzColor color in colorList)
            {
                result.Colors.Add(color);
            }

            return result;
        }
    }
}
