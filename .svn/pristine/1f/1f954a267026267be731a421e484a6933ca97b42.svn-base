:: DenverServer (192.168.240.14)
if "%APPLICATION_NAME%" == "DenverTestV2App" (
	del /q "D:\Websites\CCD_ServerAPI\*"
	for /d %%x in ("D:\Websites\CCD_ServerAPI\*") do if not "%%x" == "D:\Websites\CCD_ServerAPI\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "D:\Websites\CCD_ServerAPI"
	cd /d D:\Websites\CCD_ServerAPI
	copy appsettings.denverServerTest.json appsettings.denver.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool CCD_ServerAPI
	appcmd start site CCD_ServerAPI
	cd Scripts
	sqlcmd -S 192.168.240.14 -d CCD_V2 -U serviceaccteng -P R#?c99 -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "DenverHeatherV2App" (
	del /q "D:\Websites\CCD_ServerAPI_Heather\*"
	for /d %%x in ("D:\Websites\CCD_ServerAPI_Heather\*") do if not "%%x" == "D:\Websites\CCD_ServerAPI_Heather\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "D:\Websites\CCD_ServerAPI_Heather"
	cd /d D:\Websites\CCD_ServerAPI_Heather
	copy appsettings.denverServerHeather.json appsettings.denver.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool CCD_ServerAPI_Heather
	appcmd start site CCD_ServerAPI_Heather
	cd Scripts
	sqlcmd -S 192.168.240.14 -d CCD_V2_Heather -U serviceaccteng -P R#?c99 -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "DenverConvV2App" (
	del /q "D:\Websites\CCD_Conv_API\*"
	for /d %%x in ("D:\Websites\CCD_Conv_API\*") do if not "%%x" == "D:\Websites\CCD_Conv_API\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "D:\Websites\CCD_Conv_API"
	cd /d D:\Websites\CCD_Conv_API
	copy appsettings.denverServerConv.json appsettings.denver.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool CCD_Conv_API
	appcmd start site CCD_Conv_API
	cd Scripts
	sqlcmd -S act1-01.c2wdd2yhupkt.us-west-2.rds.amazonaws.com -d CCD_Conversion -U atims -P "DqSf)k-ne27" -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "DenverTrainingV2App" (
	del /q "D:\Websites\CCD_Training_API\*"
	for /d %%x in ("D:\Websites\CCD_Training_API\*") do if not "%%x" == "D:\Websites\CCD_Training_API\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "D:\Websites\CCD_Training_API"
	cd /d D:\Websites\CCD_Training_API
	copy appsettings.denverServerTraining.json appsettings.denver.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool CCD_Training_API
	appcmd start site CCD_Training_API
	cd Scripts
	sqlcmd -S act1-01.c2wdd2yhupkt.us-west-2.rds.amazonaws.com -d CCD_Training -U atims -P "DqSf)k-ne27" -i createTrigger.sql > log.txt
)


:: Web2019-V2
if "%APPLICATION_NAME%" == "DenverV22019App" (
	if exist "Z:\Websites\6441_DSD_ServerAPI" (
		del /q "Z:\Websites\6441_DSD_ServerAPI\*"
		for /d %%x in ("Z:\Websites\6441_DSD_ServerAPI\*") do if not "%%x" == "Z:\Websites\6441_DSD_ServerAPI\logs" @rd /s /q "%%x"
	)
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6441_DSD_ServerAPI"
	cd /d Z:\Websites\6441_DSD_ServerAPI
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool 6441_DSD_ServerAPI
	appcmd start site 6441_DSD_ServerAPI
	cd Scripts
	sqlcmd -S atims-db2017 -d DSD_V2 -U atims -P atims -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "OrangeV22019App" (
	del /q "Z:\Websites\6541_OC_ServerAPI\*"
	for /d %%x in ("Z:\Websites\6541_OC_ServerAPI\*") do if not "%%x" == "Z:\Websites\6541_OC_ServerAPI\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6541_OC_ServerAPI"
	cd /d Z:\Websites\6541_OC_ServerAPI
	del /f appsettings.denver.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool 6541_OC.ServerAPI
	appcmd start site 6541_OC_ServerAPI
	cd Scripts
    sqlcmd -S atims-db2017 -d OCSD_V2 -U atims -P atims -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "GlennV22019App" (
	del /q "Z:\Websites\6741_GCSO_ServerAPI\*"
	for /d %%x in ("Z:\Websites\6741_GCSO_ServerAPI\*") do if not "%%x" == "Z:\Websites\6741_GCSO_ServerAPI\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6741_GCSO_ServerAPI"
	cd /d Z:\Websites\6741_GCSO_ServerAPI
	del /f appsettings.orange.json appsettings.denver.json appsettings.sacramento.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool 6741_GCSO_ServerAPI
	appcmd start site 6741_GCSO_ServerAPI
	cd Scripts
    sqlcmd -S atims-db2016 -d GCSO_V2 -U atims -P atims -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "SacramentoV22019App" (
	del /q "Z:\Websites\6141_SSD_ServerAPI\*"
	for /d %%x in ("Z:\Websites\6141_SSD_ServerAPI\*") do if not "%%x" == "Z:\Websites\6141_SSD_ServerAPI\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6141_SSD_ServerAPI"
	cd /d Z:\Websites\6141_SSD_ServerAPI
	del /f appsettings.orange.json appsettings.denver.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool 6141_SSD.ServerAPI
	appcmd start site 6141_SSD_ServerAPI
	cd Scripts
    sqlcmd -S atims-db2016 -d SSD_V2 -U atims -P atims -i createTrigger.sql > log.txt
)
	
if "%APPLICATION_NAME%" == "SeleniumTest2019App" (
	del /q "Z:\Websites\DSD_ServerAPI_Test\*"
	for /d %%x in ("Z:\Websites\DSD_ServerAPI_Test\*") do if not "%%x" == "Z:\Websites\DSD_ServerAPI_Test\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\DSD_ServerAPI_Test"
	cd /d Z:\Websites\DSD_ServerAPI_Test
	copy appsettings.denverSelenium.json appsettings.denver.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool DSD_ServerAPI_TEST
	appcmd start site DSD_ServerAPI_TEST
	cd Scripts
	sqlcmd -S atims-db2016 -d CCD_Selenium -U atims -P atims -i createTrigger.sql > log.txt
)
	
if "%APPLICATION_NAME%" == "DenverConvV22019App" (
	del /q "Z:\Websites\6442_CCD_Conv_API\*"
	for /d %%x in ("Z:\Websites\6442_CCD_Conv_API\*") do if not "%%x" == "Z:\Websites\6442_CCD_Conv_API\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6442_CCD_Conv_API"
	cd /d Z:\Websites\6442_CCD_Conv_API
	copy appsettings.denverConv.json appsettings.denver.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool CCD_Conv_API
	appcmd start site 6442_CCD_Conv_API
	cd Scripts
    sqlcmd -S atims-conv -d DSD_ATIMS_Conversion -U atims -P atims -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "SacramentoConvV22019App" (
	del /q "Z:\Websites\6142_SSD_Conv_API\*"
	for /d %%x in ("Z:\Websites\6142_SSD_Conv_API\*") do if not "%%x" == "Z:\Websites\6142_SSD_Conv_API\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6142_SSD_Conv_API"
	cd /d Z:\Websites\6142_SSD_Conv_API
	copy appsettings.sacramentoConv.json appsettings.sacramento.json
	del /f appsettings.orange.json appsettings.denver.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool 6142_SSD_Conv_API
	appcmd start site 6142_SSD_Conv_API
	cd Scripts
    sqlcmd -S atims-conv -d SSD_V2_Conversion -U atims -P atims -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "GlennConvV22019App" (
	del /q "Z:\Websites\6742_GCSO_Conv_API\*"
	for /d %%x in ("Z:\Websites\6742_GCSO_Conv_API\*") do if not "%%x" == "Z:\Websites\6742_GCSO_Conv_API\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "Z:\Websites\6742_GCSO_Conv_API"
	cd /d Z:\Websites\6742_GCSO_Conv_API
	copy appsettings.glennConv.json appsettings.glenn.json
	del /f appsettings.orange.json appsettings.denver.json appsettings.sacramento.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.demo.json appsettings.demoNew.json
	appcmd start apppool 6742_GCSO_Conv_API
	appcmd start site 6742_GCSO_Conv_API
	cd Scripts
    sqlcmd -S atims-conv -d GCSO_Conversion -U atims -P atims -i createTrigger.sql > log.txt
)

:: DemoServer (192.168.250.108)
if "%APPLICATION_NAME%" == "DemoV2App" (
	del /q "D:\Websites\ServerAPI_V2\*"
	for /d %%x in ("D:\Websites\ServerAPI_V2\*") do if not "%%x" == "D:\Websites\ServerAPI_V2\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "D:\Websites\ServerAPI_V2"
	cd /d D:\Websites\ServerAPI_V2
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.denver.json appsettings.demoNew.json
	appcmd start apppool ServerAPI_V2
	appcmd start site ServerAPI_V2
	cd Scripts
	sqlcmd -S 192.168.250.108 -d ATIMS_Demo_2.2 -U atims -P At1ms! -i createTrigger.sql > log.txt
)

if "%APPLICATION_NAME%" == "DemoV2NewApp" (
	del /q "D:\Websites\ServerAPI_V2_New\*"
	for /d %%x in ("D:\Websites\ServerAPI_V2_New\*") do if not "%%x" == "D:\Websites\ServerAPI_V2_New\logs" @rd /s /q "%%x"
	robocopy /e "C:\Deployments\ServerAPISource" "D:\Websites\ServerAPI_V2_New"
	cd /d D:\Websites\ServerAPI_V2_New
	copy appsettings.demoNew.json appsettings.demo.json
	del /f appsettings.orange.json appsettings.sacramento.json appsettings.glenn.json appsettings.denverSelenium.json appsettings.denverConv.json appsettings.sacramentoConv.json appsettings.glennConv.json appsettings.denverServerTest.json appsettings.denverServerHeather.json appsettings.denverServerTraining.json appsettings.denverServerConv.json appsettings.denver.json appsettings.demoNew.json
	appcmd start apppool New_ServerAPI_V2
	appcmd start site New_ServerAPI_V2
	cd Scripts
	sqlcmd -S act1-01.c2wdd2yhupkt.us-west-2.rds.amazonaws.com -d New_ATIMS_Demo_2.2 -U atims -P "DqSf)k-ne27" -i createTrigger.sql > log.txt
)