namespace cv.deltareco.com.ViewModels
{
    public class CandidateCreateViewModel
    {
        // Candidate Fields
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }

        // CV Upload
        public IFormFile CVFile { get; set; }
    }
}
