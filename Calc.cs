using Autodesk.AutoCAD.Geometry;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpPt1
{
    /// <summary>
    /// Basic class for math operations and conversions.
    /// </summary>
    public static class Calc

    {

        public static double RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }

        /// <summary>
        /// Rounds double number.
        /// </summary>
        public static double RndXY(double value, int digits = 0)
        {
            return Math.Round(value, digits);
        }

        public static double RndXYMid(double value, int digits = 0, MidpointRounding midpoint = MidpointRounding.AwayFromZero)
        {
            return Math.Round(value, digits, midpoint);
        }

        /// <summary>
        /// Normalizes angle.
        /// </summary>
        /// <param name="Angle"></param>
        /// <returns></returns>
        public static double NormalAngle(double Angle)
        {
            return (Angle + 2 * Math.PI) % (2 * Math.PI);
        }

        /// <summary>
        /// Converts radians to angle.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static int RadToDeg(double radians)
        {
            return (int)(radians * (180 / Math.PI));
        }

        /// <summary>
        /// Checks number is in range.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static bool Between(this int num, int number1, int number2, bool inclusive = false)
        {
            if (number2 > number1)
            {
                return inclusive
                ? number2 >= num && num >= number1
                : number2 > num && num > number1;
            }
            return inclusive
                ? number1 >= num && num >= number2
                : number1 > num && num > number2;
        }

        /// <summary>
        /// Checks number is in range.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static bool Between(this decimal num, string string1, string string2, bool inclusive = false)
        {
            decimal number1 = Convert.ToDecimal(string1);
            decimal number2 = Convert.ToDecimal(string2);
            if (number2 > number1)
            {
                return inclusive
                ? number2 >= num && num >= number1
                : number2 > num && num > number1;
            }
            return inclusive
                ? number1 >= num && num >= number2
                : number1 > num && num > number2;
        }

        /// <summary>
        /// Checks number is in range.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static bool Between(this double num, double number1, double number2, bool inclusive = false)
        {
            if (number2 > number1)
            {
                return inclusive
                ? number2 >= num && num >= number1
                : number2 > num && num > number1;
            }
            return inclusive
                ? number1 >= num && num >= number2
                : number1 > num && num > number2;
        }

        /// <summary>
        /// Checks number is in range.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        public static bool Between(this decimal num, decimal number1, decimal number2, bool inclusive = false)
        {
            if (number2 > number1)
            {
                return inclusive
                ? number2 >= num && num >= number1
                : number2 > num && num > number1;
            }
            return inclusive
                ? number1 >= num && num >= number2
                : number1 > num && num > number2;
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point.</param>
        /// <returns>The distance between two points.</returns>
        public static double GetDistance(Point2d p1, Point2d p2)
        {
            double xDelta = p1.X - p2.X;
            double yDelta = p1.Y - p2.Y;

            return Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2));
        }
        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point.</param>
        /// <returns>The distance between two points.</returns>
        public static double GetDistance(Point3d p1, Point3d p2)
        {
            double xDelta = p1.X - p2.X;
            double yDelta = p1.Y - p2.Y;

            return Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2));
        }

        public static string RemoveCharsFromString(string numbchars)
        {
            return Regex.Replace(numbchars, "[^0-9n/a]", "");
        }

        public static DateTime StringToDate(string s, out DateTime date, out bool success, bool log = true)
        {
            StackTrace stackTrace = new StackTrace();
            success = false;

            if (!DateTime.TryParseExact(s, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out date))
            {
                if (!DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    if (log)
                    {
                        ErrLogger.Error("Wrong date format", "", "");
                        ErrLogger.ErrorsFound = true;
                    }
                    success = false;
                }
            }
            else
            {
                success = true;
            }
            return date;
        }
    }
}


