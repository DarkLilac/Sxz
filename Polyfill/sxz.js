/*
The Sxz Polyfill is released under the MIT License.
*/
function Sxz() {
    this.Container = new Container();

    //Use built in second rate scaling for now
    this.Scale = function (frame, width, height) {
        var boundingBox = new SxzPoint(0, 0);
        this.Container.EnsureDimensions(boundingBox);

        var tempCanvas = document.createElement('canvas');
        tempCanvas.width = boundingBox.X;
        tempCanvas.height = boundingBox.Y;
        var tempContext = tempCanvas.getContext('2d');
        this.RenderFrame(tempContext, frame, boundingBox)

        //this.Canvas.width = this.Canvas.width;
        var context = this.Canvas.getContext("2d");
        context.drawImage(tempCanvas, 0, 0, width, height);
    }

    this.RenderFrame = function (context, frame, boundingBox) {
        var imageData = context.createImageData(boundingBox.X, boundingBox.Y);

        for (var i = 0; i < frame.Chunks.length; i++) {
            //alert('drawing chunk ' + i);
            var chunk = frame.Chunks[i];
            if (chunk.IsPalette() == true) {
                continue;
            }

            if (chunk.IsBackground() == true) {
                var color = chunk.GetColor(0, 0);
                //context.fillStyle = color.Hex();
                //alert('writing background color ' + color.Hex());
                //context.fillRect(0, 0, boundingBox.X, boundingBox.Y);
                for (var j = 0; j < imageData.data.length; j += 4) {
                    imageData.data[j] = color.Red;
                    imageData.data[j + 1] = color.Green;
                    imageData.data[j + 2] = color.Blue;
                    imageData.data[j + 3] = 255;
                }

                continue;
            }

            var dimensions = chunk.GetDimensions();
            var origin = chunk.Origin
            for (var y = origin.Y; y < origin.Y + dimensions.Y; y++) {
                for (var x = origin.X; x < origin.X + dimensions.X; x++) {
                    var sxzColor = chunk.GetColor(x, y);
                    if (sxzColor == null) continue;
                    imageData.data[(y * boundingBox.X + x) * 4] = sxzColor.Red;
                    imageData.data[(y * boundingBox.X + x) * 4 + 1] = sxzColor.Green;
                    imageData.data[(y * boundingBox.X + x) * 4 + 2] = sxzColor.Blue;
                    imageData.data[(y * boundingBox.X + x) * 4 + 3] = 255;
                }
            }
        }

        context.putImageData(imageData, 0, 0);
    }

    this.Render = function () {
        var context = this.Canvas.getContext("2d");
        var boundingBox = new SxzPoint(0, 0);
        this.Container.EnsureDimensions(boundingBox);

        for (var f = 0; f < this.Container.Frames.length; f++) {
            var frame = this.Container.Frames[f];
            this.RenderFrame(context, frame, boundingBox);
        }
    }

	this.LoadLocal = function (data, canvas, callback) {
		if (typeof (callback) != 'undefined') {
			canvas.addEventListener('mousedown', callback, false);
		}

		var raw = window.atob(data);
		var rawLength = raw.length;
		//parse the data here
		var array = new Uint8Array(new ArrayBuffer(rawLength));

		for (i = 0; i < rawLength; i++) {
			array[i] = raw.charCodeAt(i);
		}

		this.Container.SetData(array);
		this.Canvas = canvas;

		this.Render();
    }

    this.HoritontalDistanceTo = function (y) {
        var result = 9007199254740992;
        for (var f = 0; f < this.Container.Frames.length; f++) {
            var frame = this.Container.Frames[f];

            var temp = frame.HorizontalDistanceTo(y);
            if (temp < result) {
                result = temp;
            }
        }

        return result;
    }
}

Sxz.prototype.Load = function (sxz, url, canvas, callback) {
    var xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);
    xhr.responseType = "arraybuffer";
    if (typeof (callback) != 'undefined') {
        canvas.addEventListener('mousedown', callback, false);
    }

    xhr.onload = function () {
        var data = new Uint8Array(xhr.response || xhr.mozResponseArrayBuffer);

        sxz.Container.SetData(data);
        sxz.Canvas = canvas;

        sxz.Render();
    }

    xhr.send(null);
}

