.PHONY: build install

build:
	dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

install:
	cp bin/Release/net10.0/linux-x64/publish/Nid ~/bin/nid
