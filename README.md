# The Zongsoft Deployment Tool

![license](https://img.shields.io/github/license/Zongsoft/Zongsoft.Tools.Deployer) ![download](https://img.shields.io/nuget/dt/Zongsoft.Tools.Deployer) ![version](https://img.shields.io/github/v/release/Zongsoft/Zongsoft.Tools.Deployer?include_prereleases) ![github stars](https://img.shields.io/github/stars/Zongsoft/Zongsoft.Tools.Deployer?style=social)

README: [English](README.md) | [简体中文](README-zh_CN.md)

-----

## Abstraction

This is an application deployment tool that instructs the deployment tool to copy specific files to the destination location by specifying the deployment file.

It is recommended to define a default deployment file named `.deploy` in the deployment project directory, and the deployment file is a plain text file in `.ini` format.

## Format Specification

The deployment file is a plain text file in `.ini` format, and its content consists of **Section**(`Paragraph`) and **Entry**(`Entry`) enclosed in square brackets, the **Section** part represents the destination directory of deployment.

The **Section** and **Entry** values both support variable references in the format of dollar sign followed by parentheses `$(...)` or double percent signs `%...%`, the referenced variable is the deployment options passed in by the command line or environment variables.

Each entry consists of **KEY** and **VALUE** parts separated by an equal sign _(`=`)_, and the **VALUE** part is optional.

- The **KEY** part consists of _Parser-Name_ and _Parser-Argument_, separated by a colon _(`:`)_;
	- **_Parser-Name_**: If missing, the default path parser is used, except for `nuget` and `delete` parsers.
	- **_Parser-Argument_**: Parsed by the specified parser, please refer to _**P**arser **A**rgument_ below for details.

- The **VALUE** part consists of _Destination_ and _Filtering_.
	- **_Destination_**: Indicates the destination path for deployment. If missing, the destination directory is specified by the **Section**, and the destination file name is the same as the source file.
	- **_Filtering_**: Indicates the preconditions for parsing, please refer to _**F**iltering_ below for details.

### Parser Argument

#### Path Parser

Default parser(_**U**nnamed_), which means copy the source file indicated by the _Parser-Argument_ to the destination location.

The _Parser-Argument_ represents the path of the source file to be deployed, the source file path supports `*`, `?` and `**` wildcards, the `**` means multi-level directory matching.

#### Delete Parser

The parser name is `delete` or `remove`, which means delete the specified destination file.

The _Parser-Argument_ represents the destination file to be deleted, and the full path of the destination file is a combination of the directory specified in the **Section** and the _Parser-Argument_.

> 🚨 **Note:** This parser does not support the _Destination_ part, so this part cannot be defined.

##### Examples

Delete the `Zongsoft.Messaging.Mqtt.option` file from the `~/plugins/zongsoft/messaging/mqtt` directory in the target location.

```ini
[plugins zongsoft messaging mqtt]
nuget:Zongsoft.Messaging.Mqtt
delete:Zongsoft.Messaging.Mqtt.option
```

> 💡 **Tip:** The deployment file in the `nuget:Zongsoft.Messaging.Mqtt` package in the example contains a default configuration file(i.e. `Zongsoft.Messaging.Mqtt.option`), but it is not needed in the real project, so the configuration file is removed later.

#### NuGet Parser

The parser name is: `nuget`, which means download the NuGet package and perform the deployment.

The format of _Parser-Argument_: `package@version/path`, where `@version` and `/{path}` parts are optional. If the version part is unspecified or it is `latest`, it means the latest version. If the path part is unspecified, it defaults to the `.deploy` file in the package.

> 💡 **Tip:** _**Z**ongsoft_'s NuGet package has a deployment file named `.deploy` in it's root directory, and the `artifacts` directory in the package includes its plugin files(`*.plugin`)_(required, one or more)_, configuration files(`*.option`), the mapping files(`*.mapping`) for [_**Z**ongsoft.**D**ata_ ORM](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data), and other ancillary files.

> 💡 **Note:** The variable named `NuGet_Server` defines the NuGet package source for this parser.
> If undefined then `https://api.nuget.org/v3/index.json` is used as its default value.

##### Examples

- Get the latest version of the `Zongsoft.Plugins` NuGet package and deploy the `Main.plugin` plugin file in it's `/plugins` directory to the destination `~/plugins` directory.
	> ```ini
	> [plugins]
	> nuget:Zongsoft.Plugins/plugins/Main.plugin
	> ```

- Get the `6.0.0` version of the `Zongsoft.Data` NuGet package and execute the `.deploy` deployment file in the package.
	> ```ini
	> [plugins zongsoft data]
	> nuget:Zongsoft.Data@6.0.0
	> [plugins zongsoft data]
	> nuget:Zongsoft.Data@6.0.0/.deploy
	> ```
	> **Note:** The above two writing styles have the same effect.

### Filtering

The part enclosed by `<` and `>` at the end of the entry is the filter condition, and entries that do not meet the filter criteria will be ignored.

Multiple conditions are supported. Each condition consists of a variable name and the comparison values, If the variable name starts with `!`, it means that the matching result of the condition is negated; If you are comparing multiple values, separate them with commas. As follows:

```plaintext
../.deploy/$(scheme)/options/app.$(environment).option       = web.option    <application>
../.deploy/$(scheme)/options/app.$(environment).option       = web.option    <!application>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <preview:A,B,C>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <!preview:X,Y,Z>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <application | debug:on>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <!application & !debug:on>
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

## Variables

This tool will sequentially load the environment variables, the contents of the `appsettings.json` file of the deployed application, and the command options for calling this tool into the variable set. If the variable has the same name, the value loaded later will overwrite the value of the variable with the same name loaded before. **Note:** Variable names are not case sensitive.

- If a property named `ApplicationName` is defined in `appsettings.json`, you can use `application` as a variable alias for that property.
- The variable named `Framework` represents the .NET *TargetFramework* identity, which is defined in https://learn.microsoft.com/en-us/dotnet/standard/frameworks

NuGet-related parameters can be specified via command options or environment variables:
- `NuGet_Server` indicates the NuGet server information, the default value is: `https://api.nuget.org/v3/index.json`.
- `NuGet_Packages` indicates the directory of NuGet packages, the default value is: `%USERPROFILE%/.nuget/packages`.

## Setup

- List tools

```bash
dotnet tool list
dotnet tool list -g
```

- Install tool

```bash
dotnet tool install -g zongsoft.tools.deployer
```

- Upgrade tool

```bash
dotnet tool update -g zongsoft.tools.deployer
```

- Uninstall tool

```bash
dotnet tool uninstall -g zongsoft.tools.deployer
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

### Command options

- `verbosity` option
	- `quiet` Displays only the necessary output information, usually only error messages.
	- `normal` Displays warning and error messages, if this command option is not specified, it is the default.
	- `detail` Displays all output messages, this option can be enabled when troubleshooting.
- `overwrite` option
	- `alway` Always copy and overwrite the destination file.
	- `never` Copies the destination file only if it does not exist.
	- `newest` Deploys file copying only if the last modification time of the source file is later than or equal to the last modification time of the destination file. if this command option is not specified, it is the default.
- `destination` option
	> The specified deployment destination directory. If this command option is not specified, it defaults to the current directory.

### Nuget Packages
If the deployment entry is library files in the Nuget package directory, it will preferentially match the library files of the *TargetFramework* version specified by the `Framework` variable.

Assuming `Framework` variable is `net7.0`, when a deployment file has the following deployment entry:
```ini
%NUGET_PACKAGES%/mysql.data/8.1.0/lib/netstandard2.1/*.dll
```

When the `mysql.data` in the Nuget package directory contains the `net7.0` target framework version, use the library files of the target framework version, otherwise use the library files of the `netstandard2.1` target framework version specified in the deployment entry.

## Others

### Reference examples

- The NuGet Packages
	- [`Zongsoft.Data`](https://github.com/Zongsoft/Framework/blob/master/Zongsoft.Data/src/Zongsoft.Data.deploy) [(NuGet)](https://www.nuget.org/packages/Zongsoft.Data)
	- [`Zongsoft.Data.MySql`](https://github.com/Zongsoft/Framework/blob/master/Zongsoft.Data/drivers/mysql/Zongsoft.Data.MySql.deploy) [(NuGet)](https://www.nuget.org/packages/Zongsoft.Data.MySql)
	- [`Zongsoft.Security`](https://github.com/Zongsoft/Framework/blob/master/Zongsoft.Security/src/Zongsoft.Security.deploy) [(NuGet)](https://www.nuget.org/packages/Zongsoft.Security)
	- [`Zongsoft.Security.Web`](https://github.com/Zongsoft/Framework/blob/master/Zongsoft.Security/api/Zongsoft.Security.Web.deploy) [(NuGet)](https://www.nuget.org/packages/Zongsoft.Security.Web)
	- [`Zongsoft.Administratives`](https://github.com/Zongsoft/Administratives/blob/master/src/Zongsoft.Administratives.deploy) [(NuGet)](https://www.nuget.org/packages/Zongsoft.Administratives)
	- [`Zongsoft.Administratives.Web`](https://github.com/Zongsoft/Administratives/blob/master/src/api/Zongsoft.Administratives.Web.deploy) [(NuGet)](https://www.nuget.org/packages/Zongsoft.Administratives.Web)

- The hosting projects
	- [`daemon`](https://github.com/Zongsoft/hosting/blob/main/daemon/.deploy)
	- [`terminal`](https://github.com/Zongsoft/hosting/blob/main/terminal/.deploy)
	- [`web`](https://github.com/Zongsoft/hosting/blob/main/web/default/.deploy)
