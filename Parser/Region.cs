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
    public abstract class Region
    {
        public Region()
        {
            Locations = new HashSet<Location>();
            BoundingBox = new BoundingBox();
        }

        public SxzPoint Origin { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public HashSet<Location> Locations;

        public abstract bool IsValid(Location location);
        public abstract HashSet<ChunkContainer> GetChunks(LocationPool locationPool);
        public abstract HashSet<SxzColor> GetColors();
        //public abstract bool HasRoomInPalette(Palette palette);

        public virtual void Add(Location location)
        {
            Locations.Add(location);
            BoundingBox.Add(location.Point);
        }

        public int Size()
        {
            return Locations.Count;
        }

        public bool Inside(Location location)
        {
            BoundingBox clone = BoundingBox.Clone();
            clone.Add(location.Point);
            return clone.Area() < (Math.Pow(2, 15) - 1);
        }

        /// <summary>
        /// Gets all the locations in this region, within this sub-boundingbox
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public HashSet<Location> GetLocations(BoundingBox boundingBox)
        {
            HashSet<Location> result = new HashSet<Location>();
            foreach (Location location in Locations)
            {
                if (boundingBox.Contains(location.Point.X, location.Point.Y))
                {
                    result.Add(location);
                }
            }

            return result;
        }

        /// <summary>
        /// Every location in the bounding box is in this region
        /// </summary>
        /// <param name="locationPool"></param>
        /// <returns></returns>
        public bool IsRectangle()
        {
            return IsRectangle(Locations, BoundingBox);
        }

        public bool IsRectangle(HashSet<Location> locations, BoundingBox boundingBox)
        {
            bool[,] boolArray = new bool[boundingBox.Width(), boundingBox.Height()];
            foreach (Location location in locations)
            {
                if (boundingBox.Contains(location.Point.X, location.Point.Y))
                {
                    boolArray[location.Point.X - boundingBox.UpperLeft.X, location.Point.Y - boundingBox.UpperLeft.Y] = true;
                }
            }

            for (int x = 0; x < boolArray.GetLength(0); x++)
            {
                for (int y = 0; y < boolArray.GetLength(1); y++)
                {
                    if (!boolArray[x, y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Somewhat optimized rectangle puller
        /// Courtesy http://www.drdobbs.com/database/the-maximal-rectangle-problem/184410529
        /// </summary>
        /// <returns></returns>
        public HashSet<BoundingBox> GetAllRectangles(HashSet<Location> locations, BoundingBox boundingBox)
        {
            //Console.WriteLine("Entering GetAllRectangles");
            DateTime start = DateTime.Now;
            HashSet<BoundingBox> result = new HashSet<BoundingBox>();
            while (locations.Count > 0)
            {
                bool[,] boolArray = new bool[boundingBox.LowerRight.X + 1, boundingBox.LowerRight.Y + 1];
                foreach (Location location in locations)
                {
                    boolArray[location.Point.X, location.Point.Y] = true;
                }

                BoundingBox best = new BoundingBox();
                for (int upperLeftX = BoundingBox.UpperLeft.X; upperLeftX < boundingBox.LowerRight.X + 1; upperLeftX++)
                {
                    for (int upperLeftY = BoundingBox.UpperLeft.Y; upperLeftY < boundingBox.LowerRight.Y + 1; upperLeftY++)
                    {
                        if (!boolArray[upperLeftX, upperLeftY])
                        {
                            continue;
                        }

                        BoundingBox candidate = GrowOnes(new SxzPoint(upperLeftX, upperLeftY), boundingBox, boolArray);
                        if (candidate.Area() > best.Area())
                        {
                            best = candidate;
                        }
                    }
                }

                if (best.UpperLeft.X == -1)
                {
                    break;
                }

                result.Add(best);
                //Console.WriteLine("Have total rectangles " + result.Count);
                //Console.WriteLine("Have remaining locations " + locations.Count);
                foreach (Location location in new HashSet<Location>(locations))
                {
                    if (best.Contains(location.Point.X, location.Point.Y))
                    {
                        locations.Remove(location);
                    }
                }
            }

            //Console.WriteLine("Leaving GetAllRectangles with time " + (DateTime.Now - start).Milliseconds + " ms");
            return result;
        }

        private static BoundingBox GrowOnes(SxzPoint upperLeft, BoundingBox boundingBox, bool[,] boolArray)
        {
            SxzPoint lowerRight = new SxzPoint(upperLeft.X, upperLeft.Y);
            BoundingBox best = new BoundingBox();
            best.UpperLeft = upperLeft;
            best.LowerRight = upperLeft;
            int maxX = boundingBox.LowerRight.X;
            int y = upperLeft.Y - 1;

            while ((y + 1) < (boundingBox.LowerRight.Y + 1) && boolArray[upperLeft.X, y + 1])
            {
                y++;
                int x = upperLeft.X;
                while ((x + 1) <= maxX && boolArray[x + 1, y])
                {
                    x++;
                }

                maxX = x;
                BoundingBox candidate = new BoundingBox();
                candidate.UpperLeft = upperLeft;
                candidate.LowerRight.X = x;
                candidate.LowerRight.Y = y;
                if (candidate.Area() > best.Area())
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}
