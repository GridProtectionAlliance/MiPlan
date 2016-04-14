﻿//******************************************************************************************************
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

        // Static Constructor
        static DataHub()
        {
            // Analyze and cache record operations of security hub
            s_recordOperationsCache = new RecordOperationsCache(typeof(DataHub));
        }

        #endregion

        // Client-side script functionality

        #region [ Patch Table Operations ]

        [RecordOperation(typeof(Patch), RecordOperation.QueryRecordCount)]
        public int QueryPatchCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecordCount();

            return m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(Patch), RecordOperation.QueryRecords)]
        public IEnumerable<Patch> QueryPatches(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Patch), RecordOperation.DeleteRecord)]
        public void DeletePatch(int id)
        {
            // For Patches, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Patch SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Patch), RecordOperation.CreateNewRecord)]
        public Patch NewPatch()
        {
            return new Patch();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(Patch), RecordOperation.AddNewRecord)]
        public void AddNewPatch(Patch record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<Patch>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(Patch), RecordOperation.UpdateRecord)]
        public void UpdatePatch(Patch record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Patch>().UpdateRecord(record);
        }

        #endregion

        #region [ CompletedMitigationPlan Table Operations ]

        [RecordOperation(typeof(CompletedMitigationPlan), RecordOperation.QueryRecordCount)]
        public int QueryCompletedMitigationPlanCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<CompletedMitigationPlan>().QueryRecordCount(new RecordRestriction("IsCompleted = 1"));

            return m_dataContext.Table<CompletedMitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 1") );
        }

        [RecordOperation(typeof(CompletedMitigationPlan), RecordOperation.QueryRecords)]
        public IEnumerable<CompletedMitigationPlan> QueryCompletedMitigationPlanes(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<CompletedMitigationPlan>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<CompletedMitigationPlan>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND IsCompleted = 1"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(CompletedMitigationPlan), RecordOperation.DeleteRecord)]
        public void DeleteCompletedMitigationPlan(int id)
        {
            // For MitigationPlanes, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE CompletedMitigationPlan SET IsDeleted=1 WHERE ID={0}", id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(CompletedMitigationPlan), RecordOperation.UpdateRecord)]
        public void UpdateCompletedMitigationPlan(CompletedMitigationPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<CompletedMitigationPlan>().UpdateRecord(record);
        }

        #endregion

        #region [ UncompletedMitigationPlan Table Operations ]

        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.QueryRecordCount)]
        public int QueryUncompletedMitigationPlanCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<UncompletedMitigationPlan>().QueryRecordCount(new RecordRestriction("IsCompleted = 0"));

            return m_dataContext.Table<UncompletedMitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsCompleted = 0"));
        }

        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.QueryRecords)]
        public IEnumerable<UncompletedMitigationPlan> QueryUncompletedMitigationPlanes(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<UncompletedMitigationPlan>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<UncompletedMitigationPlan>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND IsCompleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.DeleteRecord)]
        public void DeleteUncompletedMitigationPlan(int id)
        {
            // For MitigationPlanes, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE MitigationPlan SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.CreateNewRecord)]
        public UncompletedMitigationPlan NewUncompeletedMitigationPlan()
        {
            return new UncompletedMitigationPlan();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.AddNewRecord)]
        public void AddNewUncompletedMitigationPlan(UncompletedMitigationPlan record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.IsCompleted = false;
            m_dataContext.Table<UncompletedMitigationPlan>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.UpdateRecord)]
        public void UpdateUncompletedMitigationPlan(UncompletedMitigationPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<UncompletedMitigationPlan>().UpdateRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(UncompletedMitigationPlan), RecordOperation.UpdateRecord)]
        public void CompleteUncompletedMitigationPlan(UncompletedMitigationPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            record.IsCompleted = true;
            m_dataContext.Table<UncompletedMitigationPlan>().UpdateRecord(record);
        }

        #endregion

        #region [ ActionItem Table Operations ]


        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ActionItem), RecordOperation.QueryRecordCount)]
        public int QueryActionItemCount(int parentID)
        {
            return m_dataContext.Table<ActionItem>().QueryRecordCount(new RecordRestriction("PlanID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        public IEnumerable<ActionItem> QueryActionItems(int parentID)
        {
            return m_dataContext.Table<ActionItem>().QueryRecords(restriction: new RecordRestriction("PlanID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ActionItem), RecordOperation.QueryRecords)]
        public IEnumerable<ActionItem> QueryActionItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<ActionItem>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("PlanID = {0}", parentID));
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
        public int QueryNoticeLogCount()
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
        public IEnumerable<NoticeLog> QueryNoticeLogs(string sortField, bool ascending, int page, int pageSize)
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

        #region [ Vendor Table Operations ]

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecordCount)]
        public int QueryVendorCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecordCount();

            return m_dataContext.Table<Vendor>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecords)]
        public IEnumerable<Vendor> QueryVendors(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.DeleteRecord)]
        public void DeleteVendor(int id)
        {
            // For Vendors, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Vendor SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Vendor), RecordOperation.CreateNewRecord)]
        public Vendor NewVendor()
        {
            return new Vendor();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.AddNewRecord)]
        public void AddNewVendor(Vendor record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<Vendor>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.UpdateRecord)]
        public void UpdateVendor(Vendor record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Vendor>().UpdateRecord(record);
        }

        #endregion

        #region [ Platform Table Operations ]

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecordCount)]
        public int QueryPlatformCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecordCount();

            return m_dataContext.Table<Platform>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecords)]
        public IEnumerable<Platform> QueryPlatforms(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.DeleteRecord)]
        public void DeletePlatform(int id)
        {
            // For Platforms, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Platform SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Platform), RecordOperation.CreateNewRecord)]
        public Platform NewPlatform()
        {
            return new Platform();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.AddNewRecord)]
        public void AddNewPlatform(Platform record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<Platform>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.UpdateRecord)]
        public void UpdatePlatform(Platform record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Platform>().UpdateRecord(record);
        }

        #endregion

        #region [ BusinessUnitGroup Table Operations ]

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecordCount)]
        public int QueryBusinessUnitGroupCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<BusinessUnit>().QueryRecordCount();

            return m_dataContext.Table<BusinessUnit>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecords)]
        public IEnumerable<BusinessUnit> QueryBusinessUnitGroups(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.DeleteRecord)]
        public void DeleteBusinessUnitGroup(int id)
        {
            // For BusinessUnitGroups, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE BusinessUnitGroup SET IsDeleted=1 WHERE ID={0}", id);
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
            m_dataContext.Table<BusinessUnit>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.UpdateRecord)]
        public void UpdateBusinessUnitGroup(BusinessUnit record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<BusinessUnit>().UpdateRecord(record);
        }

        #endregion

        #region [ Page Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecordCount)]
        public int QueryPageCount()
        {
            return m_dataContext.Table<Page>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecords)]
        public IEnumerable<Page> QueryPages(string sortField, bool ascending, int page, int pageSize)
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
        public int QueryMenuCount()
        {
            return m_dataContext.Table<Menu>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.QueryRecords)]
        public IEnumerable<Menu> QueryMenus(string sortField, bool ascending, int page, int pageSize)
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
        public int QueryMenuItemCount(int parentID)
        {
            return m_dataContext.Table<MenuItem>().QueryRecordCount(new RecordRestriction("MenuID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecords)]
        public IEnumerable<MenuItem> QueryMenuItems(int parentID, string sortField, bool ascending, int page, int pageSize)
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
        public int QueryValueListGroupCount()
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.QueryRecords)]
        public IEnumerable<ValueListGroup> QueryValueListGroups(string sortField, bool ascending, int page, int pageSize)
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
        public int QueryValueListCount(int parentID)
        {
            return m_dataContext.Table<ValueList>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecords)]
        public IEnumerable<ValueList> QueryValueListItems(int parentID, string sortField, bool ascending, int page, int pageSize)
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
        public int QueryThemeFieldsCount(int parentID)
        {
            return m_dataContext.Table<ThemeFields>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.QueryRecords)]
        public IEnumerable<ThemeFields> QueryThemeFieldsItems(int parentID, string sortField, bool ascending, int page, int pageSize)
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

        #region [ LatestVendorDiscoveryResult View Operations ]

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.QueryRecordCount)]
        public int QueryLatestVendorDiscoveryResultCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecordCount();

            return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.QueryRecords)]
        public IEnumerable<LatestVendorDiscoveryResult> QueryLatestVendorDiscoveryResults(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.DeleteRecord)]
        public void DeleteLatestVendorDiscoveryResult(int id)
        {
            // Delete associated DiscoveryResult record
            m_dataContext.Table<DiscoveryResult>().DeleteRecord(id);
        }

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.CreateNewRecord)]
        public LatestVendorDiscoveryResult NewLatestVendorDiscoveryResult()
        {
            return new LatestVendorDiscoveryResult();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.AddNewRecord)]
        public void AddNewLatestVendorDiscoveryResult(LatestVendorDiscoveryResult record)
        {
            DiscoveryResult result = DeriveDiscoveryResult(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<DiscoveryResult>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.UpdateRecord)]
        public void UpdateLatestVendorDiscoveryResult(LatestVendorDiscoveryResult record)
        {
            m_dataContext.Table<DiscoveryResult>().UpdateRecord(DeriveDiscoveryResult(record));
        }

        private DiscoveryResult DeriveDiscoveryResult(LatestVendorDiscoveryResult record)
        {
            return new DiscoveryResult
            {
                ID = record.DiscoveryResultID,
                VendorID = record.VendorID,
                ReviewDate = record.ReviewDate,
                ResultKey = record.ResultKey,
                Notes = record.Notes,
                CreatedByID = record.CreatedByID,
                CreatedOn =  record.CreatedOn
            };
        }

        #endregion

        #region [ MitigationPlanActionItemsCompleted View Operations ]

        [RecordOperation(typeof(MitigationPlanActionItemsCompleted), RecordOperation.QueryRecordCount)]
        public int QueryMitigationPlanActionItemsCompletedCount()
        {
            return m_dataContext.Table<MitigationPlanActionItemsCompleted>().QueryRecordCount();
        }

        [RecordOperation(typeof(MitigationPlanActionItemsCompleted), RecordOperation.QueryRecords)]
        public IEnumerable<MitigationPlanActionItemsCompleted> QueryMitigationPlanActionItemsCompleteds(string res)
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
        public int QueryPlansActionCompletedViewCount()
        {
            return m_dataContext.Table<PlansActionCompletedView>().QueryRecordCount();
        }

        [RecordOperation(typeof(PlansActionCompletedView), RecordOperation.QueryRecords)]
        public IEnumerable<PlansActionCompletedView> QueryPlansActionCompletedViews(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<PlansActionCompletedView>().QueryRecords(sortField, ascending, page, pageSize);
        }


        [RecordOperation(typeof(PlansActionCompletedView), RecordOperation.CreateNewRecord)]
        public PlansActionCompletedView NewPlansActionCompletedView()
        {
            return new PlansActionCompletedView();
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
            AuthorizationCache.UserIDs.TryGetValue(UserInfo.CurrentUserID, out userID);
            return userID;
        }

        #endregion
    }
}
