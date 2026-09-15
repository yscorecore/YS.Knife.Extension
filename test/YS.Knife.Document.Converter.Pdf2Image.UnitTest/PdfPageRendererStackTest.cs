using PdfiumRaster;

namespace YS.Knife.Document.Converter.Pdf2Image.UnitTest
{
    /// <summary>
    /// Pure unit tests for <see cref="PdfPageRenderer.StackVertically"/>. The method stacks
    /// BGRA bitmaps vertically into a single bitmap; these tests verify pixel layout, padding
    /// behavior, and dimension arithmetic without rendering any PDF.
    /// </summary>
    public class PdfPageRendererStackTest
    {
        // BGRA layout used by PdfiumRaster.
        private const int BytesPerPixel = 4;

        /// <summary>
        /// Builds a bitmap of the given size filled with a single BGRA color.
        /// </summary>
        private static PdfBitmap MakeSolidBitmap(int width, int height, byte b, byte g, byte r, byte a = 255)
        {
            int stride = width * BytesPerPixel;
            var pixels = new byte[stride * height];
            for (int i = 0; i < pixels.Length; i += BytesPerPixel)
            {
                pixels[i] = b;
                pixels[i + 1] = g;
                pixels[i + 2] = r;
                pixels[i + 3] = a;
            }
            return new PdfBitmap(width, height, stride, pixels);
        }

        private static (byte b, byte g, byte r, byte a) ReadPixel(PdfBitmap bm, int x, int y)
        {
            int idx = y * bm.Stride + x * BytesPerPixel;
            return (bm.Pixels[idx], bm.Pixels[idx + 1], bm.Pixels[idx + 2], bm.Pixels[idx + 3]);
        }

