using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FF4_ModTools
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public ColorPicker()
        {
            InitializeComponent();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Canvas)sender).CaptureMouse();

            Point p = e.GetPosition((Canvas)sender);
            p.Offset(-6, -6);

            byte v = (byte)(p.X / 2 + p.Y > 128 ? 255 : 0);
            
            Canvas.SetTop(ColorSelection, p.Y);
            Canvas.SetLeft(ColorSelection, p.X);

            ColorSelection.Stroke.SetCurrentValue(SolidColorBrush.ColorProperty, Color.FromRgb(v, v, v));
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                ((Canvas)sender).ReleaseMouseCapture();
                return;
            }

            Point p = e.GetPosition((Canvas)sender);

            p.X = Math.Min(255, Math.Max(0, p.X)); // Clamp position
            p.Y = Math.Min(255, Math.Max(0, p.Y)); // Clamp position

            p.Offset(-6, -6); // Align center with mouse

            byte v = (byte)(p.X/2 + p.Y > 128 ? 255 : 0);

            Canvas.SetTop(ColorSelection, p.Y);
            Canvas.SetLeft(ColorSelection, p.X);

            ColorSelection.Stroke.SetCurrentValue(SolidColorBrush.ColorProperty,  Color.FromRgb(v, v, v));
        }
    }
}
