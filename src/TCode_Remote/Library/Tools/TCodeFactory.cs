using TCode_Remote.Library.Extension;
using TCode_Remote.Library.Handler;
using TCode_Remote.Library.Model;
using TCode_Remote.Library.Reference.Constants;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TCode_Remote.Library.Tools
{
	public class TCodeFactory
	{
		private double _input_start = 0.0;
		private double _input_end = 1.0;
		private Dictionary<string, double> _addedAxis = new Dictionary<string, double>();
		public void Init()
		{
			_addedAxis.Clear();
		}
		public TCodeFactory(double inputStart, double inputEnd)
		{
			_input_start = inputStart;
			_input_end = inputEnd;
		}

		public void Calculate(string axisName, double value, HashSet<ChannelValueModel> axisValues)
		{
			var tcodeAxisName = SettingsHandler.GamepadButtonMap.GetValue(axisName);
			if (tcodeAxisName != AxisNames.None)
			{
				var tcodeAxis = SettingsHandler.AvailableAxis.GetValue(tcodeAxisName);
				var isNegative = tcodeAxis.AxisName.Contains(AxisNames.NegativeModifier);
				var isPositive = tcodeAxis.AxisName.Contains(AxisNames.PositiveModifier);
				if (_addedAxis.GetValue(tcodeAxis.Channel) == 0 && value != 0)
				{
					_addedAxis.Remove(tcodeAxis.Channel);
					axisValues.RemoveWhere(x => x.c.Equals(tcodeAxis.Channel));
				}
				if (!axisValues.Any(x => x.c == tcodeAxis.Channel))
				{
					double calculatedValue = value;
					if (isNegative && value > 0)
					{
						calculatedValue = -(value);
					}
					if (value != 0 && 
						(((tcodeAxis.AxisName == AxisNames.Stroke || tcodeAxis.AxisName == AxisNames.StrokeUp || tcodeAxis.AxisName == AxisNames.StrokeDown) && SettingsHandler.InverseTcXL0) ||
						((tcodeAxis.AxisName == AxisNames.Pitch || tcodeAxis.AxisName == AxisNames.PitchForward || tcodeAxis.AxisName == AxisNames.PitchBack) && SettingsHandler.InverseTcXRollR2) ||
						((tcodeAxis.AxisName == AxisNames.Roll || tcodeAxis.AxisName == AxisNames.RollLeft || tcodeAxis.AxisName == AxisNames.RollRight) && SettingsHandler.InverseTcYRollR1)))
					{
						calculatedValue = -(value);
					}
					axisValues.Add(new ChannelValueModel()
					{
						v = CalculateTcodeRange(calculatedValue, tcodeAxis.Channel),
						c = tcodeAxis.Channel,
						s = SettingsHandler.Speed,
						i = 0
					});
					_addedAxis.Add(tcodeAxis.Channel, value);
				}
			}
		}

		public void Calculate(string gamepadAxis, bool gamepadValue, HashSet<ChannelValueModel> axisValues)
		{
			int value = gamepadValue ? 1 : 0;
			Calculate(gamepadAxis, value, axisValues);
		}

		public string FormatTCode(HashSet<ChannelValueModel> values)
		{
			var tCode = new StringBuilder();
			foreach (var value in values)
			{
				var minValue = SettingsHandler.AvailableAxis.GetValue(value.c).Min;
				var maxValue = SettingsHandler.AvailableAxis.GetValue(value.c).Max;
				var clampedValue = maxValue == 0 ? value.v : MathExtension.Clamp(value.v, minValue, maxValue);
				tCode.Append($"{value.c}{(clampedValue < 10 ? "0" : "")}{clampedValue}S{SettingsHandler.Speed} ");
			}
			return $"{tCode.ToString().Trim()}\n";
		}

		private int CalculateTcodeRange(double value, string channel)
		{
			var output_end = SettingsHandler.AvailableAxis.GetValue(channel).Max;
			var output_start = SettingsHandler.AvailableAxis.GetValue(channel).Mid;
			var slope = (output_end - output_start) / (_input_end - _input_start);
			return (int)(output_start + slope * (value - _input_start));
		}

		private int CalculateGamepadSpeed(double gpIn)
		{
			return (int)(gpIn < 0 ? -gpIn * SettingsHandler.Speed : gpIn * SettingsHandler.Speed);
		}
	}
}
