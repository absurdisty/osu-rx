using System;
using System.Runtime.CompilerServices;
using System.Threading;
using osu;
using osu.Enums;
using osu.Memory.Objects.Bindings;
using osu.Memory.Objects.Player.Beatmaps;
using osu.Memory.Objects.Player.Beatmaps.Objects;
using osu_rx.Configuration;
using osu_rx.Core.Relax.Accuracy;
using SimpleDependencyInjection;
using WindowsInput;
using WindowsInput.Native;

namespace osu_rx.Core.Relax
{
	public class Relax
	{
		public Relax()
		{
			this.osuManager = DependencyContainer.Get<OsuManager>();
			this.configManager = DependencyContainer.Get<ConfigManager>();
			this.inputSimulator = new InputSimulator();
			this.accuracyManager = new AccuracyManager();
		}

		public void Start(OsuBeatmap beatmap)
		{
			Relax.<>c__DisplayClass10_0 CS$<>8__locals1;
			CS$<>8__locals1.beatmap = beatmap;
			CS$<>8__locals1.<>4__this = this;
			this.beatmap = CS$<>8__locals1.beatmap;
			this.shouldStop = false;
			this.hitWindow50 = this.osuManager.HitWindow50((double)CS$<>8__locals1.beatmap.OverallDifficulty);
			this.leftClick = (VirtualKeyCode)this.osuManager.BindingManager.GetKeyCode(Bindings.OsuLeft);
			this.rightClick = (VirtualKeyCode)this.osuManager.BindingManager.GetKeyCode(Bindings.OsuRight);
			this.accuracyManager.Reset(CS$<>8__locals1.beatmap);
			bool flag = false;
			int num = 0;
			CS$<>8__locals1.index = this.osuManager.Player.HitObjectManager.CurrentHitObjectIndex;
			CS$<>8__locals1.currentHitObject = CS$<>8__locals1.beatmap.HitObjects[CS$<>8__locals1.index];
			CS$<>8__locals1.alternateResult = AlternateResult.None;
			CS$<>8__locals1.currentKey = this.configManager.PrimaryKey;
			CS$<>8__locals1.currentHitTimings = this.accuracyManager.GetHitObjectTimings(CS$<>8__locals1.index, false, false);
			while (this.osuManager.CanPlay && CS$<>8__locals1.index < CS$<>8__locals1.beatmap.HitObjects.Count && !this.shouldStop)
			{
				Thread.Sleep(1);
				if (this.osuManager.IsPaused)
				{
					if (flag)
					{
						flag = false;
						this.releaseAllKeys();
					}
				}
				else
				{
					int num2 = this.osuManager.CurrentTime + this.configManager.AudioOffset;
					if (num2 >= CS$<>8__locals1.currentHitObject.StartTime - this.hitWindow50)
					{
						if (!flag)
						{
							switch (this.accuracyManager.GetHitScanResult(CS$<>8__locals1.index))
							{
							case HitScanResult.CanHit:
								if (num2 < CS$<>8__locals1.currentHitObject.StartTime + CS$<>8__locals1.currentHitTimings.StartOffset)
								{
									continue;
								}
								break;
							case HitScanResult.ShouldHit:
								break;
							case HitScanResult.Wait:
								continue;
							case HitScanResult.MoveToNextObject:
								this.<Start>g__moveToNextObject|10_0(ref CS$<>8__locals1);
								continue;
							default:
								continue;
							}
							flag = true;
							num = num2;
							if (this.configManager.PlayStyle == PlayStyles.TapX && CS$<>8__locals1.alternateResult == AlternateResult.None)
							{
								this.inputSimulator.Mouse.LeftButtonDown();
								CS$<>8__locals1.currentKey = this.configManager.PrimaryKey;
							}
							else if (CS$<>8__locals1.currentKey == OsuKeys.K1 || CS$<>8__locals1.currentKey == OsuKeys.K2)
							{
								this.inputSimulator.Keyboard.KeyDown((CS$<>8__locals1.currentKey == OsuKeys.K1) ? this.leftClick : this.rightClick);
							}
							else if (CS$<>8__locals1.currentKey == OsuKeys.M1)
							{
								this.inputSimulator.Mouse.LeftButtonDown();
							}
							else
							{
								this.inputSimulator.Mouse.RightButtonDown();
							}
						}
						else if (num2 >= ((CS$<>8__locals1.currentHitObject is OsuHitCircle) ? num : CS$<>8__locals1.currentHitObject.EndTime) + CS$<>8__locals1.currentHitTimings.HoldTime)
						{
							this.<Start>g__moveToNextObject|10_0(ref CS$<>8__locals1);
							if (!(CS$<>8__locals1.currentHitObject is OsuSpinner) || CS$<>8__locals1.currentHitObject.StartTime - CS$<>8__locals1.beatmap.HitObjects[CS$<>8__locals1.index - 1].EndTime > this.configManager.HoldBeforeSpinnerTime)
							{
								flag = false;
								this.releaseAllKeys();
							}
						}
					}
				}
			}
			this.releaseAllKeys();
			while (this.osuManager.CanPlay && CS$<>8__locals1.index >= CS$<>8__locals1.beatmap.HitObjects.Count && !this.shouldStop)
			{
				Thread.Sleep(5);
			}
		}

