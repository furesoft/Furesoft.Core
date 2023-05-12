using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Furesoft.Core.Measure;

	public partial class MeasureContext
	{
		private static readonly Regex rUnit = new(@"(\(((?<1>(10|2)\^-?\d+)(\*|\.)?)*\))?(?<2>[^\*\.\-0123456789]+)(?<3>-?\d+)?|(?<4>(10|2)\^-?\d+)(?=\*|\.)?",
									RegexOptions.Compiled
									| RegexOptions.CultureInvariant
									| RegexOptions.ExplicitCapture);

		/// <summary>
		/// Tries to parse a string as a <see cref="MeasureUnit"/> and registers it in this context
		/// on success.
		/// </summary>
		/// <param name="s">The string to parse. Must not be null.</param>
		/// <param name="u">On success, the unit of measure.</param>
		/// <returns>True on success, false if the string can not be parsed as a measur of unit.</returns>
		public bool TryParse(string s, out MeasureUnit u)
		{
			if (s == null) throw new ArgumentNullException(nameof(s));
			// 1 - Normalize input by removing all white spaces.
			// Trust the Regex cache for this quite common one.
			s = Regex.Replace(s, "\\s+", string.Empty);
			if (_allUnits.TryGetValue(s, out u)) return true;
			var match = rUnit.Match(s);
			if (!match.Success) return false;
			var all = new List<ExponentMeasureUnit>();
			do
			{
				if (!TryParseExponent(match, out var e)) return false;
				if (e != MeasureUnit.None) all.Add(e);
				match = match.NextMatch();
			}
			while (match.Success);
			u = MeasureUnit.Combinator.Create(this, all);
			return true;
		}

		private bool TryParseExponent(Match match, out ExponentMeasureUnit u, bool skipFirstLookup = false)
		{
			if (!skipFirstLookup && TryGetExistingExponent(match.ToString(), out u)) return true;
			u = null;
			// Handling dimensionless unit.
			var factor = match.Groups[4].Value;
			if (factor.Length > 0)
			{
				var f = ParseFactor(factor, ExpFactor.Neutral);
				u = RegisterPrefixed(f, MeasureStandardPrefix.None, MeasureUnit.None);
				return true;
			}
			var sExp = match.Groups[3].Value;
			var exp = sExp.Length > 0 ? int.Parse(sExp) : 1;
			if (exp == 0)
			{
				u = MeasureUnit.None;
				return true;
			}
			var withoutExp = match.Groups[1].Value + match.Groups[2].Value;
			if (_allUnits.TryGetValue(withoutExp, out var unit))
			{
				if (exp == 1 && unit is ExponentMeasureUnit direct)
				{
					u = direct;
					return true;
				}
				if (!(unit is AtomicMeasureUnit atomic)) return false;
				u = RegisterExponent(exp, atomic);
				return true;
			}
			var adjustment = ExpFactor.Neutral;
			foreach (Capture f in match.Groups[1].Captures)
			{
				adjustment = ParseFactor(f.Value, adjustment);
			}
			var withoutAdjustment = match.Groups[2].Value;
			if (_allUnits.TryGetValue(withoutAdjustment, out unit))
			{
				if (!(unit is AtomicMeasureUnit basic)) return false;
				if (basic is PrefixedMeasureUnit prefix)
				{
					adjustment = adjustment.Multiply(prefix.AdjustmentFactor).Multiply(prefix.Prefix.Factor);
					basic = prefix.AtomicMeasureUnit;
				}
				u = CreateExponentPrefixed(exp, adjustment, basic);
				return true;
			}
			var p = MeasureStandardPrefix.FindPrefix(withoutAdjustment);
			if (p == MeasureStandardPrefix.None) return false;
			withoutAdjustment = withoutAdjustment[p.Abbreviation.Length..];
			if (withoutAdjustment.Length == 0) return false;
			if (_allUnits.TryGetValue(withoutAdjustment, out unit))
			{
				if (unit is not AtomicMeasureUnit basic) return false;
				if (basic is PrefixedMeasureUnit prefix) return false;
				adjustment = adjustment.Multiply(p.Factor);
				u = CreateExponentPrefixed(exp, adjustment, basic);
				return true;
			}
			return false;
		}

		private ExpFactor ParseFactor(string s, ExpFactor f)
		{
			if (s[0] == '2')
			{
				f = f.Multiply(new(short.Parse(s.Substring(2)), 0));
			}
			else
			{
				f = f.Multiply(new(0, short.Parse(s.Substring(3))));
			}

			return f;
		}

		private ExponentMeasureUnit CreateExponentPrefixed(int exp, ExpFactor adjustment, AtomicMeasureUnit atomic)
		{
			ExponentMeasureUnit u;
			if (!adjustment.IsNeutral)
			{
				var best = MeasureStandardPrefix.FindBest(adjustment, atomic.AutoStandardPrefix);
				atomic = RegisterPrefixed(best.Adjustment, best.Prefix, atomic);
			}
			u = RegisterExponent(exp, atomic);
			return u;
		}

		private bool TryGetExistingExponent(string s, out ExponentMeasureUnit u)
		{
			u = null;
			if (_allUnits.TryGetValue(s, out var unit))
			{
				Debug.Assert(unit is ExponentMeasureUnit);
				u = (ExponentMeasureUnit)unit;
				return true;
			}
			return false;
		}
	}
