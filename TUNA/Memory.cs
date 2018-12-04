using System;
using System.Collections.Generic;

public class Memory
{
    public List<double> states;
    public double reward;
	public Memory(List<double> inputStates, double inputReward)
	{
        states = new List<double>();
        foreach(double state in inputStates)
        {
            states.Add(state);
        }
        reward = inputReward;
	}
}
