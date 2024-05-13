using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EsData.Models
{
    public class EditableUserViewModel
    {
        public string Id { get; set; }
        public bool Admin { get; set; }
        public bool Worker { get; set; }
        public bool Premium { get; set; }
        public string FirstName { get; set; }
        public string SureName { get; set; }
        public string Password { get; set; }
        [Compare("Password")]

        public string PasswordConfirmation { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        


    }
}
