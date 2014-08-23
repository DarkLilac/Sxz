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
    public class LocationPool
    {
        private Location[,] locations;

        public LocationPool(int width, int height)
        {
            Unmarked = new HashSet<Location>();
            locations = new Location[width, height];
        }

        public SxzColor BackgroundColor { get; set; }
        public HashSet<Location> Unmarked;

        public void SetLocation(Location location, int x, int y)
        {
            locations[x, y] = location;
            Unmarked.Add(location);
        }

        public Location GetLocation(int x, int y)
        {
            return locations[x, y];
        }

        public int Width()
        {
            return locations.GetLength(0);
        }

        public int Height()
        {
            return locations.GetLength(1);
        }

        /// <summary>
        /// Harder than it looks, since the region extrapolation is reductive, we need to start with the whitest pixel
        /// </summary>
        /// <returns></returns>
        public Location RandomLocation()
        {
            Location start = Unmarked.ElementAt(0);
            Location next = start;
            while ((next = start.GetLocalMaxima()) != null)
            {
                start = next;
            }

            return start;
        }

        public bool Empty()
        {
            return Unmarked.Count == 0;
        }

        public void SetMarked(Location location)
        {
		    if (location.Marked) {
			    throw new Exception("Location is already marked");
		    }

		    location.Marked = true;
            Unmarked.Remove(location);
	    }

        public void Unmark(Location location)
        {
            location.Marked = false;
            Unmarked.Add(location);
        }

        public HashSet<Location> GetUnmarkedByBoundingBox(BoundingBox boundingBox)
        {
            HashSet<Location> result = new HashSet<Location>();

            foreach (Location location in Unmarked)
            {
                if (boundingBox.Contains(location.Point.X, location.Point.Y))
                {
                    result.Add(location);
                }
            }

            return result;
        }

        public HashSet<Location> GetByColor(SxzColor color)
        {
            HashSet<Location> result = new HashSet<Location>();
            int width = locations.GetLength(0);
            int height = locations.GetLength(1);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Location location = locations[j, i];
                    if (location == null)
                    {
                        continue;
                    }

                    if (!location.Marked && location.Color.Equals(color))
                    {
                        result.Add(location);
                    }
                }
            }

            return result;
        }

        public void SetMarked(SxzColor color)
        {
            int width = locations.GetLength(0);
            int height = locations.GetLength(1);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Location location = locations[j, i];
                    if (location == null)
                    {
                        continue;
                    }

                    if (location.Color.Equals(color))
                    {
                        location.Marked = true;
                        Unmarked.Remove(location);
                    }
                }
            }
        }

        public void SetNeighbors()
        {
            int width = locations.GetLength(0);
            int height = locations.GetLength(1);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Location location = locations[j, i];
                    if (location == null)
                    {
                        continue;
                    }

                    if (j > 0)
                    {
                        location.Left = locations[j - 1, i];
                        if (i > 0)
                        {
                            location.UpperLeft = locations[j - 1, i - 1];
                        }
                        if (i < height - 1)
                        {
                            location.LowerLeft = locations[j - 1, i + 1];
                        }
                    }
                    if (j < (width - 1))
                    {
                        location.Right = locations[j + 1, i];
                        if (i > 0)
                        {
                            location.UpperRight = locations[j + 1, i - 1];
                        }
                        if (i < height - 1)
                        {
                            location.LowerRight = locations[j + 1, i + 1];
                        }
                    }
                    if (i > 0)
                    {
                        location.Up = locations[j, i - 1];
                    }
                    if (i < (height - 1))
                    {
                        location.Down = locations[j, i + 1];
                    }
                }
            }
        }
    }
}
