using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    [TableName("MitigationPlanActionItemsCompleted")]
    public class MitigationPlanActionItemsCompleted
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int ActionTypeKey { get; set; }

        public string Title { get; set; }
    }
}