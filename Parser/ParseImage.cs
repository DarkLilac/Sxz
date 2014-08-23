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
using Parser;

namespace Parser
{
    public enum RegionType { MonoRegion, MultiColorRegion };

    //0, 1, 2
    public enum BackgroundType { None, Background, Rectangle };

    public class ParseImage
    {
        public ParseImage()
        {
        }

        private static SxzColor Black = new SxzColor(0, 0, 0);

        public Container Parse(string filename, SxzColor backgroundColor, bool useMostCommonColor, bool parseBackground, BackgroundType backgroundType, double bitsPerPixel, double maximumDistance)
        {
            int width = 0;
            int height = 0;

            //now we can act on our pixel byte array to get pixels out
            //and also histogram because it is informative if nothing else
            LocationPool locationPool = null;

            Dictionary<SxzColor, int> histogram = new Dictionary<SxzColor, int>();

            TransparentRegion transparentRegion = new TransparentRegion();

            //Parse all the pixels into the locationpool, histogram and remove the fully transparent pixels into the transparentregion
            using (System.Drawing.Bitmap bitMap = new System.Drawing.Bitmap(filename))
            {
                width = bitMap.Width;
                height = bitMap.Height;
                locationPool = new LocationPool(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        System.Drawing.Color pixel = bitMap.GetPixel(x, y);

                        Location location = new Location();
                        location.Color = new SxzColor(pixel.R, pixel.G, pixel.B);
                        location.Point.X = x;
                        location.Point.Y = y;

                        //Don't put transparent colors into the histogram
                        if (pixel.A == 0)
                        {
                            transparentRegion.Add(location);
                            locationPool.SetLocation(location, x, y);
                            locationPool.SetMarked(location);
                            continue;
                        }

                        if (histogram.ContainsKey(location.Color))
                        {
                            int count = histogram[location.Color];
                            histogram[location.Color] = count + 1;
                        }
                        else
                        {
                            histogram.Add(location.Color, 1);
                        }

                        locationPool.SetLocation(location, x, y);
                    }
                }
            }

            Console.WriteLine("Total pixel count " + (width * height));
            Console.WriteLine("Total transparent pixel count " + transparentRegion.Locations.Count);

            //Remove all the transparent locations
            //foreach (Location location in transparentRegion.Locations)
            //{
            //    locationPool.SetMarked(location);
            //}

            //set the neighbors after removing transparent pixels to save time since those aren't needed anymore
            locationPool.SetNeighbors();

            Console.WriteLine("Done removing transparent pixels");
            //Sort the colors by frequency
            List<KeyValuePair<SxzColor, int>> colorList = histogram.ToList();
            colorList.Sort((first, second) =>
            {
                return first.Value == second.Value ? (int)(SxzColor.GetColorDistance(second.Key, new SxzColor(0, 0, 0)) - SxzColor.GetColorDistance(first.Key, new SxzColor(0, 0, 0)))
                        : second.Value.CompareTo(first.Value);
            });

            //Find the most commonly used color
            SxzColor mostCommonColor = colorList[0].Key;
            if (useMostCommonColor)
            {
                backgroundColor = mostCommonColor;
            }

            //always start with a palette, register empty palette and fill as we go

            Console.WriteLine("Processing the most common color");

            DefaultPaletteChunk defaultPaletteChunk = new DefaultPaletteChunk();
            Palette defaultPalette = new Palette(defaultPaletteChunk);

            //Initialization overhead
            List<PaletteContainer> paletteContainers = new List<PaletteContainer>();

            PaletteContainer initialPaletteContainer = new PaletteContainer();
            initialPaletteContainer.Initial = true;
            initialPaletteContainer.Palette = defaultPalette;
            paletteContainers.Add(initialPaletteContainer);

