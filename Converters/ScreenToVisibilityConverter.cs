using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Snake.ViewModels;

namespace Snake.Converters
{
    /// <summary>
    /// Converter qui convertit un ScreenKind en Visibility selon un paramètre.
    /// </summary>
    public class ScreenToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Gérer les cas où value est null ou n'est pas un ScreenKind
            if (value == null)
            {
                Debug.WriteLine("ScreenToVisibilityConverter: value is null");
                return Visibility.Collapsed;
            }

            if (!(value is ScreenKind screen))
            {
                Debug.WriteLine($"ScreenToVisibilityConverter: value is not ScreenKind, it's {value.GetType().Name}");
                return Visibility.Collapsed;
            }

            // Gérer le paramètre (peut être string ou null)
            string? target = parameter?.ToString();
            if (string.IsNullOrEmpty(target))
            {
                Debug.WriteLine("ScreenToVisibilityConverter: parameter is null or empty");
                return Visibility.Collapsed;
            }

            // Comparer le ScreenKind avec le paramètre
            string screenName = screen.ToString();
            bool isMatch = string.Equals(screenName, target, StringComparison.OrdinalIgnoreCase);
            
            Debug.WriteLine($"ScreenToVisibilityConverter: screen={screenName}, target={target}, match={isMatch}");
            
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
