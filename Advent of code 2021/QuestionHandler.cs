using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QuestionHandler
{
    public abstract class QuestionHandler
    {
        public abstract string process(string[] toProcess);
    }

    public static class QuestionHandlerFactory
    {
        /* TODO: At some point we should factor out the non factory related code */
        struct HandlerMapElement
        {
            public readonly string name;
            public readonly int day;
            public readonly int part;
            public readonly Func<QuestionHandler> create;

            public HandlerMapElement(string name, int day, int part, Func<QuestionHandler> create)
            {
                this.name = name;
                this.day = day;
                this.part = part;
                this.create = create;
            }
        }

        private static readonly HandlerMapElement[] HANDLER_MAPPINGS =
        {
           new HandlerMapElement("Sonar Sweep", 1, 1, delegate{return new Day1Q1Handler();}),
           new HandlerMapElement("Sonar Sweep", 1, 2, delegate{return new Day1Q2Handler();}),

           new HandlerMapElement("Dive!", 2, 1, delegate{return new Day2Q1Handler();}),
           new HandlerMapElement("Dive!", 2, 2, delegate{return new Day2Q2Handler();}),

           //Today
           new HandlerMapElement("Binary Diagnostic", 3, 1, delegate{return new Day3Q1Handler();}),
        };

        //TODO: reimplement a mathmatical mapping for this
        private static HandlerMapElement? getElement(int day, int part)
        {
            HandlerMapElement? ret = null;

            if(part > 2)
            {
                throw new IndexOutOfRangeException("There are only two parts to every question");
            }
            else
            {
                foreach(var element in HANDLER_MAPPINGS)
                {
                    int eDay = element.day, ePart = element.part;
                    if((eDay == day) && (ePart == part))
                    {
                        ret = element;
                    }
                }
            }

            return ret;
        }

        public static QuestionHandler getNew(int day, int part)
        {
            HandlerMapElement? match = getElement(day, part);

            if(match == null)
            {
                return null;
            }
            else
            {
                return ((HandlerMapElement)match).create();
            }
        }

        public static string getName(int day, int part)
        {
            HandlerMapElement? match = getElement(day, part);

            if(match == null)
            {
                return null;
            }
            else
            {
                return ((HandlerMapElement)match).name;
            }
        }

        public static Int32? getDayCount()
        {
            return HANDLER_MAPPINGS[HANDLER_MAPPINGS.Length-1].day;
        }

        public static Int32? getPartCount(int day)
        {
            //TODO: reimplement a mathmatical mapping for this
            HandlerMapElement? lastGreatestPartOfDay = null;
            foreach(var element in HANDLER_MAPPINGS)
            {
                if(element.day == day)
                {
                    lastGreatestPartOfDay = element;
                }
            }

            Int32? ret = null;
            if (lastGreatestPartOfDay.HasValue)
            {
                ret = lastGreatestPartOfDay.Value.part;
            }
            return ret;
        }
    }

    public class BadInputException: Exception
    {
        public BadInputException(string message): base(message) {}
        public BadInputException(string message, Exception innerException): base(message, innerException) {}
    }

    /* QUESTION HANDLERS */
    /* Each of the question handlers are written as self contained answers
     * hence the lack of code reuse between them */

    public class Day1Q1Handler: QuestionHandler
    {
        /* Thease functions would work if the question input didn't have 2000 lines of input
        private int countLargerThanMesRec(string[] toProcess, int currPos, int prevVal, bool firstRun, int currLargerThenCount)
        {
            if(currPos == toProcess.Length)
            {
                return currLargerThenCount;
            }
            else
            {
                int currVal = Int32.Parse(toProcess[currPos]);
                if(firstRun)
                {
                    Debug.Assert(currPos == 0);
                    return countLargerThanMesRec(toProcess, currPos++, currVal, false, 0);
                }
                else if(prevVal > currVal)
                {
                    return countLargerThanMesRec(toProcess, currPos++, currVal, firstRun, currLargerThenCount++);
                }
                else
                {
                    return countLargerThanMesRec(toProcess, currPos++, currVal, firstRun, currLargerThenCount);
                }
            }
        }

        private int countLargerThanMes(string[] toProcess)
        {
            return countLargerThanMesRec(toProcess, 0, 0, true, 0);
        }*/

        public override string process(string[] toProcess)
        {
            int ret = 0, prevVal = 0;
            bool firstRun = true;
            if (toProcess.Length > 0)
            {
                foreach (string s in toProcess)
                {
                    int currVal = Int32.Parse(s);
                    if (!firstRun)
                    {
                        if (prevVal < currVal)
                        {
                            ret++;
                        }
                    }
                    else
                    {
                        firstRun = false;
                    }

                    prevVal = currVal;
                }
            }
            return ret.ToString();
        }
    }

    public class Day1Q2Handler: QuestionHandler
    {
        private class ProcessingState
        {
            private class SlidingWindow
            {
                public const int NUM_WINDOW_ELEMENTS = 3;
                private int[] lastThree;
                private int lastThreePos;

                private bool firstRun;
                private bool finished;

                private int total;

                public SlidingWindow()
                {
                    lastThree = new int[3];
                    lastThreePos = 0;

                    firstRun = true;
                    finished = false;

                    total = 0;
                }

                public bool isFinished()
                {
                    return finished;
                }

                public int getTotal()
                {
                    if (!finished)
                    {
                        throw new InvalidOperationException("Cannot get the total value untill three values have been given to nextIn");
                    }
                    else
                    {
                        return total;
                    }
                }
                
                public void nextIn(int nextIn)
                {
                    if(!finished)
                    {
                        //Go to next window pos
                        if (firstRun)
                        {
                            firstRun = false;
                        }
                        else
                            this.lastThreePos = (this.lastThreePos + 1) % 3;
                        this.lastThree[this.lastThreePos] = nextIn;

                        if (lastThreePos == 2)
                        {
                            this.finalize();
                        }
                    }
                    else
                    {
                        string errorStr = String.Format("Can't input more than {0} elements", SlidingWindow.NUM_WINDOW_ELEMENTS);
                        throw new InvalidOperationException(errorStr);
                    }
                }

                private void finalize()
                {
                    finished = true;

                    foreach(int val in this.lastThree)
                    {
                        total += val;
                    }
                }
            }

            private const int NUM_WINDOWS = 3;
            private int totalIncreced;
            private int? prevWinFinshed;
            private SlidingWindow[] windows;
            private int numPorcessed;

            public ProcessingState()
            {
                this.totalIncreced = 0;
                this.windows = new SlidingWindow[NUM_WINDOWS];
                //An arry of objects is initialized to null
                for(int i = 0; i < NUM_WINDOWS; i++)
                {
                    windows[i] = new SlidingWindow();
                }

                this.prevWinFinshed = null;
                this.numPorcessed = 0;
            }

            public int getTotalIncreced()
            {
                int tmpFinalTotalInc = this.totalIncreced;
                //Add up the one that didn't get recognized as finished before
                foreach(SlidingWindow window in this.windows)
                {
                    if (window.isFinished())
                    {
                        int finishedWinTotal = window.getTotal();
                        if (finishedWinTotal > prevWinFinshed)
                        {
                            tmpFinalTotalInc++;
                        }
                    }
                }
                return tmpFinalTotalInc;
            }

            public void nextIn(int nextIn)
            {
                for(int i = 0; i < NUM_WINDOWS; i++)
                {
                    //This is so that the inputting of values becomes staggerd
                    if((i == 0) && (numPorcessed == 0))
                    {
                        windows[i].nextIn(nextIn);
                    }
                    else if((i < NUM_WINDOWS-1) && (numPorcessed < NUM_WINDOWS-1) && (numPorcessed != 0))
                    {
                        windows[i].nextIn(nextIn);
                    }
                    else if(numPorcessed >= NUM_WINDOWS-1)
                    {
                        if(windows[i].isFinished())
                        {
                            //Compare the the one that finished previously, if greater add 1 to totalIncreced
                            int finishedWinTotal = windows[i].getTotal();
                            if(this.prevWinFinshed == null)
                            {
                                this.prevWinFinshed = windows[i].getTotal();
                            }    
                            else if(finishedWinTotal > prevWinFinshed)
                            {
                                totalIncreced++;
                                prevWinFinshed = finishedWinTotal;
                            }
                            else
                            {
                                prevWinFinshed = finishedWinTotal;
                            }
                            //reset the finished one
                            windows[i] = new SlidingWindow();
                        }

                        windows[i].nextIn(nextIn);
                    }
                }
                numPorcessed++;
            }
        }
        public override string process(string[] toProcess)
        {
            ProcessingState state = new ProcessingState();
            if (toProcess.Length > 0)
            {
                foreach (string s in toProcess)
                {
                    state.nextIn(Int32.Parse(s));
                }
            }
            return state.getTotalIncreced().ToString();
        }
    }

    public class Day2Q1Handler: QuestionHandler
    {
        struct Position
        {
            public int depth;
            public int xPos;
            public Position(int depth, int xPos)
            {
                this.depth = depth;
                this.xPos = xPos;
            }
        }
        public override string process(string[] toProcess)
        {
            var position = new Position(0, 0);
            foreach(var command in toProcess)
            {
                command.ToLower();
                string[] cmdAndVal = command.Split();
                if(cmdAndVal.Length == 2)
                {
                    switch(cmdAndVal[0])
                    {
                        case "up":
                            {
                                var newVal = position.xPos - Int32.Parse(cmdAndVal[1]);
                                if(newVal < 0)
                                {
                                    position.depth = 0;
                                }
                                else
                                {
                                    position.depth = position.depth - Int32.Parse(cmdAndVal[1]);
                                }
                                break;
                            }
                        case "down":
                            {
                                position.depth += Int32.Parse(cmdAndVal[1]);
                                break;
                            }
                        case "forward":
                            {
                                position.xPos += Int32.Parse(cmdAndVal[1]);
                                break;
                            }
                    }
                }
                else if((cmdAndVal.Length == 1) && (cmdAndVal[0].Equals("")))
                {
                    continue;
                }
                else
                {
                    throw new InvalidOperationException("The input string should contain only a command and value");
                }
            }

            return (position.depth * position.xPos).ToString();
        }
    }

    public class Day2Q2Handler : QuestionHandler
    {
        public int depth;
        public int xPos;
        public int aim;

        public Day2Q2Handler()
        {
            this.depth = 0;
            this.xPos = 0;
            this.aim = 0;
        }

        private void up(int inp)
        {
            this.aim -= inp;
        }
        private void down(int inp)
        {
            this.aim += inp;
        }
        private void forward(int inp)
        {
            this.xPos += inp;
            this.depth += this.aim * inp;
        }

        public override string process(string[] toProcess)
        {
            foreach (var command in toProcess)
            {
                command.ToLower();
                string[] cmdAndVal = command.Split();
                if (cmdAndVal.Length == 2)
                {
                    switch (cmdAndVal[0])
                    {
                        case "up":
                            {
                                up(Int32.Parse(cmdAndVal[1]));
                                break;
                            }
                        case "down":
                            {
                                down(Int32.Parse(cmdAndVal[1]));
                                break;
                            }
                        case "forward":
                            {
                                forward(Int32.Parse(cmdAndVal[1]));
                                break;
                            }
                    }
                }
                else if ((cmdAndVal.Length == 1) && (cmdAndVal[0].Equals("")))
                {
                    continue;
                }
                else
                {
                    throw new InvalidOperationException("The input string should contain only a command and value");
                }
            }

            return (this.depth * this.xPos).ToString();
        }
    }

    public class Day3Q1Handler : QuestionHandler
    {
        private class PowerConsumption
        {
            int[] input;
            uint bitWidth;
            bool[] gamma;
            bool[] epsilon;

            public PowerConsumption(int[] input, uint bitWidth)
            {
                if((input == null) || (input.Length == 0))
                {
                    string errorMsg;
                    if(input == null)
                    {
                        errorMsg = "Input cannot be null";
                    }
                    else
                    {
                        errorMsg = "Input cannot be empty";
                    }
                    throw new BadInputException(errorMsg);
                }

                if(bitWidth < 1)
                {
                    throw new BadInputException("Bit width cannot be zero or negative");
                }

                this.input = input;
                this.bitWidth = bitWidth;
                this.gamma = null;
                this.epsilon = null;
            }

            private static bool intToBool(int x)
            {
                return x != 0 ? true : false;
            }

            private static int boolToInt(bool x)
            {
                return x ? 1 : 0;
            }

            private static int boolArrToInt(List<bool> toConvert, int prev = 0)
            {
                if((toConvert == null) || (toConvert.Count == 0))
                {
                    return prev;
                }
                else
                {
                    var curr = boolToInt(toConvert[toConvert.Count - 1]);
                    toConvert.RemoveAt(toConvert.Count - 1);
                    prev = (prev << 1) + curr;
                    return boolArrToInt(toConvert, prev);
                }
            }

            private static int greatestPosBit(int i)
            {
                var count = 0;
                while(i > 0)
                {
                    i = i >> 1;
                    count++;
                }
                return count;
            }

            //TODO: rename
            private bool[] siftBits(Func<int[], int, bool> bitTotalOp)
            {
                var ret = new bool[bitWidth];

                var totalPosBits = new int[bitWidth];
                foreach(var powerVal in input)
                {
                    for(int i = 0;  i < bitWidth; i++)
                    {
                        //extract that bit and add it to the total for that position
                        int currBit = (int)((1 << i) & powerVal) >> (int)i;
                        totalPosBits[i] += currBit;
                    }
                }

                for(int i = 0; i < totalPosBits.Length; i++)
                {
                    var mostCommonBit = bitTotalOp(totalPosBits, i);
                    ret[i] = mostCommonBit;
                }

                return ret;
            }


            private void updateGamma()
            {
                this.gamma = siftBits((int[] totalPosBits, int pos) => ((float)totalPosBits[pos] >= ((float)this.input.Length) / 2.0));
            }
            private void updateEpsilon()
            {
                this.epsilon = siftBits((int[] totalPosBits, int pos) => ((float)totalPosBits[pos] <= ((float)this.input.Length) / 2.0));
            }

            public int getGammaRateInt()
            {
                if(this.gamma == null)
                {
                    updateGamma();
                }

                return boolArrToInt(new List<bool>(this.gamma));
            }

            public int getEpsilonRateInt()
            {
                if(this.epsilon == null)
                {
                    updateEpsilon();
                }

                return boolArrToInt(new List<bool>(this.epsilon));
            }
        }

        private int strBinToInt(string toConvert)
        {
            var inputChars = toConvert.ToCharArray();
            int output = 0;
            foreach(var ch in inputChars)
            {
                if((ch != '1') && (ch != '0'))
                {
                    throw new BadInputException("Can only take binary input, i.e '0's and or '1's in the string");
                }
                output = output << 1;
                output += (ch == '0') ? 0 : 1;
            }
            return output;
        }

        public override string process(string[] toProcess)
        {
            var convertedInput = new int[toProcess.Length];
            for(int i = 0; i < toProcess.Length; i++)
            {
                convertedInput[i] = strBinToInt(toProcess[i]);
            }

            var batteryLevel = new PowerConsumption(convertedInput, (uint)toProcess[0].Length);
            var gammaRate = batteryLevel.getGammaRateInt();
            var epsilonRate = batteryLevel.getEpsilonRateInt();
            return (gammaRate * epsilonRate).ToString();
        }
    }
}
