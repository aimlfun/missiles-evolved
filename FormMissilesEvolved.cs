#define USING_AI // <-- use AI, comment out to use mapping instead.
using MissilesEvolved.Game;
using System.Diagnostics;
using System.Security.Cryptography;

namespace MissilesEvolved;

public partial class FormMissilesEvolved : Form
{
    #region Constants
    /// <summary>
    /// Used as an amplifier for deltaX of the ICBM. Larger makes the ICBM move faster. Too large will be
    /// impossible to intercept.
    /// </summary>
    private const int c_deltaxIBCM = 3;

    /// <summary>
    /// A pseudo gravity. This is applied downwards regardless of the direction the missile is travelling.
    /// </summary>
    private const float c_gravityABM = 0.9f;

    /// <summary>
    /// How long it pauses when ABM succesfully intercepts the ICBM.
    /// </summary>
    private const int c_pauseInMSafterABMhitICBM = 500;
    #endregion

    #region Inter Continental Ballistic Missile
    /// <summary>
    /// Location of the ICBM in Missile Command coordinates.
    /// </summary>
    PointA icbmLocation = new(0, 0);

    /// <summary>
    /// Last location of the ICBM. currently present to know when to add to smoke trail.
    /// </summary>
    PointF lastLocationInDeviceCoordinatesICBM;

    /// <summary>
    /// Amount the ICBM moves horizontally each time.
    /// </summary>
    float chosenDeltaXforICBM;

    /// <summary>
    /// List of points to draw as a smoke trail (follows the ICBM).
    /// </summary>
    readonly List<PointF> smokeTrailPointsICBM = new();
    #endregion

    #region Anti Ballistic Missile
    /// <summary>
    /// Where the ABM is currently (in Missile Command UI coordinates).
    /// </summary>
    PointA abmLocation = new(128, 6);

    /// <summary>
    /// Speed the ABM is travelling. It's a bit simplistic without v=u+at, f=ma, and all that.
    /// Look at it more as increasing thrust.
    /// </summary>
    float abmSpeed;

    /// <summary>
    /// Angle the ABM is travelling in radians.
    /// </summary>
    double abmAngleinRadians;

    /// <summary>
    /// Last location of the ABM. Could be used for better hit detection, currently
    /// present to know when to add to smoke trail.
    /// </summary>
    PointF lastLocationInDeviceCoordinatesABM;

    /// <summary>
    /// List of points to draw as a smoke trail (follows the ABM).
    /// </summary>
    readonly List<PointF> smokeTrailPointsABM = new();

    /// <summary>
    /// A sensor to detect heat signature of ICBM.
    /// </summary>
    readonly IRSensorLeftRight abmHeatSensor = new();
    #endregion

    /// <summary>
    /// Keep score of how many times the ICBM was intercepted.
    /// </summary>
    int countOfTimesICBMHitsByABM = 0;
    
    /// <summary>
    /// Keep score of how many times the ABM failed to intercept the ICBM.
    /// </summary>
    int countOfTimesABMfailedIntercept = 0;

    /// <summary>
    /// Used to track wind direction, and have wind rotate rather than randomly switch left-right/top to bottom.
    /// </summary>
    float angleOfWindInRadians = 0;

    /// <summary>
    /// Tracks the interval, so we can slow things done.
    /// </summary>
    int timerinterval = 10;
    
    #region Resources
    /// <summary>
    /// We write the missile speed, stats on hits, fps in this font at the top.
    /// </summary>
    readonly Font fontForTextAtTopOfDisplay = new("Arial", 8);
    
    /// <summary>
    /// We draw smoke from the ABM, so it one can see the path it took.
    /// </summary>
    readonly Pen penABMSmokeTrail = new (Color.FromArgb(235, 0, 0, 255), 2); // blue smoke for ABM

    /// <summary>
    /// We draw smoke from the ICBM, so it one can see the path it took.
    /// </summary>
    readonly Pen penICBMSmokeTrail = new(Color.FromArgb(235, 255, 0, 0), 2);

    /// <summary>
    /// Line for the "windicator" that shows strength and direction in the form of an arrow.
    /// </summary>
    readonly Pen penWindIndicator = new(Color.FromArgb(200, 255, 255, 255));

    /// <summary>
    /// We plot a red triangle highlighting the quadrant the ICBM is found in.
    /// </summary>
    readonly SolidBrush brushQuadrantICBMisIn  = new(Color.FromArgb(20, 255, 0, 0));
    
