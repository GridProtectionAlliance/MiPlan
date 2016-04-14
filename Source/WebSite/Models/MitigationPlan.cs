//******************************************************************************************************
//  MitigationPlan.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/27/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GSF.Data.Model;

namespace MiPlan.Models
{
    /// <summary>
    /// Model for MiPlan.Plan table.
    /// </summary>
    [PrimaryLabel("Summary")]
    [IsDeletedFlag("IsDeleted")]
    public class MitigationPlan
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [Required]
        [Label("Patch")]
        [StringLength(80)]
        public string Title { get; set; }

        [Required]
        public int ThemeID { get; set; }

        [Required]
        public int BusinessUnitID { get; set; }

        public int ForeignKey1 { get; set; }

        public int ForeignKey2 { get; set; }

        public int ForeignKey3 { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public string Field1 { get; set; }

        public string Field2 { get; set; }

        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Description { get; set; }

        public string StatusNotes { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedByID { get; set; }

        public DateTime UpdatedOn { get; set; }

        public Guid UpdatedByID { get; set; }

        public bool IsCompleted { get; set; }
    }
}
