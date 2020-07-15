using System;
using System.Diagnostics;
using System.Threading;
using osu;
using osu.Enums;
using osu_rx.Configuration;
using SimpleDependencyInjection;

namespace osu_rx.Core.Timewarp
{
	public class Timewarp
	{
		public bool IsRunning { get; private set; }

		public Timewarp()
		{
			this.osuManager = DependencyContainer.Get<OsuManager>();
			this.configManager = DependencyContainer.Get<ConfigManager>();
		}

		public void Start()
		{
			this.shouldStop = false;
			this.initialRate = (this.osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.DoubleTime) ? 1.5 : (this.osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.HalfTime) ? 0.75 : 1.0));
			this.refresh();
			while (!this.shouldStop && this.osuManager.CanPlay)
			{
				this.setRate(this.configManager.TimewarpRate, true);
				Thread.Sleep(1);
			}
			this.setRate(this.shouldStop ? this.initialRate : 1.0, false);
		}

		public void Stop()
		{
			this.shouldStop = true;
		}

		private void refresh()
		{
			foreach (object obj in this.osuManager.OsuProcess.Process.Modules)
			{
				ProcessModule processModule = (ProcessModule)obj;
				if (processModule.ModuleName == "bass.dll")
				{
					this.audioRateAddress = (UIntPtr)((ulong)((long)processModule.BaseAddress.ToInt32()));
					break;
				}
			}
			for (int i = 0; i < this.audioRateOffsets.Length; i++)
			{
				this.audioRateAddress += this.audioRateOffsets[i];
				if (i != this.audioRateOffsets.Length - 1)
				{
					this.audioRateAddress = (UIntPtr)this.osuManager.OsuProcess.ReadUInt32(this.audioRateAddress);
				}
			}
		}

		private void setRate(double rate, bool bypass = true)
		{
			rate /= (this.osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.Nightcore) ? 1.5 : 1.0);
			if (this.osuManager.OsuProcess.ReadDouble(this.audioRateAddress) != rate)
			{
				this.osuManager.OsuProcess.WriteMemory(this.audioRateAddress, BitConverter.GetBytes(rate), 8U);
				this.osuManager.OsuProcess.WriteMemory(this.audioRateAddress + 8, BitConverter.GetBytes(rate * 1147.0), 8U);
			}
			if (bypass)
			{
				this.osuManager.Player.AudioCheckCount = int.MinValue;
			}
		}

		private readonly int[] audioRateOffsets = new int[]
		{
			213608,
			8,
			16,
			12,
			64
		};

		private const double defaultRate = 1147.0;

		private OsuManager osuManager;

		private ConfigManager configManager;

		private UIntPtr audioRateAddress = UIntPtr.Zero;

		private bool shouldStop;

		private double initialRate;
	}
}
