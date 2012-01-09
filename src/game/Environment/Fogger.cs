﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using VolumetricStudios.VoxeliqGame.Common.Logging;

namespace VolumetricStudios.VoxeliqGame.Environment
{
    public interface IFogService
    {
        /// <summary>
        /// Fog state.
        /// </summary>
        FogState State { get; }

        /// <summary>
        /// Fog vector value for current fog-state.
        /// </summary>
        Vector2 FogVector { get; }

        /// <summary>
        /// Fog vectors.
        /// </summary>
        void ToggleFog();
    }

    public class Fogger : GameComponent, IFogService
    {
        // properties
        public FogState State { get; private set; }
        public Vector2 FogVector { get { return this._fogVectors[(byte)this.State]; } }

        // fog vectors.
        private readonly Vector2[] _fogVectors = new[]
        {
            new Vector2(0, 0),  // none
            new Vector2(175, 250),  // near
            new Vector2(250, 400) // far
        };

        private static readonly Logger Logger = LogManager.CreateLogger(); // logging-facility

        public Fogger(Game game)
            : base(game)
        {
            Logger.Trace("init()");
            this.State = FogState.None;
            this.Game.Services.AddService(typeof(IFogService), this);
        }

        /// <summary>
        /// Toggles fog to near, far and none.
        /// </summary>
        public void ToggleFog()
        {
            switch (this.State)
            {
                case FogState.None:
                    this.State =FogState.Near;
                    break;
                case FogState.Near:
                    this.State = FogState.Far;
                    break;
                case FogState.Far:
                    this.State = FogState.None;
                    break;
            }
        }
    }

    /// <summary>
    /// Fog state enum.
    /// </summary>
    public enum FogState : byte
    {
        None,
        Near,
        Far
    }
}