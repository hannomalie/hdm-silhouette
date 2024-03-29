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
using Silhouette.Engine.Effects;
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
         * Die Repräsentation eines Levels im Spiel.
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
        public Vector2 Gravitation {
            get 
            { 
                return _Gravitation;
            } 
            set 
            { 
                _Gravitation = value;
                Physics.Gravity = _Gravitation;
            } 
        }

        private Vector2 _startPosition;
        [DisplayName("Start Position"), Category("General")]
        [Description("Defines the characters starting position.")]
        public Vector2 startPosition { get { return _startPosition; } set { _startPosition = value; } }

        [NonSerialized]
        private float _orientation;
        public float Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        private List<EffectObject> _effects;
        public List<EffectObject> Effects
        {
            get { return _effects; }
            set { _effects = value; }
        }

        public bool isVisible = true;

        [NonSerialized]
        private DebugViewXNA debugView;
        [NonSerialized]
        public bool DebugViewEnabled;
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
        private RenderTargetFlipFlop _flipFlop;

        public RenderTargetFlipFlop FlipFlop
        {
            get { return _flipFlop; }
            set { _flipFlop = value; }
        }
        [NonSerialized]
        private KeyboardState keyboardState;
        [NonSerialized]
        private KeyboardState oldKeyboardState;
        #endregion

        public Level()
        {
            _layerList = new List<Layer>();
        }

        public void Initialize(bool editor, ContentManager content)
        {
            
            this.spriteBatch = new SpriteBatch(GameLoop.gameInstance.GraphicsDevice);
            _flipFlop = new RenderTargetFlipFlop(ref spriteBatch);
            _flipFlop.Initialise();
            _Gravitation = new Vector2(0.0f, 9.8f);
            Physics = new World(_Gravitation);
            debugView = new DebugViewXNA(Level.Physics);
            Camera.initialize(GameSettings.Default.resolutionWidth, GameSettings.Default.resolutionHeight);
            Camera.Position = new Vector2(GameSettings.Default.resolutionWidth / 2, GameSettings.Default.resolutionHeight / 2);
            Camera.Scale = GameSettings.Default.gameCamScale;
            if (editor) 
            {
                ParticleManager.initializeInEditor(content);
            }
            else
            {
                ParticleManager.initialize();
            }

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

            if (Effects == null)
            {
                Effects = new List<EffectObject>();
            }

            foreach (EffectObject eo in Effects)
            {
                //eo.Initialise();
                eo.LoadContent();
            }

            foreach (Layer l in layerList)
            {
                l.loadLayer();
            }
        }

        public void Update(GameTime gameTime)
        {

            CalcOrientation();
            Physics.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            foreach (Layer l in layerList)
            {
                l.updateLayer(gameTime);
            }

            foreach (EffectObject eo in Effects)
            {
                eo.Update(gameTime);
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

                _flipFlop.Draw(this);

                GameLoop.gameInstance.GraphicsDevice.SetRenderTarget(null);
                GameLoop.gameInstance.GraphicsDevice.Clear(Color.White);
                spriteBatch.Begin();

                spriteBatch.Draw(_flipFlop.Result, Vector2.Zero, Color.White);
                Primitives.Instance.drawBoxFilled(spriteBatch, new Rectangle(0, 0, GameSettings.Default.resolutionWidth, 96), Color.Black);
                Primitives.Instance.drawBoxFilled(spriteBatch, new Rectangle(0, GameSettings.Default.resolutionHeight - 96, GameSettings.Default.resolutionWidth, 96), Color.Black);
                /*if (GameLoop.gameInstance.playerInstance.mState != null)
                {
                    Primitives.Instance.drawCircleFilled(spriteBatch, new Vector2(GameLoop.gameInstance.playerInstance.mState.X, GameLoop.gameInstance.playerInstance.mState.Y), 5, Color.Gray);
                }*/
                spriteBatch.End();


            }
            if (DebugViewEnabled)
            {
                debugView.RenderDebugData(ref proj, ref Camera.debugMatrix);
            }
        }

        public void AddPlayer(Layer layer)
        {
            //Player player = new Player();
            Tom player = new Tom();
            player.position = startPosition;
            player.Initialise();
            //player.position = startPosition;
            player.layer = layer;
            GameLoop.gameInstance.playerInstance = player;
            layer.loList.Add(player);
        }

        private void CalcOrientation()
        {
            Vector2 orientationReference = new Vector2(1, 0);

            Vector2 grav = Physics.Gravity;
            Gravitation = grav;
            grav.Normalize();
            orientationReference.Normalize();
            float angle = 0f;
            Vector2.Dot(ref grav, ref orientationReference, out angle);
            //angle = (float) Math.Cos((double) angle);
            Camera.Rotation = angle ;
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
                DebugLogManager.writeToLogFile("LoadLevelFile Exception: " + e.Message);
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