            HashSet<Location> locations = locationPool.GetByColor(backgroundColor);
            if (parseBackground && locations.Count > 0)
            {
                
                if (backgroundType == BackgroundType.Background)
                {
                    Console.WriteLine("Creating background chunk");
                    BackgroundChunk backgroundChunk = new BackgroundChunk();
                    BackgroundChunkContainer backgroundChunkContainer = new BackgroundChunkContainer();

                    backgroundChunkContainer.Chunk = backgroundChunk;
                    backgroundChunkContainer.Color = backgroundColor;

                    //pull out all the pixels with the most common color and throw them into a backgroundchunk
                    if (!initialPaletteContainer.Contains(backgroundColor))
                    {
                        Console.WriteLine("Background chunk in it's own palette");
                        //not in default palette so create a new palette
                        Palette palette = new Palette();
                        palette.Add(backgroundColor);
                        PaletteContainer paletteContainer = new PaletteContainer();
                        paletteContainer.Palette = palette;
                        paletteContainers.Add(paletteContainer);
                        paletteContainer.Add(backgroundChunkContainer);
                        backgroundChunk.Palette = palette.GetPaletteChunk();
                    }
                    else
                    {
                        backgroundChunk.Palette = defaultPaletteChunk;
                        initialPaletteContainer.Add(backgroundChunkContainer);
                    }

                    //AddChunk(paletteContainers, backgroundContainer, mostCommonColor);
                    backgroundChunkContainer.Locations = locations;
                }
                else if (backgroundType == BackgroundType.Rectangle)
                {
                    MonoRectangleChunk rectangleChunk = new MonoRectangleChunk();
                    rectangleChunk.Origin = new SxzPoint(0, 0);
                    rectangleChunk.Width = width;
                    rectangleChunk.Height = height;
                    MonoRectangleChunkContainer rectangleContainer = new MonoRectangleChunkContainer();
                    rectangleContainer.Color = backgroundColor;
                    rectangleContainer.Chunk = rectangleChunk;

                    if (!initialPaletteContainer.Contains(backgroundColor))
                    {
                        Console.WriteLine("Background chunk in it's own palette");
                        //not in default palette so create a new palette
                        Palette palette = new Palette();
                        palette.Add(backgroundColor);
                        PaletteContainer paletteContainer = new PaletteContainer();
                        paletteContainer.Palette = palette;
                        paletteContainers.Add(paletteContainer);
                        paletteContainer.Add(rectangleContainer);
                        rectangleChunk.Palette = palette.GetPaletteChunk();
                    }
                    else
                    {
                        rectangleChunk.Palette = defaultPaletteChunk;
                        initialPaletteContainer.Add(rectangleContainer);
                    }

                    //AddChunk(paletteContainers, backgroundContainer, mostCommonColor);
                    rectangleContainer.Locations = locations;
                }

                //Remove the pixels with the background color from the location pool
                foreach (Location location in locations)
                {
                    locationPool.SetMarked(location);
                }
            }

            Console.WriteLine("Done processing the most common color");

            Console.WriteLine("Parsing monocolor regions");
            //Pull out all regions with the same color in mono regions that were not pulled by the most common color if any
            HashSet<Region> monoRegions = Process(locationPool, maximumDistance, RegionType.MonoRegion);
            Console.WriteLine("Have mono region count " + monoRegions.Count);
            foreach (Region region in monoRegions)
            {
                HashSet<ChunkContainer> potentialContainers = region.GetChunks(locationPool);
                PaletteContainer paletteContainer = GetPaletteContainer(paletteContainers, region.GetColors());

                foreach (ChunkContainer chunkContainer in potentialContainers)
                {
                    //filter out chunks that are not storage sufficient, putting the unused locations
                    //back into the location pool
                    //...
                    if (!chunkContainer.IsValid(bitsPerPixel))
                    {
                        PrintTossedChunk(chunkContainer);
                        foreach (Location location in chunkContainer.Locations)
                        {
                            locationPool.Unmark(location);
                        }

                        continue;
                    }

                    if (paletteContainer == null)
                    {
                        paletteContainer = new PaletteContainer();
                        paletteContainer.Palette = new Palette();
                        paletteContainers.Add(paletteContainer);
                    }

                    paletteContainer.Add(chunkContainer);
                    paletteContainer.AddColors(region.GetColors());
                }
            }

            Console.WriteLine("Done processing monocolor regions");

