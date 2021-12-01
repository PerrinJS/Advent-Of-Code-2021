using System;
using System.Diagnostics;

namespace Advent_of_code_2021
{
    public abstract class QuestionHandler<T>
    {
        public abstract T process(string[] toProcess);
    }

    public class Day1Q1Handler: QuestionHandler<int>
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

        public override int process(string[] toProcess)
        {
            int ret = 0, prevVal = 0;
            bool firstRun = true;
            foreach(string s in toProcess)
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
            return ret;
        }
    }

    public class Day1Q2Handler: QuestionHandler<int>
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
        public override int process(string[] toProcess)
        {
            ProcessingState state = new ProcessingState();
            foreach(string s in toProcess)
            {
                state.nextIn(Int32.Parse(s));
            }
            return state.getTotalIncreced();
        }
    }
}
