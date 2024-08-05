using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Raylib_cs;

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
        int screenWidth = 800;
        int screenHeight = 400;
        var smallWidth = screenWidth / 2;
        var smallHeight = screenHeight / 2;
        

        Raylib.InitWindow(screenWidth, screenHeight, "Hello World");
        string workingDirectory = Environment.CurrentDirectory;
        Raylib.SetTargetFPS(60); 
        RenderTexture2D renderTexture = Raylib.LoadRenderTexture(480,240);
        Raylib.InitAudioDevice();  // Initialize audio device
        Music backgroundMusic = Raylib.LoadMusicStream($"{workingDirectory}/bg.mp3");
        Raylib.PlayMusicStream(backgroundMusic); 

        int animFrames = 0;
        //Image bg = Raylib.LoadImageAnim($"{workingDirectory}/bg.gif", animFrames);
        
        // Set up Camera
        Camera3D camera = new Camera3D();
        camera.Position = new Vector3(-14,10,-10);
        camera.Target = new Vector3(0,0,0);
        camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
        camera.Projection = CameraProjection.Perspective;
        camera.FovY = 22;
           
        // Axis colors
        Color xAxisColor = Color.Red;
        Color yAxisColor = Color.Green;
        Color zAxisColor = Color.Blue;
        float axisLength = 10.0f; 

        Model charModel = Raylib.LoadModel($"{workingDirectory}/char.obj");
        var charPos = new Vector3 (-.5f,.6f,.5f);

        while (!Raylib.WindowShouldClose())
        {
            
            /*
                UPDATE
            */
            Raylib.UpdateMusicStream(backgroundMusic);

            //  MOVE CHAR
            var cameraSpeed = 1.0f;
            if (Raylib.IsKeyDown(KeyboardKey.Q)) camera.Position.Y += cameraSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.W)) camera.Position.Y -= cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Left)) charPos.Z -= cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Right)) charPos.Z += cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Up)) charPos.X += cameraSpeed;
            if (Raylib.IsKeyPressed(KeyboardKey.Down)) charPos.X -= cameraSpeed;
            camera.Target = charPos;



            /*
                DRAW
            */
            //  DRAW texture
            Raylib.BeginTextureMode(renderTexture);
            Raylib.ClearBackground(Color.DarkBlue);
            Raylib.BeginMode3D(camera);

            // Draw Models
            Raylib.DrawModel(charModel,charPos,1.0f,Color.Beige);
            //Raylib.DrawGrid(100,1);
            drawMap();

            // Draw axis lines
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(axisLength, 0, 0), xAxisColor); // X axis
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(0, axisLength, 0), yAxisColor); // Y axis
            Raylib.DrawLine3D(new Vector3(0, 0, 0), new Vector3(0, 0, axisLength), zAxisColor); // Z axis
     
            Raylib.EndMode3D();
            Raylib.EndTextureMode();
            
            // DRAW TO SCREEN
            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(
                renderTexture.Texture,
                new Rectangle(0,0,smallWidth,-smallHeight), // mirrored because texutes use different cooridnates
                new Rectangle(0,0,screenWidth,screenHeight),
                new Vector2(0,0),
                0f,
                Color.White
            );

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