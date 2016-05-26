//******************************************************************************************************
//  HomeController.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;
using GSF.Data.Model;
using GSF.Web.Model;
using GSF.Web.Security;
using MiPlan.Models;

namespace MiPlan.Controllers
{
    /// <summary>
    /// Represents a MVC controller for the site's main pages.
    /// </summary>
    [AuthorizeControllerRole]
    public class MainController : Controller
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private readonly AppModel m_appModel;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="MainController"/>.
        /// </summary>
        public MainController()
        {
            // Establish data context for the view
            m_dataContext = new DataContext(exceptionHandler: MvcApplication.LogException);
            ViewData.Add("DataContext", m_dataContext);

            // Set default model for pages used by layout
            m_appModel = new AppModel(m_dataContext);
            ViewData.Model = m_appModel;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MainController"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_dataContext?.Dispose();
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        public ActionResult Home()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Home", ViewBag);
            DateTime today = DateTime.Today.AddDays(1).AddMinutes(-1);
            DateTime yesterday = DateTime.Today.AddDays(1).AddMinutes(-1).AddDays(-1);
            DateTime lastWeek = DateTime.Today.AddDays(-7);
            DateTime lastTwoWeeks = DateTime.Today.AddDays(-14);
            DateTime begYear = new DateTime(DateTime.Today.Year, 1, 1);
            Guid userID = new DataHub().GetCurrentUserID();

            ViewBag.todaysOpenPlansCount = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 0 AND CreatedON BETWEEN {0} AND {1}", yesterday, today));
            ViewBag.weekOpenPlansCount = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 0 AND CreatedON BETWEEN {0} AND {1}", lastWeek, today));
            ViewBag.twoWeekOpenPlansCount = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 0 AND CreatedON BETWEEN {0} AND {1}", lastTwoWeeks, today));
            ViewBag.todaysCompletedPlansCount = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 1 AND CreatedON BETWEEN {0} AND {1}", yesterday, today));
            ViewBag.weekCompletedPlansCount = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 1 AND CreatedON BETWEEN {0} AND {1}", lastWeek, today));
            ViewBag.twoWeekCompletedPlansCount = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 1 AND CreatedON BETWEEN {0} AND {1}", lastTwoWeeks, today));

            ViewBag.YTDPlansStarted = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND CreatedON BETWEEN {0} AND {1}", begYear, today ));
            ViewBag.YTDActionsCompleted = m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("ActionTypeKey = 3 AND CreatedON BETWEEN {0} AND {1}", begYear, today));
            ViewBag.YTDPlansClosed = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND isCompleted = 1 AND CreatedON BETWEEN {0} AND {1}", begYear, today));

