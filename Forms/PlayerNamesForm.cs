using System;
using System.Windows.Forms;

namespace GameTennis.Forms
{
    public partial class PlayerNamesForm : Form
    {
        public string LeftPlayerName { get; private set; } = string.Empty;
        public string RightPlayerName { get; private set; } = string.Empty;
        private readonly TextBox leftPlayerTextBox = new();
        private readonly TextBox rightPlayerTextBox = new();
        private readonly Button startButton = new();

        public PlayerNamesForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            this.Text = "Noms des Joueurs";
            this.Size = new System.Drawing.Size(400, 250);

            Label leftPlayerLabel = new Label
            {
                Text = "Joueur Gauche:",
                Location = new System.Drawing.Point(30, 30),
                Size = new System.Drawing.Size(120, 25),
                Font = new Font("Arial", 12)
            };

            leftPlayerTextBox.Location = new System.Drawing.Point(30, 60);
            leftPlayerTextBox.Size = new System.Drawing.Size(320, 30);
            leftPlayerTextBox.Font = new Font("Arial", 12);

            Label rightPlayerLabel = new Label
            {
                Text = "Joueur Droite:",
                Location = new System.Drawing.Point(30, 100),
                Size = new System.Drawing.Size(120, 25),
                Font = new Font("Arial", 12)
            };

            rightPlayerTextBox.Location = new System.Drawing.Point(30, 130);
            rightPlayerTextBox.Size = new System.Drawing.Size(320, 30);
            rightPlayerTextBox.Font = new Font("Arial", 12);

            startButton.Text = "Commencer";
            startButton.Location = new System.Drawing.Point(140, 170);
            startButton.Size = new System.Drawing.Size(120, 40);
            startButton.Font = new Font("Arial", 12);
            startButton.Click += StartButton_Click;

            this.Controls.AddRange(new Control[] { 
                leftPlayerLabel, 
                leftPlayerTextBox, 
                rightPlayerLabel, 
                rightPlayerTextBox, 
                startButton 
            });
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(leftPlayerTextBox.Text) || 
                string.IsNullOrWhiteSpace(rightPlayerTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer les noms des deux joueurs.", 
                              "Erreur", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Warning);
                return;
            }

            LeftPlayerName = leftPlayerTextBox.Text;
            RightPlayerName = rightPlayerTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}