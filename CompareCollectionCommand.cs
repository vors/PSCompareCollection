using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace PSCompareCollection
{
    using System.Collections;

    [Cmdlet("Compare", "Collection", DefaultParameterSetName = "Diff")]
    public class CompareCollectionCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Diff", Position = 1)]
        public System.Object[] Left { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Diff", Position = 2)]
        public System.Object[] Right { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "Diff")]
        public SwitchParameter UseHash { get; set; }

        private struct Position
        {
            public int x;
            public int y;

            public Position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private enum StepType
        {
            None = 0,
            Left,
            Right,
            Same,
        }

        protected override void ProcessRecord()
        {
            int[] leftHash = null;
            int[] rightHash = null;
            if (this.UseHash)
            {
                leftHash = Left.Select(x => x.GetHashCode()).ToArray();
                rightHash = Right.Select(x => x.GetHashCode()).ToArray();
            }
            StepType[][] d = new StepType[Left.Length + 1][];
            LinkedList<Position> queue = new LinkedList<Position>();
            int i, j;
            queue.AddFirst(new Position(0, 0));
            for (i = 0; i <= Left.Length; i++)
            {
                d[i] = new StepType[Right.Length + 1];
            }
            d[0][0] = StepType.None;
            while (queue.Count > 0)
            {
                Position curPos = queue.First();
                queue.RemoveFirst();
                i = curPos.x;
                j = curPos.y;
                if (i == Left.Length && j == Right.Length)
                {
                    break;
                }
                if (j < Right.Length && 
                    d[i][j + 1] == StepType.None)
                {
                   d[i][j + 1] = StepType.Right;
                    queue.AddLast(new Position(i, j + 1));
                }
                if (i < Left.Length && 
                    d[i + 1][j] == StepType.None)
                {
                   d[i + 1][j] = StepType.Left;
                   queue.AddLast(new Position(i + 1, j));
                }
                if (i < Left.Length && 
                    j < Right.Length && 
                    d[i+1][j+1] == StepType.None && 
                    (!this.UseHash || leftHash[i] == rightHash[j]) && 
                    Left[i].Equals(Right[j]))
                {
                    d[i+1][j+1] = StepType.Same;
                    queue.AddFirst(new Position(i + 1, j + 1));
                }
            }
            i = Left.Length;
            j = Right.Length;
            var diffs = new LinkedList<DiffObject>();
            while (i > 0 || j > 0)
            {
                DiffObject diffObject = new DiffObject() {SideIndicator = StepToString(d[i][j])};
                switch (d[i][j])
                {
                    case StepType.Left:  { diffObject.InputObject = Left[--i]; break; }
                    case StepType.Right: { diffObject.InputObject = Right[--j]; break; }
                    case StepType.Same: { diffObject.InputObject = Right[--j]; --i; break; }
                    default: throw new ArgumentOutOfRangeException();
                }
                diffs.AddFirst(diffObject);
            }
            foreach (var diffObject in diffs)
            {
                WriteObject(diffObject);
            }
        }

        private string StepToString(StepType step)
        {
            switch (step)
            {
                case StepType.Left:  return "<=";
                case StepType.Right: return "=>";
                case StepType.Same:  return "==";
                default: throw new ArgumentOutOfRangeException("step");
            }
        }
    }
}
