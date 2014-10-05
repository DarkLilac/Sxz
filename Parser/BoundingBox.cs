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
    public class BoundingBox
    {
        public BoundingBox()
        {
            UpperLeft = new SxzPoint(-1, -1);
            LowerRight = new SxzPoint(-1, -1);
        }

        public BoundingBox(BoundingBox input)
        {
            UpperLeft = new SxzPoint(input.UpperLeft.X, input.UpperLeft.Y);
            LowerRight = new SxzPoint(input.LowerRight.X, input.LowerRight.Y);
        }

        public SxzPoint UpperLeft { get; set; }
        public SxzPoint LowerRight { get; set; }

        public int Width()
        {
            return LowerRight.X - UpperLeft.X + 1;
        }

        public int Height()
        {
            return LowerRight.Y - UpperLeft.Y + 1;
        }

        public int Area()
        {
            if (UpperLeft.X < 0)
            {
                return 0;
            }

            if (UpperLeft.X > LowerRight.X || UpperLeft.Y > LowerRight.Y)
            {
                return 0;
            }

            return Width() * Height();
        }

        public bool Contains(int x, int y)
        {
            if (x < UpperLeft.X)
            {
                return false;
            }

            if (x > LowerRight.X)
            {
                return false;
            }

            if (y < UpperLeft.Y)
            {
                return false;
            }

            if (y > LowerRight.Y)
            {
                return false;
            }

            return true;
        }

        public void Add(SxzPoint point)
        {
            if (UpperLeft.X == -1)
            {
                UpperLeft.X = point.X;
            }

            if (LowerRight.X == -1)
            {
                LowerRight.X = point.X;
            }

            if (UpperLeft.Y == -1)
            {
                UpperLeft.Y = point.Y;
            }

            if (LowerRight.Y == -1)
            {
                LowerRight.Y = point.Y;
            }

            if (point.X < UpperLeft.X)
            {
                UpperLeft.X = point.X;
            }

            if (point.X > LowerRight.X)
            {
                LowerRight.X = point.X;
            }

            if (point.Y < UpperLeft.Y)
            {
                UpperLeft.Y = point.Y;
            }

            if (point.Y > LowerRight.Y)
            {
                LowerRight.Y = point.Y;
            }
        }

        public override string ToString()
        {
            return UpperLeft.X + ", " + UpperLeft.Y + " " + LowerRight.X + ", " + LowerRight.Y;
        }

        public BoundingBox Clone()
        {
            BoundingBox result = new BoundingBox();
            result.UpperLeft.X = this.UpperLeft.X;
            result.UpperLeft.Y = this.UpperLeft.Y;
            result.LowerRight.X = this.LowerRight.X;
            result.LowerRight.Y = this.LowerRight.Y;
            return result;
        }
    }
}
