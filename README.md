# Tofflon Wireless_Temp Test Version 😇

## 使用说明：

fork整个项目或者下载zip到本地，fork 完直接宇宙第一IDE VS编译。由于没有封装，exe在modbus_test/bin/release 下，也可以直接运行（怕你没环境）

先建立连接，然后初始化温控探头，才能开始操作

主界面探头温度变化和报表打印为独立按钮

Setting里的设置尽量不要去改

数据记录在sqlite里，不要去改

这玩意竟然能活过1.0版本了，我tm惊了！

**注意：**

    实时记录用完一定要停！！！！！！！！！！！！！！！

    软件必须和modbus直连，不能建立别的TCP连接，例如同时使用modbus Poll/Slave仿真

    遇到bug请记下你的操作然后report给我

    任何更新建议，请第一时间憋在肚子里，我不想听

    如果在你的牛逼操作下，把线程炸穿了，不要犹豫，Ctrl-Alt-Delete强杀 😆 

    实时曲线的缩放拖拽功能还在试验阶段。

## 更新内容

UI与操作逻辑重构

**旧版纪念**

![version1](/icon/1.png)

**新版截图**

![screenshot](/icon/snap1.png)

![screenshot](/icon/snap2.png)

![screenshot](/icon/snap3.png)

## 后续内容

1. 用户管理,用户操作权限分级，考虑数据库里开一个表

2. 时间插件（找不到好的，又不想给他但开一线程）

3. 连接状态检测（要不先这样）

4. 实时图线优化，别说，还真tm做出来了，后续考虑加入选取显示功能，区间选取的功能。

5. 未完待续，大概 😆