function SxzPoint(x, y) {
    this.X = x;
    this.Y = y;
}

SxzPoint.prototype.Length = function(point) {
    var dx = point.X - this.X;
    var dy = point.Y - this.Y;
    return Math.sqrt(dx * dx + dy * dy);
};

function SxzColor(red, green, blue) {
    this.Red = red;
    this.Green = green;
    this.Blue = blue;

    this.GetColorDistance = function (color) {
        if (this.Equals(color)) {
            return 0.0;
        }

        var red = this.Red - color.Red;
        var green = this.Green - color.Green;
        var blue = this.Blue - color.Blue;
        return Math.sqrt(red * red + blue * blue + green * green);
    }

    this.Hex = function () {
        return this.HexString(this);
    }

    this.Equals = function (color) {
        return this.Red == color.Red && this.Green == color.Green && this.Blue == color.Blue;
    }
}

function componentToHex(c) {
    var hex = c.toString(16);
    return hex.length == 1 ? "0" + hex : hex;
}

SxzColor.prototype.HexString = function (color) {
    return '#' + componentToHex(color.Red) + componentToHex(color.Green) + componentToHex(color.Blue);
};

function Container() {
    this.Frames = [];

    this.SetData = function (data) {
        var index = 3;
        while (index < data.byteLength) {
            var label = String.fromCharCode(data[index], data[index + 1]);
            index += 2;
            if (label == "FR") {
                var size = ToInt32(data, index);
                index += 4;
                var frame = new Frame();
                frame.SetData(data, index, size);
                this.Frames.push(frame);
                index += size;
            }
        }
    }

    this.GetSelected = function (x, y) {
        for (var i = this.Frames.length - 1; i >= 0; i--) {
            var frame = this.Frames[i];

            var boundingBox = new SxzPoint(0, 0);
            frame.EnsureDimensions(boundingBox);
            var chunk = frame.GetSelected(x, y);
            if (chunk != null) {
                return chunk;
            }
        }

        return null;
    }

    this.EnsureDimensions = function (boundingBox) {
        this.Frames.forEach(Ensure, boundingBox);
    }
}

Container.prototype.Label = "SXZ";

