@ECHO OFF
CALL common.cmd
CD %SCRIPT_PATH%\..

docker build^
    -t %IMAGE_NAME%^
    -f Dockerfile^
    ..
EXIT /b %errorlevel%
