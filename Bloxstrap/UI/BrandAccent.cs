using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Bloxstrap.UI
{
    /// <summary>
    /// Repaints the accent resources that Wpf.Ui derives from the Windows accent colour onto the
    /// BleakStrap purple. Only the hue moves: each shade keeps its own saturation, brightness and
    /// alpha, so the full set of Fluent variants stays coherent instead of collapsing into one flat
    /// colour.
    /// </summary>
    internal static class BrandAccent
    {
        // Hue of the brand purple, in degrees on the HSV wheel.
        private const double PurpleHue = 282.0;

        // Serious greys and text-on-accent colours carry no usable hue; rotating them would only
        // eat the contrast they exist to provide.
        private const double AchromaticSaturation = 0.08;

        public static void Apply()
        {
            Application? app = Application.Current;

            if (app is null)
                return;

            ResourceDictionary resources = app.Resources;
            List<object> accentKeys = new();

            // Snapshot first: ResourceDictionary throws if it is enumerated while being written to.
            foreach (object key in resources.Keys)
            {
                if (key is string name && name.Contains("Accent", StringComparison.OrdinalIgnoreCase))
                    accentKeys.Add(key);
            }

            foreach (object key in accentKeys)
            {
                switch (resources[key])
                {
                    case Color color:
                        resources[key] = Rotate(color);
                        break;

                    case SolidColorBrush brush:
                        SolidColorBrush repainted = new(Rotate(brush.Color));
                        repainted.Freeze();
                        resources[key] = repainted;
                        break;
                }
            }
        }

        private static Color Rotate(Color color)
        {
            if (color.A == 0)
                return color;

            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            if (max <= 0 || delta <= 0)
                return color;

            double saturation = delta / max;

            if (saturation < AchromaticSaturation)
                return color;

            return FromHsv(PurpleHue, saturation, max, color.A);
        }

        private static Color FromHsv(double hue, double saturation, double value, byte alpha)
        {
            double chroma = value * saturation;
            double secondary = chroma * (1 - Math.Abs(hue / 60.0 % 2 - 1));
            double offset = value - chroma;

            double r, g, b;

            if (hue < 60)
                (r, g, b) = (chroma, secondary, 0);
            else if (hue < 120)
                (r, g, b) = (secondary, chroma, 0);
            else if (hue < 180)
                (r, g, b) = (0, chroma, secondary);
            else if (hue < 240)
                (r, g, b) = (0, secondary, chroma);
            else if (hue < 300)
                (r, g, b) = (secondary, 0, chroma);
            else
                (r, g, b) = (chroma, 0, secondary);

            return Color.FromArgb(
                alpha,
                (byte)Math.Round((r + offset) * 255),
                (byte)Math.Round((g + offset) * 255),
                (byte)Math.Round((b + offset) * 255));
        }
    }
}
