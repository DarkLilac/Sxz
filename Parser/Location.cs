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
using Library;

namespace Parser
{
    public class Location
    {
        public Location()
        {
            Point = new SxzPoint();
        }

        public Location(int x, int y)
        {
            Point = new SxzPoint(x, y);
        }

        public SxzPoint Point { get; set; }

        public SxzColor Color { get; set; }
        public Region Region { get; set; }

        public bool Marked { get; set; }

        public Location Up { get; set; }
        public Location Down { get; set; }
        public Location Left { get; set; }
        public Location Right { get; set; }
        public Location UpperLeft { get; set; }
        public Location UpperRight { get; set; }
        public Location LowerLeft { get; set; }
        public Location LowerRight { get; set; }

        public override string ToString()
        {
            return Point.X + " " + Point.Y + " " + Color.ToString();
        }

        public Location GetLocalMaxima()
        {
            int total = Color.Total();

            if (Up != null && !Up.Marked && Up.Color.Total() > total)
            {
                return Up;
            }

            if (UpperLeft != null && !UpperLeft.Marked && UpperLeft.Color.Total() > total)
            {
                return UpperLeft;
            }

            if (UpperRight != null && !UpperRight.Marked && UpperRight.Color.Total() > total)
            {
                return UpperRight;
            }

            if (LowerLeft != null && !LowerLeft.Marked && LowerLeft.Color.Total() > total)
            {
                return LowerLeft;
            }

            if (LowerRight != null && !LowerRight.Marked && LowerRight.Color.Total() > total)
            {
                return LowerRight;
            }

            if (Down != null && !Down.Marked && Down.Color.Total() > total)
            {
                return Down;
            }

            if (Left != null && !Left.Marked && Left.Color.Total() > total)
            {
                return Left;
            }

            if (Right != null && !Right.Marked && Right.Color.Total() > total)
            {
                return Right;
            }

            return null;
        }
    }
}
