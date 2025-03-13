using AutoMapper;
using BusinessLayer.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using System.Collections.Generic;
using System.Linq;

namespace AddressBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;
        private readonly IValidator<RequestAddressBook> _validator;

        public AddressBookController(IAddressBookBL addressBookBL, IValidator<RequestAddressBook> validator)
        {
            _addressBookBL = addressBookBL;
            _validator = validator;
        }

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

        [HttpGet("{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> GetContactById(int id)
        {
            var contact = _addressBookBL.GetContactById(id);
            if (contact == null)
            {
                return NotFound(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = "Contact not found.",
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

        [HttpPost]
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

            return CreatedAtAction(nameof(GetContactById), new { id = newContact.Id }, new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact added successfully.",
                Data = newContact
            });
        }

        [HttpPut("{id}")]
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
                    Message = "Contact not found.",
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

        [HttpDelete("{id}")]
        public ActionResult<ResponseBody<string>> DeleteContact(int id)
        {
            var isDeleted = _addressBookBL.DeleteContact(id);
            if (!isDeleted)
            {
                return NotFound(new ResponseBody<string>
                {
                    Success = false,
                    Message = "Contact not found.",
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
