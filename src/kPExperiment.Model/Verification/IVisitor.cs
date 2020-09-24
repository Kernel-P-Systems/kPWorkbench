using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public interface IVisitor<TResult>
    {
        TResult Visit(UnaryProperty expression);

        TResult Visit(BinaryProperty expression);

        TResult Visit(NotProperty expression);

        TResult Visit(RelationalExpression expression);

        TResult Visit(BooleanExpression expression);

        TResult Visit(NotExpression expression);

        TResult Visit(ArithmeticExpression expression);

        TResult Visit(ObjectMultiplicity expression);

        TResult Visit(NumericLiteral expression);
    }
}