            Console.WriteLine("Parsing multicolor regions");
            HashSet<Region> colorRegions = Process(locationPool, maximumDistance, RegionType.MultiColorRegion);
            Console.WriteLine("Have multi color region count " + colorRegions.Count);
            foreach (Region region in colorRegions)
            {
                HashSet<ChunkContainer> potentialContainers = region.GetChunks(locationPool);
                PaletteContainer paletteContainer = GetPaletteContainer(paletteContainers, region.GetColors());
                if (paletteContainer == null)
                {
                    paletteContainer = new PaletteContainer();
                    paletteContainer.Palette = new Palette();
                    paletteContainers.Add(paletteContainer);
                }

                foreach (ChunkContainer chunkContainer in potentialContainers)
                {
                    paletteContainer.Add(chunkContainer);
                    paletteContainer.AddColors(region.GetColors());
                }
            }

            Console.WriteLine("Done processing multicolor regions");
            Console.WriteLine("Histogram count " + colorList.Count);
            Console.WriteLine("Majority color " + mostCommonColor + " with count " + colorList[0].Value);
            //alrighty, let's get some output going
            Container container = new Container();
            Frame frame = new Frame();
            container.Frames.Add(frame);

            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                PaletteChunk paletteChunk = paletteContainer.Palette.GetPaletteChunk();
                if (paletteChunk == null)
                {
                    paletteChunk = defaultPaletteChunk;
                }

                if (!paletteContainer.Initial)
                {
                    paletteChunk.Colors.Sort(new PaletteChunk.ColorSorter());
                    frame.Chunks.Add(paletteChunk);
                }

                foreach (ChunkContainer chunkContainer in paletteContainer.ChunkContainers)
                {
                    int count = chunkContainer.Chunk.GetData().Count;
                    PrintKeptChunk(chunkContainer);
                    chunkContainer.SetIndex(paletteChunk);
                    chunkContainer.Chunk.Palette = paletteChunk;

                    frame.Chunks.Add(chunkContainer.Chunk);
                }
            }

