﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Commands.Blueprint.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using ParameterSetNames = Microsoft.Azure.Commands.Blueprint.Common.BlueprintConstants.ParameterSetNames;
using ParameterHelpMessages = Microsoft.Azure.Commands.Blueprint.Common.BlueprintConstants.ParameterHelpMessages;

namespace Microsoft.Azure.Commands.Blueprint.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "Blueprint", DefaultParameterSetName = ParameterSetNames.SubscriptionScope)]
    public class GetAzureRmBlueprint : BlueprintCmdletBase
    {
        #region Parameters
        [Parameter(ParameterSetName = ParameterSetNames.SubscriptionScope, Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.SubscriptionId)]
        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionAndName, Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.SubscriptionId)]
        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionNameAndVersion, Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.SubscriptionId)]
        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionNameAndLatestPublished, Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.SubscriptionId)]
        [ValidateNotNullOrEmpty]
        public string SubscriptionId { get; set; }

        [Parameter(ParameterSetName = ParameterSetNames.ManagementGroupScope, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.ManagementGroupId)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupAndName, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.ManagementGroupId)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupNameAndVersion, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.ManagementGroupId)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupNameAndLatestPublished, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.ManagementGroupId)]
        [ValidateNotNullOrEmpty]
        public string ManagementGroupId { get; set; }

        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionAndName, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionName)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupAndName, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionName)]
        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionNameAndLatestPublished, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionName)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupNameAndLatestPublished, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionName)]
        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionNameAndVersion, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionVersion)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupNameAndVersion, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionVersion)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionNameAndVersion, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionVersion)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupNameAndVersion, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.BlueprintDefinitionVersion)]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; }

        [Parameter(ParameterSetName = ParameterSetNames.BySubscriptionNameAndLatestPublished, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.LatestPublishedFlag)]
        [Parameter(ParameterSetName = ParameterSetNames.ByManagementGroupNameAndLatestPublished, Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = ParameterHelpMessages.LatestPublishedFlag)]
        public SwitchParameter LatestPublished { get; set; }

        #endregion Parameters

        #region Cmdlet Overrides
        public override void ExecuteCmdlet()
        {

            var scope = GetCurrentScope();

            try
            {
                switch (ParameterSetName)
                {
                    case ParameterSetNames.ManagementGroupScope:
                        foreach (var bp in BlueprintClientWithVersion.ListBlueprints(scope))
                            WriteObject(bp);

                        break;
                    case ParameterSetNames.SubscriptionScope:
                        var queryScopes =
                            GetManagementGroupAncestorsForSubscription(
                                SubscriptionId ?? DefaultContext.Subscription.Id)
                                .Select(mg => FormatManagementGroupAncestorScope(mg))
                                .ToList();

                        //add current subscription scope to the list of MG scopes that we'll query
                        queryScopes.Add(scope);

                        foreach (var bp in BlueprintClientWithVersion.ListBlueprints(queryScopes))
                            WriteObject(bp);

                        break;
                    case ParameterSetNames.BySubscriptionAndName: case ParameterSetNames.ByManagementGroupAndName:
                        WriteObject(BlueprintClientWithVersion.GetBlueprint(scope, Name));
                        break;
                    case ParameterSetNames.BySubscriptionNameAndVersion: case ParameterSetNames.ByManagementGroupNameAndVersion:
                        WriteObject(BlueprintClient.GetPublishedBlueprint(scope, Name, Version));
                        break;
                    case ParameterSetNames.BySubscriptionNameAndLatestPublished: case ParameterSetNames.ByManagementGroupNameAndLatestPublished:
                        WriteObject(BlueprintClient.GetLatestPublishedBlueprint(scope, Name));
                        break;
                    default:
                        throw new PSInvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
        }
        #endregion Cmdlet Overrides

        #region Private Methods
        private string GetCurrentScope()
        {
            string scope = null;

            if (this.IsParameterBound(c => c.ManagementGroupId))
            {
                scope = string.Format(BlueprintConstants.ManagementGroupScope, ManagementGroupId);
            }
            else 
            {
                scope = this.IsParameterBound(c => c.SubscriptionId)
                    ? string.Format(BlueprintConstants.SubscriptionScope, SubscriptionId)
                    : string.Format(BlueprintConstants.SubscriptionScope, DefaultContext.Subscription.Id);
            }
            return scope;
        }
        
        #endregion Private Methods
    }
}