    /// <summary>
    /// We draw a filled circle when ABM and ICBM collide.
    /// </summary>
    readonly SolidBrush explosionCircleBrush = new(Color.FromArgb(160, 200, 255, 200));
    #endregion

    /// <summary>
    /// Constructor.
    /// </summary>
    public FormMissilesEvolved()
    {
#if !USING_AI
        if (IRSensorLeftRight.SamplePoints != 17) throw new Exception("Sample points must be 17 for the mapping array to work");
#endif
        InitializeComponent();

        // "windicator" showing direction and strength is a dotted line with an arrow
        // length of which indicates strength, direction pointing which way it blows
        penWindIndicator.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        penWindIndicator.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
    }

    /// <summary>
    /// On load we start the "timer" which moves the ABMs.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Form1_Load(object sender, EventArgs e)
    {

#if USING_AI
        Text = "Missiles Evolved - AI targeting";
        if (!AccurateABM.IsTrained) throw new Exception("AI should be trained during static constructor.");
#else
        Text = "Missiles Evolved - hard-coded targeting (not AI)";
#endif

        CreateNewICBMTargetAndResetSimulation();

        // use a "timer" to do frame animation
        timerSimulate.Tick += TimerSimulate_Tick;
        timerSimulate.Start();
    }

    /// <summary>
    /// Creates an inbound ICBM at a random point, and puts the ABM bottom center.
    /// Both have their smoke trails reset.
    /// </summary>
    private void CreateNewICBMTargetAndResetSimulation()
    {
        // initialise / reset ICBM
        icbmLocation = new(RandomNumberGenerator.GetInt32(0, 256), 231 - RandomNumberGenerator.GetInt32(0, 120));

        // ICBM moves fixed downwards, varying speed horizontally. A random number generator defines the basic direction +/-0.5
        // constant amplifies that basic amount so it moves more rapidly.
        chosenDeltaXforICBM = c_deltaxIBCM * (float)(RandomNumberGenerator.GetInt32(0, 10000) - 5000) / 10000;
        smokeTrailPointsICBM.Clear();

        // initialise / reset ABM
        abmLocation = new(128, 6);
        abmSpeed = 1f;
        abmAngleinRadians = Math.PI / 2;
        smokeTrailPointsABM.Clear();

        // timer is set to 800 or something other than 10 when a hit occurs.
        timerSimulate.Interval = timerinterval;
        angleOfWindInRadians = 0;
    }

    /// <summary>
    /// The "game play" uses a timer to move the missiles.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimerSimulate_Tick(object? sender, EventArgs e)
    {
        // off screen, or hit?
        if (IsEndOfSimulationRound())
        {
            CreateNewICBMTargetAndResetSimulation();
            return;
        }

        Bitmap imageGamePlay = new(512, 462); // we draw on this.

        using Graphics graphicsGamePlay = Graphics.FromImage(imageGamePlay);

        graphicsGamePlay.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
        graphicsGamePlay.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        graphicsGamePlay.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
        graphicsGamePlay.Clear(Color.Black);

        MoveICBM(graphicsGamePlay);

        float averageSpeed = MoveABM(graphicsGamePlay, out float windStrength);

        // draw the sensors
        abmHeatSensor.DrawFullSweepOfHeatSensor(graphicsGamePlay, Color.FromArgb(30, 255, 255, 255));
        abmHeatSensor.DrawWhereTargetIsInRespectToSweepOfHeatSensor(graphicsGamePlay, brushQuadrantICBMisIn);

        DrawWindIndicator(abmLocation, graphicsGamePlay, windStrength);

        DrawMissileSquares(icbmLocation, abmLocation, graphicsGamePlay);

        DrawExplosionCircleIfABMhitICBM(graphicsGamePlay);

        string wind = (windStrength == 0) ? "" : $"| wind strength {windStrength:#0.#} @ {MathUtils.Clamp360(MathUtils.RadiansInDegrees(angleOfWindInRadians)):##0.#} degrees";

        // write a few stats at the top
        graphicsGamePlay.DrawString($"Vel.: {(averageSpeed * 100):#.#} m/s | hits {countOfTimesICBMHitsByABM} | misses {countOfTimesABMfailedIntercept} | fps {1000 / timerinterval} {wind}", fontForTextAtTopOfDisplay, Brushes.White, 3, 3);
        graphicsGamePlay.Flush();

        pictureBox1.Image?.Dispose();
        pictureBox1.Image = imageGamePlay;
    }

