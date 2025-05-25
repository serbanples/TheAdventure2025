using Silk.NET.SDL;
using SixLabors.Fonts;
using Thread = System.Threading.Thread;
using System.IO;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer |
                                     Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        using (var gameWindow = new GameWindow(sdl))
        {
            string baseDir = AppContext.BaseDirectory;
            string fontPath = Path.Combine(baseDir, "Assets", "ARIAL.TTF");
            if (!File.Exists(fontPath))
                throw new FileNotFoundException("Couldn’t find font at", fontPath);
            var input = new Input(sdl);
            var gameRenderer = new GameRenderer(sdl, gameWindow);
            var engine = new Engine(gameRenderer, input);
            var textRenderer = new TextRenderer(gameRenderer, fontPath, 24);


            engine.SetupWorld();

            bool quit = false;
            bool paused = false;
            bool lastP = false;
            bool lastF11 = false;
            bool lastR = false;
            bool lastEscape = false;

            gameWindow.SetWindowTitle("The Adventure");

            while (!quit)
            {
                quit = input.ProcessInput();
                if (quit) break;

                // Handle pause and update window title
                bool thisP = input.IsKeyPPressed();

                if (thisP && !lastP)
                {
                    paused = !paused;
                    var suffix = paused ? " [PAUSED]" : "";
                    gameWindow.SetWindowTitle($"The Adventure{suffix}");
                }

                lastP = thisP;

                // handle fullscreen
                bool thisF11 = input.IsKeyF11Pressed();
                if (thisF11 && !lastF11)
                {
                    gameWindow.ToggleFullscreen();
                }

                lastF11 = thisF11;

                if (engine.IsGameOver)
                {
                    gameWindow.SetWindowTitle("The Adventure [GAME OVER]");
                    bool thisR = input.IsKeyRPressed();
                    if (thisR && !lastR)
                    {
                        engine = new Engine(gameRenderer, input);
                        engine.SetupWorld();
                        paused = false;
                        gameWindow.SetWindowTitle("The Adventure");
                    }

                    lastR = thisR;

                    bool thisEscape = input.IsKeyEscapePressed();
                    if (thisEscape && !lastEscape)
                    {
                        quit = true;
                        break;
                    }

                    lastEscape = thisEscape;
                }

                if (!paused && !engine.IsGameOver)
                    engine.ProcessFrame();

                engine.RenderFrame();

                const int margin = 10;
                var winSize = gameWindow.Size;
                string scoreText = $"Score: {engine.Score}";
                int x = winSize.Width - margin;
                int y = margin;
                
                var meas = TextMeasurer.MeasureSize(scoreText, new TextOptions(textRenderer.FONT));
                int textW = (int)Math.Ceiling(meas.Width);
                
                textRenderer.DrawText(scoreText, winSize.Width - margin - textW, margin);
                
                gameRenderer.PresentFrame();

                Thread.Sleep(13);
            }
        }

        sdl.Quit();
    }
}