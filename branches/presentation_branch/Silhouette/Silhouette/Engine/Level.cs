using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Runtime.Serialization.Formatters.Binary;

using Silhouette;
using Silhouette.GameMechs;
using Silhouette.Engine.Manager;
using Silhouette.Engine.Screens;
using Silhouette.Engine;
using System.ComponentModel;

//Physik-Engine Klassen
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace Silhouette.Engine
{
    [Serializable]
    public partial class Level
    {
        /* Sascha:
         * Die Repr�sentation eines Levels im Spiel.
        */

        #region Definitions
        private string _name;
        [DisplayName("Name"), Category("General")]
        [Description("The name of the level.")]
        public string name { get { return _name; } set { _name = value; } }

        [NonSerialized]
        private string _contentPath;
        [DisplayName("Content Path"), Category("General")]
        [Description("The path to the content of the Level. All textures will be safed and loaded relative to this path.")]
        public string contentPath { get { return _contentPath; } set { _contentPath = value; } }

        [NonSerialized]
        public static World Physics;

        private const float _PixelPerMeter = 100.0f;
        public static float PixelPerMeter { get { return _PixelPerMeter; } }

        private Vector2 _Gravitation;
        [DisplayName("Gravition"), Category("General")]
        [Description("The Gravitation controls the force vectors applied to every dynamic fixture.")]
        public Vector2 Gravitation { get { return _Gravitation; } set { _Gravitation = value; } }

        private Vector2 _startPosition;
        [DisplayName("Start Position"), Category("General")]
        [Description("Defines the characters starting position.")]
        public Vector2 startPosition { get { return _startPosition; } set { _startPosition = value; } }

        public bool isVisible = true;

        [NonSerialized]
        private DebugViewXNA debugView;
        [NonSerialized]
        private bool DebugViewEnabled;
        [NonSerialized]
        private bool GraphicsEnabled;
        [NonSerialized]
        SpriteBatch spriteBatch;

        private List<Layer> _layerList;
        [DisplayName("Layers"), Category("Layer")]
        [Description("The Layers of the Level.")]
        public List<Layer> layerList { get { return _layerList; } }

        [NonSerialized]
        private Matrix proj;
        [NonSerialized]
        public RenderTarget2D[] renderTargets;
        [NonSerialized]
        private KeyboardState keyboardState;
        [NonSerialized]
        private KeyboardState oldKeyboardState;
        [NonSerialized]
        private Vector2 camMovement;
        [NonSerialized]
        private ProjectMercury.Emitters.CircleEmitter rainEmitter;
        #endregion

        public Level()
        {
            _layerList = new List<Layer>();
        }

        public void Initialize()
        {

            renderTargets = new RenderTarget2D[5];
            renderTargets[1] = new RenderTarget2D(GameLoop.gameInstance.GraphicsDevice, GameSettings.Default.resolutionWidth, GameSettings.Default.resolutionHeight);
            renderTargets[2] = new RenderTarget2D(GameLoop.gameInstance.GraphicsDevice, GameSettings.Default.resolutionWidth, GameSettings.Default.resolutionHeight);
            renderTargets[3] = new RenderTarget2D(GameLoop.gameInstance.GraphicsDevice, GameSettings.Default.resolutionWidth, GameSettings.Default.resolutionHeight);
            renderTargets[4] = new RenderTarget2D(GameLoop.gameInstance.GraphicsDevice, GameSettings.Default.resolutionWidth, GameSettings.Default.resolutionHeight);

            this.spriteBatch = new SpriteBatch(GameLoop.gameInstance.GraphicsDevice);
            _Gravitation = new Vector2(0.0f, 9.8f);
            Physics = new World(_Gravitation);
            debugView = new DebugViewXNA(Level.Physics);
            Camera.initialize(GameSettings.Default.resolutionWidth, GameSettings.Default.resolutionHeight);
            Camera.Position = new Vector2(GameSettings.Default.resolutionWidth / 2, GameSettings.Default.resolutionHeight / 2);
            Camera.Scale = 0.4f;
            ParticleManager.initialize();

            this.GraphicsEnabled = true;
            this.DebugViewEnabled = false;

            this._contentPath = Path.Combine(Directory.GetCurrentDirectory(), "Content");

            foreach (Layer l in layerList)
            {
                l.initializeLayer();
            }
        }

        public void LoadContent()
        {
            proj = Matrix.CreateOrthographicOffCenter(0, GameSettings.Default.resolutionWidth / PixelPerMeter, GameSettings.Default.resolutionHeight / PixelPerMeter, 0, 0, 1);

            Layer playerLayer = getLayerByName("Player");
            Layer bossLayer = getLayerByName("Boss");

            if (playerLayer != null)
                AddPlayer(playerLayer);
            if (bossLayer != null)
                AddBoss(bossLayer);

            foreach (Layer l in layerList)
            {
                l.loadLayer();
            }
        }

        public void Update(GameTime gameTime)
        {

            Physics.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            foreach (Layer l in layerList)
            {
                l.updateLayer(gameTime);
            }

            EffectManager.Update(gameTime);

            #region DebugView
            keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.F1) && oldKeyboardState.IsKeyUp(Keys.F1))
                DebugViewEnabled = !DebugViewEnabled;
            if (keyboardState.IsKeyDown(Keys.F2) && oldKeyboardState.IsKeyUp(Keys.F2))
                EnableOrDisableFlag(DebugViewFlags.DebugPanel);
            if (keyboardState.IsKeyDown(Keys.F3) && oldKeyboardState.IsKeyUp(Keys.F3))
                EnableOrDisableFlag(DebugViewFlags.Shape);
            if (keyboardState.IsKeyDown(Keys.F4) && oldKeyboardState.IsKeyUp(Keys.F4))
                EnableOrDisableFlag(DebugViewFlags.Joint);
            if (keyboardState.IsKeyDown(Keys.F5) && oldKeyboardState.IsKeyUp(Keys.F5))
                EnableOrDisableFlag(DebugViewFlags.AABB);
            if (keyboardState.IsKeyDown(Keys.F6) && oldKeyboardState.IsKeyUp(Keys.F6))
                EnableOrDisableFlag(DebugViewFlags.CenterOfMass);
            if (keyboardState.IsKeyDown(Keys.F7) && oldKeyboardState.IsKeyUp(Keys.F7))
                EnableOrDisableFlag(DebugViewFlags.Pair);
            if (keyboardState.IsKeyDown(Keys.F8) && oldKeyboardState.IsKeyUp(Keys.F8))
            {
                EnableOrDisableFlag(DebugViewFlags.ContactPoints);
                EnableOrDisableFlag(DebugViewFlags.ContactNormals);
            }
            if (keyboardState.IsKeyDown(Keys.F9) && oldKeyboardState.IsKeyUp(Keys.F9))
                EnableOrDisableFlag(DebugViewFlags.PolygonPoints);
            if (keyboardState.IsKeyDown(Keys.F10) && oldKeyboardState.IsKeyUp(Keys.F10))
                GraphicsEnabled = !GraphicsEnabled;

            oldKeyboardState = keyboardState;
            #endregion
        }

        public void Draw()
        {
            if (!isVisible)
                return;

            if (GraphicsEnabled)
            {
                GameLoop.gameInstance.GraphicsDevice.SetRenderTarget(renderTargets[1]);
                GameLoop.gameInstance.graphics.PreferMultiSampling = true;
                
                foreach (Layer l in layerList)
                {
                    Vector2 oldCameraPosition = Camera.Position;
                    Camera.Position *= l.ScrollSpeed;

                    Effect effect = l.getShaderByType(l.shaderType);

                    // Testweise MotionBlur eingef�gt f�r alle Layer au�er Player-Layer
                    if (l != GameLoop.gameInstance.playerInstance.layer && false)
                    {
                        effect = EffectManager.MotionBlur(GameLoop.gameInstance.playerInstance.charRect.Body.LinearVelocity*(new Vector2(1,1)+l.ScrollSpeed));
                    }
                    spriteBatch.Begin(SpriteSortMode.Deferred, l.getBlendStateByEffect(l.shaderType), null, null, null, effect, Camera.matrix);
                    l.drawLayer(spriteBatch);
                    spriteBatch.End();

                    
                    camMovement = oldCameraPosition - Camera.Position;
                    Camera.Position = oldCameraPosition;
                }


                GameLoop.gameInstance.GraphicsDevice.SetRenderTarget(renderTargets[2]);
                GameLoop.gameInstance.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, EffectManager.Godrays());
                spriteBatch.Draw(renderTargets[1], Vector2.Zero, Color.White);
                spriteBatch.End();

                GameLoop.gameInstance.GraphicsDevice.SetRenderTarget(renderTargets[3]);
                GameLoop.gameInstance.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, EffectManager.Bloom());
                spriteBatch.Draw(renderTargets[2], Vector2.Zero, Color.White);
                spriteBatch.End();

                GameLoop.gameInstance.GraphicsDevice.SetRenderTarget(renderTargets[4]);
                GameLoop.gameInstance.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, EffectManager.VignettenBlur());
                spriteBatch.Draw(renderTargets[2], Vector2.Zero, Color.White);
                spriteBatch.Draw(renderTargets[1], Vector2.Zero, Color.White);
                spriteBatch.Draw(renderTargets[3], Vector2.Zero, Color.White);

                spriteBatch.End();

                GameLoop.gameInstance.GraphicsDevice.SetRenderTarget(null);
                GameLoop.gameInstance.GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null);
                spriteBatch.Draw(renderTargets[4], Vector2.Zero, Color.White);

                spriteBatch.End();

                spriteBatch.Begin();
                Primitives.Instance.drawBoxFilled(spriteBatch, new Rectangle(0, 0, GameSettings.Default.resolutionWidth, 96), Color.Black);
                Primitives.Instance.drawBoxFilled(spriteBatch, new Rectangle(0, GameSettings.Default.resolutionHeight - 96, GameSettings.Default.resolutionWidth, 96), Color.Black);

                spriteBatch.End();


            }
            if (DebugViewEnabled)
            {
                debugView.RenderDebugData(ref proj, ref Camera.debugMatrix);
            }
        }

        public void AddPlayer(Layer layer)
        {
            Player player = new Player();
            player.Initialise();
            player.position = startPosition;
            player.layer = layer;
            GameLoop.gameInstance.playerInstance = player;
            layer.loList.Add(player);
        }

        public void AddBoss(Layer layer)
        {
            EndBoss boss = new EndBoss();
            boss.Initialise();

            boss.position = new Vector2(900, 0);
            boss.layer = layer;


            layer.loList.Add(boss);
        }

        public static Level LoadLevelFile(string levelPath)
        {
            try
            {
                FileStream file = FileManager.LoadLevelFile(levelPath);
                BinaryFormatter serializer = new BinaryFormatter();
                Level level = (Level)serializer.Deserialize(file);
                file.Close();
                return level;
            }
            catch (Exception e)
            {
                return new Level();
            }
        }

        #region DebugViewMethods
        private void EnableOrDisableFlag(DebugViewFlags flag)
        {
            if ((debugView.Flags & flag) == flag)
                debugView.RemoveFlags(flag);
            else
                debugView.AppendFlags(flag);
        }
        #endregion
    }
}
