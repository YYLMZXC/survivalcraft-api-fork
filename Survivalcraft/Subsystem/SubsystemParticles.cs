using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemParticles : Subsystem, IDrawable, IUpdateable
	{
		public SubsystemTime m_subsystemTime;

		public Dictionary<ParticleSystemBase, bool> m_particleSystems = [];

		public PrimitivesRenderer3D PrimitivesRenderer = new();

		public bool ParticleSystemsDraw = true;

		public bool ParticleSystemsSimulate = true;

		public int[] m_drawOrders = new int[1]
		{
			300
		};

		public List<ParticleSystemBase> m_endedParticleSystems = [];

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public SubsystemSky SubsystemSky { get; private set; }

		public int[] DrawOrders => m_drawOrders;

		public void AddParticleSystem(ParticleSystemBase particleSystem, bool throwOnAlreadyAdded = false)
		{
			if (particleSystem.SubsystemParticles == null)
			{
				m_particleSystems.Add(particleSystem, value: true);
				particleSystem.SubsystemParticles = this;
				particleSystem.OnAdded();
				return;
			}
			if(throwOnAlreadyAdded) throw new InvalidOperationException("Particle system is already added.");
		}

		public void RemoveParticleSystem(ParticleSystemBase particleSystem, bool throwOnNotFound = false)
		{
			if (particleSystem.SubsystemParticles == this)
			{
				particleSystem.OnRemoved();
				m_particleSystems.Remove(particleSystem);
				particleSystem.SubsystemParticles = null;
				return;
			}
			if(throwOnNotFound) throw new InvalidOperationException("Particle system is not added.");
		}

		public bool ContainsParticleSystem(ParticleSystemBase particleSystem)
		{
			return particleSystem.SubsystemParticles == this;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			SubsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
		}

		public void Update(float dt)
		{
			if (ParticleSystemsSimulate)
			{
				m_endedParticleSystems.Clear();
				foreach (ParticleSystemBase key in m_particleSystems.Keys)
				{
					if (key.Simulate(m_subsystemTime.GameTimeDelta))
					{
						m_endedParticleSystems.Add(key);
					}
				}
				foreach (ParticleSystemBase endedParticleSystem in m_endedParticleSystems)
				{
					RemoveParticleSystem(endedParticleSystem);
				}
			}
		}

		public void Draw(Camera camera, int drawOrder)
		{
			if (ParticleSystemsDraw)
			{
				foreach (ParticleSystemBase key in m_particleSystems.Keys)
				{
					key.Draw(camera);
				}
				Shader shader = ContentManager.Get<Shader>("Shaders/AlphaTested");
				shader.GetParameter("u_origin").SetValue(Vector2.Zero);
				shader.GetParameter("u_viewProjectionMatrix").SetValue(camera.ViewProjectionMatrix);
				shader.GetParameter("u_viewPosition").SetValue(camera.ViewPosition);
				shader.GetParameter("u_fogYMultiplier").SetValue(SubsystemSky.VisibilityRangeYMultiplier);
				shader.GetParameter("u_fogColor").SetValue(new Vector3(SubsystemSky.ViewFogColor));
				shader.GetParameter("u_hazeStartDensity").SetValue(new Vector2(SubsystemSky.ViewHazeStart, SubsystemSky.ViewHazeDensity));
				shader.GetParameter("u_fogBottomTopDensity").SetValue(new Vector3(SubsystemSky.ViewFogBottom, SubsystemSky.ViewFogTop, SubsystemSky.ViewFogDensity));
				shader.GetParameter("u_alphaThreshold").SetValue(0f);
				ShaderParameter parameter = shader.GetParameter("u_texture");
				ShaderParameter parameter2 = shader.GetParameter("u_samplerState");
				foreach (TexturedBatch3D texturedBatch in PrimitivesRenderer.TexturedBatches)
				{
					Display.DepthStencilState = texturedBatch.DepthStencilState;
					Display.RasterizerState = texturedBatch.RasterizerState;
					Display.BlendState = texturedBatch.BlendState;
					parameter.SetValue(texturedBatch.Texture);
					parameter2.SetValue(texturedBatch.SamplerState);
					texturedBatch.FlushWithDeviceState(shader);
				}
				PrimitivesRenderer.Flush(camera.ViewProjectionMatrix);
			}
		}
	}
}
