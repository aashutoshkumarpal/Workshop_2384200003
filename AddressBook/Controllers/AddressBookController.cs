using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;

namespace AddressBook.Controllers
{
    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookController : ControllerBase
    {
        private static List<ResponseAddressBook> _contacts = new List<ResponseAddressBook>();
        private static int _nextId = 1;

        // GET: Fetch all contacts
        [HttpGet]
        public ActionResult<IEnumerable<ResponseAddressBook>> GetAllContacts()
        {
            if (_contacts.Count == 0)
                return NotFound(new { message = "No contacts found" });

            return Ok(new { message = "Contacts retrieved successfully", data = _contacts });
        }

        // GET: Fetch contact by ID
        [HttpGet("get/{id}")]
        public ActionResult<ResponseAddressBook> GetContactById(int id)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new { message = $"Contact with ID {id} not found" });

            return Ok(new { message = "Contact retrieved successfully", data = contact });
        }

        // POST: Add new contact
        [HttpPost("add")]
        public ActionResult<ResponseAddressBook> AddContact([FromBody] RequestAddressBook request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request data" });

            var newContact = new ResponseAddressBook
            {
                Id = _nextId++,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Address = request.Address
            };

            _contacts.Add(newContact);
            return CreatedAtAction(nameof(GetContactById), new { id = newContact.Id },
                new { message = "Contact added successfully", data = newContact });
        }

        // PUT: Update contact
        [HttpPut("update/{id}")]
        public ActionResult<ResponseAddressBook> UpdateContact(int id, [FromBody] RequestAddressBook request)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new { message = $"Contact with ID {id} not found" });

            contact.Name = request.Name;
            contact.PhoneNumber = request.PhoneNumber;
            contact.Email = request.Email;
            contact.Address = request.Address;

            return Ok(new { message = "Contact updated successfully", data = contact });
        }

        // DELETE: Delete contact
        [HttpDelete("delete/{id}")]
        public ActionResult DeleteContact(int id)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new { message = $"Contact with ID {id} not found" });

            _contacts.Remove(contact);
            return Ok(new { message = "Contact deleted successfully" });
        }
    }
}

