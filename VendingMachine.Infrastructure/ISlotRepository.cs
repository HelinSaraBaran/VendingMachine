using System.Collections.Generic;

namespace VendingMachine.Infrastructure
{
    using VendingMachine.Domain;

    // interface - read and update slots
    public interface ISlotRepository
    {
        // returns all slots
        List<Slot> GetAll();

        // returns a slot by code 
        Slot GetByCode(string code);

        // saves changes on a slot 
        void Update(Slot slot);
    }
}
