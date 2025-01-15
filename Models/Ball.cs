using System;
using System.Drawing;

namespace GameTennis.Models
{
    public class Ball
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float SpeedX { get; set; }
        public float SpeedY { get; set; }
        public int Radius { get; set; }
        public Paddle? TargetPaddle { get; set; }

        public Ball(int x, int y)
        {
            X = x;
            Y = y;
            Radius = 10;
            ResetBall();
        }

        public void ResetBall()
        {
            if (TargetPaddle != null)
            {
                // Calculer la direction vers la raquette ciblée
                float dx = TargetPaddle.X - X;
                float dy = TargetPaddle.Y - Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                
                // Normaliser et appliquer la vitesse
                float speed = new Random().Next(5, 8);
                SpeedX = (dx / distance) * speed;
                SpeedY = (dy / distance) * speed;
                
                // Ajouter une légère variation aléatoire
                SpeedY += new Random().Next(-2, 3);
            }
            else
            {
                // Comportement original si aucune raquette n'est ciblée
                Random rand = new Random();
                SpeedX = rand.Next(5, 8) * (rand.Next(2) == 0 ? -1 : 1);
                SpeedY = rand.Next(5, 8) * (rand.Next(2) == 0 ? -1 : 1);
            }
            
            // Réinitialiser la cible après le lancement
            TargetPaddle = null;
        }

        public void Move(Rectangle bounds)
        {
            X += SpeedX;
            Y += SpeedY;

            // Collision avec les bords haut/bas
            if (Y - Radius <= bounds.Top)
            {
                Y = bounds.Top + Radius;
                SpeedY = Math.Abs(SpeedY);
            }
            else if (Y + Radius >= bounds.Bottom)
            {
                Y = bounds.Bottom - Radius;
                SpeedY = -Math.Abs(SpeedY);
            }

            // Collision avec les bords gauche/droite
            if (X - Radius <= bounds.Left)
            {
                X = bounds.Left + Radius;
                SpeedX = Math.Abs(SpeedX);
            }
            else if (X + Radius >= bounds.Right)
            {
                X = bounds.Right - Radius;
                SpeedX = -Math.Abs(SpeedX);
            }
        }

        public void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.White, X - Radius, Y - Radius, Radius * 2, Radius * 2);
        }
    }
}