function Frame() {
    this.Chunks = [];

    this.EnsureDimensions = function (boundingBox) {
        this.Chunks.forEach(Ensure, boundingBox);
    }

    this.GetSelected = function (x, y) {
        for (var i = this.Chunks.length - 1; i >= 0; i--) {
            var chunk = this.Chunks[i];
            if (chunk.IsPalette()) {
                continue;
            }

            if (chunk.IsBackground()) {
                continue;
            }

            var boundingBox = new SxzPoint(0, 0);
            var origin = chunk.Origin;
            chunk.EnsureDimensions(boundingBox);
            if (x >= origin.X && y >= origin.Y && x < (boundingBox.X) && y < (boundingBox.Y)) {
                if (chunk.GetColor(x, y) != null) {
                    return chunk;
                }
            }
        }

        return null;
    }

    this.HorizontalDistanceTo = function (y) {
        var result = 9007199254740992;
        for (var i = this.Chunks.length - 1; i >= 0; i--) {
            var chunk = this.Chunks[i];
            if (chunk.IsPalette()) {
                continue;
            }

            if (chunk.IsBackground()) {
                continue;
            }

            var dimensions = chunk.GetDimensions();
            if (y > dimensions.Y + chunk.Origin.Y) {
                continue;
            }

            if (y < chunk.Origin.Y) {
                continue;
            }

            var temp = chunk.HorizontalDistanceTo(y);
            if (temp != null) {
                if (temp < result) {
                    result = temp;
                }
            }
        }

        return result;
    }

    this.SetData = function (data, index, size) {
        //var index = 0;
        var paletteChunk = new DefaultPaletteChunk();
        var startingIndex = index;
        while (index < (startingIndex + size)) {
            var label = String.fromCharCode(data[index], data[index + 1]);
            //alert('In Frame ' + index + ' have label ' + label);

            switch (label) {
                case "BG":
                    var background = new BackgroundChunk();
                    background.Palette = paletteChunk;
                    background.SetData(data, index);
                    this.Chunks.push(background);
                    index += 3;
                    break;
                case "PB":
                    var chunk = new PaletteChunk();
                    var count = (data[index + 2] + 1) * 3;
                    chunk.SetData(data, index);
                    this.Chunks.push(chunk);
                    index += count + 3;
                    paletteChunk = chunk;
                    break;
                case "MR":
                    var chunk = new MonoRectangleChunk();
                    chunk.Palette = paletteChunk;
                    chunk.SetData(data, index);
                    this.Chunks.push(chunk);
                    index += 11;
                    break;
                case "MB":
                    var chunk = new MonoBitPlaneChunk();
                    var chunkSize = ToInt16(data, index + 2);
                    chunk.Palette = paletteChunk;
                    chunk.SetData(data, index);
                    this.Chunks.push(chunk);
                    index += chunkSize + 4;
                    break;
                case "CR":
                    var chunk = new ColorRectangleChunk();
                    var chunkSize = ToInt16(data, index + 2);
                    chunk.Palette = paletteChunk;
                    chunk.SetData(data, index);
                    this.Chunks.push(chunk);
                    index += chunkSize + 4;
                    break;
                case "CB":
                    var chunk = new ColorBitPlaneChunk();
                    var chunkSize = ToInt16(data, index + 2);
                    chunk.Palette = paletteChunk;
                    chunk.SetData(data, index);
                    this.Chunks.push(chunk);
                    index += chunkSize + 4;
                    break;
                default:
                    alert('Frame.setdata invalid chunk ' + label);
                    return;
            }
        }
    }
}

Frame.prototype.Label = "FR";
function Ensure(element, index, array) {
    if (typeof (element.EnsureDimensions) === "function") {
        //alert('ensure dimension on ' + this.X + ' ' + this.Y);
        element.EnsureDimensions(this);
    }
}

function PaletteChunk() {
    this.Colors = [];
    this.Origin = new SxzPoint(0, 0);

    this.SetData = function (data, index) {
        index += 2;
        var count = data[index] + 1;
        index++;
        var previousColor = new SxzColor(0, 0, 0);
        for (var i = 0; i < count; i++) {
            var red = ((data[index] + previousColor.Red) & 0xff);
            index++;
            var green = ((data[index] + previousColor.Green) & 0xff);
            index++;
            var blue = ((data[index] + previousColor.Blue) & 0xff);
            index++;
            previousColor = new SxzColor(red, green, blue);
            this.Colors.push(previousColor);
        }
    }
}

PaletteChunk.prototype.Label = "PB";

PaletteChunk.prototype.IsBackground = function () {
    return false;
};
PaletteChunk.prototype.IsPalette = function () {
    return true;
};
PaletteChunk.prototype.IsTransparent = function () {
    return false;
};

