using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParse
{
    internal class Product
    {
        public string GoodBad { get; set; }
        public DateTime DateTime { get; set; }
        public string Color { get; set; }
        public bool Processed { get; set; }
        public decimal PercentGood { get; set; }

    }
}