            ViewBag.YTDWarnings = m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, UpdatedOn, ScheduledEndDate) < 14 AND CreatedON BETWEEN {0} AND {1}", begYear, today));
            ViewBag.YTDAlarms = m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, UpdatedOn, ScheduledEndDate) < 7 AND CreatedON BETWEEN {0} AND {1}", begYear, today));
            ViewBag.YTDCriticalAlarms = m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, UpdatedOn, ScheduledEndDate) < 3 AND CreatedON BETWEEN {0} AND {1}", begYear, today));
            ViewBag.YTDLateActions = m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, UpdatedOn, ScheduledEndDate) <= 0 AND CreatedON BETWEEN {0} AND {1}", begYear, today));


            ViewBag.MyOpenPlans = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 0 AND CreatedByID = {0}", userID));
            ViewBag.MyOpenAction = m_dataContext.Table<MitigationPlanActionItemsView>().QueryRecordCount(new RecordRestriction("ActionTypeKey <> 3 AND CreatedByID = {0}", userID));
            ViewBag.MyLateAction = m_dataContext.Table<MitigationPlanActionItemsView>().QueryRecordCount(new RecordRestriction("ActionTypeKey <> 3 AND DATEDIFF(day, UpdatedOn, ScheduledEndDate)<= 0 AND CreatedByID = {0}", userID));

            ActionItem[] actions = m_dataContext.Table<ActionItem>().QueryRecords("ScheduledEndDate ASC", true, 1, 20, restriction: new RecordRestriction("DATEDIFF(day, UpdatedOn, ScheduledEndDate) < 14 AND ActionTypeKey <> 3")).ToArray();
            ViewBag.ActionsInAlarm = actions;
            MitigationPlan[] plans = m_dataContext.Table<MitigationPlan>().QueryRecords("Title", restriction: new RecordRestriction("DATEDIFF(day, CreatedOn, {0}) < 1", today)).ToArray();
            ViewBag.PlansAddedToday = plans;
            return View();
        }

        public ActionResult Patches()
        {
            m_appModel.ConfigureView<Patch>(Url.RequestContext, "Patches", ViewBag);
            return View();
        }

        public ActionResult DiscoverPatches()
        {
            m_appModel.ConfigureView<LatestVendorDiscoveryResult>(Url.RequestContext, "Check", ViewBag);
            return View();
        }

        public ActionResult Help()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Help", ViewBag);
            return View();
        }

        public ActionResult Contact()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Contact", ViewBag);
            ViewBag.Message = "Contacting the Grid Protection Alliance";
            return View();
        }

        public ActionResult DisplayPDF()
        {
            // Using route ID, i.e., /Main/DisplayPDF/{id}, as page name of PDF load
            string routeID = Url.RequestContext.RouteData.Values["id"] as string ?? "UndefinedPageName";
            m_appModel.ConfigureView(Url.RequestContext, routeID, ViewBag);

            return View();
        }

        public ActionResult Documents()
        {
            m_appModel.ConfigureView<DocumentDetail>(Url.RequestContext, "Documents", ViewBag);
            return View();
        }

        public ActionResult PageTemplate1()
        {
            m_appModel.ConfigureView(Url.RequestContext, "PageTemplate1", ViewBag);
            return View();
        }

        public ActionResult Install()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Install", ViewBag);
            return View();
        }

        public ActionResult Assess()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Assess", ViewBag);
            return View();
        }

        public ActionResult History()
        {
            m_appModel.ConfigureView<MitigationPlan>(Url.RequestContext, "History", ViewBag);
            return View();
        }

        public ActionResult MitigationPlan()
        {
            int themeID = m_dataContext.Connection.ExecuteScalar<int?>("SELECT ID FROM Theme WHERE IsDefault = 1") ?? 0;
            m_appModel.ConfigureView<MitigationPlan>(Url.RequestContext, "MitigationPlan", ViewBag);
            ThemeFields[] fields = m_dataContext.Table<ThemeFields>().QueryRecords("FieldName", new RecordRestriction("ThemeID = {0}", themeID)).ToArray();
            ViewBag.ThemeFields = fields;
            ViewBag.ThemeFieldCount = m_dataContext.Table<ThemeFields>().QueryRecordCount(new RecordRestriction("ThemeID = {0}", themeID));
            return View();
        }

        public ActionResult Notification()
        {
            m_appModel.ConfigureView<NoticeLog>(Url.RequestContext, "Notification", ViewBag);
            return View();
        }


        public ActionResult Done()
        {
            m_appModel.ConfigureView<PlansActionCompletedView>(Url.RequestContext, "Done", ViewBag);
            //ActionItem[] actions = m_dataContext.Table<ActionItem>().QueryRecords().ToArray();
            //ViewBag.ActionItems = actions;
            return View();
        }

        public ActionResult ActionItem()
        {
            m_appModel.ConfigureView<ActionItem>(Url.RequestContext, "ActionItem", ViewBag);
            return View();
        }

        public ActionResult PlanView()
        {
            m_appModel.ConfigureView<MitigationPlan>(Url.RequestContext, "PlanView", ViewBag);
            return View();
        }

        #endregion
    }
}