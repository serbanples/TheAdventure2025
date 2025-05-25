using Silk.NET.SDL;
using Thread = System.Threading.Thread;

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
            var input = new Input(sdl);
            var gameRenderer = new GameRenderer(sdl, gameWindow);
            var engine = new Engine(gameRenderer, input);

            engine.SetupWorld();

            bool quit = false;
            bool paused = false;
            bool lastP = false;
            bool lastF11 = false;
            
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

                if (!paused)
                    engine.ProcessFrame();
                
                engine.RenderFrame();

                Thread.Sleep(13);
            }
        }

        sdl.Quit();
    }
}