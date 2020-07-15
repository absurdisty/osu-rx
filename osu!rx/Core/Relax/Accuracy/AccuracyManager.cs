using System;
using System.Numerics;
using osu;
using osu.Enums;
using osu.Memory.Objects.Player.Beatmaps;
using osu.Memory.Objects.Player.Beatmaps.Objects;
using osu_rx.Configuration;
using osu_rx.Helpers;
using SimpleDependencyInjection;

namespace osu_rx.Core.Relax.Accuracy
{
	public class AccuracyManager
	{
		public AccuracyManager()
		{
			this.osuManager = DependencyContainer.Get<OsuManager>();
			this.configManager = DependencyContainer.Get<ConfigManager>();
		}

		public void Reset(OsuBeatmap beatmap)
		{
			this.beatmap = beatmap;
			this.hitWindow50 = this.osuManager.HitWindow50((double)beatmap.OverallDifficulty);
			this.hitWindow100 = this.osuManager.HitWindow100((double)beatmap.OverallDifficulty);
			this.hitWindow300 = this.osuManager.HitWindow300((double)beatmap.OverallDifficulty);
			Mods currentMods = this.osuManager.Player.HitObjectManager.CurrentMods;
			this.audioRate = (currentMods.HasFlag(Mods.HalfTime) ? 0.75f : (currentMods.HasFlag(Mods.DoubleTime) ? 1.5f : 1f));
			this.minOffset = this.calculateTimingOffset(this.configManager.HitTimingsMinOffset);
			this.maxOffset = this.calculateTimingOffset(this.configManager.HitTimingsMaxOffset);
			this.minAlternateOffset = this.calculateTimingOffset(this.configManager.HitTimingsAlternateMinOffset);
			this.maxAlternateOffset = this.calculateTimingOffset(this.configManager.HitTimingsAlternateMaxOffset);
			this.hitObjectRadius = this.osuManager.HitObjectRadius(beatmap.CircleSize);
			this.missRadius = (float)this.configManager.HitScanMissRadius;
			this.canMiss = false;
			this.lastHitScanIndex = -1;
			this.lastOnNotePosition = null;
		}

		public HitObjectTimings GetHitObjectTimings(int index, bool alternating, bool doubleDelay)
		{
			HitObjectTimings hitObjectTimings = new HitObjectTimings(0, 0);
			int minValue = (int)((float)(alternating ? this.minAlternateOffset : this.minOffset) * (doubleDelay ? this.configManager.HitTimingsDoubleDelayFactor : 1f));
			int maxValue = (int)((float)(alternating ? this.maxAlternateOffset : this.maxOffset) * (doubleDelay ? this.configManager.HitTimingsDoubleDelayFactor : 1f));
			hitObjectTimings.StartOffset = MathHelper.Clamp(this.random.Next(minValue, maxValue), -this.hitWindow50, this.hitWindow50);
			if (this.beatmap.HitObjects[index] is OsuSlider)
			{
				int num = this.beatmap.HitObjects[index].EndTime - this.beatmap.HitObjects[index].StartTime;
				int num2 = (int)((float)this.configManager.HitTimingsMaxSliderHoldTime * this.audioRate);
				int value = this.random.Next(this.configManager.HitTimingsMinSliderHoldTime, num2);
				hitObjectTimings.HoldTime = MathHelper.Clamp(value, (num >= 72) ? -26 : (num / 2 - 10), num2);
			}
			else
			{
				int minValue2 = (int)((float)this.configManager.HitTimingsMinHoldTime * this.audioRate);
				int num3 = (int)((float)this.configManager.HitTimingsMaxHoldTime * this.audioRate);
				int value2 = this.random.Next(minValue2, num3);
				hitObjectTimings.HoldTime = MathHelper.Clamp(value2, 0, num3);
			}
			return hitObjectTimings;
		}

		private int calculateTimingOffset(int percentage)
		{
			float num = (float)Math.Abs(percentage) / 100f;
			int num2 = (num <= 1f) ? 0 : ((num <= 2f) ? (this.hitWindow300 + 1) : (this.hitWindow100 + 1));
			int num3 = ((num <= 1f) ? this.hitWindow300 : ((num <= 2f) ? this.hitWindow100 : this.hitWindow50)) - num2;
			if (num != 0f && num % 1f == 0f)
			{
				num = 1f;
			}
			else
			{
				num %= 1f;
			}
			return (int)((float)num2 + (float)num3 * num) * ((percentage < 0) ? -1 : 1);
		}

