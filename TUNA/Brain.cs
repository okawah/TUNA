using System;
using System.Collections.Generic;
using System.Linq;

namespace TUNA
{


    public class Brain
    {
        NeuralNetwork NN;
        List<Memory> replayMemory = new List<Memory>();
        int memoryCapacity = 10000;
        float discount = 0.99f;
        float exploreRate = 100.0f;
        float maxExploreRate = 100f;
        float minExploreRate = 0.01f;
        float exploreDecay = 0.0001f;

        public bool dropped = false;
        public List<double> qs = new List<double>();
        public int numActions;
        Random random = new System.Random();

        public Brain(int nInputs, int nOutputs, int nHidden, int nPerHidden, double alpha)
        {
            NN = new NeuralNetwork(nInputs, nOutputs, nHidden, nPerHidden, alpha);
            numActions = nOutputs;
        }

        public int ChooseAction(List<double> states)
        {
            qs = SoftMax(NN.CalcOutput(states));
            double maxQ = qs.Max();
            int maxQIndex = qs.IndexOf(maxQ);
            exploreRate = Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);
            

            if (random.Next(0, 100) < exploreRate)
            {
                maxQIndex = random.Next(0, numActions);
            }
            return maxQIndex;
        }

        public void Train(List<double> states, double reward)
        {
            Memory lastMemory = new Memory(states, reward);

            if (replayMemory.Count > memoryCapacity)
            {
                replayMemory.RemoveAt(0);
            }
            replayMemory.Add(lastMemory);
           
            if (dropped)
            {
                for (int i = 0; i < replayMemory.Count; i++)
                {
                    List<double> toutputsOld = new List<double>();
                    List<double> toutputsNew = new List<double>();
                    toutputsOld = SoftMax(NN.CalcOutput(replayMemory[i].states));

                    double maxQOld = toutputsOld.Max();
                    int action = toutputsOld.ToList().IndexOf(maxQOld);

                    double feedback;
                    if (i == replayMemory.Count - 1 || replayMemory[i].reward == -1)
                    {
                        feedback = replayMemory[i].reward;
                    }
                    else
                    {
                        toutputsNew = SoftMax(NN.CalcOutput(replayMemory[i + 1].states));
                        double maxQ = toutputsNew.Max();
                        feedback = (replayMemory[i].reward + discount * maxQ);
                    }

                    toutputsOld[action] = feedback;
                    NN.Train(replayMemory[i].states, toutputsOld);
                }
            }
        }
        List<double> SoftMax(List<double> values)
        {
            double max = values.Max();

            double scale = 0;
            for (int i = 0; i < values.Count; i++)
            {
                scale += Math.Exp((values[i] - max));
            }
            List<double> result = new List<double>();
            for (int i = 0; i < values.Count; i++)
            {
                result.Add(Math.Exp((float)(values[i] - max)) / scale);
            }
            return result;
        }


        float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
    }


}

