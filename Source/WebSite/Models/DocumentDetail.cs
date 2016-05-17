using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    /// <summary>
    /// Model for openSPM.DocumentDetail view.
    /// </summary>
    [PrimaryLabel("Filename")]
    public class DocumentDetail
    {
        [PrimaryKey]
        public string SourceTable { get; set; }

        [PrimaryKey]
        public int SourceID { get; set; }

        [PrimaryKey]
        public int DocumentID { get; set; }

        [Required]
        [StringLength(200)]
        public string Filename { get; set; }

        [Label("Document Type")]
        public int? DocumentTypeKey { get; set; }

        [InitialValue("true")]
        public bool Enabled { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedByID { get; set; }
    }
}