﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLifeProject
{
    /// <summary>
    /// form class to run the game of life with all control over user settings and options
    /// </summary>
    public partial class GameOfLife : Form
    {

        bool[,] universe;
        bool[,] scratchPad;

        Color gridColor;
        Color cellColor;

        Timer timer;

        int generations;
        int timerInterval;
        int arrayWidth;
        int arrayHeight;
        int randomSeed;

        public GameOfLife()
        {
            InitializeComponent();

            this.Text = "Game Of Life";

            graphicsPanel2.BackColor = Properties.Settings.Default.BackColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            timerInterval = Properties.Settings.Default.TimerInterval;
            arrayWidth = Properties.Settings.Default.ArrayWidth;
            arrayHeight = Properties.Settings.Default.ArrayHeight;
            randomSeed = Properties.Settings.Default.RandomSeed;

            universe = new bool[arrayWidth, arrayHeight];
            scratchPad = new bool[arrayWidth, arrayHeight];

            timer = new Timer();

            timer.Interval = timerInterval;
            timer.Tick += Timer_Tick;
            timer.Enabled = false;

            intervalStripStatusLabel1.Text = "Interval: " + timerInterval;
            seedStripStatusLabel.Text = "Seed: " + randomSeed;
            generations = 0;
        }

        /// <summary>
        /// Calculate the next generation of cells
        /// </summary>
        private void NextGeneration()
        {
            int neighborCount;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (finiteToolStripMenuItem.Checked) neighborCount = CountNeighborsFinite(x, y);
                    else neighborCount = CountNeighborsToroidal(x, y);


                    if (universe[x, y])
                    {
                        if (neighborCount < 2) scratchPad[x, y] = false;

                        if (neighborCount > 3) scratchPad[x, y] = false;

                        if (neighborCount == 2 || neighborCount == 3) scratchPad[x, y] = true;
                    }
                    else
                    {
                        if (neighborCount == 3) scratchPad[x, y] = true;

                    }
                }
            }

            universe = scratchPad;
            scratchPad = new bool[arrayWidth, arrayHeight];

            generations++;

            toolStripStatusLabelGenerations.Text = "Generations: " + generations.ToString();
            activeStripStatusLabel.Text = "Active: " + GetActiveCount().ToString();

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// Gets count of all active cells
        /// </summary>
        /// <returns>amount of alive cells</returns>
        private int GetActiveCount()
        {
            int count = 0;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x, y]) count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Counts neighbers of given cell when finite
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>The amount of neighbors </returns>
        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    if (xOffset == 0 && yOffset == 0) continue;
                    if (xCheck < 0) continue;
                    if (yCheck < 0) continue;
                    if (xCheck >= xLen) continue;
                    if (yCheck >= yLen) continue;

                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Counts neighbers of given cell when toroidal
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>The amount of neighbors </returns>
        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    if (xOffset == 0 && yOffset == 0) continue;
                    if (xCheck < 0) xCheck = xLen - 1;
                    if (yCheck < 0) yCheck = xLen - 1;
                    if (xCheck >= xLen) xCheck = 0;
                    if (yCheck >= yLen) yCheck = 0;

                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }

        /// <summary>
        /// The event called by the timer every Interval milliseconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        /// <summary>
        /// paints grid with lines around boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            float cellWidth = (float)graphicsPanel2.ClientSize.Width / (float)universe.GetLength(0);
            float cellHeight = (float)graphicsPanel2.ClientSize.Height / (float)universe.GetLength(1);

            Pen gridPen = new Pen(gridColor, 1);

            Brush cellBrush = new SolidBrush(cellColor);

            Font font = new Font("Arial", cellHeight * (float) 0.65);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            int neighbors;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    RectangleF cellRect = RectangleF.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    if (finiteToolStripMenuItem.Checked) neighbors = CountNeighborsFinite(x, y);
                    else neighbors = CountNeighborsToroidal(x, y);

                    if (neighbors > 0 && neighborCountToolStripMenuItem.Checked)
                    {
                        if (neighbors < 3) e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Red, cellRect, stringFormat);
                        if (neighbors > 2) e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Green, cellRect, stringFormat);

                    }

                    if (gridToolStripMenuItem.Checked) e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }

            if (hudToolStripMenuItem.Checked) DrawHud(e);

            gridPen.Dispose();
            cellBrush.Dispose();
        }

        /// <summary>
        /// draws hud in graphics panel 
        /// </summary>
        /// <param name="e"></param>
        private void DrawHud(PaintEventArgs e)
        {
            Font font = new Font("Arial", 15f, FontStyle.Bold);

            Brush brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));


            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Far;

            string universeSize = $"Universe Size: {{ Width: {arrayWidth}, Height: {arrayHeight} }}";
            string boundryType = $"Boundry Type: {(finiteToolStripMenuItem.Checked ? "Finite" : "Toroidal")}";
            string cellCount = $"Cell Count: {GetActiveCount()}";
            string hudGenerations = $"Generations: {generations}";
            string hud = $"{hudGenerations}\n{cellCount}\n{boundryType}\n{universeSize}";

            e.Graphics.DrawString(hud, font, brush, graphicsPanel2.ClientRectangle, stringFormat);
        }

        /// <summary>
        /// toggles cell on click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {

                float cellWidth = (float)graphicsPanel2.ClientSize.Width / (float)universe.GetLength(0);
                float cellHeight = (float)graphicsPanel2.ClientSize.Height / (float)universe.GetLength(1);


                int x = (int)Math.Floor(e.X / cellWidth);
                int y = (int)Math.Floor(e.Y / cellHeight);

                try
                {
                    universe[x, y] = !universe[x, y];
                }
                catch
                {
                    MessageBox.Show("Incorrect Click.\nTry Again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                activeStripStatusLabel.Text = "Active: " + GetActiveCount().ToString();

                graphicsPanel2.Invalidate();
            }
        }

        /// <summary>
        /// when play button is clicked timer is started
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playToolStripButton_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
            playToolStripButton.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            pauseToolStripButton.Enabled = true;
            pauseToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// when pause button is clicked stops timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pauseToolStripButton_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
            playToolStripButton.Enabled = true;
            startToolStripMenuItem.Enabled = true;
            pauseToolStripButton.Enabled = false;
            pauseToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// On next button click will write next generation of cycle 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextToolStripButton_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        /// <summary>
        /// Restarts universe and scratch pad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            universe = new bool[arrayWidth, arrayHeight];
            scratchPad = new bool[arrayWidth, arrayHeight];
            generations = 0;

            toolStripStatusLabelGenerations.Text = "Generations: " + generations.ToString();
            activeStripStatusLabel.Text = "Active: " + GetActiveCount().ToString();

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// On save click will save current universe to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files |*.*|Cells|*.cells*";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                DateTime localDate = DateTime.Now;
                StreamWriter writer = new StreamWriter(dlg.FileName);

                writer.WriteLine("!" + localDate.ToString());

                for (int y = 0; y < universe.GetLength(0); y++)
                {
                    string row = string.Empty;

                    for (int x = 0; x < universe.GetLength(1); x++)
                    {
                        if (universe[x, y]) row += "O";
                        else row += ".";
                    }

                    writer.WriteLine(row);
                }

                writer.Close();
            }
        }

        /// <summary>
        /// Once exit button is clicked asks if user wants to exit and exits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you would like to exit?", "Exit Prompt", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) this.Close();

        }

        /// <summary>
        /// Opens and resizes array for cells file when ok button is clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files |*.*|Cells|*.cells*";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);
                int maxWidth = 0;
                int maxHeight = 0;

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row[0] != '!')
                    {
                        maxHeight++;

                        if (row.Length > maxWidth) maxWidth = row.Length;

                    }
                }

                universe = new bool[maxWidth, maxHeight];
                scratchPad = new bool[maxWidth, maxHeight];

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                int yPos = 0;

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row[0] != '!')
                    {
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos] == 'O') universe[xPos, yPos] = true;
                            else universe[xPos, yPos] = false;
                        }

                        yPos++;
                    }
                }

                reader.Close();

                activeStripStatusLabel.Text = "Active: " + GetActiveCount().ToString();

                graphicsPanel2.Invalidate();
            }
        }

        /// <summary>
        /// Imports cell file when ok button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files |*.*|Cells|*.cells*";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                int yPos = 0;

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row[0] != '!')
                    {
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos] == 'O') universe[xPos, yPos] = true;
                            else universe[xPos, yPos] = false;
                        }

                        yPos++;
                    }
                }

                reader.Close();

                activeStripStatusLabel.Text = "Active: " + GetActiveCount().ToString();

                graphicsPanel2.Invalidate();
            }

        }

        /// <summary>
        /// Changes back color once ok button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel2.BackColor = dlg.Color;
            }
        }

        /// <summary>
        /// Changes cell color when ok button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;
            }

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// Changes grid color when clicked ok button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;
            }

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// Saves settings on application exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameOfLife_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.BackColor = graphicsPanel2.BackColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.TimerInterval = timerInterval;
            Properties.Settings.Default.ArrayWidth = arrayWidth;
            Properties.Settings.Default.ArrayHeight = arrayHeight;
            Properties.Settings.Default.RandomSeed = randomSeed;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Resets settings to original settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            graphicsPanel2.BackColor = Properties.Settings.Default.BackColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            timerInterval = Properties.Settings.Default.TimerInterval;
            arrayWidth = Properties.Settings.Default.ArrayWidth;
            arrayHeight = Properties.Settings.Default.ArrayHeight;
            randomSeed = Properties.Settings.Default.RandomSeed;

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// Resets Settings to saved default settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            graphicsPanel2.BackColor = Properties.Settings.Default.BackColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            timerInterval = Properties.Settings.Default.TimerInterval;
            arrayWidth = Properties.Settings.Default.ArrayWidth;
            arrayHeight = Properties.Settings.Default.ArrayHeight;
            randomSeed = Properties.Settings.Default.RandomSeed;

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// Opens option dialog to adjust timer interval, width, and height settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog dlg = new OptionsDialog();

            dlg.TimerInterval = timerInterval;
            dlg.ArrayWidth = arrayWidth;
            dlg.ArrayHeight = arrayHeight;

            if (DialogResult.OK == dlg.ShowDialog())
            {

                timerInterval = dlg.TimerInterval;
                timer.Interval = timerInterval;

                if (arrayHeight != dlg.ArrayHeight || arrayWidth != dlg.ArrayWidth)
                {
                    arrayWidth = dlg.ArrayWidth;
                    arrayHeight = dlg.ArrayHeight;
                    universe = new bool[arrayWidth, arrayHeight];
                    scratchPad = new bool[arrayWidth, arrayHeight];
                }


                intervalStripStatusLabel1.Text = "Interval: " + timerInterval;
                seedStripStatusLabel.Text = "Seed: " + randomSeed;
                graphicsPanel2.Invalidate();
            }
        }

        /// <summary>
        /// opens run to dialog and runs the number of genrations choosen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunToDialog dlg = new RunToDialog();

            if (DialogResult.OK == dlg.ShowDialog())
            {
                int loops = dlg.RunToNum;

                while (loops > 0)
                {
                    NextGeneration();

                    loops--;
                }
            }
        }

        /// <summary>
        /// opens random seed dialog and generates random grid from seed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeedDialog dlg = new SeedDialog();

            dlg.RandomSeed = randomSeed;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                randomSeed = dlg.RandomSeed;
                seedStripStatusLabel.Text = "Seed: " + randomSeed;
                RandomizeGrid();
            };
        }

        /// <summary>
        /// gnerates a random grid by time or by seed
        /// </summary>
        /// <param name="time">boolean for if rand should be time</param>
        private void RandomizeGrid(bool time = false)
        {
            Random rand;

            if (time) rand = new Random();
            else rand = new Random(randomSeed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = (rand.Next(0, 2) == 0);
                }
            }

            activeStripStatusLabel.Text = "Active: " + GetActiveCount().ToString();

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// toggles count neighbor to toridal mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toroidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finiteToolStripMenuItem.Checked = false;
            toroidalToolStripMenuItem.Checked = true;
        }

        /// <summary>
        /// toggles count neighbor to finite mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finiteToolStripMenuItem.Checked = true;
            toroidalToolStripMenuItem.Checked = false;
        }

        /// <summary>
        /// toggles adding neighbor count to cells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            neighborCountToolStripMenuItem.Checked = !neighborCountToolStripMenuItem.Checked;
            neighborCountToolStripMenuItem1.Checked = !neighborCountToolStripMenuItem1.Checked;

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// toggles whether grid will show around alive cells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridToolStripMenuItem.Checked = !gridToolStripMenuItem.Checked;
            gridToolStripMenuItem1.Checked = !gridToolStripMenuItem1.Checked;

            graphicsPanel2.Invalidate();
        }

        /// <summary>
        /// randomizes grid from current seed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizeGrid();
        }

        /// <summary>
        /// randomizes grid based of time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizeGrid(true);
        }

        /// <summary>
        /// toggles if hud should show
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hudToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hudToolStripMenuItem.Checked = !hudToolStripMenuItem.Checked;
            hudToolStripMenuItem1.Checked = !hudToolStripMenuItem1.Checked;

            graphicsPanel2.Invalidate();
        }
    }
}

