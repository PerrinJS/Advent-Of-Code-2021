using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

           new HandlerMapElement("Binary Diagnostic", 3, 1, delegate{return new Day3Q1Handler();}),
           new HandlerMapElement("Binary Diagnostic", 3, 2, delegate{return new Day3Q2Handler();}),

           new HandlerMapElement("Giant Squid", 4, 1, delegate{return new Day4Q1Handler();}),
           new HandlerMapElement("Giant Squid", 4, 2, delegate{return new Day4Q2Handler();}),

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

    //FIXME: For both of day 3's questions use a BitArray instead of int[]s'
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

                var totaledBits = new int[bitWidth];
                foreach(var powerVal in input)
                {
                    for(int i = 0;  i < bitWidth; i++)
                    {
                        //extract that bit and add it to the total for that position
                        int currBit = (int)((1 << i) & powerVal) >> (int)i;
                        totaledBits[i] += currBit;
                    }
                }

                for(int i = 0; i < totaledBits.Length; i++)
                {
                    var newBit = bitTotalOp(totaledBits, i);
                    ret[i] = newBit;
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
    
    public class Day3Q2Handler : QuestionHandler
    {
        private class LifeSupportRating
        {
            int[] input;
            uint bitWidth;
            int? oxygenGeneratorRating;
            int? co2ScrubberRating;

            public LifeSupportRating(int[] input, uint bitWidth)
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
                this.oxygenGeneratorRating = null;
                this.co2ScrubberRating = null;
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

            private int calcTotalPosBitsAt(List<int> currInput, int atPos)
            {
                var totalPosBits = 0;
                foreach(var inValue in currInput)
                {
                    //extract that bit and add it to the total for that position
                    //moveing from left to right, as i goes from bitWith-1 to 0
                    totalPosBits += ((1 << atPos) & inValue) >> atPos;
                }
                return totalPosBits;
            }

            //TODO: rename
            private int? siftBits(Func<int, List<int>, bool> bitTotalOp)
            {
                var matches = new List<int>(input);
                int? ret = null;

                for(int i = (int)bitWidth-1;  i >= 0; i--)
                {
                    var totalPosBits = calcTotalPosBitsAt(matches, i);
                    var replacementBit = bitTotalOp(totalPosBits, matches);
                    /* fill the list with matches from all the input values
                     * else select from the values selected in the previous
                     * run */
                    var newMatches = new List<int>();
                    foreach (var matchValue in matches)
                    {
                        //copy values that still match the criteria to a new list of matches
                        if(intToBool(((1 << i) & matchValue) >> i) == replacementBit)
                        {
                            newMatches.Add(matchValue);
                        }
                    }
                    matches = newMatches;

                    if(matches.Count == 0 || matches.Count == 1)
                        break;
                }

                if(matches.Count == 1)
                {
                    ret = matches[0];
                }
                return ret;
            }


            private void updateOxygenGeneratorRating()
            {
                this.oxygenGeneratorRating = siftBits((int totalPosBits, List<int> currVals) => ((float)totalPosBits >= ((float)currVals.Count) / 2.0));
            }
            private void updateCo2GeneratorRating()
            {
                this.co2ScrubberRating = siftBits((int totalPosBits, List<int> currVals) => ((float)totalPosBits < ((float)currVals.Count) / 2.0));
            }

            public int getOxygenRating()
            {
                if(this.oxygenGeneratorRating == null)
                {
                    updateOxygenGeneratorRating();
                }

                if(oxygenGeneratorRating.HasValue)
                {
                    return oxygenGeneratorRating.Value;
                }
                else
                {
                    throw new BadInputException("No valid oxygen value found");
                }

            }

            public int getCo2ScrubberRating()
            {
                if(this.co2ScrubberRating == null)
                {
                    updateCo2GeneratorRating();
                }

                if(co2ScrubberRating.HasValue)
                {
                    return co2ScrubberRating.Value;
                }
                else
                {
                    throw new BadInputException("No valid co2 scrubber value found");
                }
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

            var batteryLevel = new LifeSupportRating(convertedInput, (uint)toProcess[0].Length);
            var oxygenRating = batteryLevel.getOxygenRating();
            var co2ScrubberRating = batteryLevel.getCo2ScrubberRating();
            return (oxygenRating * co2ScrubberRating).ToString();
        }
    }

    public class Day4Q1Handler : QuestionHandler
    {
        private class BingoGame
        {
            /* A typical Bingo game utilizes the numbers 1 through 75. The five
             * columns of the card are labeled 'B', 'I', 'N', 'G', and 'O' from
             * left to right. The center space is usually marked "Free" or
             * "Free Space", and is considered automatically filled. The range
             * of printed numbers that can appear on the card is normally
             * restricted by column, with the 'B' column only containing numbers
             * between 1 and 15 inclusive, the 'I' column containing only 16
             * through 30, 'N' containing 31 through 45, 'G' containing 46
             * through 60, and 'O' containing 61 through 75.
             * Wikipedia: https://en.wikipedia.org/wiki/Bingo_(American_version)#Bingo_cards */
            private int[,] selectedValuesTable;
            private BitArray[] markedOffMap;
            private Dictionary<int,int[]> valuePositions;
            private bool bingo;

            /// <summary>
            /// The standard bingo game width and height (it's a square board)
            /// </summary>
            public const int BINGO_WNH = 5;

            public BingoGame(int[,] selectedValues)
            {
                //a multidimentional array's length is equal to the product of all its dimens
                if (selectedValues != null && selectedValues.Length/BINGO_WNH == BINGO_WNH)
                {
                    this.selectedValuesTable = selectedValues;
                    //This is to act as a fast lookup for the table positions
                    this.valuePositions = new Dictionary<int, int[]>();
                    for(int i = 0; i < BINGO_WNH; i++)
                    {
                         for(int j = 0; j < BINGO_WNH; j++)
                        {
                            //The question don't follow the number rules
                            this.valuePositions.Add(this.selectedValuesTable[i, j], new[] { i, j });
                        }
                    }
                }
                else
                {
                    //TODO: this is for if we add a setter
                    /*if (selectedValues == null)
                    {
                        this.selectedValuesTable = selectedValues;
                        this.valuePositions = null;
                    }
                    else*/
                        throw new BadInputException($"Bingo Tables need to be {BINGO_WNH}x{BINGO_WNH}");
                }

                this.markedOffMap = new BitArray[BINGO_WNH];
                for(int i = 0; i < this.markedOffMap.Length; i++)
                {
                    this.markedOffMap[i] = new BitArray(new[] { false, false, false, false, false });
                }
                //This is the "Free Space"
                this.markedOffMap[2].And(new BitArray(new[] { false, false, true, false, false }));
                this.bingo = false;
            }

            public bool hasBingo()
            {
                /*"Diagonals don't count"*/
                if(!this.bingo)
                {
                    var found = false;
                    var checkCol = new BitArray(new[] { true, true, true, true, true });
                    //If we test a row and all are false then none of the collumns can be true
                    foreach(var row in this.markedOffMap)
                    {
                        //Manually check for equality so we know if we need the check the collumns
                        var tmpFalsePos = hasFalsePos(row);

                        if (tmpFalsePos.Count == 0)
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            //set all the collumns that have false values to false to indicate they are not to be searched
                            for(int i = 0; i < tmpFalsePos.Count; i++)
                                checkCol[tmpFalsePos[i]] = false;
                        }

                    }

                    if(!found && !allFalse(checkCol))
                    {
                        var tmpFound = true;
                        for(int i = 0; i < BINGO_WNH; i++)
                        {
                            BitArray currCol = this.getCol(i);
                            var falsePos = hasFalsePos(currCol);
                            if(falsePos.Count != 0)
                            {
                                tmpFound = false;
                                break;
                            }
                        }
                        if (tmpFound)
                        {
                            found = true;
                        }
                    }

                    if (found)
                    {
                        bingo = true;
                    }
                }

                return bingo;
            }

            public void markOff(int calledOutValue)
            {
                if(!bingo)
                {
                    int[] markOffPos;
                    var hasVal = valuePositions.TryGetValue(calledOutValue, out markOffPos);
                    if (hasVal)
                    {
                        //Make a mask for the selected position
                        var tmpRow = new BitArray(new[] { false, false, false, false, false });
                        tmpRow[markOffPos[1]] = true;
                        //Set the selected field by oring the mask with the row it's in
                        markedOffMap[markOffPos[0]].Or(tmpRow);
                    }
                }
            }
            public List<int> getUnmarked()
            {
                var unmarked = new List<int>();
                Tuple<int, int> tmpUnmarkedPos = new Tuple<int, int>(0, 0);

                foreach(var val in this.valuePositions.Keys)
                {
                    int[] valPos;
                    this.valuePositions.TryGetValue(val, out valPos);

                    if(!this.markedOffMap[valPos[0]][valPos[1]])
                    {
                        unmarked.Add(val);
                    }
                }

                return unmarked;
            }

            private BitArray getCol(int colIdx)
            {
                Debug.Assert(colIdx < BINGO_WNH);
                var selectedCollumn = new BitArray(new[] { false, false, false, false });
                foreach(var row in this.markedOffMap)
                {
                    selectedCollumn[colIdx] = row[colIdx];
                }
                return selectedCollumn;
            }

            private static List<int> hasFalsePos(BitArray row)
            {
                var ret = new List<int>();
                for(int i = 0; i < row.Length; i++)
                {
                    if(row[i] == false)
                    {
                        ret.Add(i);
                    }
                }
                return ret;
            }

            private static bool allFalse(BitArray toCheck)
            {
                var ret = true;
                foreach(bool bit in toCheck)
                {
                    //If true then thair not all false;
                    if (bit)
                    {
                        ret = false;
                        break;
                    }
                }
                return ret;
            }
        }

        private class BingoGameParser: IEnumerable<BingoGame>
        {
            enum NextLineType
            {
                ValidLine,
                InvalidLine,
                EndOfInput
            }

            uint toParsePos;
            string[] toParse;

            public BingoGameParser(string[] input)
            {
                this.toParsePos = 0;
                this.toParse = input;
                this.skipToFirstValid();
            }

            public IEnumerator<BingoGame> GetEnumerator()
            {
                BingoGame nextGame = null;
                Func<BingoGame, bool> validGame = (BingoGame game) => game != null;
                do
                {
                    nextGame = getNextGame();
                    if(validGame(nextGame))
                        yield return nextGame;
                } while (validGame(nextGame));

                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private bool endOfToParse()
            {
                //toParsePos should never be greater than toParseLength
                Debug.Assert(toParsePos <= toParse.Length);
                return toParsePos == toParse.Length;
            }

            private NextLineType moveToNextValidLine()
            {
                NextLineType? tmpRet = null;
                Func<string, NextLineType?> validPos = (string toCheck) => {
                    toCheck.Trim();
                    NextLineType? lineType = null;
                    //Check we only have numbers and spaces
                    var isMatch = isValidGameLine(toCheck);
                    if(isMatch)
                    {
                        lineType = NextLineType.ValidLine;
                    }
                    else if(!isMatch && toCheck != "")
                    {
                        lineType = NextLineType.InvalidLine;
                    }

                    return lineType;
                };
                
                //Skip to the next line of note if we are alreay at a line of note do nothing
                while(!endOfToParse() && (tmpRet = validPos(toParse[toParsePos])) == null)
                    toParsePos++;

                if (endOfToParse())
                    tmpRet = NextLineType.EndOfInput;

                Debug.Assert(tmpRet.HasValue);
                return tmpRet.Value;
            }

            private BingoGame parseNextGame()
            {
                var parsedGameValues = new int[BingoGame.BINGO_WNH, BingoGame.BINGO_WNH];
                for(int i = 0; i < BingoGame.BINGO_WNH; i++)
                {
                    if (endOfToParse())
                        throw new BadInputException("This sould not be called unless at the start of a valid group of BingoGame lines");
                    var nextInputLine = parseValuesLine(toParse[toParsePos]);
                    for(int j = 0; j < BingoGame.BINGO_WNH; j++)
                    {
                        parsedGameValues[i, j] = nextInputLine[j];
                    }
                    toParsePos++;
                }
                return new BingoGame(parsedGameValues);
            }

            private BingoGame getNextGame()
            {
                BingoGame nextGame = null;

                var nextLineType = moveToNextValidLine();
                if (nextLineType != NextLineType.EndOfInput)
                {
                    nextGame = parseNextGame();
                }
                else if (nextLineType == NextLineType.InvalidLine)
                    throw new BadInputException("All BingoGame lines should only contain numbers and spaces:\n" + toParse[toParsePos]);

                return nextGame;
            }

            private static bool isValidGameLine(string toCheck)
            {
                return Regex.IsMatch(toCheck, @"^[0-9 ]+$");
            }

            private void skipToFirstValid()
            {
                bool looking = true;
                while(looking && !endOfToParse())
                {
                    if (!isValidGameLine(this.toParse[toParsePos]))
                        toParsePos++;
                        looking = false;
                }
            }

            private static int[] parseValuesLine(string toParse)
            {
                var parsedValues = new int[BingoGame.BINGO_WNH];
                toParse.Trim();
                string[] sepparatedStrings = toParse.Split();
                int parsedValuesPos = 0;
                int sepparatedStringsPos = 0;
                while (sepparatedStringsPos < sepparatedStrings.Length && parsedValuesPos < parsedValues.Length)
                {
                    var stringPart = sepparatedStrings[sepparatedStringsPos];
                    //They add spaces for number alignment
                    if (stringPart != "")
                    {
                        try
                        {
                            parsedValues[parsedValuesPos] = int.Parse(stringPart);
                        }
                        catch (FormatException e)
                        {
                            throw new BadInputException("Bad BingoGame line", e);
                        }
                        parsedValuesPos++;
                    }
                    sepparatedStringsPos++;
                }
                return parsedValues;
            }
        }

        private static List<int> parseGameInputValues(string inputValLine)
        {
            var convertedList = new List<int>();
            inputValLine.Trim();
            string[] values = inputValLine.Split(new[] { ',' });
            try
            {
                foreach(var value in values)
                {
                    convertedList.Add(int.Parse(value));
                }
            }
            catch(FormatException e)
            {
                throw new BadInputException("Input line should only contain comma separated ints", e);
            }

            return convertedList;
        }

        private static int recSum(int[] toSum, int currPos = 0, int currTotal = 0)
        {
            if(toSum.Length == currPos)
            {
                return currTotal;
            }

            //Should be tail call recursive complient
            return recSum(toSum, currPos + 1, currTotal + toSum[currPos]);
        }
        
        public override string process(string[] toProcess)
        {
            List<int> gameInputValues = parseGameInputValues(toProcess[0]);
            Tuple<int, BingoGame> lastNumAndWinBoard = null;
            List<BingoGame> bingoGames = new List<BingoGame>();

            foreach(var bingoGame in new BingoGameParser(toProcess))
            {
                bingoGames.Add(bingoGame);
            }
            
            foreach(var currVal in gameInputValues)
            {
                for(int j = 0; j < bingoGames.Count; j++)
                {
                    var currBoard = bingoGames[j];
                    currBoard.markOff(currVal);
                    if(currBoard.hasBingo())
                    {
                        lastNumAndWinBoard = new Tuple<int, BingoGame>(currVal, currBoard);
                        break;
                    }
                }
                if (lastNumAndWinBoard != null)
                    break;
            }

            if (lastNumAndWinBoard == null)
                throw new BadInputException("There is no winning board in the given input");
            else
            {
                return (lastNumAndWinBoard.Item1 * recSum(lastNumAndWinBoard.Item2.getUnmarked().ToArray())).ToString();
            }
        }
    }

    public class Day4Q2Handler: QuestionHandler
    {
        private class BingoGame
        {
            /* A typical Bingo game utilizes the numbers 1 through 75. The five
             * columns of the card are labeled 'B', 'I', 'N', 'G', and 'O' from
             * left to right. The center space is usually marked "Free" or
             * "Free Space", and is considered automatically filled. The range
             * of printed numbers that can appear on the card is normally
             * restricted by column, with the 'B' column only containing numbers
             * between 1 and 15 inclusive, the 'I' column containing only 16
             * through 30, 'N' containing 31 through 45, 'G' containing 46
             * through 60, and 'O' containing 61 through 75.
             * Wikipedia: https://en.wikipedia.org/wiki/Bingo_(American_version)#Bingo_cards */
            private int[,] selectedValuesTable;
            private BitArray[] markedOffMap;
            private Dictionary<int, int[]> valuePositions;
            private bool bingo;

            /// <summary>
            /// The standard bingo game width and height (it's a square board)
            /// </summary>
            public const int BINGO_WNH = 5;

            public BingoGame(int[,] selectedValues)
            {
                //a multidimentional array's length is equal to the product of all its dimens
                if (selectedValues != null && selectedValues.Length / BINGO_WNH == BINGO_WNH)
                {
                    this.selectedValuesTable = selectedValues;
                    //This is to act as a fast lookup for the table positions
                    this.valuePositions = new Dictionary<int, int[]>();
                    for (int i = 0; i < BINGO_WNH; i++)
                    {
                        for (int j = 0; j < BINGO_WNH; j++)
                        {
                            //The question don't follow the number rules
                            this.valuePositions.Add(this.selectedValuesTable[i, j], new[] { i, j });
                        }
                    }
                }
                else
                {
                    //TODO: this is for if we add a setter
                    /*if (selectedValues == null)
                    {
                        this.selectedValuesTable = selectedValues;
                        this.valuePositions = null;
                    }
                    else*/
                    throw new BadInputException($"Bingo Tables need to be {BINGO_WNH}x{BINGO_WNH}");
                }

                this.markedOffMap = new BitArray[BINGO_WNH];
                for (int i = 0; i < this.markedOffMap.Length; i++)
                {
                    this.markedOffMap[i] = new BitArray(new[] { false, false, false, false, false });
                }
                //This is the "Free Space"
                this.markedOffMap[2].And(new BitArray(new[] { false, false, true, false, false }));
                this.bingo = false;
            }

            public override string ToString()
            {
                string output = "";
                for(int i = 0; i < BINGO_WNH; i++)
                {
                    var row = this.markedOffMap[i];
                    for(int j = 0; j < BINGO_WNH; j++)
                    {
                        var value = row[j];
                        output += "|";
                        if(value)
                        {
                            output += "---";
                        }
                        else
                        {
                            output += this.selectedValuesTable[i, j].ToString("000");
                        }
                    }
                    output += "|\n";
                }
                //Remove the extra new line
                output.Trim();
                return output;
            }

            public bool hasBingo()
            {
                if (!this.bingo)
                {
                    /*"Diagonals don't count"*/
                    var found = false;
                    var checkCol = new BitArray(new[] { true, true, true, true, true });
                    //If we test a row and all are false then none of the collumns can be true
                    foreach (var row in this.markedOffMap)
                    {
                        //Manually check for equality so we know if we need the check the collumns
                        var tmpFalsePos = hasFalsePos(row);

                        if (tmpFalsePos.Count == 0)
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            //set all the collumns that have false values to false to indicate they are not to be searched
                            for (int i = 0; i < tmpFalsePos.Count; i++)
                                checkCol[tmpFalsePos[i]] = false;
                        }

                    }

                    if (!found && !allFalse(checkCol))
                    {
                        for (int i = 0; i < BINGO_WNH; i++)
                        {
                            BitArray currCol = this.getCol(i);
                            var falsePos = hasFalsePos(currCol);
                            if (falsePos.Count == 0)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        bingo = true;
                    }
                }

                return bingo;
            }

            public void markOff(int calledOutValue)
            {
                if(!bingo)
                {
                    int[] markOffPos;
                    var hasVal = valuePositions.TryGetValue(calledOutValue, out markOffPos);
                    if (hasVal)
                    {
                        //Make a mask for the selected position
                        var tmpRow = new BitArray(new[] { false, false, false, false, false });
                        tmpRow[markOffPos[1]] = true;
                        //Set the selected field by oring the mask with the row it's in
                        markedOffMap[markOffPos[0]].Or(tmpRow);
                    }
                }
            }
            public List<int> getUnmarked()
            {
                var unmarked = new List<int>();
                Tuple<int, int> tmpUnmarkedPos = new Tuple<int, int>(0, 0);

                foreach (var val in this.valuePositions.Keys)
                {
                    int[] valPos;
                    this.valuePositions.TryGetValue(val, out valPos);

                    if (!this.markedOffMap[valPos[0]][valPos[1]])
                    {
                        unmarked.Add(val);
                    }
                }

                return unmarked;
            }

            private BitArray getCol(int colIdx)
            {
                Debug.Assert(colIdx < BINGO_WNH);
                var selectedCollumn = new BitArray(new[] { false, false, false, false, false });
                //row i at colIdx gets mapped into col i of selected collumn
                for(int i = 0; i < this.markedOffMap.Length; i++)
                {
                    var row = this.markedOffMap[i];
                    selectedCollumn[i] = row[colIdx];
                }
                return selectedCollumn;
            }

            private static List<int> hasFalsePos(BitArray row)
            {
                var ret = new List<int>();
                for (int i = 0; i < row.Length; i++)
                {
                    if (row[i] == false)
                    {
                        ret.Add(i);
                    }
                }
                return ret;
            }

            private static bool allFalse(BitArray toCheck)
            {
                var ret = true;
                foreach (bool bit in toCheck)
                {
                    //If true then thair not all false;
                    if (bit)
                    {
                        ret = false;
                        break;
                    }
                }
                return ret;
            }

        }

        private class BingoGameParser : IEnumerable<BingoGame>
        {
            enum NextLineType
            {
                ValidLine,
                InvalidLine,
                EndOfInput
            }

            uint toParsePos;
            string[] toParse;

            public BingoGameParser(string[] input)
            {
                this.toParsePos = 0;
                this.toParse = input;
                this.skipToFirstValid();
            }

            public IEnumerator<BingoGame> GetEnumerator()
            {
                BingoGame nextGame = null;
                Func<BingoGame, bool> validGame = (BingoGame game) => game != null;
                do
                {
                    nextGame = getNextGame();
                    if (validGame(nextGame))
                        yield return nextGame;
                } while (validGame(nextGame));

                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private bool endOfToParse()
            {
                //toParsePos should never be greater than toParseLength
                Debug.Assert(toParsePos <= toParse.Length);
                return toParsePos == toParse.Length;
            }

            private NextLineType moveToNextValidLine()
            {
                NextLineType? tmpRet = null;
                Func<string, NextLineType?> validPos = (string toCheck) => {
                    toCheck.Trim();
                    NextLineType? lineType = null;
                    //Check we only have numbers and spaces
                    var isMatch = isValidGameLine(toCheck);
                    if (isMatch)
                    {
                        lineType = NextLineType.ValidLine;
                    }
                    else if (!isMatch && toCheck != "")
                    {
                        lineType = NextLineType.InvalidLine;
                    }

                    return lineType;
                };

                //Skip to the next line of note if we are alreay at a line of note do nothing
                while (!endOfToParse() && (tmpRet = validPos(toParse[toParsePos])) == null)
                    toParsePos++;

                if (endOfToParse())
                    tmpRet = NextLineType.EndOfInput;

                Debug.Assert(tmpRet.HasValue);
                return tmpRet.Value;
            }

            private BingoGame parseNextGame()
            {
                var parsedGameValues = new int[BingoGame.BINGO_WNH, BingoGame.BINGO_WNH];
                for (int i = 0; i < BingoGame.BINGO_WNH; i++)
                {
                    if (endOfToParse())
                        throw new BadInputException("This sould not be called unless at the start of a valid group of BingoGame lines");
                    var nextInputLine = parseValuesLine(toParse[toParsePos]);
                    for (int j = 0; j < BingoGame.BINGO_WNH; j++)
                    {
                        parsedGameValues[i, j] = nextInputLine[j];
                    }
                    toParsePos++;
                }
                return new BingoGame(parsedGameValues);
            }

            private BingoGame getNextGame()
            {
                BingoGame nextGame = null;

                var nextLineType = moveToNextValidLine();
                if (nextLineType != NextLineType.EndOfInput)
                {
                    nextGame = parseNextGame();
                }
                else if (nextLineType == NextLineType.InvalidLine)
                    throw new BadInputException("All BingoGame lines should only contain numbers and spaces:\n" + toParse[toParsePos]);

                return nextGame;
            }

            private static bool isValidGameLine(string toCheck)
            {
                return Regex.IsMatch(toCheck, @"^[0-9 ]+$");
            }

            private void skipToFirstValid()
            {
                bool looking = true;
                while (looking && !endOfToParse())
                {
                    if (!isValidGameLine(this.toParse[toParsePos]))
                        toParsePos++;
                    looking = false;
                }
            }

            private static int[] parseValuesLine(string toParse)
            {
                var parsedValues = new int[BingoGame.BINGO_WNH];
                toParse.Trim();
                string[] sepparatedStrings = toParse.Split();
                int parsedValuesPos = 0;
                int sepparatedStringsPos = 0;
                while (sepparatedStringsPos < sepparatedStrings.Length && parsedValuesPos < parsedValues.Length)
                {
                    var stringPart = sepparatedStrings[sepparatedStringsPos];
                    //They add spaces for number alignment
                    if (stringPart != "")
                    {
                        try
                        {
                            parsedValues[parsedValuesPos] = int.Parse(stringPart);
                        }
                        catch (FormatException e)
                        {
                            throw new BadInputException("Bad BingoGame line", e);
                        }
                        parsedValuesPos++;
                    }
                    sepparatedStringsPos++;
                }
                return parsedValues;
            }
        }

        private static List<int> parseGameInputValues(string inputValLine)
        {
            var convertedList = new List<int>();
            inputValLine.Trim();
            string[] values = inputValLine.Split(new[] { ',' });
            try
            {
                foreach (var value in values)
                {
                    convertedList.Add(int.Parse(value));
                }
            }
            catch (FormatException e)
            {
                throw new BadInputException("Input line should only contain comma separated ints", e);
            }

            return convertedList;
        }

        private static int recSum(int[] toSum, int currPos = 0, int currTotal = 0)
        {
            if (toSum.Length == currPos)
            {
                return currTotal;
            }

            //Should be tail call recursive complient
            return recSum(toSum, currPos + 1, currTotal + toSum[currPos]);
        }

        private static Tuple<int, int> findLastGameToWin(List<int> gameInputValues, List<BingoGame> bingoGames)
        {
            //keep track of how many games have been won
            int winningValue = 0;
            List<BingoGame> winOrder = new List<BingoGame>();
            BingoGame winningGame = null;


            foreach(var value in gameInputValues)
            {
                foreach(var game in bingoGames)
                {
                    game.markOff(value);
                    if(game.hasBingo())
                    {
                        if(!winOrder.Contains(game))
                        {
                            winOrder.Add(game);
                        }
                    }

                    if(winOrder.Count == bingoGames.Count)
                    {
                        winningGame = game;
                        winningValue = value;
                        break;
                    }
                }
                if(winOrder.Count == bingoGames.Count)
                    break;
            }

            return new Tuple<int, int>(winningValue, bingoGames.IndexOf(winningGame));
        }

        public override string process(string[] toProcess)
        {
            List<int> gameInputValues = parseGameInputValues(toProcess[0]);
            //the number to win the game and the index of the game that won
            Tuple<int, int> lastNumAndWinBoard = null;
            List<BingoGame> bingoGames = new List<BingoGame>();

            foreach (var bingoGame in new BingoGameParser(toProcess))
            {
                bingoGames.Add(bingoGame);
            }

            lastNumAndWinBoard = findLastGameToWin(gameInputValues, bingoGames);

            if (lastNumAndWinBoard == null)
                throw new BadInputException("There is no winning board in the given input");
            else
            {
                return (lastNumAndWinBoard.Item1 * recSum(bingoGames[lastNumAndWinBoard.Item2].getUnmarked().ToArray())).ToString();
            }
        }
    }

}
