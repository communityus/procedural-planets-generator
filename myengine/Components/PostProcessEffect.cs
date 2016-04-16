﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Components
{
    public interface IPostProcessEffect
    {
        Shader Shader { get; set; }
        bool IsEnabled { get; set; }
        bool RequiresGBufferMipMaps { get; set; }
        void BeforeBindCallBack();
    }

    public class PostProcessEffect : Component, IPostProcessEffect
    {
        public Camera Camera
        {
            get
            {
                return Entity.GetComponent<Camera>();
            }
        }

        public bool IsEnabled { get; set; }
        public Shader Shader { get; set; }
        public bool RequiresGBufferMipMaps { get; set; }

        public PostProcessEffect(Entity entity) : base(entity)
        {
            IsEnabled = true;
            RequiresGBufferMipMaps = true;
            Camera.AddPostProcessEffect(this);
        }

        public virtual void BeforeBindCallBack()
        {
            
        }
    }
}
