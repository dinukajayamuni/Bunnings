using System.Collections.Generic;
using System.Net;

namespace Bunnings.Api.Models
{
    /// <summary>
    /// Error response
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Error()
        {
            Title = "One or more error(s) occurred.";
            Errors = new List<string>();
        }

        /// <summary>
        /// Type of the error. i.e. AxError or ProjectApiError
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Title of the error
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Http status code of the error response
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Error messages. i.e. If Cosmos Db returns 
        /// </summary>
        public List<string> Errors { get; set; }
    }
}