# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the \"License\");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an \"AS IS\" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

<#
.Synopsis
Create a in-memory object for Remote Desktop Extension
.Description
Create a in-memory object for Remote Desktop Extension

.Outputs
Microsoft.Azure.PowerShell.Cmdlets.CloudService.Models.Api20201001Preview.Extension
.Link
https://docs.microsoft.com/en-us/powershell/module/az.CloudService/new-AzCloudServiceExtensionObject
#>

function New-AzCloudServiceRemoteDesktopExtensionObject {
  [OutputType('Microsoft.Azure.PowerShell.Cmdlets.CloudService.Models.Api20201001Preview.Extension')]
  param(
	[Parameter(HelpMessage="Name of Remote Desktop Extension.")]
    [Parameter(Mandatory=$true)]
    [string] $Name,
	
	[Parameter(HelpMessage="Credential for Remote Desktop Extension.")]
    [Parameter(Mandatory=$true)]
    [PSCredential] $Credential,
	
	[Parameter(HelpMessage="Expiration for Remote Desktop Extension.")]
    [DateTime] $Expiration = (Get-Date),
	
	[Parameter(HelpMessage="Remote Desktop Extension version.")]
    [string] $TypeHandlerVersion,
	
    [string[]] $RolesAppliedTo
  )

  process {
    $RDPPublisher = "Microsoft.Windows.Azure.Extensions"
    $RDPExtensionType = "RDP"
    
    $rdpSetting = "<PublicConfig><UserName>$Credential.UserName</UserName><Expiration>$Expiration</Expiration></PublicConfig>";
    $rdpProtectedSetting = "<PrivateConfig><Password>$Credential.Password</Password></PrivateConfig>";
    
    return New-AzCloudServiceExtensionObject -Name $Name -Publisher $RDPPublisher -Type $RDPExtensionType -TypeHandlerVersion $TypeHandlerVersion -Setting $rdpSetting -ProtectedSetting $rdpProtectedSetting -RolesAppliedTo $RolesAppliedTo
  }
}