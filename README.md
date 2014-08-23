Sxz
===

Sxz Reference Implementation

<h3>About</h3>
Sxz is a hybrid vector/raster binary image format designed for low-entropy(non-photographic) images.  Sxz can attain superior compression over PNG in some cases, but not all do to the immaturity of the encoder.  Two decoders are provided in C# and Javascript for demonstration.

The Sxz Reference Implementation is released under LGPL license.  The Sxz Polyfill in Javascript is released under MIT license.  The Polyfill examples are released on CC0 license.

Sxz is a patent-unecumbered image format.

<h3>Design</h3>
Sxz draws heavily from PNG.  It has no built in compression relying on an outside mechanism such as gzip.  It uses paletted index only color storage, but allows for multiple palettes per image.  Each image consists of one or more frames with one or more chunks in each frame.  The interpretation of each frame is left to the decoder.
