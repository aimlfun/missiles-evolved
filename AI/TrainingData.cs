namespace MissilesEvolved.AI;

/// <summary>
/// 
/// </summary>
internal class TrainingData
{
    internal double[] input;
    internal double[] output;

    internal TrainingData(double[] input, double[] output)
    {
        this.input = input;
        this.output = output;
    }
}