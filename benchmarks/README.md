# How to use benchmark dotnet

## Quick Run

```
dotnet exec bin/Release/netcoreapp3.1/benchmarks.dll -- --filter '*Md5VsSha256*'
```


## With Corerun

Publish to self contained app

```
dotnet build -c Release
dotnet publish -c Release -r osx-x64 --self-contained
```

Run using local Corerun

```
/Users/marek/git/my/runtime/artifacts/bin/coreclr/OSX.x64.Release/corerun bin/Release/netcoreapp3.1/osx-x64/publish/benchmarks.dll --filter '*BenchmarkClassName*'
```