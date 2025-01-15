using System;
using System.Windows.Forms;

namespace GameTennis.Forms
{
    public class GameConfigForm : Form
    {
        public float OnePointZoneSpeed { get; private set; }
        public int TwoPointZonePoints { get; private set; }
        private readonly NumericUpDown speedInput = new();
        private readonly NumericUpDown pointsInput = new();
        private readonly Button confirmButton = new();

        public GameConfigForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            this.Text = "Configuration des zones";
            this.Size = new System.Drawing.Size(400, 250);
            this.BackColor = Color.FromArgb(64, 64, 64);
            this.ForeColor = Color.White;

            // Zone de vitesse (1 point)
            Label speedLabel = new Label
            {
                Text = "Vitesse de la zone à 1 point:",
                Location = new System.Drawing.Point(30, 30),
                Size = new System.Drawing.Size(320, 25),
                Font = new Font("Arial", 12),
                ForeColor = Color.White
            };

            speedInput.Location = new System.Drawing.Point(30, 60);
            speedInput.Size = new System.Drawing.Size(320, 30);
            speedInput.Font = new Font("Arial", 12);
            speedInput.DecimalPlaces = 3;
            speedInput.Increment = 0.001M;
            speedInput.Minimum = 0.001M;
            speedInput.Maximum = 10.001M;
            speedInput.BackColor = Color.FromArgb(80, 80, 80);
            speedInput.ForeColor = Color.White;

            // Points de la zone rapide
            Label pointsLabel = new Label
            {
                Text = "Nombre de points de la zone rapide:",
                Location = new System.Drawing.Point(30, 100),
                Size = new System.Drawing.Size(320, 25),
                Font = new Font("Arial", 12),
                ForeColor = Color.White
            };

            pointsInput.Location = new System.Drawing.Point(30, 130);
            pointsInput.Size = new System.Drawing.Size(320, 30);
            pointsInput.Font = new Font("Arial", 12);
            pointsInput.Minimum = 2;
            pointsInput.Maximum = 20;
            pointsInput.BackColor = Color.FromArgb(80, 80, 80);
            pointsInput.ForeColor = Color.White;

            confirmButton.Text = "Confirmer";
            confirmButton.Location = new System.Drawing.Point(140, 170);
            confirmButton.Size = new System.Drawing.Size(120, 40);
            confirmButton.Font = new Font("Arial", 12);
            confirmButton.BackColor = Color.FromArgb(100, 100, 100);
            confirmButton.ForeColor = Color.White;
            confirmButton.Click += ConfirmButton_Click;

            this.Controls.AddRange(new Control[] { 
                speedLabel, 
                speedInput, 
                pointsLabel, 
                pointsInput, 
                confirmButton 
            });
        }

        private void ConfirmButton_Click(object? sender, EventArgs e)
        {
            if (speedInput.Value == 0)
            {
                MessageBox.Show(
                    "Veuillez entrer une vitesse pour la zone à 1 point.",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (pointsInput.Value == 0)
            {
                MessageBox.Show(
                    "Veuillez entrer le nombre de points pour la zone rapide.",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            OnePointZoneSpeed = (float)speedInput.Value;
            TwoPointZonePoints = (int)pointsInput.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}