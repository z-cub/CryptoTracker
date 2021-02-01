﻿using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CryptoTracker.Helpers {
    // #########################################################################################
    //  Object for plotting charts
    public class ChartData {
        public DateTime Date { get; set; }
        public float Value { get; set; }
        public float Low { get; set; }
        public float High { get; set; }
        public float Open { get; set; }
        public float Close { get; set; }
        public float Volume { get; set; }
        public string Category { get; set; }
        public Color Color { get; set; } = Color.FromArgb(255, 128, 128, 128);
    }
}
