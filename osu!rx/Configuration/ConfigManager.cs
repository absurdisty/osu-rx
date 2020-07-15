using System;
using SimpleIniConfig;
using WindowsInput.Native;

namespace osu_rx.Configuration
{
	public class ConfigManager
	{
		public bool EnableRelax
		{
			get
			{
				return this.config.GetValue<bool>("EnableRelax", true);
			}
			set
			{
				this.config.SetValue<bool>("EnableRelax", value);
			}
		}

		public PlayStyles PlayStyle
		{
			get
			{
				return this.config.GetValue<PlayStyles>("RelaxPlayStyle", PlayStyles.Singletap);
			}
			set
			{
				this.config.SetValue<PlayStyles>("RelaxPlayStyle", value);
			}
		}

		public OsuKeys PrimaryKey
		{
			get
			{
				return this.config.GetValue<OsuKeys>("RelaxPrimaryKey", OsuKeys.K1);
			}
			set
			{
				this.config.SetValue<OsuKeys>("RelaxPrimaryKey", value);
			}
		}

		public OsuKeys SecondaryKey
		{
			get
			{
				return this.config.GetValue<OsuKeys>("RelaxSecondaryKey", OsuKeys.K2);
			}
			set
			{
				this.config.SetValue<OsuKeys>("RelaxSecondaryKey", value);
			}
		}

		public VirtualKeyCode DoubleDelayKey
		{
			get
			{
				return this.config.GetValue<VirtualKeyCode>("RelaxDoubleDelayKey", VirtualKeyCode.SPACE);
			}
			set
			{
				this.config.SetValue<VirtualKeyCode>("RelaxDoubleDelayKey", value);
			}
		}

		public int MaxSingletapBPM
		{
			get
			{
				return this.config.GetValue<int>("RelaxMaxSingletapBPM", 500);
			}
			set
			{
				this.config.SetValue<int>("RelaxMaxSingletapBPM", value);
			}
		}

		public int AlternateIfLessThan
		{
			get
			{
				return this.config.GetValue<int>("RelaxAlternateIfLessThan", 60000 / this.MaxSingletapBPM);
			}
			set
			{
				this.config.SetValue<int>("RelaxAlternateIfLessThan", value);
			}
		}

		public SliderAlternationBinding SliderAlternationBinding
		{
			get
			{
				return this.config.GetValue<SliderAlternationBinding>("SliderAlternationBinding", SliderAlternationBinding.EndTime);
			}
			set
			{
				this.config.SetValue<SliderAlternationBinding>("SliderAlternationBinding", value);
			}
		}

		public int AudioOffset
		{
			get
			{
				return this.config.GetValue<int>("RelaxAudioOffset", 0);
			}
			set
			{
				this.config.SetValue<int>("RelaxAudioOffset", value);
			}
		}

		public int HoldBeforeSpinnerTime
		{
			get
			{
				return this.config.GetValue<int>("RelaxHoldBeforeSpinnerTime", 500);
			}
			set
			{
				this.config.SetValue<int>("RelaxHoldBeforeSpinnerTime", value);
			}
		}

		public bool EnableHitScan
		{
			get
			{
				return this.config.GetValue<bool>("EnableHitScan", true);
			}
			set
			{
				this.config.SetValue<bool>("EnableHitScan", value);
			}
		}

		public bool EnableHitScanPrediction
		{
			get
			{
				return this.config.GetValue<bool>("HitScanEnablePrediction", true);
			}
			set
			{
				this.config.SetValue<bool>("HitScanEnablePrediction", value);
			}
		}

		public int HitScanPredictionDirectionAngleTolerance
		{
			get
			{
				return this.config.GetValue<int>("HitScanPredictionDirectionAngleTolerance", 25);
			}
			set
			{
				this.config.SetValue<int>("HitScanPredictionDirectionAngleTolerance", value);
			}
		}

		public float HitScanPredictionRadiusScale
		{
			get
			{
				return this.config.GetValue<float>("HitScanPredictionRadiusScale", 0.8f);
			}
			set
			{
				this.config.SetValue<float>("HitScanPredictionRadiusScale", value);
			}
		}

		public int HitScanPredictionMaxDistance
		{
			get
			{
				return this.config.GetValue<int>("HitScanPredictionMaxDistance", 30);
			}
			set
			{
				this.config.SetValue<int>("HitScanPredictionMaxDistance", value);
			}
		}

		public int HitScanMissRadius
		{
			get
			{
				return this.config.GetValue<int>("HitScanMissRadius", 50);
			}
			set
			{
				this.config.SetValue<int>("HitScanMissRadius", value);
			}
		}

		public int HitScanMissChance
		{
			get
			{
				return this.config.GetValue<int>("HitScanMissChance", 20);
			}
			set
			{
				this.config.SetValue<int>("HitScanMissChance", value);
			}
		}

		public bool HitScanMissAfterHitWindow50
		{
			get
			{
				return this.config.GetValue<bool>("HitScanMissAfterHitWindow50", true);
			}
			set
			{
				this.config.SetValue<bool>("HitScanMissAfterHitWindow50", value);
			}
		}

		public int HitTimingsMinOffset
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsMinOffset", -40);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsMinOffset", value);
			}
		}

		public int HitTimingsMaxOffset
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsMaxOffset", 40);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsMaxOffset", value);
			}
		}

		public int HitTimingsAlternateMinOffset
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsAlternateMinOffset", -80);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsAlternateMinOffset", value);
			}
		}

		public int HitTimingsAlternateMaxOffset
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsAlternateMaxOffset", 80);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsAlternateMaxOffset", value);
			}
		}

		public int HitTimingsMinHoldTime
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsMinHoldTime", 25);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsMinHoldTime", value);
			}
		}

		public int HitTimingsMaxHoldTime
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsMaxHoldTime", 50);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsMaxHoldTime", value);
			}
		}

		public int HitTimingsMinSliderHoldTime
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsMinSliderHoldTime", -36);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsMinSliderHoldTime", value);
			}
		}

		public int HitTimingsMaxSliderHoldTime
		{
			get
			{
				return this.config.GetValue<int>("HitTimingsMaxSliderHoldTime", 50);
			}
			set
			{
				this.config.SetValue<int>("HitTimingsMaxSliderHoldTime", value);
			}
		}

		public float HitTimingsDoubleDelayFactor
		{
			get
			{
				return this.config.GetValue<float>("HitTimingsDoubleDelayFactor", 2f);
			}
			set
			{
				this.config.SetValue<float>("HitTimingsDoubleDelayFactor", value);
			}
		}

		public bool EnableTimewarp
		{
			get
			{
				return this.config.GetValue<bool>("EnableTimewarp", false);
			}
			set
			{
				this.config.SetValue<bool>("EnableTimewarp", value);
			}
		}

		public double TimewarpRate
		{
			get
			{
				return this.config.GetValue<double>("TimewarpRate", 1.0);
			}
			set
			{
				this.config.SetValue<double>("TimewarpRate", value);
			}
		}

		public bool UseCustomWindowTitle
		{
			get
			{
				return this.config.GetValue<bool>("UseCustomWindowTitle", false);
			}
			set
			{
				this.config.SetValue<bool>("UseCustomWindowTitle", value);
			}
		}

		public string CustomWindowTitle
		{
			get
			{
				return this.config.GetValue<string>("CustomWindowTitle", string.Empty);
			}
			set
			{
				this.config.SetValue<string>("CustomWindowTitle", value);
			}
		}

		public ConfigManager()
		{
			this.config = new Config(null);
		}

		private Config config;
	}
}