		public void Stop()
		{
			this.shouldStop = true;
		}

		private AlternateResult getAlternateResult(int index)
		{
			if (this.configManager.PlayStyle == PlayStyles.Alternate)
			{
				return AlternateResult.AlternateThisNote;
			}
			AlternateResult alternateResult = AlternateResult.None;
			float num = this.osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.DoubleTime) ? 1.5f : (this.osuManager.Player.HitObjectManager.CurrentMods.HasFlag(Mods.HalfTime) ? 0.75f : 1f);
			float num2 = (float)this.configManager.AlternateIfLessThan * num;
			OsuHitObject osuHitObject = this.beatmap.HitObjects[index];
			OsuHitObject osuHitObject2 = (index > 0) ? this.beatmap.HitObjects[index - 1] : null;
			OsuHitObject osuHitObject3 = (index + 1 < this.beatmap.HitObjects.Count) ? this.beatmap.HitObjects[index + 1] : null;
			SliderAlternationBinding sliderAlternationBinding = this.configManager.SliderAlternationBinding;
			if (osuHitObject2 != null && (float)(osuHitObject.StartTime - ((osuHitObject2 is OsuSlider && sliderAlternationBinding == SliderAlternationBinding.StartTime) ? osuHitObject2.StartTime : osuHitObject2.EndTime)) < num2)
			{
				alternateResult++;
			}
			if (osuHitObject3 != null && (float)(osuHitObject3.StartTime - ((osuHitObject is OsuSlider && sliderAlternationBinding == SliderAlternationBinding.StartTime) ? osuHitObject.StartTime : osuHitObject.EndTime)) < num2)
			{
				alternateResult += 2;
			}
			return alternateResult;
		}

		private void releaseAllKeys()
		{
			this.inputSimulator.Keyboard.KeyUp(this.leftClick);
			this.inputSimulator.Keyboard.KeyUp(this.rightClick);
			this.inputSimulator.Mouse.LeftButtonUp();
			this.inputSimulator.Mouse.RightButtonUp();
		}

		[CompilerGenerated]
		private void <Start>g__moveToNextObject|10_0(ref Relax.<>c__DisplayClass10_0 A_1)
		{
			int index = A_1.index;
			A_1.index = index + 1;
			if (A_1.index < A_1.beatmap.HitObjects.Count)
			{
				A_1.currentHitObject = A_1.beatmap.HitObjects[A_1.index];
				A_1.alternateResult = this.getAlternateResult(A_1.index);
				if (A_1.alternateResult.HasFlag(AlternateResult.AlternateThisNote))
				{
					A_1.currentKey = ((A_1.currentKey == this.configManager.PrimaryKey) ? this.configManager.SecondaryKey : this.configManager.PrimaryKey);
				}
				else
				{
					A_1.currentKey = this.configManager.PrimaryKey;
				}
				A_1.currentHitTimings = this.accuracyManager.GetHitObjectTimings(A_1.index, A_1.alternateResult.HasFlag(AlternateResult.AlternateThisNote), this.inputSimulator.InputDeviceState.IsKeyDown(this.configManager.DoubleDelayKey));
			}
		}

		private OsuManager osuManager;

		private ConfigManager configManager;

		private InputSimulator inputSimulator;

		private AccuracyManager accuracyManager;

		private OsuBeatmap beatmap;

		private bool shouldStop;

		private int hitWindow50;

		private VirtualKeyCode leftClick;

		private VirtualKeyCode rightClick;
	}
}
