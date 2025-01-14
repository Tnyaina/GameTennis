using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GameTennis.Models
{
    public class Paddle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Radius { get; set; }
        public float Time { get; set; }
        public float Speed { get; set; }
        public TrajectoryType CurrentTrajectory { get; set; }
        public Rectangle Bounds { get; set; }
        public int Points { get; set; } // Points que vaut cette raquette
        private float originalX;
        private bool isRightPaddle;
        private bool isFrontPaddle;

        public Paddle(int x, int y, Rectangle bounds, bool isRight, bool isFront)
        {
            X = x;
            originalX = x;
            Y = y;
            Radius = 20;
            Time = 0;
            Speed = 0.05f;
            CurrentTrajectory = TrajectoryType.Linear;
            Bounds = bounds;
            isRightPaddle = isRight;
            isFrontPaddle = isFront;
            Points = 1; // Par défaut, une raquette vaut 1 point
        }

        public void Move()
        {
            Time += Speed;
            float amplitude = Bounds.Height / 6;

            switch (CurrentTrajectory)
            {
                case TrajectoryType.Linear:
                    Y = Bounds.Top + (Bounds.Height / 2) + (float)(Math.Sin(Time) * amplitude);
                    X = originalX;
                    break;

                case TrajectoryType.Circular:
                    Y = Bounds.Top + (Bounds.Height / 2) + (float)(Math.Sin(Time) * amplitude);
                    X = originalX + (float)(Math.Cos(Time) * (amplitude * 0.75f));
                    break;

                case TrajectoryType.SemiCircular:
                    if (isRightPaddle)
                    {
                        Y = Bounds.Top + (Bounds.Height / 2) + (float)(Math.Sin(Time) * amplitude);
                        X = originalX - Math.Abs((float)(Math.Cos(Time) * (amplitude * 0.75f)));
                    }
                    else
                    {
                        Y = Bounds.Top + (Bounds.Height / 2) + (float)(Math.Sin(Time) * amplitude);
                        X = originalX + Math.Abs((float)(Math.Cos(Time) * (amplitude * 0.75f)));
                    }
                    break;

                case TrajectoryType.Sinusoidal:
                    Y = Bounds.Top + (Bounds.Height / 2) + (float)(Math.Sin(2 * Time) * amplitude);
                    X = originalX + (float)(Math.Sin(Time) * (amplitude * 0.4f));
                    break;
            }

            Y = Math.Max(Bounds.Top + Radius, Math.Min(Y, Bounds.Bottom - Radius));
            X = Math.Max(Bounds.Left + Radius, Math.Min(X, Bounds.Right - Radius));
        }

        public void Draw(Graphics g)
        {
            using (Pen trajectoryPen = new Pen(Color.White, 1))
            {
                trajectoryPen.DashStyle = DashStyle.Dot;
                float amplitude = Bounds.Height / 6;

                switch (CurrentTrajectory)
                {
                    case TrajectoryType.Linear:
                        g.DrawLine(trajectoryPen, 
                            originalX, 
                            Bounds.Top + (Bounds.Height / 2) - amplitude,
                            originalX, 
                            Bounds.Top + (Bounds.Height / 2) + amplitude);
                        break;

                    case TrajectoryType.Circular:
                        g.DrawEllipse(trajectoryPen,
                            originalX - (amplitude * 0.75f),
                            Bounds.Top + (Bounds.Height / 2) - amplitude,
                            amplitude * 1.5f,
                            amplitude * 2);
                        break;

                    case TrajectoryType.SemiCircular:
                        if (isRightPaddle)
                        {
                            g.DrawArc(trajectoryPen,
                                originalX - (amplitude * 0.75f),
                                Bounds.Top + (Bounds.Height / 2) - amplitude,
                                amplitude * 1.5f,
                                amplitude * 2,
                                -90,
                                180);
                        }
                        else
                        {
                            g.DrawArc(trajectoryPen,
                                originalX - (amplitude * 0.75f),
                                Bounds.Top + (Bounds.Height / 2) - amplitude,
                                amplitude * 1.5f,
                                amplitude * 2,
                                90,
                                180);
                        }
                        break;

                    case TrajectoryType.Sinusoidal:
                        Point[] points = new Point[100];
                        for (int i = 0; i < points.Length; i++)
                        {
                            float t = (float)i / points.Length * MathF.PI * 2;
                            points[i] = new Point(
                                (int)(originalX + Math.Sin(t) * (amplitude * 0.4f)),
                                (int)(Bounds.Top + (Bounds.Height / 2) + Math.Sin(2 * t) * amplitude)
                            );
                        }
                        g.DrawCurve(trajectoryPen, points);
                        break;
                }
            }

            // Dessiner le nombre de points sur la raquette
            g.FillEllipse(Brushes.White, X - Radius, Y - Radius, Radius * 2, Radius * 2);
            using (Font pointFont = new Font("Arial", 12, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(Points.ToString(), pointFont, Brushes.Black, new RectangleF(X - Radius, Y - Radius, Radius * 2, Radius * 2), sf);
            }
        }
    }

    public enum TrajectoryType
    {
        Linear,
        Circular,
        SemiCircular,
        Sinusoidal
    }
}