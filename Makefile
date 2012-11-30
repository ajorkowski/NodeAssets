core:
	nuget push NodeAssets.Core\bin\Release\NodeAssets.Core.$(v).nupkg

aspnet:
	nuget push NodeAssets.AspNet\bin\Release\NodeAssets.AspNet.$(v).nupkg

.PHONY: core aspnet