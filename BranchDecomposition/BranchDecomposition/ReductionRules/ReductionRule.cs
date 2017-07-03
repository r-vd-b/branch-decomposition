using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BranchDecomposition.ReductionRules
{
    abstract class ReductionRule
    {
        public abstract bool IsApplicable(Graph graph);
        public abstract Graph[] Apply(Graph graph);
    }
}
