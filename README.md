# Zongsoft.Utilities.Deployer

	这是一个应用部署工具，通过指定的部署配置文件来驱动工具复制文件到特定目录结构中。

## 使用方法

譬如在 `Automao.Web.Launcher` 这个Web宿主程序根目录下有一个名为 `deploy.ini` 的部署文件，其内容如下所示：

``` ini
[plugins]
/Zongsoft/Zongsoft.Plugins/Main.plugin
/Zongsoft/Zongsoft.Web.Plugins/Web.plugin

[plugins views]
/Zongsoft/Zongsoft.Web.Plugins/src/Mvc/Views/*

[plugins Zongsoft.Externals Json]
/Zongsoft/Zongsoft.Externals.Json/src/Zongsoft.Externals.Json.plugin
/Zongsoft/Zongsoft.Externals.Json/src/Zongsoft.Externals.Json.option
/Zongsoft/Zongsoft.Externals.Json/src/bin/$(Edition)/Zongsoft.Externals.Json.*
/Zongsoft/Zongsoft.Externals.Json/src/bin/$(Edition)/Newtonsoft.*

[plugins Zongsoft.Externals Redis]
/Zongsoft/Zongsoft.Externals.Redis/src/Zongsoft.Externals.Redis.plugin
/Zongsoft/Zongsoft.Externals.Redis/src/Zongsoft.Externals.Redis.option
/Zongsoft/Zongsoft.Externals.Redis/src/bin/$(Edition)/Zongsoft.Externals.Redis.*
/Zongsoft/Zongsoft.Externals.Redis/src/bin/$(Edition)/ServiceStack.*

[plugins Automao.Data]
/Automao/Automao.Data/src/Automao.Data.plugin
/Automao/Automao.Data/src/Automao.Data.option
/Automao/Automao.Data/src/bin/$(Edition)/Automao.Data.*

[plugins Automao.Web]
../Automao.Web/src/Automao.Web.plugin
../Automao.Web/src/Automao.Web.option
../Automao.Web/src/bin/Automao.Web.*

[plugins Automao.Web views]
../Automao.Web/src/views/*

[plugins Automao.Common]
../Automao.Common/src/Automao.Common.plugin
../Automao.Common/src/Automao.Common.option
../Automao.Common/src/automao.mapping
../Automao.Common/src/bin/$(Edition)/Automao.Common.*

../Automao.Common.Web/src/Automao.Common.Web.plugin
../Automao.Common.Web/src/Automao.Common.Web.option
../Automao.Common.Web/src/bin/Automao.Common.Web.*

[plugins Automao.Common views]
../Automao.Common.Web/src/views/*

[plugins Automao.Cashing]
../Automao.Cashing/src/Automao.Cashing.plugin
../Automao.Cashing/src/Automao.Cashing.option
../Automao.Cashing/src/bin/$(Edition)/Automao.Cashing.*

../Automao.Cashing.Web/src/Automao.Cashing.Web.plugin
../Automao.Cashing.Web/src/Automao.Cashing.Web.option
../Automao.Cashing.Web/src/bin/Automao.Cashing.Web.*

[plugins Automao.Cashing views]
../Automao.Cashing.Web/src/views/*

[plugins Automao.Customers]
../Automao.Customers/src/Automao.Customers.plugin
../Automao.Customers/src/Automao.Customers.option
../Automao.Customers/src/bin/$(Edition)/Automao.Customers.*

../Automao.Customers.Web/src/Automao.Customers.Web.plugin
../Automao.Customers.Web/src/Automao.Customers.Web.option
../Automao.Customers.Web/src/bin/Automao.Customers.Web.*

[plugins Automao.Customers views]
../Automao.Customers.Web/src/views/*

[plugins Automao.Maintenances]
../Automao.Maintenances/src/Automao.Maintenances.plugin
../Automao.Maintenances/src/Automao.Maintenances.option
../Automao.Maintenances/src/bin/$(Edition)/Automao.Maintenances.*

../Automao.Maintenances.Web/src/Automao.Maintenances.Web.plugin
../Automao.Maintenances.Web/src/Automao.Maintenances.Web.option
../Automao.Maintenances.Web/src/bin/Automao.Maintenances.Web.*

[plugins Automao.Maintenances views]
../Automao.Maintenances.Web/src/views/*

[plugins Automao.Marketing]
../Automao.Marketing/src/Automao.Marketing.plugin
../Automao.Marketing/src/Automao.Marketing.option
../Automao.Marketing/src/bin/$(Edition)/Automao.Marketing.*

../Automao.Marketing.Web/src/Automao.Marketing.Web.plugin
../Automao.Marketing.Web/src/Automao.Marketing.Web.option
../Automao.Marketing.Web/src/bin/Automao.Marketing.Web.*

[plugins Automao.Marketing views]
../Automao.Marketing.Web/src/views/*

[plugins Automao.Rescues]
../Automao.Rescues/src/Automao.Rescues.plugin
../Automao.Rescues/src/Automao.Rescues.option
../Automao.Rescues/src/bin/$(Edition)/Automao.Rescues.*

../Automao.Rescues.Web/src/Automao.Rescues.Web.plugin
../Automao.Rescues.Web/src/Automao.Rescues.Web.option
../Automao.Rescues.Web/src/bin/Automao.Rescues.Web.*

[plugins Automao.Rescues views]
../Automao.Rescues.Web/src/views/*

[plugins Automao.Externals Alipay]
../Automao.Externals.Alipay/src/Automao.Externals.Alipay.plugin
../Automao.Externals.Alipay/src/Automao.Externals.Alipay.option
../Automao.Externals.Alipay/src/bin/$(Edition)/Automao.Externals.Alipay.*

../Automao.Externals.Alipay.Web/src/Automao.Externals.Alipay.Web.plugin
../Automao.Externals.Alipay.Web/src/Automao.Externals.Alipay.Web.option
../Automao.Externals.Alipay.Web/src/bin/Automao.Externals.Alipay.Web.*

[plugins Automao.Externals Alipay views]
../Automao.Externals.Alipay.Web/src/views/*

[plugins Automao.Externals WeChat]
../Automao.Externals.WeChat/src/Automao.Externals.WeChat.plugin
../Automao.Externals.WeChat/src/Automao.Externals.WeChat.option
../Automao.Externals.WeChat/src/bin/$(Edition)/Automao.Externals.WeChat.*

../Automao.Externals.WeChat.Web/src/Automao.Externals.WeChat.Web.plugin
../Automao.Externals.WeChat.Web/src/Automao.Externals.WeChat.Web.option
../Automao.Externals.WeChat.Web/src/bin/Automao.Externals.WeChat.Web.*

[plugins Automao.Externals WeChat views]
../Automao.Externals.WeChat.Web/src/views/*

```

