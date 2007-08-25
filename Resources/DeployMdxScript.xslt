<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:as="http://schemas.microsoft.com/analysisservices/2003/engine">
	<xsl:param name="TargetDatabase"></xsl:param>

	<xsl:output indent="yes" omit-xml-declaration="yes" />
	<xsl:template match="/">
		<Alter AllowCreate="true" ObjectExpansion="ExpandFull" xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
			<Object>
				<DatabaseID>
					<xsl:value-of select="$TargetDatabase"></xsl:value-of>
				</DatabaseID>
				<CubeID>
					<xsl:value-of select="/as:Cube/as:ID"/>
				</CubeID>
				<MdxScriptID>
					<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:ID"/>
				</MdxScriptID>
			</Object>
			<ObjectDefinition>
				<MdxScript>
					<ID>
						<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:ID"/>
					</ID>
					<Name>
						<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:Name"/>
					</Name>
					<xsl:copy-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:Commands"/>
					<xsl:copy-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:CalculationProperties"/>
				</MdxScript>
				<!--<xsl:copy-of select="/as:Cube/as:MdxScripts/as:MdxScript"/>-->
				
			</ObjectDefinition>
			
		</Alter>
	</xsl:template>

	<xsl:template match="/as:Cube/as:MdxScripts/as:MdxScript">
		<xsl:apply-templates />
	</xsl:template>

	<!-- effectively remove these two child nodes-->
	<xsl:template match="//as:CreatedTimestamp"></xsl:template>
	<xsl:template match="//as:LastSchemaUpdate"></xsl:template>

	<xsl:template match="//as:MdxScript">
		<xsl:copy-of select="//as:MdxScript" />
	</xsl:template>
	
	<!--
	<MdxScript>
		<ID>
			<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:ID"/>
		</ID>
		<Name>
			<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:Name"/>
		</Name>
		<Commands>
			<Command>
				<Text>
					<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:Commands/as:Command/as:Text"/>
				</Text>
			</Command>
		</Commands>
	</MdxScript>
-->
</xsl:stylesheet>