function DefaultPaletteChunk() {
    var colorStrings = [
"000000", "000033", "000066", "000099", "0000CC", "0000FF",
"003300", "003333", "003366", "003399", "0033CC", "0033FF",
"006600", "006633", "006666", "006699", "0066CC", "0066FF",
"009900", "009933", "009966", "009999", "0099CC", "0099FF",
"00CC00", "00CC33", "00CC66", "00CC99", "00CCCC", "00CCFF",
"00FF00", "00FF33", "00FF66", "00FF99", "00FFCC", "00FFFF",
"330000", "330033", "330066", "330099", "3300CC", "3300FF",
"333300", "333333", "333366", "333399", "3333CC", "3333FF",
"336600", "336633", "336666", "336699", "3366CC", "3366FF",
"339900", "339933", "339966", "339999", "3399CC", "3399FF",
"33CC00", "33CC33", "33CC66", "33CC99", "33CCCC", "33CCFF",
"33FF00", "33FF33", "33FF66", "33FF99", "33FFCC", "33FFFF",
"660000", "660033", "660066", "660099", "6600CC", "6600FF",
"663300", "663333", "663366", "663399", "6633CC", "6633FF",
"666600", "666633", "666666", "666699", "6666CC", "6666FF",
"669900", "669933", "669966", "669999", "6699CC", "6699FF",
"66CC00", "66CC33", "66CC66", "66CC99", "66CCCC", "66CCFF",
"66FF00", "66FF33", "66FF66", "66FF99", "66FFCC", "66FFFF",
"990000", "990033", "990066", "990099", "9900CC", "9900FF",
"993300", "993333", "993366", "993399", "9933CC", "9933FF",
"996600", "996633", "996666", "996699", "9966CC", "9966FF",
"999900", "999933", "999966", "999999", "9999CC", "9999FF",
"99CC00", "99CC33", "99CC66", "99CC99", "99CCCC", "99CCFF",
"99FF00", "99FF33", "99FF66", "99FF99", "99FFCC", "99FFFF",
"CC0000", "CC0033", "CC0066", "CC0099", "CC00CC", "CC00FF",
"CC3300", "CC3333", "CC3366", "CC3399", "CC33CC", "CC33FF",
"CC6600", "CC6633", "CC6666", "CC6699", "CC66CC", "CC66FF",
"CC9900", "CC9933", "CC9966", "CC9999", "CC99CC", "CC99FF",
"CCCC00", "CCCC33", "CCCC66", "CCCC99", "CCCCCC", "CCCCFF",
"CCFF00", "CCFF33", "CCFF66", "CCFF99", "CCFFCC", "CCFFFF",
"FF0000", "FF0033", "FF0066", "FF0099", "FF00CC", "FF00FF",
"FF3300", "FF3333", "FF3366", "FF3399", "FF33CC", "FF33FF",
"FF6600", "FF6633", "FF6666", "FF6699", "FF66CC", "FF66FF",
"FF9900", "FF9933", "FF9966", "FF9999", "FF99CC", "FF99FF",
"FFCC00", "FFCC33", "FFCC66", "FFCC99", "FFCCCC", "FFCCFF",
"FFFF00", "FFFF33", "FFFF66", "FFFF99", "FFFFCC", "FFFFFF"
            ];

    this.Colors = new Array();
    for (var i = 0; i < colorStrings.length; i++) {
        var colorValue = colorStrings[i];
        var red = parseInt(colorValue.substring(0, 2), 16);
        var green = parseInt(colorValue.substring(2, 4), 16);
        var blue = parseInt(colorValue.substring(4, 6), 16);

        this.Colors.push(new SxzColor(red, green, blue));
    }

    this.GetColor = function (index) {
        return this.Colors[index];
    }
}

function MonoRectangleChunk() {
    this.Width = 0;
    this.Height = 0;
    this.ColorIndex = 0;
    this.Origin = new SxzPoint(0, 0);
    this.Palette = new PaletteChunk();
    this.ClickColor = new SxzColor(85, 26, 139);
    this.IsClick = false;

    this.SetData = function (data, index) {
        index += 2;
        this.Width = ToInt16(data, index);
        index += 2;
        this.Height = ToInt16(data, index);
        index += 2;
        var x = ToInt16(data, index);
        index += 2;
        var y = ToInt16(data, index);
        index += 2;
        this.Origin = new SxzPoint(x, y);

        this.ColorIndex = data[index];
    }

    this.GetColor = function (x, y) {
        if (this.IsClick == true) {
            return this.ClickColor;
        }
        else {
            return this.Palette.Colors[this.ColorIndex];
        }
    }

    this.EnsureDimensions = function (boundingBox) {
        if (this.Origin.X + this.Width > boundingBox.X) {
            boundingBox.X = this.Origin.X + this.Width;
        }

        if (this.Origin.Y + this.Height > boundingBox.Y) {
            boundingBox.Y = this.Origin.Y + this.Height;
        }

        //alert('later ensure dimension on ' + boundingBox.X + ' ' + boundingBox.Y);
    }

    this.GetDimensions = function () {
        return new SxzPoint(this.Width, this.Height);
    }

    this.MouseDown = function () {
        this.IsClick = true;
    }

    this.MouseUp = function () {
        this.IsClick = false;
    }

    this.HorizontalDistanceTo = function (y) {
        if (y < this.Origin.Y) {
            return null;
        }

        if (y > (this.Origin.Y + this.Height)) {
            return null;
        }

        return this.Origin.X;
    }
}

