using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace cv.deltareco.com.Models
{
    public class UploadCVViewModel
    {
        // Single OR Multiple CV files
        public List<IFormFile>? CVFiles { get; set; }
    }
}
