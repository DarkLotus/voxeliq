﻿/*
 * Copyright (C) 2011 - 2013 Int6 Studios - http://www.int6.org,
 * Voxeliq Engine - http://www.voxeliq.org - https://github.com/raistlinthewiz/voxeliq
 *
 * This program is free software; you can redistribute it and/or modify 
 * it under the terms of the Microsoft Public License (Ms-PL).
 */

using Microsoft.Xna.Framework;
using VoxeliqEngine.Common.Logging;
using VoxeliqEngine.Core;

namespace VoxeliqEngine.Graphics
{
    /// <summary>
    /// Screen service for controlling screen.
    /// </summary>
    public interface IGraphicsManager
    {
        /// <summary>
        /// Returns true if game is set to fixed time steps.
        /// </summary>
        bool FixedTimeStepsEnabled { get; }

        /// <summary>
        /// Returns true if vertical sync is enabled.
        /// </summary>
        bool VerticalSyncEnabled { get; }

        /// <summary>
        /// Returns true if full-screen is enabled.
        /// </summary>
        bool FullScreenEnabled { get; }

        /// <summary>
        /// Toggles fixed time steps.
        /// </summary>
        void ToggleFixedTimeSteps();

        /// <summary>
        /// Sets vertical sync on or off.
        /// </summary>
        /// <param name="enabled"></param>
        void EnableVerticalSync(bool enabled);

        /// <summary>
        /// Sets full screen on or off.
        /// </summary>
        /// <param name="enabled"></param>
        void EnableFullScreen(bool enabled);
    }

    /// <summary>
    /// The screen manager that controls various graphical aspects.
    /// </summary>
    public sealed class GraphicsManager : IGraphicsManager
    {
        // settings
        public bool FixedTimeStepsEnabled { get; private set; } // Returns true if game is set to fixed time steps.
        public bool VerticalSyncEnabled { get; private set; } // Returns true if vertical sync is enabled.
        public bool FullScreenEnabled { get; private set; } // Returns true if full-screen is enabled.

        // principal stuff
        private readonly Game _game; // the attached game.
        private readonly GraphicsDeviceManager _graphicsDeviceManager; // attached graphics device manager.

        // misc
        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility.

        public GraphicsManager(GraphicsDeviceManager graphicsDeviceManager, Game game)
        {
            Logger.Trace("ctor()");

            this._game = game;
            this._graphicsDeviceManager = graphicsDeviceManager;
            this._game.Services.AddService(typeof(IGraphicsManager), this); // export service.

            this.FullScreenEnabled = this._graphicsDeviceManager.IsFullScreen = Engine.Instance.Configuration.Graphics.FullScreenEnabled;
            this._graphicsDeviceManager.PreferredBackBufferWidth = Engine.Instance.Configuration.Graphics.Width;
            this._graphicsDeviceManager.PreferredBackBufferHeight = Engine.Instance.Configuration.Graphics.Height;
            this.FixedTimeStepsEnabled = this._game.IsFixedTimeStep = Engine.Instance.Configuration.Graphics.FixedTimeStepsEnabled;
            this.VerticalSyncEnabled = this._graphicsDeviceManager.SynchronizeWithVerticalRetrace = Engine.Instance.Configuration.Graphics.VerticalSyncEnabled;
            this._graphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// Toggles fixed time steps.
        /// </summary>
        public void ToggleFixedTimeSteps()
        {
            this.FixedTimeStepsEnabled = !this.FixedTimeStepsEnabled;
            this._game.IsFixedTimeStep = this.FixedTimeStepsEnabled;
            this._graphicsDeviceManager.ApplyChanges();
        }

        public void EnableVerticalSync(bool enabled)
        {
            this.VerticalSyncEnabled = enabled;
            this._graphicsDeviceManager.SynchronizeWithVerticalRetrace = this.VerticalSyncEnabled;
            this._graphicsDeviceManager.ApplyChanges();
        }

        /// <summary>
        /// Sets full screen on or off.
        /// </summary>
        /// <param name="enabled"></param>
        public void EnableFullScreen(bool enabled)
        {
            this.FullScreenEnabled = enabled;
            this._graphicsDeviceManager.IsFullScreen = this.FullScreenEnabled;
            this._graphicsDeviceManager.ApplyChanges();
        }
    }
}