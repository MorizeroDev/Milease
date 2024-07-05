using System;
using System.Globalization;
using UnityEngine;

namespace Milease.Utils.Colors
{
    public static class Tools
    {
        private static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;

        internal static void LogInfo(string s)
        {
#if UNITY_EDITOR
            Debug.Log(s);
#endif
        }

        internal static void LogError(string s)
        {
#if UNITY_EDITOR
            Debug.LogError(s);
#endif
        }

        internal static bool TryParseHexToUint(string s, out uint result)
        {
            return uint.TryParse(s, NumberStyles.HexNumber, ParseCulture, out result);
        }

        internal static bool TryParseDouble(string s, out double result)
        {
            return double.TryParse(s, NumberStyles.Any, ParseCulture, out result);
        }

        internal static bool TryParsePercentage(string s, out double result)
        {
            result = 0;
            if (!s.EndsWith('%'))
            {
                return false;
            }

            s = s[..^1];
            var ret = double.TryParse(s, NumberStyles.Any, ParseCulture, out result);
            if (!ret) return ret;

            result = result / 100;
            return true;
        }

        internal static bool TryParseAngle(string s, out double degrees)
        {
            degrees = 0;
            double value;
            string number;

            if (s.EndsWith("deg"))
            {
                number = s[..^3].Trim(); // Remove the last three characters "deg"
                if (TryParseDouble(number, out value))
                {
                    degrees = value; // Already in degrees
                    return true;
                }
            }
            else if (s.EndsWith("grad"))
            {
                number = s[..^4].Trim(); // Remove the last four characters "grad"
                if (TryParseDouble(number, out value))
                {
                    degrees = value * (360.0 / 400.0); // Convert gradians to degrees
                    return true;
                }
            }
            else if (s.EndsWith("rad"))
            {
                number = s[..^3].Trim(); // Remove the last three characters "rad"
                if (TryParseDouble(number, out value))
                {
                    degrees = value * (180.0 / Math.PI); // Convert radians to degrees
                    return true;
                }
            }
            else if (s.EndsWith("turn"))
            {
                number = s[..^4].Trim(); // Remove the last four characters "turn"
                if (TryParseDouble(number, out value))
                {
                    degrees = value * 360.0; // Convert turns to degrees
                    return true;
                }
            }
            else
            {
                if (TryParseDouble(s, out value))
                {
                    degrees = value; // Assume the input is already in degrees if no unit is specified
                    return true;
                }
            }

            return false;
        }
    }
}
