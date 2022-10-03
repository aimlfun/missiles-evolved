using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissilesEvolved.Game;

public class PointA
{
    /// <summary>
    /// Vertical location for a point on a MissileCommand display (inverted compared to GDI). Pixels are 0-231.
    /// </summary>
    public float AltitudeInMissileCommandDisplayPX;

    /// <summary>
    /// Horizontal location for a point on a MissileCommand display. Pixels are 0-255.
    /// </summary>
    public float HorizontalInMissileCommandDisplayPX;

    /// <summary>
    /// New PointA() from X/Y in MC coordinates.
    /// </summary>
    /// <param name="altitudeInMissileCommandDisplayPX"></param>
    /// <param name="horizontalInMissileCommandDisplayPX"></param>
    public PointA(int horizontalInMissileCommandDisplayPX, int altitudeInMissileCommandDisplayPX)
    {
        AltitudeInMissileCommandDisplayPX = altitudeInMissileCommandDisplayPX;
        HorizontalInMissileCommandDisplayPX = horizontalInMissileCommandDisplayPX;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="altitudeInMissileCommandDisplayPX"></param>
    /// <param name="horizontalInMissileCommandDisplayPX"></param>
    public PointA(float horizontalInMissileCommandDisplayPX, float altitudeInMissileCommandDisplayPX)
    {
        AltitudeInMissileCommandDisplayPX = altitudeInMissileCommandDisplayPX;
        HorizontalInMissileCommandDisplayPX = horizontalInMissileCommandDisplayPX;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public PointF MCCoordsToDeviceCoordinates()
    {
//            Debug.WriteLine($"MC: {HorizontalInMissileCommandDisplayPX},{AltitudeInMissileCommandDisplayPX} = DC: {new Point((int)HorizontalInMissileCommandDisplayPX * 2, 2 * ((int)(231 - AltitudeInMissileCommandDisplayPX)))}");

        return new PointF(HorizontalInMissileCommandDisplayPX * 2,
                           2 * (231 - AltitudeInMissileCommandDisplayPX));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Point MCCoordsToDeviceCoordinatesP()
    {
        return new Point((int)HorizontalInMissileCommandDisplayPX * 2,
                         2 * ((int)(231 - AltitudeInMissileCommandDisplayPX)));
    }

    public override string? ToString()
    {
        return $"({this.HorizontalInMissileCommandDisplayPX},{this.AltitudeInMissileCommandDisplayPX})";
    }
}
