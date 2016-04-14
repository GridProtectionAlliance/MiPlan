using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    [System.Web.DynamicData.TableName("PlansActionCompletedView")]
    public class PlansActionCompletedView
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int BusinessUnitID { get; set; }
        public string Title { get; set; }
        public string Field1 { get; set; }
    }
}