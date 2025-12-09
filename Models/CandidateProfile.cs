namespace cv.deltareco.com.Models
{
    public class CandidateProfile 
    {
        public int Id { get; set; }

        public int CandidateCVId { get; set; }
        public CandidateCV CandidateCV { get; set; }

        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now; 
    }

}
