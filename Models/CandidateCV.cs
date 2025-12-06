using System;
using System.ComponentModel.DataAnnotations;

namespace cv.deltareco.com.Models
{
    public class CandidateCV
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadedOn { get; set; }
    }
}
