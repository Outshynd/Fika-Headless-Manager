using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FikaHeadlessManager.Models
{
    public record Settings
    {
        public string? ProfileId { get; set; }
        public Uri? BackendUrl { get; set; }
    }
}
