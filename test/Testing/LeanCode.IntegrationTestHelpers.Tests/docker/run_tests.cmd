docker-compose build --pull test
IF %errorlevel% NEQ 0 EXIT /b %errorlevel%
docker-compose run test
IF %errorlevel% NEQ 0 EXIT /b %errorlevel%
docker-compose down -v