    /// <summary>
    /// Detect whether to end the simulation round. 
    /// </summary>
    /// <returns></returns>
    private bool IsEndOfSimulationRound()
    {
        if (ABMhitICBM()) // ABM hit ICBM.
        {
            countOfTimesICBMHitsByABM++;
            return true;
        }

        if (abmLocation.AltitudeInMissileCommandDisplayPX > 331 || // ABM has gone off top of screen by 100px
            abmLocation.AltitudeInMissileCommandDisplayPX < 6 ||   // ABM hit ground
            icbmLocation.AltitudeInMissileCommandDisplayPX < 6)    // ICBM hit ground target
        {
            countOfTimesABMfailedIntercept++;
            return true;
        }

        // both missiles are moving
        return false;
    }

    /// <summary>
    /// Two missiles are touching or colliding.
    /// </summary>
    /// <returns>true - ABM has hit ICBM | false - it hasn't</returns>
    private bool ABMhitICBM()
    {
        return MathUtils.DistanceBetweenTwoPoints(icbmLocation, abmLocation) < 4;
    }

    /// <summary>
    /// Draws an explosion circle and pauses.
    /// </summary>
    /// <param name="graphicsGamePlay"></param>
    private void DrawExplosionCircleIfABMhitICBM(Graphics graphicsGamePlay)
    {
        if (!ABMhitICBM()) return;

        // make it "pause" so user sees explosion "circle"
        timerSimulate.Interval = c_pauseInMSafterABMhitICBM;

        // an explosion circle filled. Nothing fancy, just minimum.
        PointF point = abmLocation.MCCoordsToDeviceCoordinates();
        graphicsGamePlay.FillEllipse(explosionCircleBrush, new RectangleF(point.X - 10, point.Y - 10, 20, 20));
    }

    /// <summary>
    /// Moves the anti-ballistic missile using "AI" neural network.
    /// </summary>
    /// <param name="graphicsGamePlay"></param>
    /// <param name="windStrength"></param>
    private float MoveABM(Graphics graphicsGamePlay, out float windStrength )
    {
        windStrength = ComputeWindStrengthAndAngle();

        // 0 is horizontal to right, we want it to be vertical upwards, so we rotate left 90 degrees
        double angleInDeg = MathUtils.RadiansInDegrees(abmAngleinRadians) - 90;
        angleInDeg = MathUtils.Clamp360(angleInDeg); // keep in range 0..359.

        // the sensor tells us which quadrant the heat was found in
        double[] output = abmHeatSensor.Read(angleInDeg, abmLocation, icbmLocation);

#if USING_AI
        // it's the "offset" angle that is returned. Physical angles won't work as we're rotating the missile
        // and in doing so orientation changes.
        abmAngleinRadians += AccurateABM.networkMissile.FeedForward(output)[0];
#else
        abmAngleinRadians += MappingBasedGuidanceSystem(output);
#endif

        // delta to move vertically
        // - applying a very simplistic gravity
        float dy = (float)Math.Sin(abmAngleinRadians) * abmSpeed - c_gravityABM - (float)(windStrength * Math.Sin(angleOfWindInRadians));

        // delta to move horizontally
        // - apply a simple wind that pushes left or right.
        float dx = (float)Math.Cos(abmAngleinRadians) * abmSpeed - (float)(windStrength * Math.Cos(angleOfWindInRadians));

        // move based on the intended acceleration (that increases)
        abmLocation.AltitudeInMissileCommandDisplayPX += dy;

        // ensure when initially launching it goes upwards and doesn't hit the floor
        abmLocation.AltitudeInMissileCommandDisplayPX = abmLocation.AltitudeInMissileCommandDisplayPX.Clamp(6, int.MaxValue);

        abmLocation.HorizontalInMissileCommandDisplayPX -= dx;
        abmSpeed += 0.07f;

        ManageABMsmokeTrail(graphicsGamePlay);

        // this is really not desirable, but we have no choice. If we don't do it, then the heat sensor appears to come from the
        // last position of the missile (which it did, let's be clear). So we fake a call here to set the triangles relative to
        // where it moved to.

        // 0 is horizontal to right, we want it to be vertical upwards, so we rotate left 90 degrees
        angleInDeg = MathUtils.RadiansInDegrees(abmAngleinRadians) - 90;
        angleInDeg = MathUtils.Clamp360(angleInDeg); // keep in range 0..359.

        // the sensor tells us which quadrant the heat was found in
        output = abmHeatSensor.Read(angleInDeg, abmLocation, icbmLocation);

        return (float)Math.Sqrt(dx * dx + dy * dy); // average speed takes into account the fact it is a vector (x and y speeds)
    }

