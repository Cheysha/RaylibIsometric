using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_CsLo;
//using static Raylib_cs.Raylib;

namespace HelloWorld;

class Program
{
    public static int[,] gameMap = new int[20, 14]{
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1},
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1},
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
        var smallWidth = screenWidth / 2;
        var smallHeight = screenHeight / 2;
        Color xAxisColor = CustomColors.RED;
        Color yAxisColor = CustomColors.GREEN;
        Color zAxisColor = CustomColors.BLUE;
        float axisLength = 10.0f; 
        
        // Raylib Init
        Raylib.InitWindow(screenWidth, screenHeight, "Hello World");
        string workingDirectory = Environment.CurrentDirectory;
        Raylib.SetTargetFPS(60); 
        RenderTexture renderTexture = Raylib.LoadRenderTexture(smallWidth, smallHeight);
        Raylib.InitAudioDevice();  // Initialize audio device
        Music backgroundMusic = Raylib.LoadMusicStream($"{workingDirectory}/bg.mp3");
        Raylib.PlayMusicStream(backgroundMusic); 
        
        // Set up Camera
        Camera3D camera = new Camera3D();
        camera.target = new Vector3(0,0,0);
        camera.up = new Vector3(0.0f, 1.0f, 0.0f);
        camera.projection = (int)CameraProjection.CAMERA_PERSPECTIVE;
        camera.fovy = 45;
           
        // Setting up background
        Image bg;
        int animFrames = 0;
        int currentAnimFrame = 0;
        int frameDelay = 6;
        int frameCounter = 0;
        uint nextFrameDataOffset = 0; 
        unsafe{ // because we insist on pointer, which c# hates
                string path = $"{workingDirectory}/bg.gif";
                byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(path);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(pathBytes.Length + 1);
                Marshal.Copy(pathBytes, 0, unmanagedPointer, pathBytes.Length);
                Marshal.WriteByte(unmanagedPointer + pathBytes.Length, 0); // Null-terminate the string
                sbyte* sbytePointer = (sbyte*)unmanagedPointer;
                int* framesPointer = &animFrames;
                bg = Raylib.LoadImageAnim(sbytePointer, framesPointer);   // finally load the fucking bg
        }
        Texture bgtexture = Raylib.LoadTextureFromImage(bg);
        
        // Load models
        Model charModel = Raylib.LoadModel($"{workingDirectory}/char.obj");
        var charPos = new Vector3 (-.5f,.6f,.5f);

        // set up camera
        Vector3 cameraOffset = new Vector3(10,10,10);
        camera.position = new Vector3(-11,10,10);
        camera.target = new Vector3(0,0,0);
        
        /*
                GAME LOOP
        */
        while (!Raylib.WindowShouldClose())
        {
            Raylib.UpdateMusicStream(backgroundMusic);
            Vector3 cameraFoward = Vector3.Normalize(camera.target - camera.position);

            // Update bg
            frameCounter++;
            if (frameCounter >= frameDelay){
                currentAnimFrame++;

            if (currentAnimFrame >= animFrames) 
                currentAnimFrame = 0;
                
            nextFrameDataOffset = (uint)(bg.width*bg.height*currentAnimFrame);
            unsafe{ Raylib.UpdateTexture(bgtexture, (uint*)bg.data + nextFrameDataOffset); }
            frameCounter = 0;
            }
            

            //  MOVE CHAR
            var cameraSpeed = 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_Q)) camera.position.Y += cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) camera.position.Y -= cameraSpeed;
            // this will include subtracting the camera angle 
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT)) charPos.Z -= cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT)) charPos.Z += cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP)) charPos.X += cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN)) charPos.X -= cameraSpeed;
            camera.target = charPos;
      

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
                                    0f,CustomColors.WHITE); 

            Raylib.ClearBackground(CustomColors.DARKBLUE);
            Raylib.BeginMode3D(camera);

            // Draw Models
            Raylib.DrawModel(charModel,charPos,1.0f,CustomColors.BEIGE);
            drawMap();

            // Draw axis lines
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(axisLength, 0, 0), xAxisColor); // X axis
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(0, axisLength, 0), yAxisColor); // Y axis
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(0, 0, axisLength), zAxisColor); // Z axis
     
            Raylib.EndMode3D();
            Raylib.EndTextureMode();

            // Draw our scales texture to screen, flipped
            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(renderTexture.texture,
                                    new Rectangle(0,0,smallWidth,-smallHeight),
                                    new Rectangle(0,0,screenWidth,screenHeight),
                                    new Vector2(0,0),
                                    0f,CustomColors.WHITE);

            // DRAW DEBUG
            Raylib.DrawText($"{charPos}",0,0,20,CustomColors.WHITE);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();


        void drawMap(){
            for (int x = 0; x < 14; x++){
                for (int y = 0; y < 20; y++){
                    int tileIndex = gameMap[y, x]; 
                    if(tileIndex == 1){
                        // Determine the color based on the sum of x and y
                        Color color = ((x + y) % 2 == 0) ? CustomColors.WHITE : CustomColors.BLACK;
                        Vector3 position = new Vector3(x + 0.5f, 0, y + 0.5f);
                        Raylib.DrawCube(position, 1, 0.1f, 1, color);
                    }
                }
            }
        }
    }
}