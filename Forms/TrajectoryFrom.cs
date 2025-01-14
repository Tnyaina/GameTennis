using System;
using System.Windows.Forms;
using GameTennis.Models;

namespace GameTennis.Forms
{
    public partial class TrajectoryForm : Form
    {
        private readonly Paddle paddle;
        private NumericUpDown? speedInput;
        private ComboBox? comboTrajectories;
        private NumericUpDown? pointsInput;

        public TrajectoryForm(Paddle targetPaddle, string playerName, string paddlePosition)
        {
            paddle = targetPaddle;
            InitializeComponent(playerName, paddlePosition);
            this.TopMost = true;
        }

        private void InitializeComponent(string playerName, string paddlePosition)
        {
            this.Text = $"Configuration {playerName} - {paddlePosition}";
            this.Size = new System.Drawing.Size(600, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(64, 64, 64);
            this.ForeColor = Color.White;

            // Groupe Trajectoire
            GroupBox trajectoryGroup = new GroupBox
            {
                Text = "Type de Trajectoire",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(400, 100),
                ForeColor = Color.White
            };

            comboTrajectories = new ComboBox
            {
                Location = new System.Drawing.Point(10, 25),
                Size = new System.Drawing.Size(400, 50),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White
            };
            
            comboTrajectories.Items.AddRange(Enum.GetNames(typeof(TrajectoryType)));
            comboTrajectories.SelectedItem = paddle.CurrentTrajectory.ToString();
            comboTrajectories.SelectedIndexChanged += ComboTrajectories_SelectedIndexChanged;
            
            trajectoryGroup.Controls.Add(comboTrajectories);

            // Groupe Vitesse
            GroupBox speedGroup = new GroupBox
            {
                Text = "Vitesse",
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(400, 100),
                ForeColor = Color.White
            };

            speedInput = new NumericUpDown
            {
                Location = new System.Drawing.Point(10, 25),
                Size = new System.Drawing.Size(400, 50),
                DecimalPlaces = 3,
                Increment = 0.001M,
                Minimum = 0.001M,
                Maximum = 0.2M,
                Value = (decimal)paddle.Speed,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White
            };
            
            speedInput.ValueChanged += SpeedInput_ValueChanged;
            
            speedGroup.Controls.Add(speedInput);

            // Groupe Points
            GroupBox pointsGroup = new GroupBox
            {
                Text = "Points",
                Location = new System.Drawing.Point(20, 180),
                Size = new System.Drawing.Size(400, 100),
                ForeColor = Color.White
            };

            pointsInput = new NumericUpDown
            {
                Location = new System.Drawing.Point(10, 25),
                Size = new System.Drawing.Size(330, 25),
                Minimum = 1,
                Maximum = 2,
                Value = paddle.Points,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White
            };
            
            pointsInput.ValueChanged += PointsInput_ValueChanged;
            
            pointsGroup.Controls.Add(pointsInput);

            this.Controls.AddRange(new Control[] { trajectoryGroup, speedGroup, pointsGroup });
        }

        private void ComboTrajectories_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (comboTrajectories?.SelectedItem != null)
            {
                paddle.CurrentTrajectory = (TrajectoryType)Enum.Parse(typeof(TrajectoryType), comboTrajectories.SelectedItem.ToString()!);
            }
        }

        private void SpeedInput_ValueChanged(object? sender, EventArgs e)
        {
            if (speedInput != null)
            {
                paddle.Speed = (float)speedInput.Value;
            }
        }

        private void PointsInput_ValueChanged(object? sender, EventArgs e)
        {
            if (pointsInput != null)
            {
                paddle.Points = (int)pointsInput.Value;
            }
        }
    }
}