    /// <summary>
    /// Pick a random wind strength if they have not ticked "constant wind".
    /// If it has a deviation, pick a random angle to add to the wind to simulate the fact wind
    /// is not blowing in a constant direction but moves up/down
    /// </summary>
    /// <returns></returns>
    private float ComputeWindStrengthAndAngle()
    {
        // add wind shear
        // if wind provided, we add a "wind" factor. The wind can be a constant value or variable up to limit
        float windStrength = (float)numericUpDownWindStrength.Value * (checkBoxConstantWind.Checked ? 1f : (float)(RandomNumberGenerator.GetInt32(0, 10000)) / 10000);
        
        if (numericUpDownMaxDeviation.Value > 0)
        {
            float randomNumber = RandomNumberGenerator.GetInt32(0, 1000) - 500; // -500..500
            randomNumber /= 500f; // -1..1
            randomNumber *= (float)numericUpDownMaxDeviation.Value; // -deviation..+deviation degrees
            randomNumber /= 360; // % of circle
            angleOfWindInRadians += (float)(randomNumber * 2 * Math.PI); // radians            
        }
        else
        {
            angleOfWindInRadians = 0;
        }

        return windStrength;
    }

#if !USING_AI
    /// <summary>
    /// The AI is mapping heat sensor 0..16 to the values below, we can skip the AI stuff and do it hard coded.
    /// </summary>
    /// <param name="outputFromHeatSensor">Heatsensor output 1/0 for each sensor.</param>
    /// <returns>Angle in radians required to rotate based on heat sensor.</returns>
    private static double MappingBasedGuidanceSystem(double[] outputFromHeatSensor)
    {
        // because our table is hard-coded, we aren't flexible on this. If you want one with more or less, then
        // copy the AI output per segment into this array.
        if (outputFromHeatSensor.Length != 17) throw new ArgumentOutOfRangeException(nameof(outputFromHeatSensor), "This guidance expects 17 heat sensors");

        // how much the ABM needs to rotate for each sensor
        float[] mapOfAngleToRotateRelativeToHeatSensor = new float[17] { -0.819f, -0.709f, -0.6f, -0.491f, -0.382f, -0.273f, -0.164f, -0.054f, 0.054f, 0.164f, 0.273f, 0.382f, 0.491f, 0.6f, 0.709f, 0.818f, 0.871f };

        // whichever has a "1" indicates that particular sensor has spotted the ICBM
        for (int i = 0; i < outputFromHeatSensor.Length; i++)
        {
            if (outputFromHeatSensor[i] == 1) return mapOfAngleToRotateRelativeToHeatSensor[i]; // this will make it rotate
        }

        return 0; // on target requiring no deviation OR target is outside the sensor cone.
    }
#endif

    /// <summary>
    /// Builds a list of points the ABM touches to draw a smoke trail.
    /// </summary>
    /// <param name="graphicsGamePlay"></param>
    private void ManageABMsmokeTrail(Graphics graphicsGamePlay)
    {
        PointF locationInDeviceCoordinates = abmLocation.MCCoordsToDeviceCoordinates();

        if (locationInDeviceCoordinates.Y >= 21 &&
            (locationInDeviceCoordinates.X != lastLocationInDeviceCoordinatesABM.X ||
            locationInDeviceCoordinates.Y != lastLocationInDeviceCoordinatesABM.Y))
        {
            smokeTrailPointsABM.Add(new PointF(locationInDeviceCoordinates.X, locationInDeviceCoordinates.Y));
        }

        lastLocationInDeviceCoordinatesABM = new PointF(locationInDeviceCoordinates.X, locationInDeviceCoordinates.Y);

        if (smokeTrailPointsABM.Count > 1) // 2 points minimum to draw a "line".
        {
            graphicsGamePlay.DrawLines(penABMSmokeTrail, smokeTrailPointsABM.ToArray()); // these are stored in device coordinates
        }
    }

