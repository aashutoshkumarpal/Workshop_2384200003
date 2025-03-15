using AutoMapper;
using BusinessLayer.Interface;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text.Json;
using NLog;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly IAddressBookRL _addressBookRL;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string CacheKey = "AllContacts";

        public AddressBookBL(IAddressBookRL addressBookRL, ICacheService cacheService, IMapper mapper)
        {
            _addressBookRL = addressBookRL ?? throw new ArgumentNullException(nameof(addressBookRL));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public IEnumerable<ResponseAddressBook> GetAllContacts()
        {
            try
            {
                // Attempt to retrieve from cache
                var cacheData = _cacheService.GetCache(CacheKey);
                if (!string.IsNullOrEmpty(cacheData))
                {
                    Console.WriteLine("Cache Hit! Returning from Cache.");
                    return JsonSerializer.Deserialize<IEnumerable<ResponseAddressBook>>(cacheData);
                }

                // Fetch from database
                var contacts = _addressBookRL.GetAllContacts();
                if (contacts == null) return new List<ResponseAddressBook>();

                var mappedContacts = _mapper.Map<IEnumerable<ResponseAddressBook>>(contacts);

                // Store in cache
                var serializedData = JsonSerializer.Serialize(mappedContacts);
                _cacheService.SetCache(CacheKey, serializedData, 10);

                return mappedContacts;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in GetAllContacts");
                return new List<ResponseAddressBook>();
            }
        }

        public ResponseAddressBook GetContactById(int id)
        {
            try
            {
                var contact = _addressBookRL.GetContactById(id);
                return contact != null ? _mapper.Map<ResponseAddressBook>(contact) : null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error retrieving contact with ID: {id}");
                return null;
            }
        }

        public ResponseAddressBook AddContact(RequestAddressBook contact)
        {
            if (contact == null) throw new ArgumentNullException(nameof(contact), "Contact data cannot be null.");

            try
            {
                var entity = _mapper.Map<AddressBookEntry>(contact);
                var newContact = _addressBookRL.AddContact(entity);

                _cacheService.RemoveCache(CacheKey); // Invalidate cache

                return _mapper.Map<ResponseAddressBook>(newContact);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding new contact");
                return null;
            }
        }

        public ResponseAddressBook UpdateContact(int id, RequestAddressBook contact)
        {
            if (contact == null) throw new ArgumentNullException(nameof(contact), "Contact data cannot be null.");

            try
            {
                var entity = _mapper.Map<AddressBookEntry>(contact);
                var updatedContact = _addressBookRL.UpdateContact(id, entity);

                if (updatedContact == null)
                {
                    return null; // Contact not found
                }

                _cacheService.RemoveCache(CacheKey); // Invalidate cache

                return _mapper.Map<ResponseAddressBook>(updatedContact);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error updating contact with ID: {id}");
                return null;
            }
        }

        public bool DeleteContact(int id)
        {
            try
            {
                bool isDeleted = _addressBookRL.DeleteContact(id);
                if (isDeleted)
                {
                    _cacheService.RemoveCache(CacheKey);
                }
                return isDeleted;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error deleting contact with ID: {id}");
                return false;
            }
        }
    }
}
