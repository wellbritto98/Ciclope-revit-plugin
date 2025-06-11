using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTemplate.Models
{
    public class FilterRevitElements
    {
        public string Campo { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }
}