using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Entity
{
    public class AddressBookEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required, Phone, MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [EmailAddress, MaxLength(255)]
        public string? Email { get; set; }

        public string? Address { get; set; }


        [Required]
        [ForeignKey("User")] // Defines UserId as a foreign key to the User table
        public int UserId { get; set; }

        // Navigation property to establish the relationship
        public virtual User User { get; set; }
    }
}
