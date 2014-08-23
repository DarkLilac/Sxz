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

namespace Library
{
    public class SxzColor
    {
        public SxzColor()
        {
        }

        public SxzColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public int Total()
        {
            return Red + Green + Blue;
        }

        public static SxzColor Hex(string hexString)
        {
            return new SxzColor(byte.Parse(hexString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hexString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hexString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }

        public override bool Equals(Object obj)
        {
            SxzColor otherColor = obj as SxzColor;
            return this.Red == otherColor.Red && this.Green == otherColor.Green && this.Blue == otherColor.Blue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Red.GetHashCode();
                hash = hash * 29 + Green.GetHashCode();
                hash = hash * 31 + Blue.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return Red + " " + Green + " " + Blue;
        }

        public static double GetColorDistance(SxzColor source, SxzColor target) {
		    if (source.Equals(target)) {
			    return 0.0d;
		    }

		    double red = source.Red - target.Red;
		    double green = source.Green - target.Green;
		    double blue = source.Blue - target.Blue;
		    return Math.Sqrt(red * red + blue * blue + green * green);
	    }
    }
}
