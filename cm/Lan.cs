using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cm
{
    internal class Lan
    {
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public Lan(string fileName, string fileExtension=null)
        {
            FileExtension = fileExtension.ToLower();
            FileName = fileName.ToLower();
        }
    }
}
