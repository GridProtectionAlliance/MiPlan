using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    [TableName("MitigationPlanDocument")]
    public class MitigationPlanDocument
    {
        public int MitigationPlanID { get; set; }
        public int DocumentID { get; set; }
    }
}