using cv.deltareco.com.Models;

namespace cv.deltareco.com.ViewModels
{
    public class HomeViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? AvatarUrl { get; set; }
        // Counters
        public int? TotalUsers { get; set; }
        public int? TotalCandidateCVCounts { get; set; }
        public int? TotalCandidateCounts { get; set; }
        public int? RecentUploadedCVCounts { get; set; }
        public List<int> ProfileCounts { get; set; } = new List<int>();
        public List<int> CVCounts { get; set; } = new List<int>();
        public List<CandidateProfile> Profiles { get; set; }
        public List<ApplicationUser> User { get; set; }
    public List<CandidateCV> CandidateCVs { get; set; }
        public List<ActivityLog> ActivityLogs { get; set; }
}

}
