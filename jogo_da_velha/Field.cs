using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jogo_da_velha
{
    public class Field
    {
        public int column { get; set; }
        public int row{ get; set; }
        public EPlayer? owner { get; set; }

        public Field(int column, int row, EPlayer? owner) {
            this.column = column;
            this.row = row;
            this.owner = owner;
        }
    }
}
