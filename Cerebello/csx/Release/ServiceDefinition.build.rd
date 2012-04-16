<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="Cerebello" generation="1" functional="0" release="0" Id="12d16902-5bff-4bae-b685-f776c52bef2c" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="CerebelloGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="CerebelloWebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/Cerebello/CerebelloGroup/LB:CerebelloWebRole:Endpoint1" />
          </inToChannel>
        </inPort>
        <inPort name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/Cerebello/CerebelloGroup/LB:CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="CerebelloWebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCerebelloWebRoleInstances" />
          </maps>
        </aCS>
        <aCS name="Certificate|CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/Cerebello/CerebelloGroup/MapCertificate|CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:CerebelloWebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="MapCerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapCerebelloWebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRoleInstances" />
          </setting>
        </map>
        <map name="MapCertificate|CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="CerebelloWebRole" generation="1" functional="0" release="0" software="D:\Projetos\Azure\Cerebello\Cerebello\csx\Release\roles\CerebelloWebRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/Cerebello/CerebelloGroup/SW:CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;CerebelloWebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;CerebelloWebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRoleInstances" />
            <sCSPolicyFaultDomainMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyFaultDomain name="CerebelloWebRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="CerebelloWebRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="bc34e710-9c5a-4a5e-aeb4-f79db264dfc9" ref="Microsoft.RedDog.Contract\ServiceContract\CerebelloContract@ServiceDefinition.build">
      <interfacereferences>
        <interfaceReference Id="0281b2c2-2479-4328-a2e2-6c6d64aa280d" ref="Microsoft.RedDog.Contract\Interface\CerebelloWebRole:Endpoint1@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="421e7df4-4b6c-4134-8121-2fd5d6ac9af2" ref="Microsoft.RedDog.Contract\Interface\CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition.build">
          <inPort>
            <inPortMoniker name="/Cerebello/CerebelloGroup/CerebelloWebRole:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>