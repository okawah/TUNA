using System;
using System.Collections.Generic;

public class Neuron
{
    public int numInputs;
    public double bias;
    public double output;
    public double errorGradient;
    public List<double> weights = new List<double>();
    public List<double> inputs = new List<double>();
    Random random = new Random();

	public Neuron(int nInputs)
	{
        double weightRange = 2.4f / nInputs;
        bias = random.NextDouble() * (weightRange + weightRange) - weightRange;
        numInputs = nInputs;
        for(int i = 0; i < nInputs; i++)
        {
            weights.Add(random.NextDouble() * (weightRange + weightRange) - weightRange);
        }
	}


}
