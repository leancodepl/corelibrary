@ECHO OFF
CALL common.cmd

docker rmi %IMAGE_NAME%
EXIT /b %errorlevel%
