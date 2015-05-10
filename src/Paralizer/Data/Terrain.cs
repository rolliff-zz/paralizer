using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paralizer.Data
{
    public class Terrain
    {
        public decimal movement_cost { get; set; }
        public decimal attack { get; set; }
        public decimal defence { get; set; }
        public int attrition { get; set; }
        public int precipitation { get; set; }
        public int temperature { get; set; }
        public List<int> color { get; set; }
    }
}
