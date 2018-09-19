using System;
using System.Collections.Generic;
using System.Text;

namespace ContainerTest
{
    public interface IRepo<T>
    {
        IEnumerable<T> GetAll();
    }
}
