using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace BABSA_Annotation_Tool.Models
{
    public partial class DataRowItem : ObservableObject
    {
        // Stores the original data for the row, column name as key
        [ObservableProperty]
        private Dictionary<string, string> _originalData;

        // Stores the annotations for this row
        [ObservableProperty]
        private ObservableCollection<Annotation> _annotations;

        // Property to easily display the text to be annotated (from the selected column)
        [ObservableProperty]
        private string _textToAnnotate;

        public DataRowItem(Dictionary<string, string> originalData)
        {
            OriginalData = originalData;
            Annotations = new ObservableCollection<Annotation>();
            TextToAnnotate = string.Empty; // Will be set when annotation column is chosen
        }
    }
}
