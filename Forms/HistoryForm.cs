using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using GameTennis.Data;

namespace GameTennis.Forms
{
    public class HistoryForm : Form
    {
        private readonly DatabaseConnection dbConnection;
        private DataGridView dataGridView;
        private Button refreshButton;
        private Label titleLabel;

        public HistoryForm(DatabaseConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            InitializeComponents();
            LoadHistoryData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);
            this.Text = "Historique des Matchs";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Titre
            titleLabel = new Label
            {
                Text = "Historique des Matchs de Tennis",
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // Bouton Rafraîchir
            refreshButton = new Button
            {
                Text = "Rafraîchir",
                Size = new Size(100, 30),
                Location = new Point(10, 50),
                BackColor = Color.LightBlue
            };
            refreshButton.Click += async (s, e) => await LoadHistoryData();

            // Grille de données
            dataGridView = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(965, 460),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

            // Configuration des colonnes
            dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                {
                    Name = "MatchDate",
                    HeaderText = "Date du Match",
                    DataPropertyName = "MatchDate"
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Player1Name",
                    HeaderText = "Joueur 1",
                    DataPropertyName = "Player1Name"
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Player2Name",
                    HeaderText = "Joueur 2",
                    DataPropertyName = "Player2Name"
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "WinnerName",
                    HeaderText = "Gagnant",
                    DataPropertyName = "WinnerName"
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "ScoreProgression",
                    HeaderText = "Progression du Score",
                    DataPropertyName = "ScoreProgression"
                }
            });

            // Ajout des contrôles
            this.Controls.AddRange(new Control[] { titleLabel, refreshButton, dataGridView });
        }

        private async Task LoadHistoryData()
        {
            try
            {
                var matches = await dbConnection.GetMatchHistory();
                dataGridView.DataSource = matches;

                // Formatage des colonnes
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    if (column.Name == "MatchDate")
                    {
                        column.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    }
                }

                // Mise en évidence des gagnants
                dataGridView.CellFormatting += (s, e) =>
                {
                    if (e.ColumnIndex == dataGridView.Columns["WinnerName"].Index)
                    {
                        e.CellStyle.ForeColor = Color.Green;
                        e.CellStyle.Font = new Font(dataGridView.Font, FontStyle.Bold);
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors du chargement de l'historique : {ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}