MonoRectangleChunk.prototype.Label = "MR";

MonoRectangleChunk.prototype.IsBackground = function () {
    return false;
};

MonoRectangleChunk.prototype.IsPalette = function () {
    return false;
};

MonoRectangleChunk.prototype.IsTransparent = function () {
    return false;
};

function BackgroundChunk() {
    this.ColorIndex = 0;
    this.Palette = new PaletteChunk();
    this.Origin = new SxzPoint(0, 0);

    this.SetData = function (data, index) {
        index += 2;
        this.ColorIndex = data[index];
    }

    this.GetColor = function (x, y) {
        return this.Palette.Colors[this.ColorIndex];
    }
}

BackgroundChunk.prototype.Label = "BG";

BackgroundChunk.prototype.IsBackground = function () {
    return true;
};

BackgroundChunk.prototype.IsPalette = function () {
    return false;
};

BackgroundChunk.prototype.IsTransparent = function () {
    return false;
};

BackgroundChunk.prototype.EnsureDimensions = function (boundingBox) {
};

BackgroundChunk.prototype.GetDimensions = function () {
    return new SxzPoint(0, 0);
};

function MonoBitPlaneChunk() {
    this.Width = 0;
    this.Height = 0;
    this.ColorIndex = 0;
    this.BitPlane = new BitPlane(0, 0);
    this.Origin = new SxzPoint(0, 0);
    this.Palette = new PaletteChunk();
    this.ClickColor = new SxzColor(85, 26, 139);
    this.IsClick = false;

    this.SetData = function (data, index) {
        index += 4;
        this.Width = ToInt16(data, index);
        index += 2;
        this.Height = ToInt16(data, index);
        index += 2;
        var x = ToInt16(data, index);
        index += 2;
        var y = ToInt16(data, index);
        index += 2;
        this.Origin = new SxzPoint(x, y);

        this.ColorIndex = data[index++];
        this.BitPlane = new BitPlane(this.Width * this.Height, this.Width);
        var size = SizeOfBitPlaneInBytes(this.Width * this.Height);
        var bits = new Array();
        for (var i = index; i < (index + size); i++) {
            bits.push(data[i]);
        }

        this.BitPlane.SetData(bits, this.Width * this.Height);
    }

    this.GetColor = function (x, y) {
        if (this.BitPlane.HasColor(x - this.Origin.X, y - this.Origin.Y)) {
            if (this.IsClick == true) {
                return this.ClickColor;
            }
            else {
                return this.Palette.Colors[this.ColorIndex];
            }
        }

        return null;
    }

    this.EnsureDimensions = function (boundingBox) {
        if (this.Origin.X + this.Width > boundingBox.X) {
            boundingBox.X = this.Origin.X + this.Width;
        }

        if (this.Origin.Y + this.Height > boundingBox.Y) {
            boundingBox.Y = this.Origin.Y + this.Height;
        }

        //alert('later ensure dimension on ' + boundingBox.X + ' ' + boundingBox.Y);
    }

    this.GetDimensions = function () {
        return new SxzPoint(this.Width, this.Height);
    }

    this.MouseDown = function () {
        this.IsClick = true;
    }

    this.MouseUp = function () {
        this.IsClick = false;
    }

    this.HorizontalDistanceTo = function (y) {
        if (y < this.Origin.Y) {
            return null;
        }

        if (y > (this.Origin.Y + this.Height)) {
            return null;
        }

        return this.BitPlane.HorizontalDistanceTo(y - this.Origin.Y) + this.Origin.X;
    }
}

MonoBitPlaneChunk.prototype.Label = "MB";

