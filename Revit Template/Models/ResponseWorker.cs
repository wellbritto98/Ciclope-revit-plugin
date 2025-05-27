using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplate.Models
{
    public class ResponseWorker
    {
        public string Method { get; set; }
        public string[] Args { get; set; } = new string[0];

    }
}
