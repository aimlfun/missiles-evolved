using MissilesEvolved.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissilesEvolved.Game;

internal static class AccurateABM
{
    /// <summary>
    /// Make a singleton neural network.
    /// </summary>
    static internal readonly NeuralNetwork networkMissile;

    /// <summary>
    /// Indicate whether AI is trained
    /// </summary>
    private static bool isTrained = false;

    /// <summary>
    /// External getter to detect whether trained.
    /// </summary>
    static internal bool IsTrained
    {
        get
        {
            return isTrained;
        }
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    static AccurateABM()
    {
        // this work well
        int[] layers = new int[2] { IRSensorLeftRight.SamplePoints, 1 };

        ActivationFunctions[] activationFunctions = new ActivationFunctions[3]
        {
            ActivationFunctions.TanH,
            ActivationFunctions.TanH,
            ActivationFunctions.TanH
        };

        networkMissile = new(0, layers, activationFunctions, false);

        Train();
    }


    /// <summary>
    /// Train NN to provide an angle in response to the sensor.
    /// </summary>
    /// <returns></returns>
    private static void Train()
    {
        List<TrainingData> trainingData = new();

        PointA locOfABM = new(128, 6);
        IRSensorLeftRight sensor = new();

        PointA[] Cone = sensor.GetTriangle(0, locOfABM);

        // Paranoia, as my initial training data gave points that weren't visible to the sensor
        // leading to confusing it and messing up back propagation.
        ProveTestDataIsWithCone(Cone);

        // trains the NN using the training data.
        DoTraining(trainingData, locOfABM, sensor, Cone, out List<PointA> TargetsInCode, out bool successFullyTrained);

        // plots the accuracy. With reduced data required this isn't as pretty as it once was.
        DrawAccuracy(trainingData, locOfABM, TargetsInCode);

        isTrained = successFullyTrained;
    }

    /// <summary>
    /// Trains the neural network to provide the correct response based on the sensor output.
    /// </summary>
    /// <param name="trainingData"></param>
    /// <param name="locOfABM"></param>
    /// <param name="sensor"></param>
    /// <param name="Cone"></param>
    /// <param name="TargetsInCone"></param>
    /// <param name="successFullyTrained"></param>
    private static void DoTraining(List<TrainingData> trainingData, PointA locOfABM, IRSensorLeftRight sensor, PointA[] Cone, out List<PointA> TargetsInCone, out bool successFullyTrained)
    {
        TrainingDataPoints.Reset();

        TargetsInCone = new();

        while (TrainingDataPoints.GetPoints(Cone, out PointA icbm))
        {
            // if the training point isn't within the heat sensor cone, skip it.
            if (!MathUtils.PtInTriangle(icbm, Cone[0], Cone[1], Cone[2])) continue;

            double[] output = sensor.Read(0, locOfABM, icbm);

            TargetsInCone.Add(icbm);
          
            // missile needs to go from (128,6)->(x,y). We compute the angle, and teach the AI to associate the sensor
            // quadrant output with that angle.
            double angle = Math.Atan2(-(icbm.AltitudeInMissileCommandDisplayPX - locOfABM.AltitudeInMissileCommandDisplayPX),
                                       (icbm.HorizontalInMissileCommandDisplayPX - locOfABM.HorizontalInMissileCommandDisplayPX));

            angle += Math.PI / 2; // make it relative to 0.
            Debug.WriteLine($"({locOfABM.HorizontalInMissileCommandDisplayPX},{locOfABM.AltitudeInMissileCommandDisplayPX})-({icbm.HorizontalInMissileCommandDisplayPX}-{icbm.AltitudeInMissileCommandDisplayPX}) {string.Join("|", output)} thetaRad={angle} thetaDeg={MathUtils.RadiansInDegrees(angle)}");

            trainingData.Add(new TrainingData(input: output, output: new double[] { angle }));
        }

        successFullyTrained = networkMissile.Train(trainingData.ToArray(),
                                                        maxError: 0.001, // 0.011F, // maximum deviation we'll tolerate
                                                        checkAfter: 1000, 
                                                        maxAttempts: 2000000);
        /*
            SUCCESS LOOKS LIKE -

            0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 =  0.871 Deviation from desired value: 0.002
            0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 =  0.819 Deviation from desired value: 0.001 ]]]]]]]]]]]]]]]]]
            0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 =  0.709 Deviation from desired value: 0     £££££££££££££££ ]
            0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 =  0.6 Deviation from desired value: 0       ************* £ ]
            0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 =  0.491 Deviation from desired value: 0     >>>>>>>>>>> * £ ]
            0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 =  0.382 Deviation from desired value: 0     +++++++++ > * £ ]
            0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 =  0.273 Deviation from desired value: 0     ####### + > * £ ]
            0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 =  0.164 Deviation from desired value: 0     ----+ # + > * £ ]
            0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 =  0.054 Deviation from desired value: 0.001   = : # + > * £ ]
            0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 = -0.054 Deviation from desired value: 0       = : # + > * £ ]
            0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 = -0.164 Deviation from desired value: 0     ----+ # + > * £ ]
            0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 = -0.273 Deviation from desired value: 0     ####### + > * £ ]
            0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 = -0.382 Deviation from desired value: 0     +++++++++ > * £ ]
            0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 = -0.491 Deviation from desired value: 0     >>>>>>>>>>> * £ ]
            0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 = -0.6 Deviation from desired value: 0       ************* £ ] 
            0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 = -0.709 Deviation from desired value: 0     £££££££££££££££ ]
            1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 = -0.817 Deviation from desired value: 0.001 ]]]]]]]]]]]]]]]]]

            It's slightly asymetrical because sensor regions and not calc'ing from center of triangles. But it
            doesn't stop the logic working beautifully.

            Here we have a heat sensor => accurate enough angle mapping.

            Each "1" is approximately 6 degrees apart. i.e. We have +/-50 degrees = 100 degrees / 17 sensors = 5.88 degrees.

            Each value in the above is the radian angle. So although a fun AI challenge, a far simpler approach is a mere
            mapping hard-coded. float[17] = { 0.817f, 0.819f, 0.709f....-0.817f }. But where's the fun in that?
         */
    
        if (!successFullyTrained)
        {
            Debug.WriteLine("** TRAINING FAILED **");
        }
    }

    /// <summary>
    /// Remember we have 17 heat sensors over a +/-50 degree arc. We are therefore never
    /// going to be "accurate", just accurate enough. This draws the blobs (mostly outside the image), 
    /// and the lines skewering them.
    /// </summary>
    /// <param name="trainingData"></param>
    /// <param name="locOfABM"></param>
    /// <param name="TargetsInCode"></param>
    private static void DrawAccuracy(List<TrainingData> trainingData, PointA locOfABM,  List<PointA> TargetsInCode)
    {
        using Bitmap ImageShowingAccuracy = new(512, 462);
        using Graphics graphicsOfAccuracyImage = Graphics.FromImage(ImageShowingAccuracy);
        graphicsOfAccuracyImage.Clear(Color.Black);

        DrawBlob(locOfABM, graphicsOfAccuracyImage, Brushes.Green);

        TrainingDataPoints.Reset();
        
        var trainingDataArray = trainingData.ToArray();
        int indexOfTrainingData = 0;

        foreach (PointA start in TargetsInCode)
        {
            double nnangle = networkMissile.FeedForward(trainingDataArray[indexOfTrainingData].input)[0];

            OutputImage(graphicsOfAccuracyImage, locOfABM, start, nnangle-Math.PI/2);
            nnangle = (nnangle * 4) - Math.PI;;
        }

        graphicsOfAccuracyImage.Flush();

        ImageShowingAccuracy.Save($@"c:\temp\accuracy.png", ImageFormat.Png);
    }

    /// <summary>
    /// Because the arc of dots that constitute the 17 triange are off screen
    /// this looks like a white background with a few dots. It was used when
    /// training filled the whole cone with dots to target.
    /// </summary>
    /// <param name="Cone"></param>
    private static void ProveTestDataIsWithCone(PointA[] Cone)
    {
        using Bitmap Image = new(512, 462);
        using Graphics g = Graphics.FromImage(Image);
        g.Clear(Color.White);

        while (TrainingDataPoints.GetPoints(Cone, out PointA start))
        {
            Point pstart = start.MCCoordsToDeviceCoordinatesP();

            g.FillRectangle(
                Brushes.Black,
                (int)pstart.X, (int)pstart.Y,
                2, 2);
        }

        g.Flush();

        Image.Save(@"c:\temp\cone.png", ImageFormat.Png);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="locOfABM"></param>
    /// <param name="start"></param>
    /// <param name="angle"></param>
    private static void OutputImage(Graphics g, PointA locOfABM, PointA start, double angle)
    {
        DrawBlob(start, g, Brushes.Red);

        PointF p = locOfABM.MCCoordsToDeviceCoordinates();
        PointF end = new((float)(p.X + 512f * Math.Cos(angle)), (float)(p.Y + 512f * Math.Sin(angle)));

        using Pen pen = new(Color.White);
        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        g.DrawLine(pen, p, end);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loc"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    private static void DrawBlob(PointA loc, Graphics g, Brush b)
    {
        PointF p = loc.MCCoordsToDeviceCoordinates();

        g.FillEllipse(b, p.X - 3, p.Y - 3, 6, 6);
    }
}