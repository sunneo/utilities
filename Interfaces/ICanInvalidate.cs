using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Interfaces
{
    public interface ICanInvalidate<T>
    {
        T Value { get; set; }
        bool Invalidate();
    }
}