MonoBitPlaneChunk.prototype.IsBackground = function () {
    return false;
};

MonoBitPlaneChunk.prototype.IsPalette = function () {
    return false;
};

MonoBitPlaneChunk.prototype.IsTransparent = function () {
    return false;
};

//Direction down, left, right up -> 0, 1, 2, 3
function ColorRectangleChunk() {
    this.Width = 0;
    this.Height = 0;
    this.Direction = 0;
    this.Data = [this.Width * this.Height];
    this.Origin = new SxzPoint(0, 0);
    this.Palette = new PaletteChunk();
    this.ClickColor = new SxzColor(85, 26, 139);
    this.IsClick = false;

    this.SetData = function (data, index) {
        index += 4;
        this.Width = ToInt16(data, index);
        index += 2;
        this.Height = ToInt16(data, index);
        index += 2;
        var x = ToInt16(data, index);
        index += 2;
        var y = ToInt16(data, index);
        index += 2;
        this.Origin = new SxzPoint(x, y);

        this.Direction = data[index++];

        this.Data = new Array(this.Width * this.Height);
        var numberOfIndexes = this.Width * this.Height;
        var indexes = new Array();
        for (var i = index; i < (index + numberOfIndexes); i++) {
            indexes.push(data[i]);
        }

        this.SetDataList(indexes);
    }

    this.SetDataList = function (values) {
        var index = 0;

        switch (this.Direction) {
            case 1:
                for (var x = this.Width - 1; x >= 0; x--) {
                    for (var y = 0; y < this.Height; y++) {
                        this.SetByteIndex(values[index++], x, y);
                    }
                }

                break;
            case 2:
                for (var x = 0; x < this.Width; x++) {
                    for (var y = 0; y < this.Height; y++) {
                        this.SetByteIndex(values[index++], x, y);
                    }
                }

                break;
            case 3:
                for (var y = this.Height - 1; y >= 0; y--) {
                    for (var x = 0; x < this.Width; x++) {
                        this.SetByteIndex(values[index++], x, y);
                    }
                }

                break;
            case 0:
                for (var y = 0; y < this.Height; y++) {
                    for (var x = 0; x < this.Width; x++) {
                        this.SetByteIndex(values[index++], x, y);
                    }
                }

                break;
        }
    }

    this.GetByteIndex = function (x, y) {
        var location = y * this.Width + x;
        return this.Data[location];
    }

    this.SetByteIndex = function (value, x, y) {
        var location = y * this.Width + x;
        this.Data[location] = value;
    }

    this.GetColor = function (x, y) {
        if (this.IsClick == true) {
            return this.ClickColor;
        }
        else {
            return this.Palette.Colors[this.GetByteIndex(x - this.Origin.X, y - this.Origin.Y)];
        }
    }

    this.SetColor = function (color, x, y) {
        this.SetByteIndex(this.Palette.Colors.indexOf(color), x - this.Origin.X, y - this.Origin.Y);
    }

    this.EnsureDimensions = function (boundingBox) {
        if (this.Origin.X + this.Width > boundingBox.X) {
            boundingBox.X = this.Origin.X + this.Width;
        }

        if (this.Origin.Y + this.Height > boundingBox.Y) {
            boundingBox.Y = this.Origin.Y + this.Height;
        }

        //alert('later ensure dimension on ' + boundingBox.X + ' ' + boundingBox.Y);
    }

    this.MouseDown = function () {
        this.IsClick = true;
    }

    this.MouseUp = function () {
        this.IsClick = false;
    }

    this.GetDimensions = function () {
        return new SxzPoint(this.Width, this.Height);
    }

    this.HorizontalDistanceTo = function (y) {
        if (y < this.Origin.Y) {
            return null;
        }

        if (y > (this.Origin.Y + this.Height)) {
            return null;
        }

        return this.Origin.X;
    }
}

ColorRectangleChunk.prototype.Label = "CR";

ColorRectangleChunk.prototype.IsBackground = function () {
    return false;
};

ColorRectangleChunk.prototype.IsPalette = function () {
    return false;
};

