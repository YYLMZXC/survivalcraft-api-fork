using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemTime : Subsystem
	{
		public struct DelayedExecutionRequest
		{
			public double GameTime;

			public Action Action;
		}

		public float MaxGameTimeDelta = 0.1f;

		public float MaxFixedGameTimeDelta = 0.1f;

        public float DefaultFixedTimeStep = 0.05f;

		public int DefaultFixedUpdateStep = 20;

		public float GameMenuDialogTimeFactor = 0f;

        public double m_gameTime;

		public float m_gameTimeDelta;

		public float m_prevGameTimeDelta;

		public float m_gameTimeFactor = 1f;

		public List<DelayedExecutionRequest> m_delayedExecutionsRequests = [];

		public SubsystemPlayers m_subsystemPlayers;

		public SubsystemUpdate m_subsystemUpdate;

		public double GameTime => m_gameTime;

		public float GameTimeDelta => m_gameTimeDelta;

		public float PreviousGameTimeDelta => m_prevGameTimeDelta;

		public float GameTimeFactor
		{
			get
			{
				return m_gameTimeFactor;
			}
			set
			{
				m_gameTimeFactor = Math.Clamp(value, 0f, 256f);
			}
		}

		public float? FixedTimeStep
		{
			get;
			private set;
		}

		public virtual void NextFrame()
		{
			m_prevGameTimeDelta = m_gameTimeDelta;
			m_gameTimeDelta = !FixedTimeStep.HasValue
				? MathUtils.Min(Time.FrameDuration * m_gameTimeFactor, MaxGameTimeDelta)
				: MathUtils.Min(FixedTimeStep.Value * m_gameTimeFactor, MaxFixedGameTimeDelta);
			ModsManager.HookAction("ChangeGameTimeDelta", loader =>
			{
				loader.ChangeGameTimeDelta(this, ref m_gameTimeDelta);
				return false;
			});
			m_gameTime += m_gameTimeDelta;
			int num = 0;
			while (num < m_delayedExecutionsRequests.Count)
			{
				DelayedExecutionRequest delayedExecutionRequest = m_delayedExecutionsRequests[num];
				if (delayedExecutionRequest.GameTime >= 0.0 && GameTime >= delayedExecutionRequest.GameTime)
				{
					m_delayedExecutionsRequests.RemoveAt(num);
					delayedExecutionRequest.Action();
				}
				else
				{
					num++;
				}
			}
			int numSleepingPlayers = 0;
			int numDeadPlayers = 0;
			foreach (ComponentPlayer componentPlayer in m_subsystemPlayers.ComponentPlayers)
			{
				if (componentPlayer.ComponentHealth.Health == 0f)
				{
					numDeadPlayers++;
				}
				else if (componentPlayer.ComponentSleep.SleepFactor == 1f)
				{
					numSleepingPlayers++;
				}
			}
			if (numSleepingPlayers + numDeadPlayers == m_subsystemPlayers.ComponentPlayers.Count && numSleepingPlayers >= 1)
			{
				FixedTimeStep = DefaultFixedTimeStep;
				m_subsystemUpdate.UpdatesPerFrame = DefaultFixedUpdateStep;
			}
			else
			{
				FixedTimeStep = null;
				m_subsystemUpdate.UpdatesPerFrame = 1;
			}
			bool flag = true;
			foreach (ComponentPlayer componentPlayer2 in m_subsystemPlayers.ComponentPlayers)
			{
				if (!componentPlayer2.ComponentGui.IsGameMenuDialogVisible())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				GameTimeFactor = GameMenuDialogTimeFactor;
			}
			else if (GameTimeFactor == GameMenuDialogTimeFactor)
			{
				GameTimeFactor = 1f;
			}
		}

		public void QueueGameTimeDelayedExecution(double gameTime, Action action)
		{
			m_delayedExecutionsRequests.Add(new DelayedExecutionRequest
			{
				GameTime = gameTime,
				Action = action
			});
		}

		public bool PeriodicGameTimeEvent(double period, double offset)
		{
			double num = GameTime - offset;
			double num2 = Math.Floor(num / period) * period;
			if (num >= num2)
			{
				return num - GameTimeDelta < num2;
			}
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
			m_subsystemUpdate = Project.FindSubsystem<SubsystemUpdate>(throwOnError: true);
		}
	}
}
