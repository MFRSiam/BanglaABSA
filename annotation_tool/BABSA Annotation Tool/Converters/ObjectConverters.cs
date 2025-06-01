using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace BABSA_Annotation_Tool.Converters
{
    public class ObjectConverters
    {
        public static readonly IValueConverter IsNotNull = new IsNotNullConverter();
        public static readonly IValueConverter IsNull = new IsNullConverter();
        public static readonly IValueConverter SentimentToBrush = new SentimentToBrushConverter();
    }

    public class IsNotNullConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SentimentToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string sentiment)
            {
                return sentiment.ToLowerInvariant() switch
                {
                    "positive" => new SolidColorBrush(Color.Parse("#10b981")), // Green
                    "negative" => new SolidColorBrush(Color.Parse("#ef4444")), // Red  
                    "neutral" => new SolidColorBrush(Color.Parse("#6b7280")), // Gray
                    "mixed" => new SolidColorBrush(Color.Parse("#f59e0b")), // Orange
                    _ => new SolidColorBrush(Color.Parse("#6366f1")) // Default blue
                };
            }
            return new SolidColorBrush(Color.Parse("#6366f1"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StringConverters
    {
        public static readonly IValueConverter Equals = new StringEqualsConverter();
    }

    public class StringEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string parameterString)
            {
                return string.Equals(stringValue, parameterString, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}