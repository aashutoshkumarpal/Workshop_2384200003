using ModelLayer.Model;
using RepositoryLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interface
{
    public interface IAddressBookRL
    {
        IEnumerable<AddressBookEntry> GetAllContacts();
        AddressBookEntry GetContactById(int id);
        AddressBookEntry AddContact(AddressBookEntry contact);
        AddressBookEntry UpdateContact(int id, AddressBookEntry contact);
        bool DeleteContact(int id);
    }
}