同时，在该部署文件的同一目录中，分别有 `deploy-debug.bat` 和 `deploy-release.bat` 这两个脚本文件，其内容分别如下：

``` DOS
\Zongsoft\Zongsoft.Utilities.Deployer\src\bin\Debug\Zongsoft.Utilities.Deployer.exe -edition:Debug "deploy.ini"
```

``` DOS
\Zongsoft\Zongsoft.Utilities.Deployer\src\bin\Debug\Zongsoft.Utilities.Deployer.exe -edition:Release "deploy.ini"
```

-------

以下是一个终端应用程序的部署文件的大致内容，仅供参考：
```
[bin $(Edition) plugins]
/Zongsoft/Zongsoft.Plugins/Main.plugin
/Zongsoft/Zongsoft.Terminals.Plugins/Terminals.plugin

[bin $(Edition) plugins Zongsoft.Externals Json]
/Zongsoft/Zongsoft.Externals.Json/src/Zongsoft.Externals.Json.plugin
/Zongsoft/Zongsoft.Externals.Json/src/Zongsoft.Externals.Json.option
/Zongsoft/Zongsoft.Externals.Json/src/bin/$(Edition)/Zongsoft.Externals.Json.*
/Zongsoft/Zongsoft.Externals.Json/src/bin/$(Edition)/Newtonsoft.*

[bin $(Edition) plugins Zongsoft.Externals Redis]
/Zongsoft/Zongsoft.Externals.Redis/src/Zongsoft.Externals.Redis.plugin
/Zongsoft/Zongsoft.Externals.Redis/src/Zongsoft.Externals.Redis.option
/Zongsoft/Zongsoft.Externals.Redis/src/bin/$(Edition)/Zongsoft.Externals.Redis.*
/Zongsoft/Zongsoft.Externals.Redis/src/bin/$(Edition)/ServiceStack.*
```
