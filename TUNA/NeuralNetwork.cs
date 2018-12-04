using System;
using System.Collections.Generic;

public class NeuralNetwork
{
    public int numInputs;
    public int numOutputs;
    public int numHidden;
    public int numNeuronPerHidden;
    public double alpha;
    List<Layer> layers = new List<Layer>();

	public NeuralNetwork(int nInputs, int nOutputs, int nHidden, int nPerHidden, double a)
	{
        numInputs = nInputs;
        numOutputs = nOutputs;
        numHidden = nHidden;
        numNeuronPerHidden = nPerHidden;
        alpha = a;

        if(numHidden > 0)
        {
            layers.Add(new Layer(numNeuronPerHidden, numInputs));

            for(int i = 0; i < numHidden - 1; i++)
            {
                layers.Add(new Layer(numNeuronPerHidden, numNeuronPerHidden));
            }
            layers.Add(new Layer(numOutputs, numNeuronPerHidden));
        }
        else
        {
            layers.Add(new Layer(numOutputs, numInputs));
        }
	}

    public List<double> Train(List<double> inputValues, List<double> desiredOutputs)
    {
        List<double> outputValues = new List<double>();
        outputValues = CalcOutput(inputValues);
        UpdateWeights(outputValues, desiredOutputs);
        return outputValues;
    }

    public List<double> CalcOutput(List<double> inputValues)
    {
        List<double> inputs = new List<double>();
        List<double> outputValues = new List<double>();
        

        //if(inputs.Count != numInputs)
        //{
        //    Console.WriteLine("(*_*)");
        //    return outputValues;
        //}

        inputs = new List<double>(inputValues);
        for(int i = 0; i < numHidden + 1; i++)
        {
            if(i > 0)
            {
                inputs = new List<double>(outputValues);
            }
            outputValues.Clear();

            for(int j = 0; j < layers[i].numNeurons; j++)
            {
                double N = 0;
                layers[i].neurons[j].inputs.Clear();

                for(int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    layers[i].neurons[j].inputs.Add(inputs[k]);
                    N += layers[i].neurons[j].weights[k] * inputs[k];
                }

                N -= layers[i].neurons[j].bias;

                if (i == numHidden)
                {
                    layers[i].neurons[j].output = ActivationFunctionSigmoid(N);
                }
                else
                {
                    layers[i].neurons[j].output = ActivationFunctionTanH(N);
                }
                outputValues.Add(layers[i].neurons[j].output);
            }
        }

        return outputValues;
    }

    void UpdateWeights(List<double> outputs, List<double> desiredOutputs)
    {
        double error = 0;
        for(int i = numHidden; i >= 0; i--)
        {
            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                if (i == numHidden)
                {
                    error = desiredOutputs[j] - outputs[j];
                    layers[i].neurons[j].errorGradient = outputs[j] * (1 - outputs[j]) * error;
                }
                else
                {
                    layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1 - layers[i].neurons[j].output);
                    double errorGradSum = 0;
                    for (int n = 0; n < layers[i + 1].numNeurons; n++)
                    {
                        errorGradSum += layers[i + 1].neurons[n].errorGradient * layers[i + 1].neurons[n].weights[j];
                    }
                    layers[i].neurons[j].errorGradient *= errorGradSum;
                }
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    if(i == numHidden)
                    {
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * error;
                    }
                    else
                    {
                        layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
                    }
                }
                layers[i].neurons[j].bias += alpha * -1 * layers[i].neurons[j].errorGradient;
        }   }
    }

    double ActivationFunctionTanH(double value)
    {
        return TanH(value);
    }

    double ActivationFunctionSigmoid(double value)
    {
        return Sigmoid(value);
    }

    double TanH(double value)
    {
        return (2.0 / (1.0 + Math.Exp(-2 * value))) - 1;
    }

    double ReLu(double value)
    {
        if (value > 0) return value;
        else return 0;
    }

    double Linear(double value)
    {
        return value;
    }

    double LeakyRelu(double value)
    {
        if (value < 0) return 0.01 * value;
        else return value;
    }

    double Sigmoid(double value)
    {
        return 1.0 / (1.0 + Math.Exp(-value));
    }
}