		public HitScanResult GetHitScanResult(int index)
		{
			OsuHitObject osuHitObject = this.beatmap.HitObjects[index];
			OsuHitObject osuHitObject2 = (index + 1 < this.beatmap.HitObjects.Count) ? this.beatmap.HitObjects[index + 1] : null;
			if (!this.configManager.EnableHitScan || osuHitObject is OsuSpinner)
			{
				return HitScanResult.CanHit;
			}
			if (this.lastHitScanIndex != index)
			{
				this.canMiss = (this.configManager.HitScanMissChance != 0 && this.random.Next(1, 101) <= this.configManager.HitScanMissChance);
				this.lastHitScanIndex = index;
				this.lastOnNotePosition = null;
			}
			Vector2 value = (osuHitObject is OsuSlider) ? (osuHitObject as OsuSlider).PositionAtTime(this.osuManager.CurrentTime) : osuHitObject.Position;
			Vector2 c = (osuHitObject2 != null) ? osuHitObject2.Position : Vector2.Zero;
			Vector2 vector = this.osuManager.WindowManager.ScreenToPlayfield(this.osuManager.Player.Ruleset.MousePosition);
			float num = Vector2.Distance(vector, value);
			float num2 = Vector2.Distance(vector, this.lastOnNotePosition ?? Vector2.Zero);
			if (this.osuManager.CurrentTime > osuHitObject.EndTime + this.hitWindow50)
			{
				if (this.configManager.HitScanMissAfterHitWindow50 && num <= this.hitObjectRadius + this.missRadius && !this.intersectsWithOtherHitObjects(index + 1))
				{
					return HitScanResult.ShouldHit;
				}
				return HitScanResult.MoveToNextObject;
			}
			else
			{
				if (this.configManager.EnableHitScanPrediction)
				{
					if (num > this.hitObjectRadius * this.configManager.HitScanPredictionRadiusScale && num <= this.hitObjectRadius && this.lastOnNotePosition != null && osuHitObject2 != null && (MathHelper.GetAngle(this.lastOnNotePosition.Value, vector, c) <= (double)this.configManager.HitScanPredictionDirectionAngleTolerance || num2 <= (float)this.configManager.HitScanPredictionMaxDistance))
					{
						return HitScanResult.ShouldHit;
					}
					if (num <= this.hitObjectRadius * this.configManager.HitScanPredictionRadiusScale)
					{
						this.lastOnNotePosition = new Vector2?(vector);
					}
					else
					{
						this.lastOnNotePosition = null;
					}
				}
				if (num <= this.hitObjectRadius)
				{
					return HitScanResult.CanHit;
				}
				if (this.canMiss && num <= this.hitObjectRadius + this.missRadius && !this.intersectsWithOtherHitObjects(index + 1))
				{
					return HitScanResult.CanHit;
				}
				return HitScanResult.Wait;
			}
		}

		private bool intersectsWithOtherHitObjects(int startIndex)
		{
			int currentTime = this.osuManager.CurrentTime;
			double num = this.osuManager.DifficultyRange((double)this.beatmap.ApproachRate, 1800.0, 1200.0, 450.0);
			Vector2 value = this.osuManager.WindowManager.ScreenToPlayfield(this.osuManager.Player.Ruleset.MousePosition);
			for (int i = startIndex; i < this.beatmap.HitObjects.Count; i++)
			{
				OsuHitObject osuHitObject = this.beatmap.HitObjects[i];
				if ((double)osuHitObject.StartTime - num > (double)currentTime)
				{
					break;
				}
				if (Vector2.Distance(value, osuHitObject.Position) <= this.hitObjectRadius)
				{
					return true;
				}
			}
			return false;
		}

		private OsuManager osuManager;

		private ConfigManager configManager;

		private OsuBeatmap beatmap;

		private int hitWindow50;

		private int hitWindow100;

		private int hitWindow300;

		private float audioRate;

		private int minOffset;

		private int maxOffset;

		private int minAlternateOffset;

		private int maxAlternateOffset;

		private float hitObjectRadius;

		private float missRadius;

		private bool canMiss;

		private int lastHitScanIndex;

		private Vector2? lastOnNotePosition;

		private Random random = new Random();
	}
}
