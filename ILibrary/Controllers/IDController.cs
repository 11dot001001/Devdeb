using System.Collections.Generic;

namespace ILibrary.Controllers
{
    public class IDController
    {
        private List<int> _idList;
        private int _capacity;
        public int CapacityFactor;

        public int Capacity { get => _capacity; set => _capacity = value; }
        public int Count => _idList.Count;

        public IDController() : this(1) { }
        public IDController(int capacity) { _idList = new List<int>(Capacity = capacity); CreateIdList(); }

        private void CreateIdList()
        {
            CapacityFactor = _capacity;
            for (int i = 0; i < _capacity; i++)
            {
                _idList.Add(i);
            }
        }

        private void ExpandCapacity(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _idList.Add(_capacity + i);
            }
            _capacity += count;
        }

        public int GetID()
        {
            if (_idList.Count == 1) ExpandCapacity(CapacityFactor);
            int id = _idList[0];
            _idList.RemoveAt(0);
            return id;
        }

        public void ReturnID(int value) => _idList.Add(value);
    }
}