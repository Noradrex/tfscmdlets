using System.Management.Automation;
using Microsoft.TeamFoundation.Framework.Client;

namespace TfsCmdlets.Cmdlets.TeamProjectCollection
{
    [Cmdlet(VerbsCommon.Remove, "TeamProjectCollection", ConfirmImpact = ConfirmImpact.High,
        SupportsShouldProcess = true)]
    public class RemoveTeamProjectCollection : CollectionLevelCmdlet
    {
        protected override void ProcessRecord()
        {
            var tpc = GetCollection();

            if (!ShouldProcess(tpc.Name, "Delete Team Project Collection")) return;

            var configServer = tpc.ConfigurationServer;
            var tpcService = configServer.GetService<ITeamProjectCollectionService>();
            var collectionInfo = tpcService.GetCollection(tpc.InstanceId);
            collectionInfo.Delete();
        }

        [Parameter(Position =0, Mandatory = true, ValueFromPipeline = true)]
        public override object Collection { get; set; }
    }
}