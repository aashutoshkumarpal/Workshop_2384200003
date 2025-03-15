using AutoMapper;
using BusinessLayer.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Middleware.RabbitMQ;
using ModelLayer.Model;
using System.Collections.Generic;
using System.Linq;

namespace AddressBook.Controllers
{
    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;
        private readonly IValidator<RequestAddressBook> _validator;
        private readonly RabbitMqService _rabbitMqService;

        public AddressBookController(IAddressBookBL addressBookBL,IValidator<RequestAddressBook> validator,RabbitMqService rabbitMqService)
        {
            _addressBookBL = addressBookBL;
            _validator = validator;
            _rabbitMqService = rabbitMqService;
        }

        // GET: Fetch all contacts
        [HttpGet]
        public ActionResult<ResponseBody<IEnumerable<ResponseAddressBook>>> GetAllContacts()
        {
            var contacts = _addressBookBL.GetAllContacts();
            return Ok(new ResponseBody<IEnumerable<ResponseAddressBook>>
            {
                Success = true,
                Message = "Contacts retrieved successfully.",
                Data = contacts
            });
        }

        // GET: Fetch contact by ID
        [HttpGet("get/{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> GetContactById(int id)
        {
            var contact = _addressBookBL.GetContactById(id);
            if (contact == null)
            {
                return NotFound(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found.",
                    Data = null
                });
            }

            return Ok(new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact retrieved successfully.",
                Data = contact
            });
        }

        // POST: Add new contact (Sends a message to RabbitMQ)
        [HttpPost("add")]
        public ActionResult<ResponseBody<ResponseAddressBook>> AddContact([FromBody] RequestAddressBook dto)
        {
            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ResponseBody<object>
                {
                    Success = false,
                    Message = "Validation failed.",
                    Data = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var newContact = _addressBookBL.AddContact(dto);

            // Send message to RabbitMQ
            _rabbitMqService.PublishMessage($"New contact added: {newContact.Name}, {newContact.Email}, {newContact.PhoneNumber}");

            return CreatedAtAction(nameof(GetContactById), new { id = newContact.Id }, new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact added successfully.",
                Data = newContact
            });
        }

        // PUT: Update contact
        [HttpPut("update/{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> UpdateContact(int id, [FromBody] RequestAddressBook dto)
        {
            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ResponseBody<object>
                {
                    Success = false,
                    Message = "Validation failed.",
                    Data = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var updatedContact = _addressBookBL.UpdateContact(id, dto);
            if (updatedContact == null)
            {
                return NotFound(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found.",
                    Data = null
                });
            }

            return Ok(new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact updated successfully.",
                Data = updatedContact
            });
        }

        // DELETE: Delete contact
        [HttpDelete("delete/{id}")]
        public ActionResult<ResponseBody<string>> DeleteContact(int id)
        {
            var isDeleted = _addressBookBL.DeleteContact(id);
            if (!isDeleted)
            {
                return NotFound(new ResponseBody<string>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found.",
                    Data = null
                });
            }

            return Ok(new ResponseBody<string>
            {
                Success = true,
                Message = "Contact deleted successfully.",
                Data = "Deleted"
            });
        }
    }
}
