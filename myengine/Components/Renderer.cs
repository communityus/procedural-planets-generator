﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine.Components
{
    public enum RenderStatus
    {
        NotRendered = 0,
        Rendered = (1 << 1),
        RenderedForced = (1 << 2),
        Visible = (1 << 3),
        RenderedAndVisible = (1 << 1) | (1 << 3),
        Unknown = (1 << 9),
    }
    public class RenderContext
    {
        public static readonly object Geometry = new RenderContext("geometry");
        public static readonly object Shadows = new RenderContext("shadow");
        public static readonly object Depth = new RenderContext("depth");
        public string Name { get; set; }
        public RenderContext(string name)
        {
            this.Name = name;
        }
        public override string ToString()
        {
            return this.Name;
        }
    }
    public interface IRenderable
    {
        Material Material { get; }
        bool ForcePassFrustumCulling { get; }
        bool ShouldRender(object renderContext);
        Bounds GetBounds(WorldPos viewPointPos);
        void UploadUBOandDraw(Camera camera, UniformBlock ubo);
        void SetCameraRenderStatus(Camera camera, RenderStatus renderStatus);

        /*
        bool ShouldRenderGeometry { get; }
        bool ShouldCastShadows { get; }
        Material Material { get; }
        */
    }

    public abstract class Renderer : Component, IRenderable, IDisposable
    {

        public virtual RenderingMode RenderingMode { get; set; }
        public virtual Material Material { get; set; }
        
        Dictionary<Camera, RenderStatus> cameraToRenderStatus = new Dictionary<Camera, RenderStatus>();

        public virtual bool ForcePassFrustumCulling { get; set; }

        bool last_ShouldRenderGeometry = false;
        bool last_ShouldCastShadows = false;

        MyWeakReference<RenderableData> dataToRender;
        public Renderer(Entity entity) : base(entity)
        {
            dataToRender = new MyWeakReference<RenderableData>(Entity.Scene.DataToRender);
            dataToRender.Target?.Add(this);
        }
        public abstract Bounds GetBounds(WorldPos viewPointPos);

        public virtual void UploadUBOandDraw(Camera camera, UniformBlock ubo)
        {
        }

        public virtual void SetCameraRenderStatus(Camera camera, RenderStatus renderStatus)
        {
            cameraToRenderStatus[camera] = renderStatus;
        }
        public virtual RenderStatus GetCameraRenderStatus(Camera camera)
        {
            return cameraToRenderStatus.GetValue(camera, RenderStatus.Unknown);
        }

        public virtual bool ShouldRender(object renderContext)
        {
            if (renderContext == RenderContext.Geometry && RenderingMode.HasFlag(RenderingMode.RenderGeometry)) return true;
            if (renderContext == RenderContext.Shadows && RenderingMode.HasFlag(RenderingMode.CastShadows)) return true;
            return false;
        }

        public void Dispose()
        {
            dataToRender.Target?.Remove(this);
        }
        public override string ToString()
        {
            return Entity.Name;
        }
    }
}