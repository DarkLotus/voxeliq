﻿/*
 * Copyright (C) 2011-2012 voxeliq project 
 *
 */

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using VolumetricStudios.VoxeliqGame.Common.Logging;
using VolumetricStudios.VoxeliqGame.Debugging;
using VolumetricStudios.VoxeliqGame.Environment;
using VolumetricStudios.VoxeliqGame.Graphics;
using VolumetricStudios.VoxeliqGame.Universe;

namespace VolumetricStudios.VoxeliqGame.Input
{
    /// <summary>
    /// Interface that allows interracting with the input manager.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Should the game capture mouse?
        /// </summary>
        bool CaptureMouse { get; }

        /// <summary>
        /// Should the mouse cursor centered on screen?
        /// </summary>
        bool CursorCentered { get; }
    }

    /// <summary>
    /// Handles user mouse & keyboard input.
    /// </summary>
    public class InputManager : GameComponent, IInputManager
    {
        // properties.
        public bool CaptureMouse { get; private set; } // Should the game capture mouse?
        public bool CursorCentered { get; private set; } // Should the mouse cursor centered on screen?

        // previous input states.
        private MouseState _previousMouseState; 
        private KeyboardState _previousKeyboardState;

        // required services.
        private IWorldService _world;
        private IPlayer _player;
        private IGraphicsManager _graphicsManager;       
        private ICameraControlService _cameraController;        
        private IInGameDebuggerService _ingameDebuggerService;
        private IFogService _fogService;
        private ISkyService _skyService;
        private IStatisticsGraphs _statisticsGraphs;

        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        /// <summary>
        /// Creates a new input manager.
        /// </summary>
        /// <param name="game"></param>
        public InputManager(Game game)
            : base(game)
        {
            this.Game.Services.AddService(typeof(IInputManager), this); // export service.

            this.CaptureMouse = true; // capture the mouse by default.
            this.CursorCentered = true; // center the mouse by default.        
        }

        /// <summary>
        /// Initializes the input manager.
        /// </summary>
        public override void Initialize()
        {
            Logger.Trace("init()");

            // import required services.
            this._world = (IWorldService) this.Game.Services.GetService(typeof (IWorldService));
            this._player = (IPlayer) this.Game.Services.GetService(typeof (IPlayer));
            this._graphicsManager = (IGraphicsManager)this.Game.Services.GetService(typeof(IGraphicsManager));
            this._cameraController = (ICameraControlService)this.Game.Services.GetService(typeof(ICameraControlService));
            this._ingameDebuggerService = (IInGameDebuggerService)this.Game.Services.GetService(typeof(IInGameDebuggerService));
            this._fogService = (IFogService) this.Game.Services.GetService(typeof (IFogService));
            this._skyService = (ISkyService)this.Game.Services.GetService(typeof(ISkyService));
            this._statisticsGraphs = (IStatisticsGraphs) this.Game.Services.GetService(typeof (IStatisticsGraphs));

            #if DEBUG // if in debug mode, print debug-keys.
                this.PrintDebugKeys();
            #endif

            // get current mouse & keyboard states.
            this._previousKeyboardState = Keyboard.GetState();
            this._previousMouseState = Mouse.GetState();

            base.Initialize();
        }

        /// <summary>
        /// Handles input updates.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            this.ProcessMouse();
            this.ProcessKeyboard(gameTime);
        }

        /// <summary>
        /// Processes mouse input by user.
        /// </summary>
        private void ProcessMouse()
        {
            var currentState = Mouse.GetState();

            if (currentState == this._previousMouseState || !this.CaptureMouse) // if there's no mouse-state change or if it's not captured, just return.
                return;

            float rotation = currentState.X - GraphicsConfig.Instance.Width / 2;
            if (rotation != 0) _cameraController.RotateCamera(rotation);

            float elevation = currentState.Y - GraphicsConfig.Instance.Height / 2;
            if (elevation != 0) _cameraController.ElevateCamera(elevation);

            if (currentState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released) this._player.Weapon.Use();
            if (currentState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released) this._player.Weapon.SecondaryUse();

            this._previousMouseState = currentState;
            this.CenterCursor();
        }

        /// <summary>
        /// Processes keyboard input by user.
        /// </summary>
        /// <param name="gameTime"></param>
        private void ProcessKeyboard(GameTime gameTime)
        {
            var currentState = Keyboard.GetState();

            if (currentState.IsKeyDown(Keys.Escape) && this._previousKeyboardState.IsKeyUp(Keys.Escape)) // allows quick exiting the game.
                this.Game.Exit();

            // movement keys.
            if (currentState.IsKeyDown(Keys.Up) || currentState.IsKeyDown(Keys.W)) _player.Move(gameTime,MoveDirection.Forward);
            if (currentState.IsKeyDown(Keys.Down) || currentState.IsKeyDown(Keys.S)) _player.Move(gameTime, MoveDirection.Backward);
            if (currentState.IsKeyDown(Keys.Left) || currentState.IsKeyDown(Keys.A)) _player.Move(gameTime, MoveDirection.Left);
            if (currentState.IsKeyDown(Keys.Right) || currentState.IsKeyDown(Keys.D)) _player.Move(gameTime, MoveDirection.Right);
            if (_previousKeyboardState.IsKeyUp(Keys.Space) && currentState.IsKeyDown(Keys.Space)) _player.Jump();  

            // debug keys.
            if (_previousKeyboardState.IsKeyUp(Keys.F1) && currentState.IsKeyDown(Keys.F1)) this._world.ToggleInfinitiveWorld();
            if (_previousKeyboardState.IsKeyUp(Keys.F2) && currentState.IsKeyDown(Keys.F2)) this._player.ToggleFlyForm();
            if (_previousKeyboardState.IsKeyUp(Keys.F3) && currentState.IsKeyDown(Keys.F3)) this._fogService.ToggleFog();
            if (_previousKeyboardState.IsKeyUp(Keys.F4) && currentState.IsKeyDown(Keys.F4)) this._skyService.ToggleDynamicClouds();
            if (_previousKeyboardState.IsKeyUp(Keys.F5) && currentState.IsKeyDown(Keys.F5)) this.CaptureMouse = !this.CaptureMouse;
            if (currentState.IsKeyDown(Keys.F9) && _previousKeyboardState.IsKeyUp(Keys.F9)) this._statisticsGraphs.Toggle();
            if (_previousKeyboardState.IsKeyUp(Keys.F10) && currentState.IsKeyDown(Keys.F10)) this._ingameDebuggerService.ToggleInGameDebugger();
            
            if (_previousKeyboardState.IsKeyUp(Keys.F11) && currentState.IsKeyDown(Keys.F11)) // toggles frame-limiter.
            {
                this._graphicsManager.ToggleFixedTimeSteps();
                this._graphicsManager.ToggleVerticalSync();
            }

            if (_previousKeyboardState.IsKeyUp(Keys.F12) && currentState.IsKeyDown(Keys.F12)) // toggles rasterizer.
                ((VoxeliqGame) Game).Rasterizer.ToggleRasterMode();

            this._previousKeyboardState = currentState;
        }

        /// <summary>
        /// Centers cursor on screen.
        /// </summary>
        private void CenterCursor()
        {
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
        }

        /// <summary>
        /// Prints debug keys.
        /// </summary>
        private void PrintDebugKeys()
        {
            // TODO: should only work in #debug mode.
            Debug.WriteLine("Debug keys:");
            Debug.WriteLine("-----------------------------");
            Debug.WriteLine("F1: Infinitive-world: On/Off.");
            Debug.WriteLine("F2: Fly-mode: On/Off.");
            Debug.WriteLine("F3: Fog-mode: None/Near/Far.");
            Debug.WriteLine("F4: Dynamic Clouds: On/Off.");
            Debug.WriteLine("F5: Capture Mouse: On/Off.");
            Debug.WriteLine("F9: Debug Graphs: On/Off.");
            Debug.WriteLine("F10: In-game Debugger: On/Off.");
            Debug.WriteLine("F11: Frame-limiter: On/Off.");
            Debug.WriteLine("F12: Wireframe mode: On/Off.");
            Debug.WriteLine("-----------------------------");
        }
    }
}