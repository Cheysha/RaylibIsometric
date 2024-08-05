using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace HelloWorld;

class Program
{
    public static int[,] gameMap = new int[20, 14]{
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1},
        {0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}};
    
    public static void Main()
    {
        int screenWidth = 1280;
        int screenHeight = 720;
        var smallWidth = screenWidth / 1;
        var smallHeight = screenHeight / 1;
        
        Raylib.InitWindow(screenWidth, screenHeight, "Hello World");
        string workingDirectory = Environment.CurrentDirectory;
        Raylib.SetTargetFPS(60); 
        RenderTexture2D renderTexture = Raylib.LoadRenderTexture(smallWidth, smallHeight);
        Raylib.InitAudioDevice();  // Initialize audio device
        Music backgroundMusic = Raylib.LoadMusicStream($"{workingDirectory}/bg.mp3");
        Raylib.PlayMusicStream(backgroundMusic); 
        

        
        // Set up Camera
        Camera3D camera = new Camera3D();
        camera.Target = new Vector3(0,0,0);
        camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
        camera.Projection = CameraProjection.Perspective;
        camera.FovY = 45;
           
        // Axis colors
        Color xAxisColor = Color.Red;
        Color yAxisColor = Color.Green;
        Color zAxisColor = Color.Blue;
        float axisLength = 10.0f; 

        Image bg;
        int animFrames = 0;
        int currentAnimFrame = 0;
        int frameDelay = 6;
        int frameCounter = 0;
        uint nextFrameDataOffset = 0; 
        // because we insist on pointer, which c# hates
        unsafe{ 
                string path = $"{workingDirectory}/bg.gif";
                byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(path);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(pathBytes.Length + 1);
                Marshal.Copy(pathBytes, 0, unmanagedPointer, pathBytes.Length);
                Marshal.WriteByte(unmanagedPointer + pathBytes.Length, 0); // Null-terminate the string
                sbyte* sbytePointer = (sbyte*)unmanagedPointer;
                int* framesPointer = &animFrames;

                // finally load the fucking bg
                bg = Raylib.LoadImageAnim(sbytePointer, framesPointer);
        }
        Texture2D bgtexture = Raylib.LoadTextureFromImage(bg);

        Model charModel = Raylib.LoadModel($"{workingDirectory}/char.obj");
        var charPos = new Vector3 (-.5f,.6f,.5f);
        Vector3 cameraOffset = new Vector3(10,10,10);
        camera.Position = new Vector3(-11,10,10);
        camera.Target = new Vector3(0,0,0);
        while (!Raylib.WindowShouldClose())
        {
            
            /*
                UPDATE
            */
            Raylib.UpdateMusicStream(backgroundMusic);
            Vector3 cameraFoward = Vector3.Normalize(camera.Target - camera.Position);

            // Update bg
            frameCounter++;
            if (frameCounter >= frameDelay){
                // Move to next frame
                // NOTE: If final frame is reached we return to first frame
                currentAnimFrame++;
                if (currentAnimFrame >= animFrames) 
                    currentAnimFrame = 0;

                nextFrameDataOffset = (uint)(bg.Width*bg.Height*currentAnimFrame);
                unsafe{ UpdateTexture(bgtexture, (uint*)bg.Data + nextFrameDataOffset); }
                frameCounter = 0;
            }


            //  MOVE CHAR
            var cameraSpeed = 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.Q)) camera.Position.Y += cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.W)) camera.Position.Y -= cameraSpeed;
            // this will include subtracting the camera angle 
            if (Raylib.IsKeyPressed(KeyboardKey.Left)) charPos.Z -= cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Right)) charPos.Z += cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Up)) charPos.X += cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Down)) charPos.X -= cameraSpeed;
            camera.Target = charPos;
      

            /*
                DRAW
            */
        
            //  DRAW game to texture
            Raylib.BeginTextureMode(renderTexture);
            
            // draw background, this can be changed if want different style
            Raylib.DrawTexturePro(bgtexture,
                                    new Rectangle(0,0,smallWidth,-smallHeight),
                                    new Rectangle(0,0,screenWidth,screenHeight),
                                    new Vector2(0,0),
                                    0f,Color.White); 

            Raylib.ClearBackground(Color.DarkBlue);
            Raylib.BeginMode3D(camera);

            // Draw Models
            Raylib.DrawModel(charModel,charPos,1.0f,Color.Beige);
            drawMap();

            // Draw axis lines
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(axisLength, 0, 0), xAxisColor); // X axis
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(0, axisLength, 0), yAxisColor); // Y axis
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(0, 0, axisLength), zAxisColor); // Z axis
     
            Raylib.EndMode3D();
            Raylib.EndTextureMode();

            // Draw our scales texture to screen, flipped
            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(renderTexture.Texture,
                                    new Rectangle(0,0,smallWidth,-smallHeight),
                                    new Rectangle(0,0,screenWidth,screenHeight),
                                    new Vector2(0,0),
                                    0f,Color.White);

            // DRAW DEBUG
            Raylib.DrawText($"{charPos}",0,0,0,Color.White);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();


        void drawMap(){
            for (int x = 0; x < 14; x++){
                for (int y = 0; y < 20; y++){
                    int tileIndex = gameMap[y, x]; 
                    if(tileIndex == 1){
                        // Determine the color based on the sum of x and y
                        Color color = ((x + y) % 2 == 0) ? Color.White : Color.Black;
                        Vector3 position = new Vector3(x + 0.5f, 0, y + 0.5f);
                        Raylib.DrawCube(position, 1, 0.1f, 1, color);
                    }
                }
            }
        }
    }
}