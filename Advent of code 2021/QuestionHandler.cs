using System;
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

           new HandlerMapElement("Dive!", 1, 1, delegate{return new Day2Q1Handler();}),
        };
        private static int mapDay(int day)
        {
             return (int)Math.Ceiling(((double)day) / 2.0);
        }

        private static HandlerMapElement? getElement(int day, int part)
        {
            if(part > 2)
            {
                throw new IndexOutOfRangeException("There are only two parts to every question");
            }
            else
            {
                int index = mapDay(day);
                index--;
                index += part;
                index--;


                if(index > HANDLER_MAPPINGS.Length)
                {
                    return null;
                }
                else
                {
                    return HANDLER_MAPPINGS[index];
                }
            }
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

        public static int getDayCount()
        {
            return mapDay(HANDLER_MAPPINGS.Length);
        }

        public static int getPartCount(int day)
        {
            //FIXME: make this return the #parts for the guiven day
            //The total length minus how many total days we have
            int remainder = mapDay(HANDLER_MAPPINGS.Length) * 2 - HANDLER_MAPPINGS.Length;
            if (remainder == 0)
                return 2;
            else
                return 1;
        }
    }


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
        public override string process(string[] toProcess)
        {
            return "";
        }
    }
}
