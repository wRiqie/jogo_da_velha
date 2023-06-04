using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jogo_da_velha
{
    public class WinProbability
    {
        public List<(int, int)> fields;

        public WinProbability(List<(int, int)> fields)
        {
            this.fields = fields;
        }
    }
}
