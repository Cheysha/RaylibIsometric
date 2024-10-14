using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Raylib_CsLo;
//using static Raylib_cs.Raylib;

namespace HelloWorld;

class Program
{
    public const int MAPWIDTH = 14; 
    public const int MAPHEIGHT = 20; 

    public static int screenWidth = 1280;
    public static int screenHeight = 720;
    public static float scaledWidth = screenWidth / 2;
    public static float scaledHeight = screenHeight / 2;
    public static Color xAxisColor = CustomColors.RED;
    public static Color yAxisColor = CustomColors.GREEN;
    public static Color zAxisColor = CustomColors.BLUE;
    public static float axisLength = 10.0f; 
    public static float zOffset = 0.6f;

    public static int[,] gameMap = new int[MAPHEIGHT, MAPWIDTH]{
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1},
        {0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1},
        {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0},
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
        /*
            INIT
        */

        // Raylib Init
        Raylib.InitWindow(screenWidth, screenHeight, "Hello World");
        string workingDirectory = Environment.CurrentDirectory;
        Raylib.SetTargetFPS(60); 
        RenderTexture renderTexture = Raylib.LoadRenderTexture((int)scaledWidth, (int)scaledHeight);
        Raylib.InitAudioDevice();  
        Music backgroundMusic = Raylib.LoadMusicStream($"{workingDirectory}/bg.mp3");
        Raylib.PlayMusicStream(backgroundMusic); 
        
        // Set up Camera
        Camera3D camera = new Camera3D();
        camera.up = new Vector3(0.0f, 1.0f, 0.0f); // this makes y up!!
        camera.projection = (int)CameraProjection.CAMERA_PERSPECTIVE;
        camera.fovy = 45;
           
        // Setting up background
        Image bg;
        int animFrames;
        int currentAnimFrame = 0;
        int frameDelayCycles = 4;
        int delayCounter = 0;
        uint nextFrameDataOffset;
        var cameraSpeed = 1.0f;
        var playerSpeed = 1.0f;

        bool battling = false;
        string path = $"{workingDirectory}/bg.gif";

        //Load BG
        unsafe{ bg = Raylib.LoadImageAnim(path, &animFrames); }
        Texture bgtexture = Raylib.LoadTextureFromImage(bg);
        
        // Load models
        Model charModel = Raylib.LoadModel($"{workingDirectory}/char.obj");
        var charPos = new Vector2 (1, 1);
        var emnPos = new Vector2(8, 6);

        // set up camera
        Vector3 cameraOffset = new Vector3(10,10,10);
        camera.position = new Vector3(-11,10,10);
        camera.target = new Vector3(0,0,0);

        
        /*
                GAME LOOP
        */
        while (!Raylib.WindowShouldClose())
        {
            if(Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)){battling = false;} // failsafe to stop battle

