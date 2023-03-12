set dest=C:\projects\BIDS Helper\GitHub\bideveloperextensions2\DLLs\SQL2022\Reference

c:

cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSIS\160\Binn"
copy /Y /B "Microsoft.DataTransformationServices.Controls.dll" "%dest%"
copy /Y /B "Microsoft.SQLServer.DTSPipelineWrap.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.DTSRuntimeWrap.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.ExecPackageTaskWrap.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.ManagedDTS.dll" "%dest%"

cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSIS\160\BIShared"
copy /Y /B "Microsoft.DataWarehouse.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Project.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.VsIntegration.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Controls.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Design.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.Interfaces.dll" "%dest%"