    /// <summary>
    /// Moves the ICBM in a general direction with a little bit of "random" movement.
    /// </summary>
    /// <param name="graphicsGamePlay"></param>
    private void MoveICBM(Graphics graphicsGamePlay)
    {
        icbmLocation.HorizontalInMissileCommandDisplayPX += chosenDeltaXforICBM + (float)(RandomNumberGenerator.GetInt32(0, 10000) - 5000) / 10000;

        // hit the walls? bounce off them. Artistic license, after all, I have never seen a real one.
        if (icbmLocation.HorizontalInMissileCommandDisplayPX < 10 ||
            icbmLocation.HorizontalInMissileCommandDisplayPX > 246)
            chosenDeltaXforICBM = -chosenDeltaXforICBM; // reverse direction

        icbmLocation.HorizontalInMissileCommandDisplayPX.Clamp(0, 255); // just in case
        icbmLocation.AltitudeInMissileCommandDisplayPX -= 0.5f; // fixed vertical drop.

        ManageICBMsmokeTrail(graphicsGamePlay);
    }

    /// <summary>
    /// Builds a list of points the ICBM touches to draw a smoke trail.
    /// </summary>
    /// <param name="graphicsGamePlay"></param>
    private void ManageICBMsmokeTrail(Graphics graphicsGamePlay)
    {
        PointF locationInDeviceCoordinates = icbmLocation.MCCoordsToDeviceCoordinates();

        if (locationInDeviceCoordinates.Y >= 6 &&
            (locationInDeviceCoordinates.X != lastLocationInDeviceCoordinatesICBM.X ||
            locationInDeviceCoordinates.Y != lastLocationInDeviceCoordinatesICBM.Y))
        {
            smokeTrailPointsICBM.Add(new PointF(locationInDeviceCoordinates.X, locationInDeviceCoordinates.Y));
        }

        if (smokeTrailPointsICBM.Count > 1) // 2 points minimum to draw a "line".
        {
            graphicsGamePlay.DrawLines(penICBMSmokeTrail, smokeTrailPointsICBM.ToArray()); // these are stored in device coordinates
        }

        lastLocationInDeviceCoordinatesICBM = new PointF(locationInDeviceCoordinates.X, locationInDeviceCoordinates.Y);
    }

    /// <summary>
    /// Draws the "windicator", showing a line of how strong the wind is (as it can be random).
    /// </summary>
    /// <param name="locOfABM"></param>
    /// <param name="g"></param>
    /// <param name="windStrength"></param>
    private void DrawWindIndicator(PointA locOfABM, Graphics g, float windStrength)
    {
        if (windStrength == 0) return;

        float xWind, yWind;

        Point pt = locOfABM.MCCoordsToDeviceCoordinatesP();
        
        if (checkBoxConstantWind.Checked)
        {
            // wind is from an edge (because it is constant)
            penWindIndicator.Width = 3;
            pt.X = windStrength * Math.Cos(angleOfWindInRadians) > 0 ? 0 : 512; // place on the logical edge depending on direction
        }
        else
        {
            // wind is rotated around missile
            penWindIndicator.Width = 1; // subtle but visible.
        }

        xWind = (float)(10 * windStrength * Math.Cos(angleOfWindInRadians) + pt.X);
        yWind = (float)(10 * windStrength * Math.Sin(angleOfWindInRadians) + pt.Y);
 
        g.DrawLine(penWindIndicator, pt, new Point((int)(xWind), (int)yWind));
    }

    /// <summary>
    /// Draws the "missiles", which are a 2x2px white rectangle.
    /// </summary>
    /// <param name="icbmLocation"></param>
    /// <param name="g"></param>
    private static void DrawMissileSquares(PointA icbmLocation, PointA abmLocation, Graphics g)
    {
        PointF p = icbmLocation.MCCoordsToDeviceCoordinates();

        g.FillRectangle(Brushes.White, p.X - 1, p.Y - 1, 2, 2);

        p = abmLocation.MCCoordsToDeviceCoordinates();

        g.FillRectangle(Brushes.White, p.X - 1, p.Y - 1, 2, 2);
    }

    /// <summary>
    /// User can press keys to pause/slow.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.P:
                // "P" pauses the timer (and what's happening)
                timerSimulate.Enabled = !timerSimulate.Enabled;

                if (timerSimulate.Enabled) timerSimulate.Start(); else timerSimulate.Stop();

                break;

            case Keys.F:
                // "F" rotate thru slow modes (fps)
                StepThroughSpeeds();
                break;
        }
    }

    /// <summary>
    /// "S" slows things down, 25x slower, 50x slower, 100x slower, then back to normal speed.
    /// </summary>
    private void StepThroughSpeeds()
    {
        var newInterval = timerSimulate.Interval switch
        {
            10 => 50,
            50 => 250,
            250 => 500,
            500 => 1000,
            _ => 10,
        };

        timerinterval = newInterval;
        timerSimulate.Interval = newInterval; // so we can restore speed starting next ICBM
    }
}