using System;
using System.Collections.Generic;
/*using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;*/

namespace Advent_of_code_2021
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/

            //For now we will make this a console only app then refactor
            List<string> input = new List<string>();
            Console.WriteLine("Please paste input in now:");
            string tmpInput = null;
            while((tmpInput = Console.ReadLine()) != null)
            {
                input.Add(tmpInput);
            }

            QuestionHandler<int> d1Q2H = new Day1Q2Handler();
            string output = String.Format("There where {0} increces", d1Q2H.process(input.ToArray()));
            Console.WriteLine(output);
            Console.ReadLine();
        }
    }
    
}
