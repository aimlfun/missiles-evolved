using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissilesEvolved.Game;

internal static class TrainingDataPoints
{
    /// <summary>
    /// 
    /// </summary>
    private static List<PointA>? trainingPoints = null;

    /// <summary>
    /// 
    /// </summary>
    private static PointA[]? targets;

    /// <summary>
    /// 
    /// </summary>
    internal static int TrainingDataIndex = -1;

    internal static void Reset()
    {
        TrainingDataIndex = -1;
    }

    /// <summary>
    /// Points chosen not at random; they were *carefully* selected. If you look at the entries below,
    /// you'll see we are trying to teach the AI to return an offset angle based on the 17 quadrants
    /// that the sensor is capable of sensing. Note: ONE value ONLY per angle we want returned.
    /// </summary>
    /// <returns></returns>
    internal static PointA[] GenerateTrainingPoints()
    {
        trainingPoints = new List<PointA>();

        List<string> listOfSensorValuesAlreadyAddedToList = new(); // we don't want multiple points giving slightly different angles as it is then non-backpropagateable.

        IRSensorLeftRight sensor = new();

        /* 17 positions, 17 angles to return
            (128,6)-(304.95627-154.48393)  0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1 thetaRad= 0.8726646491402315 thetaDeg= 50.000001326000046
            (128,6)-(296.58627-163.92299)  0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1|0 thetaRad= 0.8180450034826174 thetaDeg= 46.870526151318714
            (128,6)-(278.39487-181.33505)  0|0|0|0|0|0|0|0|0|0|0|0|0|0|1|0|0 thetaRad= 0.7089801944990417 thetaDeg= 40.62157290315932
            (128,6)-(258.4163-196.66356)   0|0|0|0|0|0|0|0|0|0|0|0|0|1|0|0|0 thetaRad= 0.5999154355754236 thetaDeg= 34.37262252322421
            (128,6)-(236.8853-209.72774)   0|0|0|0|0|0|0|0|0|0|0|0|1|0|0|0|0 thetaRad= 0.4908377938596990 thetaDeg= 28.122934013673067
            (128,6)-(214.05836-220.37108)  0|0|0|0|0|0|0|0|0|0|0|1|0|0|0|0|0 thetaRad= 0.3817521176953933 thetaDeg= 21.87278516412751
            (128,6)-(190.20844-228.46597)  0|0|0|0|0|0|0|0|0|0|1|0|0|0|0|0|0 thetaRad= 0.2726667159769051 thetaDeg= 15.622652039169
            (128,6)-(165.61893-233.91624)  0|0|0|0|0|0|0|0|0|1|0|0|0|0|0|0|0 thetaRad= 0.1635810630161658 thetaDeg= 9.372504519089862
            (128,6)-(140.58232-236.65707)  0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0 thetaRad= 0.0544958921701790 thetaDeg= 3.1223846221512868
            (128,6)-(115.396614-236.65593) 0|0|0|0|0|0|0|1|0|0|0|0|0|0|0|0|0 thetaRad=-0.0545872160710154 thetaDeg=-3.127617096237886
            (128,6)-(90.36028-233.91281)   0|0|0|0|0|0|1|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.1636722823722740 thetaDeg=-9.377731003204765
            (128,6)-(65.77128-228.4603)    0|0|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.2727579095707313 thetaDeg=-15.627877047213868
            (128,6)-(41.922047-220.3632)   0|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.3818435253316086 thetaDeg=-21.87802243589791
            (128,6)-(19.096096-209.71779)  0|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.4909291096350641 thetaDeg=-28.12816602220445
            (128,6)-(-2.4336948-196.65164) 0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.6000067699104998 thetaDeg=-34.377855595148716
            (128,6)-(-22.410868-181.32133) 0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.7090714514032088 thetaDeg=-40.62680153861952
            (128,6)-(-40.600704-163.90758) 1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0 thetaRad=-0.8181364136039648 thetaDeg=-46.875763565476696
         */

        for (float angle = -50; angle <= 50; angle += 0.01f)
        {
            // cone starts -50 degres to + 50 degrees. Cos & Sin rotate a radius about an angle
            float x = (float)(128f + 231f * Math.Cos(MathUtils.DegreesInRadians(angle + 90)));
            float y = (float)(6f + 231f * Math.Sin(MathUtils.DegreesInRadians(angle + 90)));

            // if the ICBM is at x,y and ABM at 128,6 what heat signature does it see?
            double[] output = sensor.Read(0, new PointA(128, 6), new PointA(x, y));

            // We do this because if we have multiple rows for the SAME 1's & 0's with a different angle it'll never train.
            // We actually don't even care which one it picks, it's plenty accurate enough as the tiny part of the cone
            // reaches the target
            string s = string.Join("-", output);

            if (listOfSensorValuesAlreadyAddedToList.Contains(s)) continue;

            listOfSensorValuesAlreadyAddedToList.Add(s);

            // this is a unique training point
            trainingPoints.Add(new PointA(x, y));
        }

        // we need an array so we can step thru them sequentially        
        return trainingPoints.ToArray();
    }

    /// <summary>
    /// Return the next training point, unless "repeat" is set in which case we return the same as last time
    /// </summary>
    /// <param name="repeat"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <exception cref="Exception"></exception>
    internal static bool GetPoints(PointA[] Cone, out PointA start)
    {
        if (trainingPoints is null) targets = GenerateTrainingPoints();

        if (targets is null) throw new Exception("targets incorrectly initialised");

        // out parameters must be set EVEN if we return "false" (no point left)
        start = new PointA(-1, -1);

        while (true)
        {
            if (++TrainingDataIndex > targets.Length - 1) return false;

            start = targets[TrainingDataIndex];

            // ensure every training point we create is actually within the sensor cone.
            // if we don't, it's going to confuse the AI.
            if (MathUtils.PtInTriangle(start, Cone[0], Cone[1], Cone[2])) return true;
        }
    }
}