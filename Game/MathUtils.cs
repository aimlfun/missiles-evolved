using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissilesEvolved.Game;

/// <summary>
/// Maths related utility functions.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Calculates whether a point is within a triangle.
    /// </summary>
    /// <param name="pointToTestIsInTriangle"></param>
    /// <param name="triangleVertex1"></param>
    /// <param name="triangleVertex2"></param>
    /// <param name="triangleVertex3"></param>
    /// <returns></returns>
    public static bool PtInTriangle(PointA pointToTestIsInTriangle, PointA triangleVertex1, PointA triangleVertex2, PointA triangleVertex3)
    {
        double det = (triangleVertex2.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX) * (triangleVertex3.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) - (triangleVertex2.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) * (triangleVertex3.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX);

        return det * ((triangleVertex2.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX) * (pointToTestIsInTriangle.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) - (triangleVertex2.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) * (pointToTestIsInTriangle.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX)) >= 0 &&
               det * ((triangleVertex3.HorizontalInMissileCommandDisplayPX - triangleVertex2.HorizontalInMissileCommandDisplayPX) * (pointToTestIsInTriangle.AltitudeInMissileCommandDisplayPX - triangleVertex2.AltitudeInMissileCommandDisplayPX) - (triangleVertex3.AltitudeInMissileCommandDisplayPX - triangleVertex2.AltitudeInMissileCommandDisplayPX) * (pointToTestIsInTriangle.HorizontalInMissileCommandDisplayPX - triangleVertex2.HorizontalInMissileCommandDisplayPX)) >= 0 &&
               det * ((triangleVertex1.HorizontalInMissileCommandDisplayPX - triangleVertex3.HorizontalInMissileCommandDisplayPX) * (pointToTestIsInTriangle.AltitudeInMissileCommandDisplayPX - triangleVertex3.AltitudeInMissileCommandDisplayPX) - (triangleVertex1.AltitudeInMissileCommandDisplayPX - triangleVertex3.AltitudeInMissileCommandDisplayPX) * (pointToTestIsInTriangle.HorizontalInMissileCommandDisplayPX - triangleVertex3.HorizontalInMissileCommandDisplayPX)) >= 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="angleInDegrees"></param>
    /// <returns></returns>
    public static double Clamp360(double angleInDegrees)
    {
        if (angleInDegrees < 0) angleInDegrees += 360;
        if (angleInDegrees >= 360) angleInDegrees -= 360;
        
        return angleInDegrees;
    }

    /// <summary>
    /// Logic requires radians but we track angles in degrees, this converts.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static double DegreesInRadians(double angle)
    {
       // if (angle < 0 || angle > 360) Debugger.Break();

        return (double)Math.PI * angle / 180;
    }

    /// <summary>
    /// Converts radians into degrees. 
    /// One could argue, WHY not just use degrees? Preference. Degrees are more intuitive than 2*PI offset values.
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static double RadiansInDegrees(double radians)
    {
        // radians = PI * angle / 180
        // radians * 180 / PI = angle
        return radians * 180F / Math.PI;
    }

    /// <summary>
    /// Computes the distance between 2 points using Pythagoras's theorem a^2 = b^2 + c^2.
    /// </summary>
    /// <param name="pt1">First point.</param>
    /// <param name="pt2">Second point.</param>
    /// <returns></returns>
    public static float DistanceBetweenTwoPoints(PointA pt1, PointA pt2)
    {
        float dx = pt2.HorizontalInMissileCommandDisplayPX - pt1.HorizontalInMissileCommandDisplayPX;
        float dy = pt2.AltitudeInMissileCommandDisplayPX - pt1.AltitudeInMissileCommandDisplayPX;

        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    
    /// <summary>
    /// Returns a value between min and max (never outside of).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0)
        {
            return min;
        }

        if (val.CompareTo(max) > 0)
        {
            return max;
        }

        return val;
    }
}
