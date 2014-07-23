using System;

namespace OpenFAST.Error
{
    class FastSocketClosedException : FastException
    {
        public FastSocketClosedException(Exception e) : base(e)
        {
            
        }
    }
}
