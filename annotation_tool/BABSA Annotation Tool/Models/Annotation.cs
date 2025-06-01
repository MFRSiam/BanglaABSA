using CommunityToolkit.Mvvm.ComponentModel;



namespace BABSA_Annotation_Tool.Models
{
    public partial class Annotation : ObservableObject
    {
        [ObservableProperty]
        private string _aspect;
        [ObservableProperty]
        private string _sentiment;

        public Annotation(string aspect, string sentiment)
        {
            Aspect = aspect;
            Sentiment = sentiment;
        }
    }
}
