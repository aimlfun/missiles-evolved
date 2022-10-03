using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissilesEvolved.Game;

internal class IRSensorLeftRight
{
    /// <summary>
    /// How many heat sensors that spot the ICBM. e.g. sample -8..+8 => 17
    /// </summary>
    internal const int SamplePoints = 17;

    /// <summary>
    /// We launch ABMs straight upwards, to have a chance of locating target we look up to 50 degrees left.
    /// </summary>
    private readonly float FieldOfVisionStartInDegrees = -50;

    /// <summary>
    /// We launch ABMs straight upwards, to have a chance of locating target we look up to 50 degrees right.
    /// </summary>
    private readonly float FieldOfVisionStopInDegrees = 50;

    /// <summary>
    /// The field of vision is split into sample segments, this is the angle that represents.
    /// </summary>
    private float VisionAngleInDegrees
    {
        get
        {
            return (SamplePoints == 1) ? 0 : (FieldOfVisionStopInDegrees - FieldOfVisionStartInDegrees) / (SamplePoints - 1);
        }
    }

    /// <summary>
    /// Stores the triangles for the heat sensor segments.
    /// </summary>
    private readonly List<PointF[]> heatSensorSweepTrianglePolygonsInDeviceCoordinates = new();

    /// <summary>
    /// Stores the triangle within the heat sensor indicating where the ICBM was found.
    /// </summary>
    private readonly List<PointF[]> heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates = new();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="angleCentreToSweep"></param>
    /// <param name="objectMCLocation"></param>
    /// <returns></returns>
    internal PointA[] GetTriangle(double angleCentreToSweep, PointA objectMCLocation)
    {
        List<PointA> items = new();

        // e.g 
        // input to the neural network
        //   _ \ | / _   
        //   0 1 2 3 4 
        //        
        double fieldOfVisionStartInDegrees = FieldOfVisionStartInDegrees;

        //   _ \ | / _   
        //   0 1 2 3 4
        //   [-] this
        double sensorVisionAngleInDegrees = VisionAngleInDegrees;

        //   _ \ | / _   
        //   0 1 2 3 4
        //   ^ this
        double sensorAngleToCheckInDegrees = fieldOfVisionStartInDegrees - sensorVisionAngleInDegrees / 2 + angleCentreToSweep;

        // how far ahead we look. Given this could be diagonal across the screen, it needs to be sufficient
        double DepthOfVisionInPixels = 700; // needs to cover whole screen during training

        //     -45  0  45
        //  -90 _ \ | / _ 90   <-- relative to direction of missile, hence + angle missile is pointing
        double LIDARangleToCheckInRadiansMin = MathUtils.DegreesInRadians(sensorAngleToCheckInDegrees);
        double LIDARangleToCheckInRadiansMax = LIDARangleToCheckInRadiansMin + MathUtils.DegreesInRadians(sensorVisionAngleInDegrees * SamplePoints);

        /*  p1        p2
            *   +--------+
            *    \      /
            *     \    /     this is our imaginary "heat sensor" triangle
            *      \  /
            *       \/
            *    abmLocation
            */
        PointA p1 = new((float)(Math.Sin(LIDARangleToCheckInRadiansMin) * DepthOfVisionInPixels + objectMCLocation.HorizontalInMissileCommandDisplayPX),
                        (float)(Math.Cos(LIDARangleToCheckInRadiansMin) * DepthOfVisionInPixels + objectMCLocation.AltitudeInMissileCommandDisplayPX));

        PointA p2 = new((float)(Math.Sin(LIDARangleToCheckInRadiansMax) * DepthOfVisionInPixels + objectMCLocation.HorizontalInMissileCommandDisplayPX),
                        (float)(Math.Cos(LIDARangleToCheckInRadiansMax) * DepthOfVisionInPixels + objectMCLocation.AltitudeInMissileCommandDisplayPX));

        items.Add(p1);
        items.Add(p2);
        items.Add(objectMCLocation);

        return items.ToArray();
    }

