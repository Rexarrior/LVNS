linux: dir buildL configure 
windows: dir buildL configure
linuxD: dir buildLD configure 
windowsD: dir buildWD configure  


dir:
	mkdir -p ./bin

buildL: 
	mcs -target:exe -platform:anycpu -r:NLog.dll -d:LINUX  *.cs -out:./bin/Core.exe  

buildW: 
	mcs -target:exe -platform:anycpu -r:NLog.dll   *.cs -out:./bin/Core.exe  


buildLD: 
	mcs -target:exe -platform:anycpu -r:NLog.dll -d:LINUX  -d:DEBUG *.cs -out:./bin/Core.exe  

buildWD: 
	mcs -target:exe -platform:anycpu -r:NLog.dll -d:DEBUG   *.cs -out:./bin/Core.exe  

configure: 
	cp NLog.dll ./bin/NLog.dll 
	cp NLog.config ./bin/NLog.config


clear:
	rm -rf ./bin

