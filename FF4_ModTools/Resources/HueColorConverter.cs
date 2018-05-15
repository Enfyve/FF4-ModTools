using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FF4_ModTools.Resources
{
    [ValueConversion(typeof(double), typeof(Color))]
    public class HueColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var h = (double)value / 60.0;
            int i = (int)Math.Floor(h);

            double f = h - i; // Get fraction

            double j = (1 - f);

            byte R, G, B;

            switch (i)
            {
                case 0:
                    R = 255;
                    G = (byte)Math.Round(f * 255, MidpointRounding.AwayFromZero);
                    B = 0;
                    break;
                case 1:
                    R = (byte)Math.Round(j * 255, MidpointRounding.AwayFromZero);
                    G = 255;
                    B = 0;
                    break;
                case 2:
                    R = 0;
                    G = 255;
                    B = (byte)Math.Round(f * 255, MidpointRounding.AwayFromZero);
                    break;
                case 3:
                    R = 0;
                    G = (byte)Math.Round(j * 255, MidpointRounding.AwayFromZero);
                    B = 255;
                    break;
                case 4:
                    R = (byte)Math.Round(f * 255, MidpointRounding.AwayFromZero);
                    G = 0;
                    B = 255;
                    break;
                default: // 5
                    R = 255;
                    G = 0;
                    B = (byte)Math.Round(j * 255, MidpointRounding.AwayFromZero);
                    break;
            }

            return Color.FromRgb(R, G, B);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (Color)value;

            double R = color.R / 255d;
            double G = color.R / 255d;
            double B = color.R / 255d;

            double Hue;

            var max = Math.Max(R, Math.Max(G, B));
            var min = Math.Min(R, Math.Min(G, B));
            var delta = max - min;

            if ((int)delta == 0) // saturation is 0, so hue is irrelevant 
                return 0;

            if (max == R)
                Hue = (G - B) / delta;
            else if (max == G)
                Hue = 2 + (B - R) / delta;
            else
                Hue = 4 + (R - G) / delta;

            return Hue * 60;
        }
    }
}