    /// <summary>
    /// Reads the "heat" sensor used to detect the ICBM.
    /// </summary>
    /// <param name="angleABMisPointing">The direction the ABM is pointing.</param>
    /// <param name="abmMCLocation">The position of the ABM.</param>
    /// <param name="icbmMCLocation">The position of the ICBM.</param>
    /// <param name="heatSensorRegionsOutput">The o</param>
    /// <returns></returns>
    internal double[] Read(double angleABMisPointing, PointA abmMCLocation, PointA icbmMCLocation)
    {
        heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates.Clear();
        heatSensorSweepTrianglePolygonsInDeviceCoordinates.Clear();

        double[] heatSensorRegionsOutput = new double[SamplePoints];

        // e.g 
        // input to the neural network
        //   _ \ | / _   
        //   0 1 2 3 4 
        //   ^     
        double fieldOfVisionStartInDegrees = FieldOfVisionStartInDegrees;

        //   _ \ | / _   
        //   0 1 2 3 4
        //   [-] this
        double sensorVisionAngleInDegrees = VisionAngleInDegrees;

        //   _ \ | / _   
        //   0 1 2 3 4
        //   ^ this
        double sensorAngleToCheckInDegrees = fieldOfVisionStartInDegrees - sensorVisionAngleInDegrees / 2 + angleABMisPointing;

        // how far ahead we look. Given this could be diagonal across the screen, it needs to be sufficient
        double DepthOfVisionInPixels = 700; // needs to cover whole screen plus some

        for (int LIDARangleIndex = 0; LIDARangleIndex < SamplePoints; LIDARangleIndex++)
        {
            //     -45  0  45
            //  -90 _ \ | / _ 90   <-- relative to direction of missile, hence + missile is pointing
            double LIDARangleToCheckInRadiansMin = MathUtils.DegreesInRadians(sensorAngleToCheckInDegrees);
            double LIDARangleToCheckInRadiansMax = LIDARangleToCheckInRadiansMin + MathUtils.DegreesInRadians(sensorVisionAngleInDegrees);

            /*  p1        p2
             *   +--------+
             *    \      /
             *     \    /     this is our imaginary "heat sensor" triangle
             *      \  /
             *       \/
             *    objectMCLocation
             */
            PointA p1 = new((float)(Math.Sin(LIDARangleToCheckInRadiansMin) * DepthOfVisionInPixels + abmMCLocation.HorizontalInMissileCommandDisplayPX),
                            (float)(Math.Cos(LIDARangleToCheckInRadiansMin) * DepthOfVisionInPixels + abmMCLocation.AltitudeInMissileCommandDisplayPX));

            PointA p2 = new((float)(Math.Sin(LIDARangleToCheckInRadiansMax) * DepthOfVisionInPixels + abmMCLocation.HorizontalInMissileCommandDisplayPX),
                            (float)(Math.Cos(LIDARangleToCheckInRadiansMax) * DepthOfVisionInPixels + abmMCLocation.AltitudeInMissileCommandDisplayPX));

            heatSensorSweepTrianglePolygonsInDeviceCoordinates.Add(new PointF[] { abmMCLocation.MCCoordsToDeviceCoordinates(),
                                                                                  p1.MCCoordsToDeviceCoordinates(),
                                                                                  p2.MCCoordsToDeviceCoordinates() });

            double mult = 0; // assume no target in this triangle, OR we are lined up centered with target

            if (MathUtils.PtInTriangle(icbmMCLocation, abmMCLocation, p1, p2))
            {

                if ((SamplePoints - 1) / 2 != LIDARangleIndex) // center mult=0, we don't need to deviate
                {
                    mult = 1f; // within the sensor, "1" chosen because it simplifies back-propagation. We don't want to tell it distance or offset within as that is cheating.
                }

                // track the triangle with the heat sensor.
                heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates.Add(new PointF[] { abmMCLocation.MCCoordsToDeviceCoordinates(),
                                                                                           p1.MCCoordsToDeviceCoordinates(),
                                                                                           p2.MCCoordsToDeviceCoordinates()
                                                                                          });
            }

            heatSensorRegionsOutput[LIDARangleIndex] = mult;

            //   _ \ | / _         _ \ | / _   
            //   0 1 2 3 4         0 1 2 3 4
            //  [-] from this       [-] to this
            sensorAngleToCheckInDegrees += sensorVisionAngleInDegrees;
        }

        // these two values tell the steering gimbal how desperately and in which direction we needs to compensate to correct course
        return heatSensorRegionsOutput;
    }

    /// <summary>
    /// Draws the full triangle sweep range.
    /// +--------+
    ///  \      /
    ///   \    /     this is our imaginary "heat sensor" triangle
    ///    \  /
    ///     \/
    ///     ABM
    /// </summary>
    /// <param name="g"></param>
    /// <param name="triangleSweepColour"></param>
    internal void DrawFullSweepOfHeatSensor(Graphics g, Color triangleSweepColour)
    {
        using SolidBrush brushOrange = new(triangleSweepColour);
        using Pen p = new(Color.FromArgb(8, 255, 255, 255));
        p.DashStyle = DashStyle.Dash;

        foreach (PointF[] point in heatSensorSweepTrianglePolygonsInDeviceCoordinates)
        {
            g.FillPolygon(brushOrange, point);
            g.DrawPolygon(p, point);
        }
    }

    /// <summary>
    /// Draws the region of the sweep that the target is in.
    /// +---++---+
    ///  \  ||  /
    ///   \ || /     hopefully the center strip
    ///    \||/
    ///     \/
    ///     ABM
    /// </summary>
    /// <param name="g"></param>
    internal void DrawWhereTargetIsInRespectToSweepOfHeatSensor(Graphics g, Brush sbColor)
    {
        // draw the heat sensor
        foreach (PointF[] point in heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates) g.FillPolygon(sbColor, point);
    }
}