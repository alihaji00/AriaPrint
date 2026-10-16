@echo off
dotnet clean
dotnet restore
dotnet build --verbosity minimal