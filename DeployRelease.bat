


SET /P version=Enter a version (example: 0.423b):
IF NOT DEFINED version SET "version=UNKNOWN"


SET sevenZipLocation="C:\Program Files\7-Zip\7z.exe"

SET deployDir=.\Deploy\TCodeRemote_v%version%_Release\
SET deployZipDir=.\Deploy\

xcopy .\src\TCode_Remote\bin\Release\*.exe %deployDir% /s /i /K /D /H /Y
xcopy .\src\TCode_Remote\bin\Release\*.config %deployDir% /s /i /K /D /H /Y
xcopy .\src\TCode_Remote\bin\Release\*.dll %deployDir% /s /i /K /D /H /Y
xcopy .\src\TCode_Remote\bin\Release\*.winmd %deployDir% /s /i /K /D /H /Y
xcopy "Virtual serial port howto.pdf" %deployDir% /i /K /D /H /Y

%sevenZipLocation% a -tzip %deployZipDir%TCodeRemote_v%version%_Release.zip %deployDir%

xcopy %deployZipDir%TCodeRemote_v%version%_Release.zip "\\RASPBERRYPI.local\STK\Hardware\my software\TCode_Remote\"  /s /i /K /D /H /Y

pause