ColorRectangleChunk.prototype.IsTransparent = function () {
    return false;
};

function ColorBitPlaneChunk() {
    this.Width = 0;
    this.Height = 0;
    this.Direction = 0;
    this.Data = new Array(this.Width * this.Height);
    this.BitPlane = new BitPlane(0, 0);
    this.Origin = new SxzPoint(0, 0);
    this.Palette = new PaletteChunk();
    this.ClickColor = new SxzColor(85, 26, 139);
    this.IsClick = false;

    this.SetData = function (data, index) {
        var totalSize = ToInt16(data, index + 2);
        index += 4;
        this.Width = ToInt16(data, index);
        index += 2;
        this.Height = ToInt16(data, index);
        index += 2;
        var x = ToInt16(data, index);
        index += 2;
        var y = ToInt16(data, index);
        index += 2;
        this.Origin = new SxzPoint(x, y);


        this.Direction = data[index++];
        this.BitPlane = new BitPlane(this.Width * this.Height, this.Width);
        var size = SizeOfBitPlaneInBytes(this.Width * this.Height);
        var bits = new Array();
        for (var i = index; i < (index + size); i++) {
            bits.push(data[i]);
        }

        this.BitPlane.SetData(bits, this.Width * this.Height);
        index += size;

        this.Data = new Array(this.Width * this.Height);
        var numberOfIndexes = ToInt16(data, index) + 1;

        index += 2;
        var indexes = new Array();
        for (var i = index; i < (index + numberOfIndexes); i++) {
            indexes.push(data[i]);
        }

        this.SetDataList(indexes);
    }

    this.SetDataList = function (values) {
        var index = 0;

        switch (this.Direction) {
            case 1:
                for (var x = this.Width - 1; x >= 0; x--) {
                    for (var y = 0; y < this.Height; y++) {
                        if (this.BitPlane.HasColor(x, y)) {
                            this.SetByteIndex(values[index++], x, y);
                        }
                        else {
                            this.SetByteIndex(0, x, y);
                        }
                    }
                }

                break;
            case 2:
                for (var x = 0; x < this.Width; x++) {
                    for (var y = 0; y < this.Height; y++) {
                        if (this.BitPlane.HasColor(x, y)) {
                            this.SetByteIndex(values[index++], x, y);
                        }
                        else {
                            this.SetByteIndex(0, x, y);
                        }
                    }
                }

                break;
            case 3:
                for (var y = this.Height - 1; y >= 0; y--) {
                    for (var x = 0; x < this.Width; x++) {
                        if (this.BitPlane.HasColor(x, y)) {
                            this.SetByteIndex(values[index++], x, y);
                        }
                        else {
                            this.SetByteIndex(0, x, y);
                        }
                    }
                }

                break;
            case 0:
                for (var y = 0; y < this.Height; y++) {
                    for (var x = 0; x < this.Width; x++) {
                        if (this.BitPlane.HasColor(x, y)) {
                            this.SetByteIndex(values[index++], x, y);
                        }
                        else {
                            this.SetByteIndex(0, x, y);
                        }
                    }
                }

                break;
        }
    }

    this.GetByteIndex = function (x, y) {
        var location = y * this.Width + x;
        return this.Data[location];
    }

    this.SetByteIndex = function (value, x, y) {
        var location = y * this.Width + x;
        this.Data[location] = value;
    }

    this.GetColor = function (x, y) {
        if (this.BitPlane.HasColor(x - this.Origin.X, y - this.Origin.Y)) {
            if (this.IsClick == true) {
                return this.ClickColor;
            }
            else {
                return this.Palette.Colors[this.GetByteIndex(x - this.Origin.X, y - this.Origin.Y)];
            }
        }

        return null;
    }

    this.SetColor = function (color, x, y) {
        this.SetByteIndex(this.Palette.Colors.indexOf(color), x - this.Origin.X, y - this.Origin.Y);
    }

    this.EnsureDimensions = function (boundingBox) {
        if (this.Origin.X + this.Width > boundingBox.X) {
            boundingBox.X = this.Origin.X + this.Width;
        }

        if (this.Origin.Y + this.Height > boundingBox.Y) {
            boundingBox.Y = this.Origin.Y + this.Height;
        }
    }

    this.MouseDown = function () {
        this.IsClick = true;
    }

    this.MouseUp = function () {
        this.IsClick = false;
    }

    this.GetDimensions = function () {
        return new SxzPoint(this.Width, this.Height);
    }

    this.HorizontalDistanceTo = function (y) {
        if (y < this.Origin.Y) {
            return null;
        }

        if (y > (this.Origin.Y + this.Height)) {
            return null;
        }

        return this.BitPlane.HorizontalDistanceTo(y - this.Origin.Y) + this.Origin.X;
    }
}

