namespace cv.deltareco.com.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string TableName { get; set; }   // User, CandidateCV, CandidateProfile
        public string ActionType { get; set; }  // Create, Update, Delete
        public string RecordId { get; set; }    // जिस record पर action हुआ

        public string OldValues { get; set; }   // Before Update (JSON)
        public string NewValues { get; set; }   // After Update (JSON)

        public string PerformedBy { get; set; } // UserName or UserId
        public DateTime PerformedAt { get; set; }
    }

}