            // Update the main game window
            if (!battling){
                Raylib.UpdateMusicStream(backgroundMusic);

                // Update bg
                delayCounter++;
                if (delayCounter >= frameDelayCycles){
                    currentAnimFrame++;
                    if (currentAnimFrame >= animFrames) 
                        currentAnimFrame = 0;
                    nextFrameDataOffset = (uint)(bg.width*bg.height*currentAnimFrame);
                    unsafe{ Raylib.UpdateTexture(bgtexture, (uint*)bg.data + nextFrameDataOffset); }
                    delayCounter = 0;
                }

                // Update Input 
                var cahrMovementVector = Vector2.Zero;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_Q)) 
                    camera.position.Y += cameraSpeed;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) 
                    camera.position.Y -= cameraSpeed;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT)) 
                    cahrMovementVector.Y = -playerSpeed;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT)) 
                    cahrMovementVector.Y = playerSpeed;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP)) 
                    cahrMovementVector.X = playerSpeed;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN)) 
                    cahrMovementVector.X = -playerSpeed;

                // Update State
                if(cahrMovementVector != Vector2.Zero){
                    var charPos2 = new Vector2(charPos.X + cahrMovementVector.X, charPos.Y + cahrMovementVector.Y);
                    var emnPos2 = new Vector2(emnPos.X, emnPos.Y);

                    if (charPos2.X >= 0 && charPos2.X <= MAPWIDTH && charPos2.Y >= 0 && charPos2.Y <= MAPHEIGHT){// keep char in bounds
                        if(checkFloorTile(charPos2)){
                            if(charPos2 != emnPos2){
                                charPos.X = charPos2.X;
                                charPos.Y = charPos2.Y;
                            }
                        else battling = true;
                        }
                    } 
                }
                camera.target = new Vector3(charPos.X,zOffset,charPos.Y);
            } // END NOT BATTLE
            
            // Update the Battle Window
            if(battling){

            }

            /*
                DRAW
            */

            // Render 
            Raylib.BeginTextureMode(renderTexture);

            Raylib.DrawTexturePro(  texture: bgtexture,
                                    source: new Rectangle(0,0,scaledWidth,-scaledHeight),
                                    dest: new Rectangle(0,0,screenWidth,screenHeight),
                                    origin: new Vector2(0,0),
                                    rotation:0f, tint: CustomColors.WHITE); 
                                    
            Raylib.ClearBackground(CustomColors.WHITE);
            
            // Render Models, Map, Axis
            Raylib.BeginMode3D(camera);

            Raylib.DrawModel(   model:charModel,
                                position: new Vector3(charPos.X, zOffset, charPos.Y),
                                scale:1.0f, tint: CustomColors.BEIGE);
            Raylib.DrawModel(   model: charModel,
                                position: new Vector3(emnPos.X, zOffset, emnPos.Y),
                                scale: 1.0f, tint: CustomColors.RED);

            drawMap();
            drawAxisLines();

            Raylib.EndMode3D();
            Raylib.EndTextureMode();

            // Draw our scales texture to screen, flipped
            Raylib.BeginDrawing();
            Raylib.DrawTexturePro(  texture: renderTexture.texture,
                                    source: new Rectangle(0,0,scaledWidth,-scaledHeight),
                                    dest: new Rectangle(0,0,screenWidth,screenHeight),
                                    origin: new Vector2(0,0),
                                    rotation: 0f,
                                    tint: CustomColors.WHITE);
            
            // Draw the battle window
            if(battling){
            Raylib.DrawRectangleV(  position: new Vector2(screenWidth /2 - screenWidth /4, screenHeight /2 - screenHeight /4),
                                    size: new Vector2(screenWidth /2, screenHeight /2),
                                    color: CustomColors.BLUE
            );
            }
            
            // DRAW DEBUG
            Raylib.DrawText(text: $"{charPos}", posX: 0, posY: 0, fontSize: 20, color: CustomColors.WHITE);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();


        void drawMap(){
            for (int x = 0; x < MAPWIDTH; x++){
                for (int y = 0; y < MAPHEIGHT; y++){
                    int tileId = gameMap[y, x]; 
                    if(tileId == 1){
                        // Determine the color based on the sum of x and y, checkerboard algo found on chatgpt
                        Color color = ((x + y) % 2 == 0) ? CustomColors.WHITE : CustomColors.DARKGRAY;
                        Vector3 position = new Vector3(x, 0, y);
                        Raylib.DrawCube(position: position, width: 1, height: 0.1f, length: 1, color: color);
                    } else continue;
                }
            }
        }
        void drawAxisLines(){
                // X axis
            Raylib.DrawLine3D(  startPos: new Vector3(0, 0, 0),
                                endPos:  new Vector3(axisLength, 0, 0), 
                                color: xAxisColor); 
                // Y axis
            Raylib.DrawLine3D(  startPos: new Vector3(0, 0, 0),
                                endPos: new Vector3(0, axisLength, 0),
                                color: yAxisColor); 
                // Z axis
            Raylib.DrawLine3D(  startPos: new Vector3(0, 0, 0), 
                                endPos: new Vector3(0, 0, axisLength),
                                color: zAxisColor); 
        }
        bool checkFloorTile(Vector2 position){
            if(gameMap[(int)position.Y,(int)position.X ] == 1)
                return true;
            else return false;
        }
    }
    
}
