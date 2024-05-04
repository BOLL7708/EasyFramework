This is a convenience class library for my own C# .NET 8 WPF applications. 

This submodule is _very_ specific to my wants and needs for my set of solutions, but might be useful as reference material for anyone else.

As this is a class library, you can add it to the root of the project folder, next to the main project namespace root.

```shell
git submodule add https://github.com/BOLL7708/EasyFramework.git EasyFramework
```

Then reference this class library it in the main project .csproj file, in `<ItemGroup>`.
```xml
<ProjectReference Include="..\EasyFramework\EasyFramework.csproj" />
```