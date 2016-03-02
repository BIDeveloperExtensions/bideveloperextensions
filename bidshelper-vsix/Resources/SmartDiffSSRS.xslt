<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:SSRS2005="http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition" xmlns:SSRS2008="http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition">

  <xsl:output cdata-section-elements="SSRS2005:Code SSRS2008:Code SSRS2005:CommandText SSRS2008:CommandText"/>
  
  <xsl:template match="node()">
		<xsl:copy>

      <!-- sort attributes by name -->
      <xsl:apply-templates select="@*">
        <xsl:sort order="ascending" select="name()"/>
      </xsl:apply-templates>

      <!-- sort nodes by name then by the Name attribute -->
      <xsl:apply-templates select="node()">
        <xsl:sort order="ascending" select="name()"/>
        <xsl:sort order="ascending" select="@Name"/>
        <xsl:sort order="ascending" select="@SSRS2005:Name"/>
        <xsl:sort order="ascending" select="@SSRS2008:Name"/>
      </xsl:apply-templates>

    </xsl:copy>
	</xsl:template>
	<xsl:template match="@*">
		<xsl:attribute namespace="{namespace-uri()}" name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>

  <!--remove any ZIndex elements if they appear directly under a TableCell (since only one textbox can be in a table cell) -->
  <xsl:template match="//SSRS2005:TableCell/SSRS2005:ReportItems/SSRS2005:Textbox/SSRS2005:ZIndex|//SSRS2008:TableCell/SSRS2008:ReportItems/SSRS2008:Textbox/SSRS2008:ZIndex"/>

</xsl:stylesheet>


