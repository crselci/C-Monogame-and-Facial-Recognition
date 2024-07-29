using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Emgu.CV;
using Color = Microsoft.Xna.Framework.Color;
using Emgu.CV.CvEnum;
using System.Threading;
using Emgu.CV.Structure;
using FaceDetect;
using ImageProcess;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Project3
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Image<Bgr, byte> faceImage;
       
        Texture2D targetSprite;
        Texture2D crosshairsSprite;
        Texture2D backgroundSprite;
        SpriteFont galleryFont;
         
        const int TR = 45;
        const int CH = 25;
        int score = 0;
        Random r = new Random();
        
        Vector2 targetPos;
        Vector2 targetMoving;
        Vector2 crosshairs;
        Color w = Color.White;
        
        MouseState mstate;
        bool mrel = true;
        bool hit = false;

        int xpos;
        int ypos;

        double timer = 0;

        Texture2D cameraTexture;
        VideoCapture capture;
        Mat frame;
        Thread cameraThread;
        bool isFrameReady = false;
        object frameLock = new object();
        bool webcamPicture = false;
        bool pictureInvalid = false;
        bool threadStart = true;

        Vector2 uploadPos;
        Vector2 webcamPos;
        Vector2 webcamHitPos;
        
        Texture2D upload;
        Texture2D webcam;
        int decision = 0;
        bool choiceMade = false;
        const int boxSize = 300;

        SoundEffect shoot;
        SoundEffect bgMusic;

        double soundTimer = 107;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            xpos = r.Next(TR, _graphics.PreferredBackBufferWidth - TR);
            ypos = r.Next(TR, _graphics.PreferredBackBufferHeight - TR);
            mstate = Mouse.GetState();
            targetPos = new Vector2(xpos, ypos);
            targetMoving = new Vector2(targetPos.X - TR, targetPos.Y - TR);
            crosshairs = new Vector2(mstate.Position.ToVector2().X, mstate.Position.ToVector2().Y);
            uploadPos = new Vector2(0, (_graphics.PreferredBackBufferHeight-boxSize)/2);
            webcamPos = new Vector2(_graphics.PreferredBackBufferWidth - boxSize, (_graphics.PreferredBackBufferHeight - boxSize) / 2);
            webcamHitPos = new Vector2((_graphics.PreferredBackBufferWidth - boxSize)+boxSize/2, ((_graphics.PreferredBackBufferHeight - boxSize) / 2)+boxSize / 2);
           
            
        }

        private void CaptureAndSaveImage()
        {
            Mat frame = new Mat();

            capture.Read(frame);


            faceImage = FaceDetector.DetectAndCropFace(frame.ToImage<Bgr, byte>());

            if (FaceDetector.approved)
            {
                targetSprite = ImageProcessor.SaveCircularImage(GraphicsDevice, faceImage.ToBitmap(),  45);
                UnloadContent();
            }
            else
            {
                pictureInvalid = true;
            }
        }

        private void CameraCaptureLoop()
        {
            while (!webcamPicture)
            {

                capture.Read(frame);

                lock (frameLock)
                {
                    isFrameReady = true;
                }
            }
        }

        protected override void UnloadContent()
        {
            // Call Dispose method to release resources
            Dispose(true);
            base.UnloadContent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stop the camera thread and release resources
                webcamPicture = true;
                try
                {
                    cameraThread.Join();
                }// Wait for the thread to finish
                catch{
                    cameraThread = null;
                }
               
                if (capture != null)
                {
                    capture.Dispose();  // Dispose of the VideoCapture object
                    capture = null;
                }
                if (frame != null)
                {
                    frame.Dispose();
                    frame = null;
                }
                if (cameraTexture != null)
                {
                    cameraTexture.Dispose();
                    cameraTexture = null;
                }
            }
        }

        protected override void Initialize()
        {
            
           
            capture = new VideoCapture(0);
            capture.Set(CapProp.FrameWidth, _graphics.PreferredBackBufferWidth);  // Adjust as needed
            capture.Set(CapProp.FrameHeight, _graphics.PreferredBackBufferHeight);
            frame = new Mat();
            cameraThread = new Thread(CameraCaptureLoop);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            cameraTexture = new Texture2D(GraphicsDevice, (int)capture.Width, (int)capture.Height);

            crosshairsSprite = Content.Load<Texture2D>("crosshairs");
            backgroundSprite = Content.Load<Texture2D>("sky");
            galleryFont = Content.Load<SpriteFont>("galleryFont");
            upload = Content.Load<Texture2D>("upload");
            webcam = Content.Load<Texture2D>("webcam");
            shoot = Content.Load<SoundEffect>("Document 1");
            bgMusic = Content.Load<SoundEffect>("bgSound");


            base.Initialize();
        }

        protected override void LoadContent()
        {
            bgMusic.Play();
        }

        protected override void Update(GameTime gameTime)
        {

            if (decision == 2)
            {

                if (threadStart) { cameraThread.Start(); threadStart = false; }
                if (!webcamPicture)
                {

                    lock (frameLock)
                    {
                        if (isFrameReady)
                        {
                            int width = frame.Width;
                            int height = frame.Height;
                            byte[] frameData = new byte[width * height * 4];
                            byte[] frameBytes = frame.ToImage<Bgr, byte>().Bytes;

                            for (int i = 0; i < frameBytes.Length; i += 3)
                            {
                                int index = (i / 3) * 4;
                                frameData[index] = frameBytes[i + 2];    // Red
                                frameData[index + 1] = frameBytes[i + 1];  // Green
                                frameData[index + 2] = frameBytes[i];      // Blue
                                frameData[index + 3] = 255;                 // Alpha
                            }

                            // Update the texture with the frame data
                            cameraTexture.SetData(frameData);

                            isFrameReady = false;
                        }
                    }
                }
            }
            else if (decision == 1)
            {

            }



            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            mstate = Mouse.GetState();

            if (soundTimer > 0)
            {
                soundTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                soundTimer = 107;
                //bgMusic.Play();
            }

            if (timer > 0)
            {
                timer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                timer = 0;
            }


            if (mstate.LeftButton == ButtonState.Pressed && mrel == true)
            {
                
                

                //float mouseUploadtDist = Vector2.Distance(uploadPos, mstate.Position.ToVector2());
                Vector2 corner = mstate.Position.ToVector2();
                if (corner.Y <= _graphics.PreferredBackBufferWidth && corner.X <= _graphics.PreferredBackBufferWidth && corner.X >=0 && corner.Y >= 0) { shoot.Play(); Console.WriteLine("distance: " + corner.X); }
                    
                if (!choiceMade) {
                   /* if (mouseUploadtDist < boxSize)
                    {
                        decision = 1;
                        UnloadContent();
                    }*/
                    if(corner.X >= webcamPos.X && corner.X<=webcamPos.X+boxSize && corner.Y >= webcamPos.Y && corner.Y<=webcamPos.Y+boxSize)
                    {
                        decision = 2;
                        choiceMade = true;
                    }
                }

                if (!webcamPicture && !threadStart)
                {
                    CaptureAndSaveImage();
                }

                float mouseTargetDist = Vector2.Distance(targetPos, mstate.Position.ToVector2());
                if (mouseTargetDist < TR)
                {
                    if (timer > 0)
                    {
                        score++;
                    }
                    else
                    {
                        score = 0;
                        timer = 5;
                    }
                    hit = true;
                }
                mrel = false;
            }
            if (hit == true)
            {
                xpos = r.Next(TR, _graphics.PreferredBackBufferWidth - TR);
                ypos = r.Next(TR, _graphics.PreferredBackBufferHeight - TR);
                
                targetPos = new Vector2(xpos, ypos);
                targetMoving = new Vector2(targetPos.X - TR, targetPos.Y - TR);
                hit = false;
            }
            if (mstate.LeftButton == ButtonState.Released)
            {
                mrel = true;
            }






            crosshairs = new Vector2(mstate.Position.ToVector2().X, mstate.Position.ToVector2().Y);
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (choiceMade)
            {
                if (!webcamPicture)
                {
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(cameraTexture, Vector2.Zero, Color.White);
                    _spriteBatch.DrawString(galleryFont, "Press anywhere to take webcamPicture:", new Vector2(0, 0), w);
                    if (pictureInvalid) { _spriteBatch.DrawString(galleryFont, "Picture invalid, make sure there are no obstacles", new Vector2(0, 28), w); }
                    
                }
                else
                {
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(backgroundSprite, new Vector2(0, 0), w);
                    _spriteBatch.DrawString(galleryFont, "Score: " + score.ToString(), new Vector2(_graphics.PreferredBackBufferWidth / 2 - (24 * 7) / 2, 0), w);
                    _spriteBatch.DrawString(galleryFont, "Timer: " + Math.Ceiling(timer).ToString(), new Vector2(0, 0), w);
                    if (timer > 0)
                    {
                        _spriteBatch.Draw(targetSprite, targetMoving, w);
                    }
                    else
                    {
                        Vector2 tryPos = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
                        xpos = _graphics.PreferredBackBufferWidth / 2 + 75 + TR;
                        ypos = _graphics.PreferredBackBufferHeight / 2;
                        targetPos = new Vector2(xpos, ypos);
                        _spriteBatch.DrawString(galleryFont, "Play:", new Vector2(tryPos.X, tryPos.Y), w);
                        _spriteBatch.Draw(targetSprite, new Vector2(targetPos.X - TR, targetPos.Y - TR), Color.Green);
                    }

                }
            }
            else
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(upload, new Vector2(uploadPos.X, uploadPos.Y), w);
                _spriteBatch.Draw(webcam, new Vector2(webcamPos.X, webcamPos.Y), w);
            }
            _spriteBatch.Draw(crosshairsSprite, new Vector2(crosshairs.X - CH, crosshairs.Y - CH), w);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
