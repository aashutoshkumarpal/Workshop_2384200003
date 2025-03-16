using BusinessLayer.Interface;
using ModelLayer.Model;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Testing.TestClasses
{
    public class AddressBookTests
    {
        private Mock<IAddressBookBL> _addressBookMock;

        [SetUp]
        public void Setup()
        {
            _addressBookMock = new Mock<IAddressBookBL>();
        }

        [Test]
        public void AddContact_ValidInput_ReturnsResponse()
        {
            // Arrange
            var request = new RequestAddressBook
            {
                UserId = 1,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                Address = "123 Main St"
            };

            var response = new ResponseAddressBook
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                Address = "123 Main St"
            };

            _addressBookMock.Setup(service => service.AddContact(request))
                            .Returns(response);

            // Act
            var result = _addressBookMock.Object.AddContact(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(response.Name));
        }

        [Test]
        public void GetAllContacts_ReturnsContactList()
        {
            // Arrange
            var contacts = new List<ResponseAddressBook>
            {
                new ResponseAddressBook { Id = 1, Name = "John Doe", Email = "john@example.com", PhoneNumber = "1234567890", Address = "123 Main St" },
                new ResponseAddressBook { Id = 2, Name = "Jane Doe", Email = "jane@example.com", PhoneNumber = "0987654321", Address = "456 Oak St" }
            };

            _addressBookMock.Setup(service => service.GetAllContacts())
                            .Returns(contacts);

            // Act
            var result = _addressBookMock.Object.GetAllContacts();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetContactById_ValidId_ReturnsContact()
        {
            // Arrange
            var contact = new ResponseAddressBook
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                Address = "123 Main St"
            };

            _addressBookMock.Setup(service => service.GetContactById(1))
                            .Returns(contact);

            // Act
            var result = _addressBookMock.Object.GetContactById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void GetContactById_InvalidId_ReturnsNull()
        {
            // Arrange
            _addressBookMock.Setup(service => service.GetContactById(99))
                            .Returns((ResponseAddressBook?)null);

            // Act
            var result = _addressBookMock.Object.GetContactById(99);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void UpdateContact_ValidInput_ReturnsUpdatedContact()
        {
            // Arrange
            var request = new RequestAddressBook
            {
                UserId = 1,
                Name = "John Updated",
                Email = "john.updated@example.com",
                PhoneNumber = "9999999999",
                Address = "789 New St"
            };

            var updatedContact = new ResponseAddressBook
            {
                Id = 1,
                Name = "John Updated",
                Email = "john.updated@example.com",
                PhoneNumber = "9999999999",
                Address = "789 New St"
            };

            _addressBookMock.Setup(service => service.UpdateContact(1, request))
                            .Returns(updatedContact);

            // Act
            var result = _addressBookMock.Object.UpdateContact(1, request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("John Updated"));
        }

        [Test]
        public void DeleteContact_ValidId_ReturnsTrue()
        {
            // Arrange
            _addressBookMock.Setup(service => service.DeleteContact(1))
                            .Returns(true);

            // Act
            var result = _addressBookMock.Object.DeleteContact(1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void DeleteContact_InvalidId_ReturnsFalse()
        {
            // Arrange
            _addressBookMock.Setup(service => service.DeleteContact(99))
                            .Returns(false);

            // Act
            var result = _addressBookMock.Object.DeleteContact(99);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
