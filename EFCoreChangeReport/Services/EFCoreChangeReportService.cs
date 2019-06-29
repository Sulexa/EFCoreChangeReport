using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using EFCoreChangeReport.Models;
using EFCoreChangeReport.Configuration;
using Microsoft.Extensions.Options;

namespace EFCoreChangeReport.Services
{
    public interface IEFCoreChangeReportService
    {
        void PrepareChangeReports(DbContext context);
        void SaveChangeReports(DbContext context);
    }

    public class EFCoreChangeReportService : IEFCoreChangeReportService
    {
        private readonly EFCoreChangeReportConfiguration _eFCoreChangeReportConfiguration;
        private List<Report> reports;
        private List<EntityEntry> addedEntries;

        public EFCoreChangeReportService(IOptionsMonitor<EFCoreChangeReportConfiguration> databaseChangeReportConfiguration)
        {
            this._eFCoreChangeReportConfiguration = databaseChangeReportConfiguration.CurrentValue;
        }

        public void PrepareChangeReports(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this.reports = new List<Report>();

            this.addedEntries = new List<EntityEntry>();

            var entries = context.ChangeTracker.Entries().ToArray();
            for (int i = 0; i < entries.Count(); i++)
            {
                var entry = entries[i];
                switch (entry.State)
                {
                    case EntityState.Added:
                        this.addedEntries.Add(entry);
                        break;
                    case EntityState.Modified:
                        this.reports.Add(this.AddModifiedReport<Report>(entry));
                        break;
                    case EntityState.Deleted:
                        this.reports.Add(this.AddDeletedReport<Report>(entry));
                        break;
                    case EntityState.Detached:
                    case EntityState.Unchanged:
                    default:
                        break;
                }
            }
        }

        public void SaveChangeReports(DbContext context)
        {
            for (int i = 0; i < addedEntries.Count; i++)
            {
                var entry = addedEntries[i];
                this.reports.Add(this.AddAddedReport<Report>(entry));
            }
            _eFCoreChangeReportConfiguration.SinkReports.Invoke(this.reports);
            this.reports.Clear();
        }

        private TReport AddAddedReport<TReport>(EntityEntry entry) where TReport : Report, new()
        {
            var report = new TReport();

            report.TableName = entry.Metadata.Relational().TableName;

            var properties = entry.Properties;

            var json = new JObject();

            foreach (var prop in properties)
            {
                if (prop.Metadata.IsKey() || prop.Metadata.IsForeignKey())
                {
                    continue;
                }

                json[prop.Metadata.Name] = prop.CurrentValue != null
                    ? JToken.FromObject(prop.CurrentValue)
                    : JValue.CreateNull();
            }

            report.RowId = this.PrimaryKey(entry); ;
            report.EntityState = EntityState.Added;
            report.Changed = json.ToString(_eFCoreChangeReportConfiguration.JsonSerializerSettings.Formatting);

            return report;
        }

        private TReport AddModifiedReport<TReport>(EntityEntry entry) where TReport : Report, new()
        {
            var report = new TReport();

            report.TableName = entry.Metadata.Relational().TableName;

            var properties = entry.Properties;

            var json = new JObject();
            var bef = new JObject();
            var aft = new JObject();

            foreach (var prop in properties)
            {
                if (prop.IsModified)
                {
                    if (prop.OriginalValue != null)
                    {
                        if (prop.OriginalValue != prop.CurrentValue)
                        {
                            bef[prop.Metadata.Name] = JToken.FromObject(prop.OriginalValue);
                        }
                        else
                        {
                            var originalValue = entry.GetDatabaseValues().GetValue<object>(prop.Metadata.Name);
                            bef[prop.Metadata.Name] = originalValue != null
                                ? JToken.FromObject(originalValue)
                                : JValue.CreateNull();
                        }
                    }
                    else
                    {
                        bef[prop.Metadata.Name] = JValue.CreateNull();
                    }

                    aft[prop.Metadata.Name] = prop.CurrentValue != null
                    ? JToken.FromObject(prop.CurrentValue)
                    : JValue.CreateNull();
                }
            }

            json["before"] = bef;
            json["after"] = aft;

            report.RowId = this.PrimaryKey(entry);
            report.EntityState = EntityState.Modified;
            report.Changed = json.ToString(_eFCoreChangeReportConfiguration.JsonSerializerSettings.Formatting);

            return report;
        }

        private TReport AddDeletedReport<TReport>(EntityEntry entry) where TReport : Report, new()
        {
            var report = new TReport();

            report.TableName = entry.Metadata.Relational().TableName;

            var properties = entry.Properties;

            var json = new JObject();

            foreach (var prop in properties)
            {
                json[prop.Metadata.Name] = prop.OriginalValue != null
                    ? JToken.FromObject(prop.OriginalValue)
                    : JValue.CreateNull();
            }
            report.RowId = this.PrimaryKey(entry);
            report.EntityState = EntityState.Deleted;
            report.Changed = json.ToString(_eFCoreChangeReportConfiguration.JsonSerializerSettings.Formatting);

            return report;
        }

        private string PrimaryKey(EntityEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var key = entry.Metadata.FindPrimaryKey();

            var values = new List<object>();
            foreach (var property in key.Properties)
            {
                var value = entry.Property(property.Name).CurrentValue;
                if (value != null)
                {
                    values.Add(value);
                }
            }

            return string.Join(",", values);
        }
    }
}