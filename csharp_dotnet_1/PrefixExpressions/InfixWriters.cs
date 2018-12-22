using System;
using System.Text;

namespace PrefixExpressions
{
    /// <summary>
    /// Writes formulas in infix notation fully parenthesized.
    /// </summary>
    public class FullInfixWriter : IVisitor<string>
    {
        public string Visit(PlusOpNode visited)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(visited.LeftSon.Accept(this));
            sb.Append("+");
            sb.Append(visited.RightSon.Accept(this));
            sb.Append(")");
            return sb.ToString();
        }

        public string Visit(MinusOpNode visited)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(visited.LeftSon.Accept(this));
            sb.Append("-");
            sb.Append(visited.RightSon.Accept(this));
            sb.Append(")");
            return sb.ToString();

        }

        public string Visit(MultiplyOpNode visited)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(visited.LeftSon.Accept(this));
            sb.Append("*");
            sb.Append(visited.RightSon.Accept(this));
            sb.Append(")");
            return sb.ToString();

        }

        public string Visit(DivideOpNode visited)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(visited.LeftSon.Accept(this));
            sb.Append("/");
            sb.Append(visited.RightSon.Accept(this));
            sb.Append(")");
            return sb.ToString();
        }

        public string Visit(NegateOpNode visited)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append("-");
            sb.Append(visited.Son.Accept(this));
            sb.Append(")");
            return sb.ToString();
        }

        public string Visit(ConstantNode visited)
        {
            return visited.Value.ToString();
        }
    }

    /// <summary>
    /// Writes formulas in infix notation with minimum number of parentheses. 
    /// </summary>
    public class SimpleInfixWriter : IVisitor<string>
    {
        readonly IVisitor<int> priorityGetter = new OpPriorityFinder();
        
        public string Visit(PlusOpNode visited)
        {
            var sb = new StringBuilder();
            // + has lowest priority and is associative - no need for parentheses around either child
            sb.Append(visited.LeftSon.Accept(this)); 
            sb.Append("+");
            sb.Append(visited.RightSon.Accept(this));
            return sb.ToString();
        }

        public string Visit(MinusOpNode visited)
        {
            var sb = new StringBuilder();

            // - has lowest priority - no need for parentheses around left child
            sb.Append(visited.LeftSon.Accept(this));
            sb.Append("-");
            
            // - is not associative - parentheses needed if right child has same priority
            int thisPriority = visited.Accept(priorityGetter);
            int rightSonPriority = visited.RightSon.Accept(priorityGetter);
            
            if (rightSonPriority <= thisPriority) sb.Append("(");
            sb.Append(visited.RightSon.Accept(this));
            if (rightSonPriority <= thisPriority) sb.Append(")");

            return sb.ToString();
        }

        public string Visit(MultiplyOpNode visited)
        {
            var sb = new StringBuilder();

            // parentheses might be needed around children with lower priorities
            int thisPriority = visited.Accept(priorityGetter);
            int sonPriority = visited.LeftSon.Accept(priorityGetter);
            
            if (sonPriority < thisPriority) sb.Append("(");
            sb.Append(visited.LeftSon.Accept(this));
            if (sonPriority < thisPriority) sb.Append(")");
            
            sb.Append("*");
            sonPriority = visited.RightSon.Accept(priorityGetter);
            
            // * is associative - no need for parentheses when RightSon has same priority
            if (sonPriority < thisPriority) sb.Append("(");
            sb.Append(visited.RightSon.Accept(this));
            if (sonPriority < thisPriority) sb.Append(")");

            return sb.ToString();
        }

        public string Visit(DivideOpNode visited)
        {
            var sb = new StringBuilder();

            // parentheses might be needed around children with lower priorities
            int thisPriority = visited.Accept(priorityGetter);
            int sonPriority = visited.LeftSon.Accept(priorityGetter);
            
            if (sonPriority < thisPriority) sb.Append("(");
            sb.Append(visited.LeftSon.Accept(this));
            if (sonPriority < thisPriority) sb.Append(")");
            
            sb.Append("/");
            sonPriority = visited.RightSon.Accept(priorityGetter);
            
            // "/" is not associative - same priority right sons need parentheses
            if (sonPriority <= thisPriority) sb.Append("(");
            sb.Append(visited.RightSon.Accept(this));
            if (sonPriority <= thisPriority) sb.Append(")");

            return sb.ToString();
        }

        public string Visit(NegateOpNode visited)
        {
            var sb = new StringBuilder();

            sb.Append("-");

            int thisPriority = visited.Accept(priorityGetter);
            int sonPriority = visited.Son.Accept(priorityGetter);
            
            //everything except value nodes needs parentheses
            if (sonPriority < thisPriority) sb.Append("(");
            sb.Append(visited.Son.Accept(this));
            if (sonPriority < thisPriority) sb.Append(")");

            return sb.ToString();
        }

        public string Visit(ConstantNode visited)
        {
            return visited.Value.ToString();
        }
    }

    /// <summary>
    /// Gets the priority of operator nodes in a formula.
    /// </summary>
    public class OpPriorityFinder : IVisitor<int>
    {
        public int Visit(PlusOpNode visited)
        {
            return 0;
        }

        public int Visit(MinusOpNode visited)
        {
            return 0;
        }

        public int Visit(MultiplyOpNode visited)
        {
            return 5;
        }

        public int Visit(DivideOpNode visited)
        {
            return 5;
        }

        public int Visit(NegateOpNode visited)
        {
            return 10;
        }

        public int Visit(ConstantNode visited)
        {
            return 100;    // high value guarantees no parentheses around constants
        }
    }

}