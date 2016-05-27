using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSF.Data.Model;

namespace MiPlan.Models
{
    [TableName("NoticeLogView")]
    public class NoticeLogView: NoticeLog
    {
        public string Title { get; set; }
    }
}