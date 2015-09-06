Sxz
===

Sxz Reference Implementation

<h3>About</h3>
Sxz is a hybrid vector/raster binary image format designed for low-entropy(non-photographic) images.  It allows for non-rectangular shapes and holes.  Sxz can attain superior compression over PNG in some cases, but not all due to the immaturity of the encoder.  Two decoders are provided in C# and Javascript for demonstration.

The Sxz Reference Implementation is released under LGPL license.  The Sxz Polyfill in Javascript is released under MIT license.  The Polyfill demos are released on CC0 license.

Sxz is a patent-unecumbered, royalty free image format.

<h3>Design</h3>
Sxz draws heavily from PNG.  Sxz differs from many other image formats in that it is designed to be slow encoding and fast decoding.  This will allow the encoder the option to check many different possible encoding strategies potentially.  It has no built in compression relying on an outside mechanism such as gzip.  It uses paletted index only color storage, but allows for multiple palettes per image.  Sxz stores more than 256 and less 1000 colors very well.   Each image consists of one or more frames with one or more chunks in each frame.  The interpretation of each frame is left to the decoder.

<h3>Responsive Images</h3>
Sxz supports  features of responsive imaging.  It can do clickable image maps using the data in the image itself without the need for an extra map file.  It can also scale parts of itself differently to achieve <a href="http://usecases.responsiveimages.org/#art-direction">art direction</a>.  By splitting the image into separate frames, each frame can be treated differently by the decoder.  It also support text flow around non-rectangular areas.

<h3>Platform</h3>
Tested on Firefox 29+ and Chrome 36

<h3>Demos</h3>
Demos are located in the Polyfill folder along with the Javascript decoder/rasterer sxz.js.<br />
New: <a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/transparency_demo_base64.html">Transparent Jpeg Example</a> <br />
<a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/text_flow_demo_base64.html">Text Flow like CSS Shapes</a> <br />
<a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/art_direction_demo_base64.html">Art Direction</a> <br />
<a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/click_by_chunk_demo_base64.html">Image Map Demo</a> <br />
<a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/click_by_location_demo_base64.html">Another Image Map Demo</a><br />
<a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/image_with_hole_demo_base64.html">Image with a hole, and clickable</a><br />
<a href="https://rawgit.com/DarkLilac/Sxz/master/Polyfill/iphone_scale_demo_base64.html">Example of image map working after scaling</a> <br />
