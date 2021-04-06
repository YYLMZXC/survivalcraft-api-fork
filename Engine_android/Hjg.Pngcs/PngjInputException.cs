using System;

namespace Hjg.Pngcs
{
    internal class PngjInputException : PngjException
    {
        public PngjInputException(string message, Exception cause)
            : base(message, cause)
        {
        }

        public PngjInputException(string message)
            : base(message)
        {
        }

        public PngjInputException(Exception cause)
            : base(cause)
        {
        }
    }
}
