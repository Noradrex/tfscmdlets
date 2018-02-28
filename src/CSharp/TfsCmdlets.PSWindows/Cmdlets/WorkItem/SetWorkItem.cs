using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace TfsCmdlets.Cmdlets.WorkItem
{
    [Cmdlet(VerbsCommon.Set, "WorkItem", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    [OutputType("Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem")]
    public class SetWorkItem : WorkItemCmdletBase
    {
        protected override void ProcessRecord()
        {
            var wi = GetWorkItem();

            if (!ShouldProcess($"Work Item #{wi.Id}", "Set work item's field values")) return;

            if (BypassRules)
            {
                wi = WorkItemService.GetWorkItem(wi.Id, true);
            }

            if (!string.IsNullOrEmpty(Title)) wi.Title = Title;
            if (!string.IsNullOrEmpty(AssignedTo)) wi.Fields["System.AssignedTo"].Value = AssignedTo;
            if (!string.IsNullOrEmpty(State)) wi.State = State;
            if (!string.IsNullOrEmpty(AreaPath)) wi.AreaPath = AreaPath;
            if (!string.IsNullOrEmpty(IterationPath)) wi.IterationPath = IterationPath;
            if (Description != null) wi.Description = Description;

            if (Fields != null)
            {
                foreach (var f in Fields)
                {
                    wi.Fields[f.Key].Value = f.Value;
                }
            }

            FixAreaIterationValues(wi);

            if (!SkipSave)
            {
                try
                {
                    WorkItemService.Save(wi);
                }
                catch (Exception ex) when (ex.GetType().Name.Equals("Microsoft.TeamFoundation.WorkItemTracking.Client.ValidationException"))
                {
                    var errors = WorkItemService.Validate(wi);
                    var invalidFields = string.Join(", ", errors.Select(f => f.ReferenceName));

                    throw new Exception($"Unable to save work item. The following fields have invalid values: {invalidFields}. Check the supplied values and try again", ex);
                }
            }

            if (Passthru || SkipSave)
            {
                WriteObject(wi);
            }
        }

        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 0)]
        public override object WorkItem { get; set; }

        [Parameter(Position = 1)]
        public string Title { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public string State { get; set; }

        [Parameter]
        public string AssignedTo { get; set; }

        [Parameter]
        public string AreaPath { get; set; }

        [Parameter]
        public string IterationPath { get; set; }

        [Parameter]
        public IDictionary<string, object> Fields { get; set; }

        [Parameter]
        public override object Project { get; set; }

        [Parameter]
        public SwitchParameter SkipSave { get; set; }

        [Parameter]
        public SwitchParameter BypassRules { get; set; }

        [Parameter]
        public SwitchParameter Passthru { get; set; }
    }
}
