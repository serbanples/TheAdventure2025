using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace TheAdventure
{
    public unsafe class TextRenderer
    {
        private readonly Font _font;
        private readonly GameRenderer _renderer;

        // temporary storage so we don't need GC pressure every frame
        private byte[] _pixelBuffer = Array.Empty<byte>();

        public TextRenderer(GameRenderer renderer, string fontPath, float fontSize)
        {
            if (!File.Exists(fontPath))
                throw new FileNotFoundException("Font file not found", fontPath);

            _renderer = renderer;

            var collection = new FontCollection();
            var family = collection.Add(fontPath);
            _font = family.CreateFont(fontSize);
        }

        public Font FONT => _font;

        public void DrawText(string text, int x, int y)
        {
            // measure text size using RendererOptions
            var options = new TextOptions(_font);
            var sizeF = TextMeasurer.MeasureSize(text, options);
            int w = (int)Math.Ceiling(sizeF.Width);
            int h = (int)Math.Ceiling(sizeF.Height);
            if (w == 0 || h == 0) return;

            // ensure buffer capacity
            int needed = w * h * 4;
            if (_pixelBuffer.Length < needed)
                _pixelBuffer = new byte[needed];

            // render into an Image<Rgba32>
            using var img = new Image<Rgba32>(w, h);
            img.Mutate(ctx =>
            {
                // correct DrawText overload: (text, font, color, position)
                ctx.DrawText(text, _font, Color.White, new PointF(0, 0));
            });

            img.CopyPixelDataTo(_pixelBuffer);

            int texId = _renderer.CreateTextureFromRGBA(_pixelBuffer, w, h);
            
            _renderer.RenderUiTexture(texId,
                new Rectangle<int>(0, 0, w, h),
                new Rectangle<int>(x, y, w, h));
        }
    }
}