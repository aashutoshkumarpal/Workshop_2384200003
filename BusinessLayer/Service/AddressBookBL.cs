using AutoMapper;
using BusinessLayer.Interface;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly IAddressBookRL _addressBookRL;
        private readonly IMapper _mapper;

        public AddressBookBL(IAddressBookRL addressBookRL, IMapper mapper)
        {
            _addressBookRL = addressBookRL;
            _mapper = mapper;
        }

        public IEnumerable<ResponseAddressBook> GetAllContacts()
        {
            try
            {
                var contacts = _addressBookRL.GetAllContacts();
                return _mapper.Map<IEnumerable<ResponseAddressBook>>(contacts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllContacts: {ex.Message}");
                return null;
            }
        }

        public ResponseAddressBook GetContactById(int id)
        {
            try
            {
                var contact = _addressBookRL.GetContactById(id);
                return contact == null ? null : _mapper.Map<ResponseAddressBook>(contact);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetContactById: {ex.Message}");
                return null;
            }
        }

        public ResponseAddressBook AddContact(RequestAddressBook contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact), "Contact data cannot be null.");
            }

            try
            {
                var entity = _mapper.Map<AddressBookEntry>(contact);
                var newContact = _addressBookRL.AddContact(entity);
                return _mapper.Map<ResponseAddressBook>(newContact);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddContact: {ex.Message}");
                return null;
            }
        }

        public ResponseAddressBook UpdateContact(int id, RequestAddressBook contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact), "Contact data cannot be null.");
            }

            try
            {
                var entity = _mapper.Map<AddressBookEntry>(contact);
                var updatedContact = _addressBookRL.UpdateContact(id, entity);
                return updatedContact == null ? null : _mapper.Map<ResponseAddressBook>(updatedContact);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateContact: {ex.Message}");
                return null;
            }
        }

        public bool DeleteContact(int id)
        {
            try
            {
                return _addressBookRL.DeleteContact(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteContact: {ex.Message}");
                return false;
            }
        }
    }
}
