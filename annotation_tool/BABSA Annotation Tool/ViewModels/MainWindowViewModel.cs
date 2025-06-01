// --- START OF FILE MainWindowViewModel.cs ---

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using BABSA_Annotation_Tool.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BABSA_Annotation_Tool.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // Observable Properties with backing fields
        [ObservableProperty]
        private ObservableCollection<DataRowItem> dataRows = new();

        [ObservableProperty]
        private ObservableCollection<string> columnHeaders = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAnnotationCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveFileCommand))]
        private DataRowItem? selectedDataRow;

        [ObservableProperty]
        private string? selectedAnnotationColumnHeader;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAnnotationCommand))]
        private string newAspectText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddAnnotationCommand))]
        private string newSentimentText = string.Empty;

        [ObservableProperty]
        private string statusMessage = "Ready. Load a file to begin.";

        public ObservableCollection<string> AvailableSentiments { get; } = new()
        {
            "Positive", "Negative", "Neutral", "Mixed"
        };

        private string? _loadedFilePath;
        private readonly IStorageProvider? _storageProvider;

        static MainWindowViewModel()
        {
            // Set EPPlus license for version 8+ - MUST be set before any ExcelPackage usage
            ExcelPackage.License.SetNonCommercialPersonal("MFRSiam");
        }

        // Constructor for XAML Designer
        public MainWindowViewModel()
        {
            StatusMessage = "ViewModel created (designer or no storage provider).";
        }

        // Constructor for runtime use
        public MainWindowViewModel(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
            StatusMessage = "Ready. Load a file to begin.";
        }

        // Commands
        [RelayCommand]
        private async Task LoadFileAsync()
        {
            if (_storageProvider == null)
            {
                StatusMessage = "Error: Storage provider is not available.";
                return;
            }

            var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open XLSX or CSV File",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                    new FilePickerFileType("Excel Files") { Patterns = new[] { "*.xlsx" } },
                    new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
                }
            });

            if (files.Count == 1)
            {
                _loadedFilePath = files[0].TryGetLocalPath();
                if (string.IsNullOrEmpty(_loadedFilePath))
                {
                    StatusMessage = "Error: Could not get local path for the selected file.";
                    return;
                }

                StatusMessage = $"Loading {_loadedFilePath}...";
                try
                {
                    await ProcessFileAsync(_loadedFilePath);
                    StatusMessage = $"File loaded: {Path.GetFileName(_loadedFilePath)}. Select annotation column.";
                    // Notify that SaveFileCommand can execute state may have changed
                    SaveFileCommand.NotifyCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading file: {ex.Message}";
                    DataRows.Clear();
                    ColumnHeaders.Clear();
                    _loadedFilePath = null;
                    SaveFileCommand.NotifyCanExecuteChanged();
                }
            }
            else
            {
                StatusMessage = "File selection cancelled or no file selected.";
            }
        }

        private async Task ProcessFileAsync(string filePath)
        {
            DataRows.Clear();
            ColumnHeaders.Clear();

            var tempRows = new List<DataRowItem>();
            var tempHeaders = new List<string>();
            bool success = true;

            await Task.Run(() =>
            {
                try
                {
                    if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        using var package = new ExcelPackage(new FileInfo(filePath));
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null || worksheet.Dimension == null)
                        {
                            StatusMessage = "Excel file is empty or worksheet could not be read.";
                            success = false;
                            return;
                        }

                        // Read headers
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            tempHeaders.Add(worksheet.Cells[1, col].Text ?? $"Column{col}");
                        }

                        // Read data rows
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var originalData = new Dictionary<string, string>();
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                if (col - 1 < tempHeaders.Count)
                                {
                                    originalData[tempHeaders[col - 1]] = worksheet.Cells[row, col].Text ?? string.Empty;
                                }
                            }
                            if (originalData.Any()) tempRows.Add(new DataRowItem(originalData));
                        }
                    }
                    else if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                            MissingFieldFound = null,
                            HeaderValidated = null,
                        };
                        using var reader = new StreamReader(filePath, Encoding.UTF8);
                        using var csv = new CsvReader(reader, config);

                        csv.Read();
                        csv.ReadHeader();
                        if (csv.HeaderRecord != null)
                        {
                            tempHeaders.AddRange(csv.HeaderRecord);
                        }
                        else
                        {
                            StatusMessage = "CSV file does not have a header row or is empty.";
                            success = false;
                            return;
                        }

                        while (csv.Read())
                        {
                            var originalData = new Dictionary<string, string>();
                            foreach (var header in tempHeaders)
                            {
                                originalData[header] = csv.GetField(header) ?? string.Empty;
                            }
                            if (originalData.Any()) tempRows.Add(new DataRowItem(originalData));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in ProcessFileAsync: {ex}");
                    StatusMessage = $"Error processing file content: {ex.Message}";
                    success = false;
                }
            });

            if (!success)
            {
                DataRows.Clear();
                ColumnHeaders.Clear();
                return;
            }

            if (!tempHeaders.Any() && !tempRows.Any() && success)
            {
                StatusMessage = "No headers or data rows found in the file after processing.";
                return;
            }

            // Add data to observable collections
            foreach (var header in tempHeaders) ColumnHeaders.Add(header);
            foreach (var row in tempRows) DataRows.Add(row);

            if (ColumnHeaders.Any())
            {
                SelectedAnnotationColumnHeader = ColumnHeaders.First();
            }
            else if (!DataRows.Any())
            {
                StatusMessage = "No data or headers found in the file.";
            }
        }

        partial void OnSelectedAnnotationColumnHeaderChanged(string? value)
        {
            if (value != null)
            {
                foreach (var row in DataRows)
                {
                    row.TextToAnnotate = row.OriginalData.TryGetValue(value, out var text) ? text : "N/A";
                }
                StatusMessage = $"Annotation column set to: {value}";
            }
        }

        private bool CanAddAnnotation() =>
            SelectedDataRow != null &&
            !string.IsNullOrWhiteSpace(NewAspectText) &&
            !string.IsNullOrWhiteSpace(NewSentimentText);

        [RelayCommand(CanExecute = nameof(CanAddAnnotation))]
        private void AddAnnotation()
        {
            if (SelectedDataRow == null) return;

            SelectedDataRow.Annotations.Add(new Annotation(NewAspectText, NewSentimentText));

            // Clear the input fields
            NewAspectText = string.Empty;
            NewSentimentText = string.Empty;

            StatusMessage = $"Annotation added. Total annotations for this row: {SelectedDataRow.Annotations.Count}";
        }

        [RelayCommand]
        private void RemoveAnnotation(Annotation? annotationToRemove)
        {
            if (SelectedDataRow != null && annotationToRemove != null)
            {
                SelectedDataRow.Annotations.Remove(annotationToRemove);
                StatusMessage = $"Annotation removed. Remaining annotations: {SelectedDataRow.Annotations.Count}";
            }
        }

        private bool CanSaveFile() => DataRows.Any() && !string.IsNullOrEmpty(_loadedFilePath);

        [RelayCommand(CanExecute = nameof(CanSaveFile))]
        private async Task SaveFileAsync()
        {
            if (string.IsNullOrEmpty(_loadedFilePath))
            {
                StatusMessage = "No file loaded to save.";
                return;
            }

            string savePath = _loadedFilePath;
            StatusMessage = $"Saving to {savePath}...";

            try
            {
                await Task.Run(() =>
                {
                    const string annotatedAspectColName = "AnnotatedAspect";
                    const string annotatedSentimentColName = "AnnotatedSentiment";

                    if (savePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        using var package = new ExcelPackage(new FileInfo(savePath));
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            throw new InvalidOperationException("Worksheet not found for saving.");
                        }

                        // Find or create annotation columns
                        int aspectCol = FindOrCreateColumn(worksheet, annotatedAspectColName,
                            (worksheet.Dimension?.End.Column ?? 0) + 1);
                        int sentimentCol = FindOrCreateColumn(worksheet, annotatedSentimentColName,
                            (worksheet.Dimension?.End.Column ?? 0) + 1);

                        // Write annotations
                        for (int i = 0; i < DataRows.Count; i++)
                        {
                            var dataRow = DataRows[i];
                            int excelRow = i + 2; // Excel is 1-based, and row 1 has headers

                            // Join multiple annotations with semicolons
                            var aspects = dataRow.Annotations.Select(a => a.Aspect).Where(a => !string.IsNullOrEmpty(a));
                            var sentiments = dataRow.Annotations.Select(a => a.Sentiment).Where(s => !string.IsNullOrEmpty(s));

                            worksheet.Cells[excelRow, aspectCol].Value = string.Join("; ", aspects);
                            worksheet.Cells[excelRow, sentimentCol].Value = string.Join("; ", sentiments);
                        }

                        package.Save();
                    }
                    else if (savePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        var records = new List<IDictionary<string, object>>();
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true
                        };

                        var allHeaders = new List<string>(ColumnHeaders);
                        if (!allHeaders.Contains(annotatedAspectColName))
                            allHeaders.Add(annotatedAspectColName);
                        if (!allHeaders.Contains(annotatedSentimentColName))
                            allHeaders.Add(annotatedSentimentColName);

                        foreach (var dataRowItem in DataRows)
                        {
                            var record = new Dictionary<string, object>();
                            foreach (var header in allHeaders)
                            {
                                if (header == annotatedAspectColName)
                                {
                                    record[header] = string.Join("; ", dataRowItem.Annotations.Select(a => a.Aspect));
                                }
                                else if (header == annotatedSentimentColName)
                                {
                                    record[header] = string.Join("; ", dataRowItem.Annotations.Select(a => a.Sentiment));
                                }
                                else if (dataRowItem.OriginalData.TryGetValue(header, out var val))
                                {
                                    record[header] = val;
                                }
                                else
                                {
                                    record[header] = string.Empty;
                                }
                            }
                            records.Add(record);
                        }

                        using var writer = new StreamWriter(savePath, false, Encoding.UTF8);
                        using var csv = new CsvWriter(writer, config);

                        // Write headers
                        foreach (var header in allHeaders) csv.WriteField(header);
                        csv.NextRecord();

                        // Write data
                        foreach (var record in records)
                        {
                            foreach (var header in allHeaders)
                            {
                                csv.WriteField(record.TryGetValue(header, out var value) ? value : string.Empty);
                            }
                            csv.NextRecord();
                        }
                    }
                });

                StatusMessage = "File saved successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving file: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Save error: {ex}");
            }
        }

        private int FindOrCreateColumn(ExcelWorksheet worksheet, string columnName, int defaultCol)
        {
            // If worksheet is empty, create the column
            if (worksheet.Dimension == null)
            {
                worksheet.Cells[1, defaultCol].Value = columnName;
                return defaultCol;
            }

            // Search for existing column
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                if (worksheet.Cells[1, col].Text.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }

            // Column not found, create it
            int newColIndex = worksheet.Dimension.End.Column + 1;
            worksheet.Cells[1, newColIndex].Value = columnName;
            return newColIndex;
        }
    }
}
// --- END OF FILE MainWindowViewModel.cs ---