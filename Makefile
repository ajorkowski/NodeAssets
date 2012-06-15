core:
	nuget pack NodeAssets.Core/NodeAssets.Core.csproj
	nuget push NodeAssets.Core.$(v).nupkg
	rm NodeAssets.Core.$(v).nupkg

aspnet:
	nuget pack NodeAssets.AspNet/NodeAssets.AspNet.csproj
	nuget push NodeAssets.AspNet.$(v).nupkg
	rm NodeAssets.AspNet.$(v).nupkg

.PHONY: core aspnet