        [Fact]
        public void StackVertically_EmptyInput_Throws()
        {
            Action act = () => PdfPageRenderer.StackVertically(Array.Empty<PdfBitmap>());
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void StackVertically_SingleBitmap_ReturnsSameDimensions()
        {
            var src = MakeSolidBitmap(4, 3, b: 10, g: 20, r: 30);

            var result = PdfPageRenderer.StackVertically(new[] { src });

            result.Width.Should().Be(4);
            result.Height.Should().Be(3);
            result.Stride.Should().Be(4 * BytesPerPixel);
            result.Pixels.Should().BeEquivalentTo(src.Pixels);
        }

        [Fact]
        public void StackVertically_SameWidth_PreservesRowsTopToBottom()
        {
            var top = MakeSolidBitmap(3, 2, b: 10, g: 20, r: 30);   // 3x2
            var bottom = MakeSolidBitmap(3, 2, b: 200, g: 100, r: 50); // 3x2

            var result = PdfPageRenderer.StackVertically(new[] { top, bottom });

            result.Width.Should().Be(3);
            result.Height.Should().Be(4);
            result.Stride.Should().Be(3 * BytesPerPixel);

            // Rows 0..1 -> top color, rows 2..3 -> bottom color.
            for (int y = 0; y < 4; y++)
            {
                byte eb = y < 2 ? (byte)10 : (byte)200;
                byte eg = y < 2 ? (byte)20 : (byte)100;
                byte er = y < 2 ? (byte)30 : (byte)50;
                for (int x = 0; x < 3; x++)
                {
                    var px = ReadPixel(result, x, y);
                    px.b.Should().Be(eb);
                    px.g.Should().Be(eg);
                    px.r.Should().Be(er);
                    px.a.Should().Be(255);
                }
            }
        }

        [Fact]
        public void StackVertically_DifferentWidths_WidestWinsAndPadsRightColumnsWithWhite()
        {
            var narrow = MakeSolidBitmap(2, 1, b: 0, g: 0, r: 0);   // 2x1 black
            var wide = MakeSolidBitmap(4, 1, b: 0, g: 0, r: 255);   // 4x1 blue-ish (R=255)

            var result = PdfPageRenderer.StackVertically(new[] { narrow, wide });

            result.Width.Should().Be(4, "the widest source width should win");
            result.Height.Should().Be(2);
            result.Stride.Should().Be(4 * BytesPerPixel);

            // Row 0 (narrow page): cols 0..1 are black, cols 2..3 are padded white.
            var r0p0 = ReadPixel(result, 0, 0);
            r0p0.b.Should().Be(0);
            r0p0.g.Should().Be(0);
            r0p0.r.Should().Be(0);
            r0p0.a.Should().Be(255);

            var r0p3 = ReadPixel(result, 3, 0);
            r0p3.b.Should().Be(255);
            r0p3.g.Should().Be(255);
            r0p3.r.Should().Be(255);
            r0p3.a.Should().Be(255);

            // Row 1 (wide page): all columns carry the wide page's color.
            var r1p3 = ReadPixel(result, 3, 1);
            r1p3.b.Should().Be(0);
            r1p3.g.Should().Be(0);
            r1p3.r.Should().Be(255);
            r1p3.a.Should().Be(255);
        }

        [Fact]
        public void StackVertically_ThreePages_DimensionsAreSummedAndEachPageRowIsPreserved()
        {
            var a = MakeSolidBitmap(3, 1, b: 1, g: 1, r: 1);
            var b = MakeSolidBitmap(3, 2, b: 2, g: 2, r: 2);
            var c = MakeSolidBitmap(3, 3, b: 3, g: 3, r: 3);

            var result = PdfPageRenderer.StackVertically(new[] { a, b, c });

            result.Width.Should().Be(3);
            result.Height.Should().Be(1 + 2 + 3);

            // rows 0 -> a, rows 1..2 -> b, rows 3..5 -> c
            byte expectedColor = 1;
            for (int row = 0; row < result.Height; row++)
            {
                if (row == 1) expectedColor = 2;
                if (row == 3) expectedColor = 3;
                var px = ReadPixel(result, 0, row);
                px.b.Should().Be(expectedColor);
                px.g.Should().Be(expectedColor);
                px.r.Should().Be(expectedColor);
                px.a.Should().Be((byte)255);
            }
        }

        [Fact]
        public void StackVertically_StrictWidthCalculatedFromMaxSourceWidthNotStride()
        {
            // Pages with the same logical width but different strides (e.g. one padded to 4-byte alignment
            // while another is tight). The combined width must be the source pixel width, not the stride.
            int width = 3;
            int tightStride = width * BytesPerPixel; // 12 bytes/row
            int paddedStride = 16; // padded row
            var tight = MakeSolidBitmap(width, 1, b: 7, g: 8, r: 9);
            var paddedPixels = new byte[paddedStride * 1];
            for (int i = 0; i < width * BytesPerPixel; i++) paddedPixels[i] = (i % 4) switch
            {
                0 => 7,  // B
                1 => 8,  // G
                2 => 9,  // R
                _ => 255 // A
            };
            var padded = new PdfBitmap(width, 1, paddedStride, paddedPixels);

            var result = PdfPageRenderer.StackVertically(new[] { tight, padded });

            result.Width.Should().Be(width);
            result.Stride.Should().Be(width * BytesPerPixel, "combined stride should be tight (4 bpp * max width)");
        }

        [Fact]
        public void StackVertically_AllTransparentPixelsAreOverwrittenToOpaqueWhiteInPaddingArea()
        {
            // Narrow page is fully transparent black (B=G=R=A=0). Padding columns must still be opaque white.
            var narrow = MakeSolidBitmap(2, 1, b: 0, g: 0, r: 0, a: 0);
            var wide = MakeSolidBitmap(4, 1, b: 0, g: 0, r: 0, a: 255);

            var result = PdfPageRenderer.StackVertically(new[] { narrow, wide });

            // First row, last column (outside narrow page) must be opaque white padding.
            var px = ReadPixel(result, 3, 0);
            px.b.Should().Be(255);
            px.g.Should().Be(255);
            px.r.Should().Be(255);
            px.a.Should().Be(255);
        }
    }
}
