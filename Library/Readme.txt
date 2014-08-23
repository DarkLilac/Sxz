Chunks:
Each chunk gets a unique id ordered as found with the first chunk getting zero

PaletteChunk - PB
Specify colors in a palette of maximum size 256

DefaultPaletteChunk - DP
Loads web safe palette of colors described here: http://www.w3schools.com/html/html_colors.asp  and allows 40 more to be added
Is included automatically until another palette is defined

BackgroundChunk - BG
Non paletted single color but with no width or height specified

MonoRectangleChunk - MR
Rectangle with a single color in the palette

MonoChunk - MI
Bitplane region with a single color in the palette

LocalPaletteChunk - LP
Non-global paletted bitplaned region with a flexibly sized local palette store, ordered by brightness 0,0,0 -> 255,255,255
Stores up to 256 separate colors in the local palette at one of various levels of depth -> 1, 2, 3, 4, 5, 6, 7, 8 bits for
2, 4, 8, 16, 32, 64, 128, 256 colors respectively

BagChunk - BC
Paletted region without a bitplane painting on unmarked locations within a bounding box, depends on current palette

TransparentChunk - TM
Bitplane region which describes a region to not be drawn on

TransparentRectangleChunk - TR
Rectangle of a region to not be drawn

ReuseChunk - RU
Specifies the id for a previously created chunk and re-uses it

Other Structures:
Bitplane
A bit array represented in code as bools, and a rectangular region where colors should be drawn.  Used by non-rectangular drawing chunks

LocalPalette
A sorted array of SxzColors and Used by LocalPaletteChunk to hold the indexes into a LocalPalette to tell the chunk to draw what, where
Depth is limited from 1 to 8 bits (2->256 colors)

Frame - FR
A single sprite within an image.  An image consists of one or more frames
Each frame comes with a default palette that can be used, extended or ignored and an entirely new one used

Container - SXZ
Holds an ordered list of frames starting with index 0

Merit:
Decoder is simple enough to be written in JavaScript + HTML Canvas.  JavaScript based driver allows for customization, links, behaviors, animation...
An image with a hole has usefullness
Improved compression for many images
Allows for improved compression with greater processing power during the encoding process
Allow greater than 256 colors in an indexed image format
Secure since the decoder is in JavaScript
Firefox and Chrome extensions possible
Hopefully will spawn market for resource intensive encoders
Can encode the same image at multiple resolutions within one image file, with high compression due to re-used chunks
Server side or client side image manipulation?
Uses underlying compression already built into browsers

Sample Images:
Container with 3 frames of a similar sprite with size 32x32, 64x64, 128x128 and 100, 200 and 300 colors sharing a palette, 
to allow the decoder to choose based on window size
Re-hash of a common logo with greater than 256 colors and equivalent size
Example of an image with a hole and the savings vs. png
Container with two sprites and the encoder animates one over the other
Sprites with transparency but clickable links only on the non-transparent area
A small sized but large image

Automatic Encoder:
Generate a framework for multiple different strategies of encoding
Run multiple encoders and then asynchronously select the output of the best one

Manual Encoder:
Desktop application with GUI to build images manually
Rasterer to .NET graphics api
Gui options to add chunks and demark the bitplane and select colors for the local palette







