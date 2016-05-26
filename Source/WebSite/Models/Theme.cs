using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    public class Theme
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Required]
        [StringLength(12)]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }

        [Required]
        public bool IsDefault { get; set; }
    }
}