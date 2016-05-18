//******************************************************************************************************
//  DataHub.cs - Gbtc
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
//  01/14/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GSF;
using GSF.Collections;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Reflection;
using GSF.Security;
using GSF.Security.Model;
using GSF.Web.Model;
using GSF.Web.Security;
using Microsoft.AspNet.SignalR;
using MiPlan.Models;

namespace MiPlan
{
    [AuthorizeHubRole]
    public class DataHub : Hub, IRecordOperationsHub
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private DataContext m_buContext;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public DataHub()
        {
            m_dataContext = new DataContext(exceptionHandler: MvcApplication.LogException);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="IRecordOperationsHub.RecordOperationsCache"/> for SignalR hub.
        /// </summary>
        public RecordOperationsCache RecordOperationsCache => s_recordOperationsCache;

        // Gets reference to MiPlan context, creating it if needed
        private DataContext BUContext => m_buContext ?? (m_buContext = new DataContext("businessUnitDB", exceptionHandler: MvcApplication.LogException));


        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataHub"/> object and optionally releases the managed resources.
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

        public override Task OnConnected()
        {
            // Store the current connection ID for this thread
            s_connectionID.Value = Context.ConnectionId;
            s_connectCount++;

            //MvcApplication.LogStatusMessage($"DataHub connect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {s_connectCount}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                s_connectCount--;
                //MvcApplication.LogStatusMessage($"DataHub disconnect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {s_connectCount}");
            }

            return base.OnDisconnected(stopCalled);
        }

        #endregion

        #region [ Static ]

        // Static Properties

        /// <summary>
        /// Gets the hub connection ID for the current thread.
        /// </summary>
        public static string CurrentConnectionID => s_connectionID.Value;

        // Static Fields
        private static volatile int s_connectCount;
        private static readonly ThreadLocal<string> s_connectionID = new ThreadLocal<string>();
        private static readonly RecordOperationsCache s_recordOperationsCache;

        /// <summary>
        /// Gets statically cached instance of <see cref="RecordOperationsCache"/> for <see cref="DataHub"/> instances.
        /// </summary>
        /// <returns>Statically cached instance of <see cref="RecordOperationsCache"/> for <see cref="DataHub"/> instances.</returns>
        public static RecordOperationsCache GetRecordOperationsCache() => s_recordOperationsCache;


        // Static Constructor
        static DataHub()
        {
            // Analyze and cache record operations of security hub
            s_recordOperationsCache = new RecordOperationsCache(typeof(DataHub));
        }

        #endregion

        // Client-side script functionality

        #region [ MitigationPlan Table Operations ]

        [RecordOperation(typeof(MitigationPlan), RecordOperation.QueryRecordCount)]
        public int QueryMitigationPlanCount(bool showDeleted, bool isCompleted, string filterText)
        {
            if (showDeleted)
                return m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsCompleted = {0}", isCompleted));

            return m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = {0}", isCompleted));
        }

