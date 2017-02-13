﻿using Neitri;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine
{
	public class Factory
	{
		public Shader DefaultGBufferShader => GetShader("internal/deferred.gBuffer.PBR.shader");
		public Shader DefaultDepthGrabShader => GetShader("internal/depthGrab.default.shader");

		public Mesh SkyBoxMesh => GetMesh("internal/skybox.obj");
		public Mesh QuadMesh => GetMesh("internal/quad.obj");

		public Texture2D whiteTexture => GetTexture2D("internal/white.png");
		public Texture2D greyTexture => GetTexture2D("internal/grey.png");
		public Texture2D blackTexture => GetTexture2D("internal/black.png");

		[Dependency]
		public IDependencyManager Dependency { get; private set; }

		ConcurrentDictionary<string, Shader> allShaders = new ConcurrentDictionary<string, Shader>();

		public Shader GetShader(string file)
		{
			Shader s;
			if (!allShaders.TryGetValue(file, out s))
			{
				s = Dependency.Create<Shader>(FileSystem.FindFile(file));
				allShaders[file] = s;
			}
			return s;
		}

		public void ReloadAllShaders()
		{
			foreach (var s in allShaders)
			{
				s.Value.shouldReload = true;
			}
		}

		public Material NewMaterial()
		{
			return Dependency.Create<Material>();
		}

		[Dependency(Register = true)]
		ObjLoader objLoader;

		[Dependency]
		FileSystem FileSystem;

		ConcurrentDictionary<string, Mesh> allMeshes = new ConcurrentDictionary<string, Mesh>();

		public Mesh GetMesh(string file, bool allowDuplicates = false)
		{
			//if (!resource.originalPath.EndsWith(".obj")) throw new System.Exception("Resource path does not end with .obj");

			Mesh s;
			if (allowDuplicates || !allMeshes.TryGetValue(file, out s))
			{
				s = objLoader.Load(this.FileSystem.FindFile(file));
				allMeshes[file] = s;
			}
			return s;
		}

		ConcurrentDictionary<string, Texture2D> allTexture2Ds = new ConcurrentDictionary<string, Texture2D>();

		public Texture2D GetTexture2D(string file)
		{
			Texture2D s;
			if (!allTexture2Ds.TryGetValue(file, out s))
			{
				s = new Texture2D(this.FileSystem.FindFile(file));
				allTexture2Ds[file] = s;
			}
			return s;
		}

		ConcurrentDictionary<string, Cubemap> allCubeMaps = new ConcurrentDictionary<string, Cubemap>();

		public Cubemap GetCubeMap(string[] files)
		{
			Cubemap s;
			var key = string.Join("###", files.Select((x) => x.ToString()));
			if (!allCubeMaps.TryGetValue(key, out s))
			{
				s = new Cubemap(FileSystem.Findfiles(files).ToArray());
				allCubeMaps[key] = s;
			}
			return s;
		}
	}
}