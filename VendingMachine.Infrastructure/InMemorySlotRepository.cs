using System.Collections.Generic;

namespace VendingMachine.Infrastructure
{
    using VendingMachine.Domain;

    // in-memory implementation of slot repository
    public class InMemorySlotRepository : ISlotRepository
    {
        // list of slots in memory
        private List<Slot> slots;

        // constructor 
        public InMemorySlotRepository(List<Slot> initialSlots)
        {
            slots = initialSlots;
        }

        // returns all slots
        public List<Slot> GetAll()
        {
            return slots;
        }

        // find slot by code 
        public Slot GetByCode(string code)
        {
            // loop
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Code == code)
                {
                    return slots[i];
                }
            }
            return null;
        }

        // update slot 
        public void Update(Slot slot)
        {
            
        }
    }
}

