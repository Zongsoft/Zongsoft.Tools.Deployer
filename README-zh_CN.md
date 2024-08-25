# Zongsoft 部署工具

![license](https://img.shields.io/github/license/Zongsoft/Zongsoft.Tools.Deployer) ![download](https://img.shields.io/nuget/dt/Zongsoft.Tools.Deployer) ![version](https://img.shields.io/github/v/release/Zongsoft/Zongsoft.Tools.Deployer?include_prereleases) ![github stars](https://img.shields.io/github/stars/Zongsoft/Zongsoft.Tools.Deployer?style=social)

README: [English](https://github.com/Zongsoft/Zongsoft.Tools.Deployer/blob/master/README.md) | [简体中文](https://github.com/Zongsoft/Zongsoft.Tools.Deployer/blob/master/README-zh_CN.md)

-----

## 概述

这是一个应用部署工具，通过指定的部署文件来指示部署工具复制特定文件到目标位置。

建议在部署项目目录定义一个名为 `.deploy` 的默认部署文件，部署文件为 `.ini` 格式的纯文本文件。


### 参考范例

- 部署项目
	- [`/Zongsoft/Framework/Zongsoft.Data/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Data/drivers/mssql/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data/drivers/mssql/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Data/drivers/mysql/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Data/drivers/mysql/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Security/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Security/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Security/api/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Security/api/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Messaging.Mqtt/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Messaging.Mqtt/.deploy)
	- [`/Zongsoft/Framework/Zongsoft.Messaging.Kafka/.deploy`](https://github.com/Zongsoft/Framework/tree/master/Zongsoft.Messaging.Kafka/.deploy)

- 部署目标 *(宿主项目)*
	- [`/Zongsoft/hosting/terminal/.deploy`](https://github.com/Zongsoft/Framework/tree/master/hosting/terminal/.deploy)
	- [`/Zongsoft/hosting/web/.deploy`](https://github.com/Zongsoft/Framework/tree/master/hosting/web/.deploy)


## 格式

部署文件为 `.ini` 格式的纯文本文件，其内容由中括号包裹的**段落**(`Section`)和**条目**(`Entry`) 两种内容组成。其中**段落**部分表示部署的目标目录，而**条目**部分表示待部署的源文件路径，源文件路径支持 `*`、`?` 以及 `**` 三种通配符匹配。

**段落**和**条目**值均支持以美元符接圆括号 `$(...)` 或双百分号 `%...%` 格式的变量引用，引用的变量为部署命令传入的选项参数或环境变量，具体效果请参考上述部署文件内容。

### 变量

本工具会依次加载环境变量、部署应用程序的`appsettings.json`文件内容、调用本工具的命令选项到变量集中，如果有重名则后加载的会覆盖之前加载的同名变量值。注意：变量名不区分大小写。

- 如果 `appsettings.json` 中定义了名为 `ApplicationName` 的属性，则可以使用 `application` 作为该属性的变量别名。
- 名称为 `Framework` 的变量表示 .NET *目标框架* 标识，有关该 *目标框架* 标识的定义请参考：https://learn.microsoft.com/zh-cn/dotnet/standard/frameworks

可以通过命令选项或环境变量来指定 NuGet 相关参数：
- `NuGet_Server` 表示 NuGet 服务器信息，默认值为：`https://api.nuget.org/v3/index.json`
- `NuGet_Packages` 表示 NuGet 包的目录，默认值为：`%USERPROFILE%/.nuget/packages`

### 过滤

在条目的尾部以 `<` 和 `>` 括起来的部分即为过滤条件，不满足过滤条件的条目会被忽略。

支持多个条件组合，每个条件由变量名和比较值组成，变量名若以 `!` 打头则表示对该条件的匹配结果取反；如果要比对多个值则以逗号分隔。如下所示：

```plaintext
../.deploy/$(scheme)/options/app.$(environment).option       = web.option    <application>
../.deploy/$(scheme)/options/app.$(environment).option       = web.option    <!application>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <preview:A,B,C>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <!preview:X,Y,Z>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <application | debug:on>
../.deploy/$(scheme)/options/app.$(environment)-debug.option = web.option    <!application & !debug:on>
```

> 1. `<application>` 表示存在名为 `application` 的变量(*不论其内容*)，则结果为真。
> 2. `<!application>` 表示不存在名为 `application` 的变量(*不论其内容*)，则结果为真。
> 3. `<preview:A,B,C>` 表示名为 `preview` 的变量值为“`A`,`B`,`C`”(*忽略大小写*)中的任何一个，则结果为真。
> 4. `<!preview:X,Y,Z>` 表示名为 `preview` 的变量值不是“`X`,`Y`,`Z`”(*忽略大小写*)中的任何一个，则结果为真。
> 5. `<application | debug:on>` 表示存在名为 `application` 的变量(*不论其内容*) **或者** 名为 `debug` 的变量值为 `on`(*忽略大小写*)，则结果为真。
> 6. `<!application & !debug:on>` 表示不存在名为 `application` 的变量(*不论其内容*) **并且** 名为 `debug` 的变量值不是 `on`(*忽略大小写*)，则结果为真。

支持对 *目标框架* 进行匹配及版本比较，如果 *目标框架* 以`^`符结尾则表示当前部署 *目标框架* 版本必须大于或等于该版本，如下所示：

```plaintext
%NUGET_PACKAGES%/mysql.data/8.1.0/lib/netstandard2.1/*.dll     <framework:net7.0^>
%NUGET_PACKAGES%/mysql.data/6.10.9/lib/netstandard2.0/*.dll    <framework:net5.0,net6.0>
```

## 安装

- 查看工具
```bash
dotnet tool list
dotnet tool list -g
```

- 首次安装
```bash
dotnet tool install zongsoft.tools.deployer -g
```

- 升级更新
```bash
dotnet tool update zongsoft.tools.deployer -g
```

- 卸载
```bash
dotnet tool uninstall zongsoft.tools.deployer -g
```


## 执行

- 在目标(宿主)目录执行默认部署：
```bash
dotnet deploy -edition:Debug -framework:net7.0
```

- 如果目标(宿主)目录没有默认部署文件(`.deploy`)，则必须手动指定部署文件名(支持多个部署文件)：
```bash
dotnet deploy -edition:Debug -framework:net7.0 MyProject1.deploy MyProject2.deploy MyProject3.deploy
```

- 为了部署方便可以在目标(宿主)项目创建相应版本的部署脚本文件，譬如：
	- deploy-debug.cmd
		> `dotnet deploy -edition:Debug -framework:net7.0`
	- deploy-release.cmd
		> `dotnet deploy -edition:Release -framework:net7.0`

### 命令选项

- `verbosity` 选项
	- `quiet` 只显示必要的输出信息，通常只显示错误信息。
	- `normal` 显示警示和错误信息，如果未指定该选项，其为默认值。
	- `detailed` 显示所有的输出信息，在排查问题时可以启用该选项。
- `overwrite` 选项
	- `alway` 始终复制并覆盖目标文件。
	- `never` 只有当目标文件不存在才复制。
	- `newest` 只有当源文件的最后修改时间晚于或等于目标文件的最后修改时间才执行文件复制部署，如果未指定该参数，其为默认值。
- `destination` 选项
	> 指定的部署目的目录，如果未指定该选项则默认为当前目录。

### Nuget 包
如果部署项为 Nuget 包目录下中的库文件，会优先匹配 `Framework` 变量指定的 *目标框架* 版本的库文件。

假设 `Framework` 变量为 `net7.0`，当某部署文件中有如下部署项：
```ini
%NUGET_PACKAGES%/mysql.data/8.1.0/lib/netstandard2.1/*.dll
```

当 Nuget 包目录下的 `mysql.data` 含有 `net7.0` 目标框架版本，则使用该目标框架版本的库文件，否则使用部署项中所指定的 `netstandard2.1` 目标框架版本的库文件。
