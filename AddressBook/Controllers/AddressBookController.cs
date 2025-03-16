using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using BusinessLayer.Interface;
using FluentValidation;
using Middleware.RabbitMQ;
using ModelLayer.Model;
using System.Collections.Generic;
using System.Linq;

namespace AddressBook.Controllers
{
    /// <summary>
    /// API Controller for Address Book Management.
    /// Provides endpoints for CRUD operations on contact records.
    /// </summary>
    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;
        private readonly IValidator<RequestAddressBook> _validator;
        private readonly RabbitMqService _rabbitMqService;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the AddressBookController.
        /// Injects dependencies for business logic, validation, and RabbitMQ service.
        /// </summary>
        /// <param name="addressBookBL">Business logic layer interface.</param>
        /// <param name="validator">FluentValidation validator for request validation.</param>
        /// <param name="rabbitMqService">RabbitMQ service for message publishing.</param>
        public AddressBookController(IAddressBookBL addressBookBL, IValidator<RequestAddressBook> validator, RabbitMqService rabbitMqService)
        {
            _addressBookBL = addressBookBL;
            _validator = validator;
            _rabbitMqService = rabbitMqService;
        }

        /// <summary>
        /// Retrieves all contacts from the address book.
        /// </summary>
        /// <returns>A list of contacts with success status and message.</returns>
        [HttpGet]
        public ActionResult<ResponseBody<IEnumerable<ResponseAddressBook>>> GetAllContacts()
        {
            _logger.Info("Fetching all contacts.");
            var contacts = _addressBookBL.GetAllContacts();
            return Ok(new ResponseBody<IEnumerable<ResponseAddressBook>>
            {
                Success = true,
                Message = "Contacts retrieved successfully.",
                Data = contacts
            });
        }

        /// <summary>
        /// Retrieves a specific contact by ID.
        /// </summary>
        /// <param name="id">The ID of the contact.</param>
        /// <returns>The contact details if found, or an error message if not.</returns>
        [HttpGet("get/{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> GetContactById(int id)
        {
            _logger.Info($"Fetching contact with ID {id}");
            var contact = _addressBookBL.GetContactById(id);
            if (contact == null)
            {
                _logger.Warn($"Contact with ID {id} not found.");
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

        /// <summary>
        /// Adds a new contact to the address book.
        /// Validates input and sends a notification message to RabbitMQ.
        /// </summary>
        /// <param name="dto">Request data for the new contact.</param>
        /// <returns>The newly added contact with a success message.</returns>
        [HttpPost("add")]
        public ActionResult<ResponseBody<ResponseAddressBook>> AddContact([FromBody] RequestAddressBook dto)
        {
            _logger.Info($"Adding a new contact: {dto.Name}");

            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                _logger.Warn("Validation failed while adding a contact.");
                return BadRequest(new ResponseBody<object>
                {
                    Success = false,
                    Message = "Validation failed.",
                    Data = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var newContact = _addressBookBL.AddContact(dto);
            _rabbitMqService.PublishMessage($"New contact added: {newContact.Name}, {newContact.Email}");

            _logger.Info($"Contact {newContact.Id} added successfully.");
            return CreatedAtAction(nameof(GetContactById), new { id = newContact.Id }, new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact added successfully.",
                Data = newContact
            });
        }

        /// <summary>
        /// Updates an existing contact by ID.
        /// Validates input before updating.
        /// </summary>
        /// <param name="id">The ID of the contact to update.</param>
        /// <param name="dto">Updated contact data.</param>
        /// <returns>The updated contact or an error if not found.</returns>
        [HttpPut("update/{id}")]
        public ActionResult<ResponseBody<ResponseAddressBook>> UpdateContact(int id, [FromBody] RequestAddressBook dto)
        {
            _logger.Info($"Updating contact with ID {id}");

            var validationResult = _validator.Validate(dto);
            if (!validationResult.IsValid)
            {
                _logger.Warn($"Validation failed for contact update (ID {id}).");
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
                _logger.Warn($"Contact with ID {id} not found for update.");
                return NotFound(new ResponseBody<ResponseAddressBook>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found.",
                    Data = null
                });
            }

            _logger.Info($"Contact with ID {id} updated successfully.");
            return Ok(new ResponseBody<ResponseAddressBook>
            {
                Success = true,
                Message = "Contact updated successfully.",
                Data = updatedContact
            });
        }

        /// <summary>
        /// Deletes a contact by ID.
        /// </summary>
        /// <param name="id">The ID of the contact to delete.</param>
        /// <returns>A success message if deleted, or an error if not found.</returns>
        [HttpDelete("delete/{id}")]
        public ActionResult<ResponseBody<string>> DeleteContact(int id)
        {
            _logger.Info($"Attempting to delete contact with ID {id}");
            var isDeleted = _addressBookBL.DeleteContact(id);
            if (!isDeleted)
            {
                _logger.Warn($"Contact with ID {id} not found.");
                return NotFound(new ResponseBody<string>
                {
                    Success = false,
                    Message = $"Contact with ID {id} not found.",
                    Data = null
                });
            }

            _logger.Info($"Contact with ID {id} deleted successfully.");
            return Ok(new ResponseBody<string>
            {
                Success = true,
                Message = "Contact deleted successfully.",
                Data = "Deleted"
            });
        }
    }
}
