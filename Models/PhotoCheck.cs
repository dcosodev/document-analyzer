namespace ImageAnalysisAPI.Models
{
    public class PhotoCheck
    {
        public bool Genuine { get; set; }
        public bool Live { get; set; }
        public bool Modified { get; set; }

        public PhotoCheck(bool genuine, bool live, bool modified)
        {
            Genuine = genuine;
            Live = live;
            Modified = modified;
        }

        // Default constructor
        public PhotoCheck() { }
    }
}
