using AutoMapper;
using BusinessLayer.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using System.Collections.Generic;
using System.Linq;

namespace AddressBook.Controllers
{
    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookController : ControllerBase
    {
        private static List<AddressBookEntry> _contacts = new List<AddressBookEntry>();
        private static int _nextId = 1;
        private readonly IMapper _mapper;
        private readonly IValidator<RequestAddressBook> _validator;

        public AddressBookController(IMapper mapper, IValidator<RequestAddressBook> validator)
        {
            _mapper = mapper;
            _validator = validator;
        }

        // GET: Fetch all contacts
        [HttpGet]
        public ActionResult<ResponseBody<IEnumerable<ResponseAddressBook>>> GetAllContacts()
        {
            var response = _mapper.Map<IEnumerable<ResponseAddressBook>>(_contacts);
            return Ok(new ResponseBody<IEnumerable<ResponseAddressBook>>
            {
                Success = true,
                Message = "Contacts retrieved successfully",
                Data = response
            });
        }

        // GET: Fetch contact by ID
        [HttpGet("get/{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> GetContactById(int id)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found"
                });

            var response = _mapper.Map<ResponseAddressBook>(contact);
            return Ok(new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact retrieved successfully",
                Data = response
            });
        }

        // POST: Add new contact
        [HttpPost("add")]
        public ActionResult<ResponseBody<ResponseAddressBook>> AddContact([FromBody] RequestAddressBook request)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = "Validation failed",
                    Data = null
                });

            var newContact = _mapper.Map<AddressBookEntry>(request);
            newContact.Id = _nextId++;

            _contacts.Add(newContact);

            var response = _mapper.Map<ResponseAddressBook>(newContact);
            return CreatedAtAction(nameof(GetContactById), new { id = response.Id }, new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact added successfully",
                Data = response
            });
        }

        // PUT: Update contact
        [HttpPut("update/{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> UpdateContact(int id, [FromBody] RequestAddressBook request)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
                return BadRequest(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = "Validation failed"
                });

            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found"
                });

            _mapper.Map(request, contact);
            var response = _mapper.Map<ResponseAddressBook>(contact);

            return Ok(new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact updated successfully",
                Data = response
            });
        }

        // DELETE: Delete contact
        [HttpDelete("delete/{id}")]
        public ActionResult<ResponseBody<string>> DeleteContact(int id)
        {
            var contact = _contacts.Find(c => c.Id == id);
            if (contact == null)
                return NotFound(new ResponseBody<string>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found"
                });

            _contacts.Remove(contact);
            return Ok(new ResponseBody<string>
            {
                Success = true,
                Message = "Contact deleted successfully",
                Data = $"Deleted contact ID: {id}"
            });
        }
    }
}
