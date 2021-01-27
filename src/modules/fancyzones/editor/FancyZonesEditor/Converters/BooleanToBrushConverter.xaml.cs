﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FancyZonesEditor.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        private static readonly Brush _selectedBrush = Application.Current.FindResource("SystemControlBackgroundAccentBrush") as SolidColorBrush;
        private static readonly Brush _normalBrush = Application.Current.FindResource("LayoutItemBackgroundBrush") as SolidColorBrush;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ? _selectedBrush : _normalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == _selectedBrush;
        }
    }
}
