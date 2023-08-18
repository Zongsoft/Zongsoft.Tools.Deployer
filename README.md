# The Zongsoft Deployment Tool

![license](https://img.shields.io/github/license/Zongsoft/Zongsoft.Tools.Deployer) ![download](https://img.shields.io/nuget/dt/Zongsoft.Tools.Deployer) ![version](https://img.shields.io/github/v/release/Zongsoft/Zongsoft.Tools.Deployer?include_prereleases) ![github stars](https://img.shields.io/github/stars/Zongsoft/Zongsoft.Tools.Deployer?style=social)

README: [English](https://github.com/Zongsoft/Zongsoft.Tools.Deployer/blob/master/README.md) | [简体中文](https://github.com/Zongsoft/Zongsoft.Tools.Deployer/blob/master/README-zh_CN.md)

-----

## Abstraction

This is an application deployment tool that instructs the deployment tool to copy specific files to the destination location by specifying the deployment file.

It is recommended to define a default deployment file named `.deploy` in the deployment project directory, and the deployment file is a plain text file in `.ini` format.

### Reference examples

- Deployment source projects
	- [`/Zongsoft/Framework/Zongsoft.Data/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Data/drivers/mssql/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data/drivers/mssql/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Data/drivers/mysql/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data/drivers/mysql/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Security/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Security/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Security/api/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Security/api/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Messaging.Mqtt/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Messaging.Mqtt/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Messaging.Kafka/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Messaging.Kafka/.deploy)

- Deployment destination projects *(hosting projects)*
	- [`/Zongsoft/Framework/hosting/terminal/.deploy`](https://github.com/Zongsoft/Framework/tree/master/hosting/terminal/.deploy)
	- [`/Zongsoft/Framework/hosting/web/.deploy`](https://github.com/Zongsoft/Framework/tree/master/hosting/web/.deploy)


## Format Specification

The deployment file is a plain text file in `.ini` format, and its content consists of **Paragraph**(`Section`) and **Entry**(`Entry`) enclosed in square brackets. The **paragraph** part represents the destination directory of deployment, and the **Entry** part represents the source file path to be deployed. The source file path supports three wildcard matching: `*`, `?` and `**`.

**Paragraph** and **Entry** values both support variable references in the format of dollar sign followed by parentheses `$(...)` or double percent signs `%...%`, the referenced variable is the deployment Option parameters passed in by the command line or environment variables. For the specific effect, please refer to the content of the above deployment file.

**Notice:**
If an entry starts with a `!` exclamation mark, it means to delete the file specified by the entry in the deployment target location.

### Variables

This tool will sequentially load the environment variables, the contents of the `appsettings.json` file of the deployed application, and the command options for calling this tool into the variable set. If the variable has the same name, the value loaded later will overwrite the value of the variable with the same name loaded before. **Note:** Variable names are not case sensitive.

- If a property named `ApplicationName` is defined in `appsettings.json`, you can use `application` as a variable alias for that property.
- The variable named `Framework` represents the .NET *TargetFramework* identity, which is defined in https://learn.microsoft.com/en-us/dotnet/standard/frameworks

### filtering

The part enclosed by `<` and `>` at the end of the entry is the filter condition, and entries that do not meet the filter criteria will be ignored.

Multiple conditions are supported. Each condition consists of a variable name and the comparison values, If the variable name starts with `!`, it means that the matching result of the condition is negated; If you are comparing multiple values, separate them with commas. As follows:

```plaintext
../.deploy/options/$(cloud)/app.$(environment).option       = web.option    <application>
../.deploy/options/$(cloud)/app.$(environment).option       = web.option    <!application>
../.deploy/options/$(cloud)/app.$(environment)-debug.option = web.option    <preview:A,B,C>
../.deploy/options/$(cloud)/app.$(environment)-debug.option = web.option    <!preview:X,Y,Z>
../.deploy/options/$(cloud)/app.$(environment)-debug.option = web.option    <application | debug:on>
../.deploy/options/$(cloud)/app.$(environment)-debug.option = web.option    <!application & !debug:on>
```

> 1. `<application>` means that there is a variable named `application` (*Regardless of its content*), then the result is true.
> 2. `<!application>` means that there is no variable named `application` (*Regardless of its content*), then the result is true.
> 3. `<preview:A,B,C>` means that the value of the variable named `preview` is any one of "`A`, `B`, `C`" (*Ignoring case*), then The result is true.
> 4. `<!preview:X,Y,Z>` means that the value of the variable named `preview` is not any one of "`X`, `Y`, `Z`" (*Ignoring case*), then the result is true.
> 5. `<application | debug:on>` indicates that there is a variable named `application` (*Regardless of its content*) **OR** a variable named `debug` is `on` (*Ignoring case*), the result is true.
> 6. `<!application & !debug:on>` means that there is no variable named `application` (*Regardless of its content*) **AND** the variable named `debug` is not `on`(*Ignoring case*), the result is true.

Supports matching and version comparison of *TargetFramework*. If *TargetFramework* ends with `^`, it means that the version of the current deployment *TargetFramework* must be greater than or equal to this version, as follows:

```plaintext
%NUGET_PACKAGES%/mysql.data/8.1.0/lib/netstandard2.1/*.dll     <framework:net7.0^>
%NUGET_PACKAGES%/mysql.data/6.10.9/lib/netstandard2.0/*.dll    <framework:net5.0,net6.0>
```

## The tool setup

- List tools
```bash
dotnet tool list
dotnet tool list -g
```

- Install tool
```bash
dotnet tool install zongsoft.tools.deployer -g
```

- Upgrade tool
```bash
dotnet tool update zongsoft.tools.deployer -g
```

- Uninstall tool
```bash
dotnet tool uninstall zongsoft.tools.deployer -g
```


## Deploy

- Execute the default deployment in the host(target) directory:
```bash
dotnet deploy -edition:Debug -framework:net7.0
```

- If the host(target) directory does not have a default deployment file (`.deploy`), you must manually specify the deployment file name (multiple deployment files are supported):
```bash
dotnet deploy -edition:Debug -framework:net7.0 MyProject1.deploy MyProject2.deploy MyProject3.deploy
```

- For the convenience of deployment, you can create a corresponding edition of the deployment script files in the host(target) project, for example:
	- deploy-debug.cmd
		> `dotnet deploy -edition:Debug -framework:net7.0`
	- deploy-release.cmd
		> `dotnet deploy -edition:Release -framework:net7.0`

### Nuget Packages
If the deployment entry is library files in the Nuget package directory, it will preferentially match the library files of the *TargetFramework* version specified by the `Framework` variable.

Assuming `Framework` variable is `net7.0`, when a deployment file has the following deployment entry:
```ini
%NUGET_PACKAGES%/mysql.data/8.1.0/lib/netstandard2.1/*.dll
```

When the `mysql.data` in the Nuget package directory contains the `net7.0` target framework version, use the library files of the target framework version, otherwise use the library files of the `netstandard2.1` target framework version specified in the deployment entry.
