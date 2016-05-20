core:
	nuget push NodeAssets.Core\package\NodeAssets.Core.$(v).nupkg

aspnet:
	nuget push NodeAssets.AspNet\package\NodeAssets.AspNet.$(v).nupkg

coffee:
	nuget push NodeAssets.Compilers.CoffeeSharp\package\NodeAssets.Compilers.CoffeeSharp.$(v).nupkg

minify:
	nuget push NodeAssets.Compilers.Minify\package\NodeAssets.Compilers.Minify.$(v).nupkg

sass:
	nuget push NodeAssets.Compilers.Sass\package\NodeAssets.Compilers.Sass.$(v).nupkg

node:
	nuget push NodeAssets.Compilers.Node\package\NodeAssets.Compilers.Node.$(v).nupkg

.PHONY: core aspnet coffee minify sass node