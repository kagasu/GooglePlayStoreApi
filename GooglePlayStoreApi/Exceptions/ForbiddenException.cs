using System;
using System.ComponentModel;
using System.Linq;

namespace GooglePlayStoreApi
{
    public class ForbiddenException: Exception
    {
        public ForbiddenException(string str) : base(str) { }
    }
}
