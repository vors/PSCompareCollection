namespace PSCompareCollection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Threading.Tasks;

    public class DiffObject
    {
        public string SideIndicator { get; set; }
        public object InputObject { get; set; }
    }
}
