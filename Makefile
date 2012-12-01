core:
	nuget push NodeAssets.Core\bin\Release\NodeAssets.Core.$(v).nupkg

aspnet:
	nuget push NodeAssets.AspNet\bin\Release\NodeAssets.AspNet.$(v).nupkg

coffee:
	nuget push NodeAssets.Compilers.CoffeeSharp\bin\Release\NodeAssets.Compilers.CoffeeSharp.$(v).nupkg

minify:
	nuget push NodeAssets.Compilers.Minify\bin\Release\NodeAssets.Compilers.Minify.$(v).nupkg

sass:
	nuget push NodeAssets.Compilers.Sass\bin\Release\NodeAssets.Compilers.Sass.$(v).nupkg

node:
	nuget push NodeAssets.Compilers.Node\bin\Release\NodeAssets.Compilers.Node.$(v).nupkg

typescript:
	nuget push NodeAssets.Compilers.Typescript\bin\Release\NodeAssets.Compilers.Typescript.$(v).nupkg

.PHONY: core aspnet coffee minify sass node