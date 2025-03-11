using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;

namespace AddressBook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressBookController : ControllerBase
    {
        private static List<ResponseAddressBook> _contacts = new List<ResponseAddressBook>();
        private static int _nextId = 1;

        // GET: Fetch all contacts
        [HttpGet]
        public ActionResult<IEnumerable<ResponseAddressBook>> GetAllContacts()
        {
            return Ok(_contacts);
        }

        // GET: Fetch contact by ID
        [HttpGet("{id}")]
        public ActionResult<ResponseAddressBook> GetContactById(int id)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new { message = "Contact Not Found" });

            return Ok(contact);
        }

        // POST: Add new contact
        [HttpPost]
        public ActionResult<ResponseAddressBook> AddContact([FromBody] RequestAddressBook request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid Request Data" });

            var newContact = new ResponseAddressBook
            {
                Id = _nextId++,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Address = request.Address
            };

            _contacts.Add(newContact);
            return CreatedAtAction(nameof(GetContactById), new { id = newContact.Id }, newContact);
        }

        // PUT: Update contact
        [HttpPut("{id}")]
        public ActionResult<ResponseAddressBook> UpdateContact(int id, [FromBody] RequestAddressBook request)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new { message = "Contact Not Found" });

            contact.Name = request.Name;
            contact.PhoneNumber = request.PhoneNumber;
            contact.Email = request.Email;
            contact.Address = request.Address;

            return Ok(contact);
        }

        // DELETE: Delete contact
        [HttpDelete("{id}")]
        public ActionResult DeleteContact(int id)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new { message = "Contact Not Found" });

            _contacts.Remove(contact);
            return NoContent();
        }
    }
}

