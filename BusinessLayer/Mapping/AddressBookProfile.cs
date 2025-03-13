using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;
using ModelLayer.Model;

namespace BusinessLayer.Mapping
{
    public class AddressBookProfile : Profile
    {
        public AddressBookProfile()
        {

            CreateMap<RequestAddressBook, AddressBookEntry>();
            CreateMap<AddressBookEntry, ResponseAddressBook>();
        }
    }
}
