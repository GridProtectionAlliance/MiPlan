using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    public class MitigationPlanActionItemsView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public Guid CreateByID { get; set; }
        public int PlanID { get; set; }
        public int ActionTypeKey { get; set; }
        public DateTime ScheduledEndDate { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}