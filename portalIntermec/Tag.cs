using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace portalIntermec
{
    public class Tag
    {
        public int id { get; private set; }
        public string dataHora { get; private set; }
        public string tag { get; private set; }

        public Tag(int ID, string DataHora, string tTag)
        {
            this.id = ID;
            this.dataHora = DataHora;
            this.tag = tTag;
        }
    }
}
