using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Advent_of_code_2021
{
    public partial class MainWin : Form
    {
        public MainWin()
        {
            InitializeComponent();
        }

        private void MainWin_Load(object sender, EventArgs e)
        {
            int dayCount = 0, partCount = 0;
            object[] daysList = null;
            object[] partsList = null;

            dayCount = QuestionHandler.QuestionHandlerFactory.getDayCount() ?? 0;
            if(dayCount > 0)
            {
                daysList = numSelectionArayFromInt(1, dayCount);

                partCount = QuestionHandler.QuestionHandlerFactory.getPartCount(dayCount) ?? 0;
                if(partCount > 0)
                    partsList = numSelectionArayFromInt(1, partCount);
            }

            Debug.Assert((daysList != null) && (partsList != null));

            //Add all the options for the user to select
            this.daySelectComboBox.Items.AddRange(daysList);
            this.partSelectComboBox.Items.AddRange(partsList);

            //Set the default position to last
            int lastDay, lastPart;
            lastDay = this.daySelectComboBox.Items.Count-1;
            lastPart = this.partSelectComboBox.Items.Count-1;
            this.daySelectComboBox.SelectedIndex = lastDay;
            this.partSelectComboBox.SelectedIndex = lastPart;
        }

        private static object[] numSelectionArayFromInt(int lowerBound, int upperBound)
        {
            if(upperBound > lowerBound)
            {
                object[] ret = new object[(upperBound-lowerBound)+1];

                for(int i = 0; lowerBound+i <= upperBound; i++)
                {
                    ret[i] = i + lowerBound;
                }

                return ret;
            }
            else if(upperBound == lowerBound)
            {
                object[] ret = new object[1];
                ret[0] = lowerBound;
                return ret;
            }
            else
            {
                return null;
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Show the about box
            new Aoc2021AboutBox().Show();
        }

        private void solveButton_Click(object sender, EventArgs e)
        {
            bool validState =
                ((this.daySelectComboBox.SelectedItem != null) &&
                 (this.partSelectComboBox.SelectedItem != null));

            if (validState)
            {
                int part, day;
                string questionInput;
                string[] processedQuestionInput;
                day = (int)this.daySelectComboBox.SelectedItem;
                part = (int)this.partSelectComboBox.SelectedItem;
                questionInput = this.questionInputTextBox.Text;

                QuestionHandler.QuestionHandler handler =
                    QuestionHandler.QuestionHandlerFactory.getNew(day, part);

                /* This should never be null as the user inputs are generated from what is in the table.
                 * hence they should be by deffinition consistent. */
                Debug.Assert(handler != null);

                questionInput.Trim();
                processedQuestionInput = questionInput.Split(new[] { "\r\n" }, StringSplitOptions.None);
                if((processedQuestionInput.Length >= 1) && (!processedQuestionInput[0].Equals(""))){
                    try
                    {
                        this.questionOutputTextBox.Text =
                            handler.process(processedQuestionInput);
                    }
                    catch (Exception except)
                    {
                        this.badInputUserNotification(except);
                    }
                }
            }
        }

        private void daySelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox daySelect = (ComboBox)sender;
            var partCount = QuestionHandler.QuestionHandlerFactory.getPartCount(daySelect.SelectedIndex + 1) ?? 0;
            object[] parts = null;
            if(partCount > 0)
                parts = numSelectionArayFromInt(1, partCount);

            Debug.Assert(!(parts == null));

            //Clear out the previous contense
            int end = this.partSelectComboBox.Items.Count;
            for(int i = 0; i < end; i++)
            {
                var item = this.partSelectComboBox.Items[0];
                this.partSelectComboBox.Items.Remove(item);
            }

            this.partSelectComboBox.Items.AddRange(parts);
        }

        private void badInputUserNotification(Exception e)
        {
            MessageBox.Show("Processing failed with the following message:\n" + e.Message,
                "Processing failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