            return container;
        }

        private static void PrintTossedChunk(ChunkContainer chunkContainer)
        {
            int count = chunkContainer.Size();
            Console.WriteLine("Tossing " + chunkContainer.Chunk.GetType().Name.ToString() + " with size " + count + " for pixel count " + chunkContainer.Locations.Count + " ratio " + ((float)count / (float)chunkContainer.Locations.Count));
        }

        private static void PrintKeptChunk(ChunkContainer chunkContainer)
        {
            int count = chunkContainer.Size();
            double ratio = 0.0;
            if (chunkContainer.Locations.Count != 0)
            {
                ratio = ((float)count / (float)chunkContainer.Locations.Count);
            }
            Console.WriteLine("Keeping " + chunkContainer.Chunk.GetType().Name.ToString() + " with size " + count + " for pixel count " + chunkContainer.Locations.Count + " ratio " + ratio);
        }

        //For background chunk
        public static void AddChunk(List<PaletteContainer> paletteContainers, ChunkContainer chunkContainer, SxzColor color)
        {
            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.Contains(color))
                {
                    paletteContainer.Add(chunkContainer);
                    return;
                }
            }

            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.HasRoom(color))
                {
                    paletteContainer.Add(chunkContainer);
                    return;
                }
            }
        }

        public static void AddChunk(List<PaletteContainer> paletteContainers, ChunkContainer chunkContainer, HashSet<SxzColor> colors)
        {
            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.Contains(colors))
                {
                    paletteContainer.Add(chunkContainer);
                    return;
                }
            }

            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.HasRoom(colors))
                {
                    paletteContainer.Add(chunkContainer);
                    return;
                }
            }
        }

        public static bool IsInPaletteList(List<PaletteContainer> paletteContainers, HashSet<SxzColor> colors)
        {
            foreach (SxzColor color in colors)
            {
                if (!IsInPalette(paletteContainers, color))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsInPalette(List<PaletteContainer> paletteContainers, SxzColor color)
        {
            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.Contains(color))
                {
                    return true;
                }
            }

            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.HasRoom(color))
                {
                    return true;
                }
            }

            return false;
        }

        public static PaletteContainer GetPaletteContainer(List<PaletteContainer> paletteContainers, HashSet<SxzColor> colors)
        {
            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.Contains(colors))
                {
                    return paletteContainer;
                }
            }

            foreach (PaletteContainer paletteContainer in paletteContainers)
            {
                if (paletteContainer.HasRoom(colors))
                {
                    return paletteContainer;
                }
            }

            return null;
        }

        /// <summary>
        /// Region growing to encode the source image data.
        /// </summary>
        /// <param name="locationPool"></param>
        /// <param name="maximumDistance"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static HashSet<Region> Process(LocationPool locationPool, double maximumDistance, RegionType type)
        {
            HashSet<Region> result = new HashSet<Region>();
            List<Location> todoList = new List<Location>();
            //let's process pixels into regions
            Region region = null;

            HashSet<Location> alreadyChecked = new HashSet<Location>();

            while (!locationPool.Empty())
            {
                alreadyChecked.Clear();
                Location seed = locationPool.RandomLocation();
                //Console.WriteLine("Have seed " + seed);
                if (type == RegionType.MonoRegion)
                {
                    region = new MonoRegion();
                }
                else if (type == RegionType.MultiColorRegion)
                {
                    MultiColorRegion multiColorRegion = new MultiColorRegion(maximumDistance);
                    region = multiColorRegion;
                }

                region.Add(seed);
                region.Origin = seed.Point;
                seed.Region = region;
                locationPool.SetMarked(seed);
                alreadyChecked.Add(seed);

                AddNeighbors(seed, todoList, alreadyChecked);
                todoList.Sort((x, y) => (int)(SxzColor.GetColorDistance(seed.Color, x.Color) - SxzColor.GetColorDistance(seed.Color, y.Color)));

                int sortCounter = 0;

                //inner loop
                while (todoList.Count != 0)
                {
                    //Console.WriteLine("Unmarked total " + locationPool.Unmarked.Count + " and todolist total " + todoList.Count);
                    seed = todoList[0];
                    todoList.RemoveAt(0);
                    sortCounter++;

                    if (seed.Marked)
                    {
                        throw new Exception("Location already marked!");
                    }

                    if (!region.IsValid(seed))
                    {
                        //we can process non-adjacent pixels by adding neighbors here and sorting before returning but limit the distance from an already
                        //validated location
                        continue;
                    }

                    //Console.WriteLine("Parsed pixel " + seed);
                    //we have a winner!
                    region.Add(seed);
                    locationPool.SetMarked(seed);

                    AddNeighbors(seed, todoList, alreadyChecked);
                    //if (todoList.Count < 1000)
                    if (todoList.Count < 1000 || sortCounter > 1000)
                    {
                        //let's limit the number to be sorted for performance sake
                        todoList.Sort((x1, y1) => (int)(SxzColor.GetColorDistance(seed.Color, x1.Color) - SxzColor.GetColorDistance(seed.Color, y1.Color)));
                        sortCounter = 0;
                    }
                }

                result.Add(region);
            }

            return result;
        }

        private static void CheckLocation(Location location, Location seed, HashSet<Location> alreadyChecked, List<Location> todoList)
        {
            if (location != null && !location.Marked)
            {
                if (!alreadyChecked.Contains(location) && !todoList.Contains(location))
                {
                    if (seed.Color.Equals(location.Color))
                    {
                        todoList.Insert(0, location);
                    }
                    else
                    {
                        todoList.Add(location);
                    }
                }
            }
        }

        private static void AddNeighbors(Location seed, List<Location> todoList, HashSet<Location> alreadyChecked)
        {
            //TODO: increase this to more locations for N-way 8-24 way chain code
            Location location = seed.Left;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.Right;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.Up;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.Down;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.UpperLeft;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.UpperRight;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.LowerLeft;
            CheckLocation(location, seed, alreadyChecked, todoList);

            location = seed.LowerRight;
            CheckLocation(location, seed, alreadyChecked, todoList);
        }
    }
}