        [RecordOperation(typeof(MitigationPlan), RecordOperation.QueryRecords)]
        public IEnumerable<MitigationPlan> QueryMitigationPlanes(bool showDeleted, bool isCompleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (showDeleted)
                return m_dataContext.Table<MitigationPlan>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsCompleted = {0}", isCompleted));

            return m_dataContext.Table<MitigationPlan>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND IsCompleted = {0}", isCompleted));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        public int GetLastMitigationPlanID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('MitigationPlan')") ?? 0;
        }


        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.DeleteRecord)]
        public void DeleteMitigationPlan(int id)
        {
            // For MitigationPlanes, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE MitigationPlan SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(MitigationPlan), RecordOperation.CreateNewRecord)]
        public MitigationPlan NewMitigationPlan()
        {
            return new MitigationPlan();
        }

        [AuthorizeHubRole("Administrator, Owner, Editor")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.AddNewRecord)]
        public void AddNewMitigationPlan(MitigationPlan record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.IsCompleted = false;
            m_dataContext.Table<MitigationPlan>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.UpdateRecord)]
        public void UpdateMitigationPlan(MitigationPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<MitigationPlan>().UpdateRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.UpdateRecord)]
        public void CompleteMitigationPlan(MitigationPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            record.IsCompleted = true;
            m_dataContext.Table<MitigationPlan>().UpdateRecord(record);
        }

        #endregion

        #region [ ActionItem Table Operations ]


        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(ActionItem), RecordOperation.QueryRecordCount)]
        public int QueryActionItemCount(int parentID, string filterText)
        {
            return m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("PlanID = {0}", parentID));
        }

        [AuthorizeHubRole("*")]
        public IEnumerable<ActionItem> QueryActionItems(int parentID)
        {
            return m_dataContext.Table<ActionItem>().QueryRecords(restriction: new RecordRestriction("PlanID = {0}", parentID));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(ActionItem), RecordOperation.QueryRecords)]
        public IEnumerable<ActionItem> QueryActionItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<ActionItem>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("PlanID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        public int GetLastActionItemID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('ActionItem')") ?? 0;
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ActionItem), RecordOperation.DeleteRecord)]
        public void DeleteActionItem(int id)
        {
            m_dataContext.Table<ActionItem>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ActionItem), RecordOperation.CreateNewRecord)]
        public ActionItem NewActionItem()
        {
            return new ActionItem();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ActionItem), RecordOperation.AddNewRecord)]
        public void AddNewActionItem(ActionItem record)
        {
            record.CreatedOn = DateTime.UtcNow;
            record.CreatedByID = GetCurrentUserID();
            record.UpdatedOn = record.CreatedOn;
            record.UpdatedByID = record.CreatedByID;
            record.ActionTypeKey = 1;
            m_dataContext.Table<ActionItem>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ActionItem), RecordOperation.UpdateRecord)]
        public void UpdateActionItem(ActionItem record)
        {
            record.UpdatedOn = DateTime.UtcNow;
            record.UpdatedByID = GetCurrentUserID();
        
            m_dataContext.Table<ActionItem>().UpdateRecord(record);
        }

        #endregion

        #region [ NoticeLog Table Operations ]


        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.QueryRecordCount)]
        public int QueryNoticeLogCount(string filterText)
        {
            return m_dataContext.Table<NoticeLog>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        public IEnumerable<NoticeLog> QueryNoticeLogs()
        {
            return m_dataContext.Table<NoticeLog>().QueryRecords();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.QueryRecords)]
        public IEnumerable<NoticeLog> QueryNoticeLogs(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<NoticeLog>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.DeleteRecord)]
        public void DeleteNoticeLog(int id)
        {
            m_dataContext.Table<NoticeLog>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.CreateNewRecord)]
        public NoticeLog NewNoticeLog()
        {
            return new NoticeLog();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.AddNewRecord)]
        public void AddNewNoticeLog(NoticeLog record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<NoticeLog>().AddNewRecord(record);
        }

        #endregion

        #region [ BusinessUnitGroup Table Operations ]

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecordCount)]
        public int QueryBusinessUnitGroupCount(bool showDeleted, string filterText)
        {
            if (showDeleted)
                return BUContext.Table<BusinessUnit>().QueryRecordCount();

            return BUContext.Table<BusinessUnit>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecords)]
        public IEnumerable<BusinessUnit> QueryBusinessUnitGroups(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (showDeleted)
                return BUContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize);

            return BUContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        public IEnumerable<BusinessUnit> QueryBusinessUnits()
        {
            return BUContext.Table<BusinessUnit>().QueryRecords(restriction: new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.DeleteRecord)]
        public void DeleteBusinessUnitGroup(int id)
        {
            // For BusinessUnitGroups, we only "mark" a record as deleted
            BUContext.Connection.ExecuteNonQuery("UPDATE BusinessUnitGroup SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(BusinessUnit), RecordOperation.CreateNewRecord)]
        public BusinessUnit NewBusinessUnitGroup()
        {
            return new BusinessUnit();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.AddNewRecord)]
        public void AddNewBusinessUnitGroup(BusinessUnit record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = DateTime.UtcNow;
            BUContext.Table<BusinessUnit>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.UpdateRecord)]
        public void UpdateBusinessUnitGroup(BusinessUnit record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            BUContext.Table<BusinessUnit>().UpdateRecord(record);
        }

        #endregion

        #region [ Page Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecordCount)]
        public int QueryPageCount(string filterText)
        {
            return m_dataContext.Table<Page>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecords)]
        public IEnumerable<Page> QueryPages(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<Page>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.DeleteRecord)]
        public void DeletePage(int id)
        {
            m_dataContext.Table<Page>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.CreateNewRecord)]
        public Page NewPage()
        {
            return new Page();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.AddNewRecord)]
        public void AddNewPage(Page record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Page>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.UpdateRecord)]
        public void UpdatePage(Page record)
        {
            m_dataContext.Table<Page>().UpdateRecord(record);
        }

        #endregion

        #region [ Menu Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.QueryRecordCount)]
        public int QueryMenuCount(string filterText)
        {
            return m_dataContext.Table<Menu>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.QueryRecords)]
        public IEnumerable<Menu> QueryMenus(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<Menu>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.DeleteRecord)]
        public void DeleteMenu(int id)
        {
            m_dataContext.Table<Menu>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.CreateNewRecord)]
        public Menu NewMenu()
        {
            return new Menu();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.AddNewRecord)]
        public void AddNewMenu(Menu record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Menu>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.UpdateRecord)]
        public void UpdateMenu(Menu record)
        {
            m_dataContext.Table<Menu>().UpdateRecord(record);
        }

        #endregion

        #region [ MenuItem Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecordCount)]
        public int QueryMenuItemCount(int parentID, string filterText)
        {
            return m_dataContext.Table<MenuItem>().QueryRecordCount(new RecordRestriction("MenuID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecords)]
        public IEnumerable<MenuItem> QueryMenuItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<MenuItem>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("MenuID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.DeleteRecord)]
        public void DeleteMenuItem(int id)
        {
            m_dataContext.Table<MenuItem>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.CreateNewRecord)]
        public MenuItem NewMenuItem()
        {
            return new MenuItem();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.AddNewRecord)]
        public void AddNewMenuItem(MenuItem record)
        {
            // TODO: MenuItem.Text is currently required in database, but empty should be allowed for spacer items
            if (string.IsNullOrEmpty(record.Text))
                record.Text = " ";

            m_dataContext.Table<MenuItem>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.UpdateRecord)]
        public void UpdateMenuItem(MenuItem record)
        {
            // TODO: MenuItem.Text is currently required in database, but empty should be allowed for spacer items
            if (string.IsNullOrEmpty(record.Text))
                record.Text = " ";

            m_dataContext.Table<MenuItem>().UpdateRecord(record);
        }

        #endregion

        #region [ ValueListGroup Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.QueryRecordCount)]
        public int QueryValueListGroupCount(string filterText)
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.QueryRecords)]
        public IEnumerable<ValueListGroup> QueryValueListGroups(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.DeleteRecord)]
        public void DeleteValueListGroup(int id)
        {
            m_dataContext.Table<ValueListGroup>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.CreateNewRecord)]
        public ValueListGroup NewValueListGroup()
        {
            return new ValueListGroup();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.AddNewRecord)]
        public void AddNewValueListGroup(ValueListGroup record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<ValueListGroup>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.UpdateRecord)]
        public void UpdateValueListGroup(ValueListGroup record)
        {
            m_dataContext.Table<ValueListGroup>().UpdateRecord(record);
        }

        #endregion

        #region [ ValueList Table Operations ]


        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecordCount)]
        public int QueryValueListCount(int parentID, string filterText)
        {
            return m_dataContext.Table<ValueList>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecords)]
        public IEnumerable<ValueList> QueryValueListItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<ValueList>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.DeleteRecord)]
        public void DeleteValueList(int id)
        {
            m_dataContext.Table<ValueList>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.CreateNewRecord)]
        public ValueList NewValueList()
        {
            return new ValueList();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.AddNewRecord)]
        public void AddNewValueList(ValueList record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<ValueList>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.UpdateRecord)]
        public void UpdateValueList(ValueList record)
        {
            m_dataContext.Table<ValueList>().UpdateRecord(record);
        }

        #endregion

        #region [ ThemeFields Table Operations ]


        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.QueryRecordCount)]
        public int QueryThemeFieldsCount(int parentID, string filterText)
        {
            return m_dataContext.Table<ThemeFields>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.QueryRecords)]
        public IEnumerable<ThemeFields> QueryThemeFieldsItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<ThemeFields>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.DeleteRecord)]
        public void DeleteThemeFields(int id)
        {
            m_dataContext.Table<ThemeFields>().DeleteRecord(id);
        }

     

        #endregion

        #region [ MitigationPlanActionItemsCompleted View Operations ]

        [RecordOperation(typeof(MitigationPlanActionItemsCompleted), RecordOperation.QueryRecordCount)]
        public int QueryMitigationPlanActionItemsCompletedCount(string filterText)
        {
            return m_dataContext.Table<MitigationPlanActionItemsCompleted>().QueryRecordCount();
        }

        [RecordOperation(typeof(MitigationPlanActionItemsCompleted), RecordOperation.QueryRecords)]
        public IEnumerable<MitigationPlanActionItemsCompleted> QueryMitigationPlanActionItemsCompleteds(string res, string filterText)
        {
            return m_dataContext.Table<MitigationPlanActionItemsCompleted>().QueryRecords(restriction: new RecordRestriction(res));
        }


        [RecordOperation(typeof(MitigationPlanActionItemsCompleted), RecordOperation.CreateNewRecord)]
        public MitigationPlanActionItemsCompleted NewMitigationPlanActionItemsCompleted()
        {
            return new MitigationPlanActionItemsCompleted();
        }


        #endregion

        #region [ PlansActionCompletedView View Operations ]

        [RecordOperation(typeof(PlansActionCompletedView), RecordOperation.QueryRecordCount)]
        public int QueryPlansActionCompletedViewCount(bool isDeleted, string filterText)
        {
            return m_dataContext.Table<PlansActionCompletedView>().QueryRecordCount();
        }

        [RecordOperation(typeof(PlansActionCompletedView), RecordOperation.QueryRecords)]
        public IEnumerable<PlansActionCompletedView> QueryPlansActionCompletedViews(bool isDeleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<PlansActionCompletedView>().QueryRecords(sortField, ascending, page, pageSize);
        }


        [RecordOperation(typeof(PlansActionCompletedView), RecordOperation.CreateNewRecord)]
        public PlansActionCompletedView NewPlansActionCompletedView()
        {
            return new PlansActionCompletedView();
        }


        #endregion

        #region [ MitigationPlanActionItemsView View Operations ]

        [RecordOperation(typeof(MitigationPlanActionItemsView), RecordOperation.QueryRecordCount)]
        public int QueryMitigationPlanActionItemsViewCount(string filterText)
        {
            return m_dataContext.Table<MitigationPlanActionItemsView>().QueryRecordCount();
        }

        [RecordOperation(typeof(MitigationPlanActionItemsView), RecordOperation.QueryRecords)]
        public IEnumerable<MitigationPlanActionItemsView> QueryMitigationPlanActionItemsViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<MitigationPlanActionItemsView>().QueryRecords(sortField, ascending, page, pageSize);
        }


        [RecordOperation(typeof(MitigationPlanActionItemsView), RecordOperation.CreateNewRecord)]
        public MitigationPlanActionItemsView NewMitigationPlanActionItemsView()
        {
            return new MitigationPlanActionItemsView();
        }


        #endregion

        #region [ Document Table Operations ]

        [RecordOperation(typeof(Document), RecordOperation.QueryRecordCount)]
        public int QueryDocumentCount(string filterText = "%")
        {
            return m_dataContext.Table<Document>().QueryRecordCount();
        }

        //[RecordOperation(typeof(Document), RecordOperation.QueryRecords)]
        //public IEnumerable<Document> QueryDocuments(int sourceID, string sourceField, string tableName)
        //{
        //    IEnumerable<int> documentIDs =
        //        m_dataContext.Connection.RetrieveData($"SELECT DocumentID FROM {tableName} WHERE {sourceField} = {{0}}", sourceID)
        //            .AsEnumerable()
        //            .Select(row => row.ConvertField<int>("DocumentID", 0));            

        //    return m_dataContext.Table<Document>().QueryRecords("Filename", new RecordRestriction($"ID IN ({string.Join(", ", documentIDs)})"));
        //}

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.DeleteRecord)]
        public void DeleteDocument(int id, string filterText = "%")
        {
            m_dataContext.Table<Document>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.CreateNewRecord)]
        public Document NewDocument()
        {
            return new Document();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.AddNewRecord)]
        public void AddNewDocument(Document record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Document>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.UpdateRecord)]
        public void UpdateDocument(Document record)
        {
            m_dataContext.Table<Document>().UpdateRecord(record);
        }

        #endregion

        #region [ DocumentDetail View Operations ]

        [RecordOperation(typeof(DocumentDetail), RecordOperation.QueryRecordCount)]
        public int QueryDocumentDetailCount(string sourceTable, int sourceID, string filterText)
        {
            return m_dataContext.Table<DocumentDetail>().QueryRecordCount(new RecordRestriction("SourceTable = {0} AND SourceID = {1}", sourceTable, sourceID));
        }

        [RecordOperation(typeof(DocumentDetail), RecordOperation.QueryRecords)]
        public IEnumerable<DocumentDetail> QueryDocumentDetailResults(string sourceTable, int sourceID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<DocumentDetail>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("SourceTable = {0} AND SourceID = {1}", sourceTable, sourceID));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.DeleteRecord)]
        public void DeleteDocumentDetail(string sourceTable, int sourceID, int documentID)
        {
            m_dataContext.Connection.ExecuteNonQuery($"DELETE FROM {sourceTable}Document WHERE {sourceTable}ID = {{0}} AND DocumentID = {{1}}", sourceID, documentID);
            m_dataContext.Table<Document>().DeleteRecord(documentID);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.CreateNewRecord)]
        public DocumentDetail NewDocumentDetail()
        {
            return new DocumentDetail();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.AddNewRecord)]
        public void AddNewDocumentDetail(DocumentDetail record)
        {
            // Stub function exists to assign rights to document related UI operations
            throw new NotImplementedException();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.UpdateRecord)]
        public void UpdateDocumentDetail(DocumentDetail record)
        {
            // Stub function exists to assign rights to document related UI operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ MitigationPlanDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC, Viewer")]
        [RecordOperation(typeof(MitigationPlanDocument), RecordOperation.UpdateRecord)]
        public void UpdatePatchDocument(MitigationPlanDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ ActionItemDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC, Viewer")]
        [RecordOperation(typeof(ActionItemDocument), RecordOperation.UpdateRecord)]
        public void UpdateActionItemDocument(ActionItemDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion


        #region [ Miscellaneous Hub Operations ]

        /// <summary>
        /// Gets page setting for specified page.
        /// </summary>
        /// <param name="pageID">ID of page record.</param>
        /// <param name="key">Setting key name.</param>
        /// <param name="defaultValue">Setting default value.</param>
        /// <returns>Page setting for specified page.</returns>
        public string GetPageSetting(int pageID, string key, string defaultValue)
        {
            Page page = m_dataContext.Table<Page>().LoadRecord(pageID);
            Dictionary<string, string> pageSettings = (page?.ServerConfiguration ?? "").ParseKeyValuePairs();
            AppModel model = MvcApplication.DefaultModel;
            return model.GetPageSetting(pageSettings, model.Global.PageDefaults, key, defaultValue);
        }

        /// <summary>
        /// Gets the absolute path for a virtual path, e.g., ~/Images/Menu
        /// </summary>
        /// <param name="path">Virtual path o convert to absolute path.</param>
        /// <returns>Absolute path for a virtual path.</returns>
        public string GetAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            return VirtualPathUtility.ToAbsolute(path);
        }

        /// <summary>
        /// Gets UserAccount table ID for current user.
        /// </summary>
        /// <returns>UserAccount.ID for current user.</returns>
        public Guid GetCurrentUserID()
        {
            Guid userID;
            AuthorizationCache.UserIDs.TryGetValue(Thread.CurrentPrincipal.Identity.Name, out userID);
            return userID;
        }

        #endregion
    }
}
