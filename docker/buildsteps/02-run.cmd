@ECHO OFF
CALL common.cmd

docker run^
    --rm^
    -e BUILD_NUMBER=%BUILD_NUMBER%^
    -e COMMIT_HASH=%COMMIT_HASH%^
    -e NUGET_APIKEY=%NUGET_KEY%^
    %IMAGE_NAME%
EXIT /b %errorlevel%