ColorBitPlaneChunk.prototype.Label = "CB";

ColorBitPlaneChunk.prototype.IsBackground = function () {
    return false;
};

ColorBitPlaneChunk.prototype.IsPalette = function () {
    return false;
};

ColorBitPlaneChunk.prototype.IsTransparent = function () {
    return false;
};

function SizeOfBitPlaneInBytes(size) {
	var divisor = (size / 8) | 0;
	if (size % 8 == 0)
	{
		return divisor;
	}

	return divisor + 1;
}

function ToInt16 (data, index) {
    var b1, b2;
    b1 = data[index++];
    b2 = data[index] << 8;
    return b1 | b2;
}

function ToInt32 (data, index) {
    var b1, b2, b3, b4;
    b1 = data[index++];
    b2 = data[index++] << 8;
    b3 = data[index++] << 16;
    b4 = data[index] << 24;
    return b1 | b2 | b3 | b4;
};

function BitPlane(size, width) {
	this.Size = size;
	this.Width = width;
	this.Data = new Array(size);
	for (var i = 0; i < size; i++)
	{
		this.Data[i] = false;
	}

    this.HasColor = function (x, y) {
        var location = (y * this.Width) + x;
        return this.Data[location];
    }

    this.SetData = function (bytes, size) {
        this.Data = this.ConvertBytesToBools(bytes, size);
    }

    this.DrawLocation = function (x, y) {
        var location = (y * this.Width) + x;
        this.Data[location] = true;
    }

    this.UnDrawLocation = function (x, y) {
        var location = (y * this.Width) + x;
        this.Data[location] = false;
    }

    this.HorizontalDistanceTo = function (y) {
        for (var x = 0; x <= this.Width; x++) {
            if (this.HasColor(x, y)) {
                return x;
            }
        }

        return 9007199254740992;
    }

    this.GetPixelCount = function () {
        var result = 0;
        for (var i = 0; i < this.Data.length; i++) {
            if (this.Data[i]) {
                result++;
            }
        }

        return result;
    }

    this.GetData = function () {
        return this.ConvertBoolsToBytes(this.Data);
    }
};

BitPlane.prototype.Masks = [ 1, 2, 4, 8, 16, 32, 64, 128 ];

BitPlane.prototype.ConvertBytesToBools = function (bytes, size) {
	var result = new Array();

	var index = 0;
	for (var i = 0; i < bytes.length; i++)
	{
		//read a bytes worth of bits at a time
		for (var j = 0; j < this.Masks.length; j++)
		{
			var b = bytes[i];
			var boolean = (b & this.Masks[j]) != 0;
			result.push(boolean);
			index++;
			if (index >= size)
			{
				return result;
			}
		}
	}

	return null;
};

BitPlane.prototype.ConvertBoolsToBytes = function (data) {
	var result = new Array();
	//wrap into 8 bit bunches of a byte

	var eightBits = 0;
	var counter = 0;
	for (var i = 0; i < data.length; i++)
	{
		var b = data[i];
		//write to next location on eightBits

		if (b)
		{
			eightBits = eightBits | this.Masks[counter];
		}

		counter++;

		if (counter > 7)
		{
			counter = 0;
			result.push(eightBits);
			eightBits = 0;
		}
	}

	if (counter > 0)
	{
		//pad out the eightBits with zeros then add
		result.Add(eightBits);
	}

	return result;
};