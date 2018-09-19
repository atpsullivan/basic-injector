using System;
using System.Collections.Generic;
using System.Text;

namespace ContainerTest
{
    public class TestRepo : IRepo<TestEntity>
    {
        public IEnumerable<TestEntity> GetAll()
        {
            return new List<TestEntity> { new TestEntity(1), new TestEntity(2) };
        }
    }
}
