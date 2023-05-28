---
title: 基于Hexo的个人博客搭建流程
categories:
  - 技术文档
tags:
  - Hexo
  - 博客搭建
date: 2023-05-28 17:31:25
---


## 简介
Hexo是一个基于Node.js的静态博客框架，它可以帮助用户快速构建、部署和维护静态博客网站。
Hexo的设计目标是简洁、快速和高效，使用户能够专注于写作而不必担心复杂的技术细节。

Hexo使用Markdown作为主要的写作格式，支持使用主题和插件来定制网站外观和功能。
用户可以通过自定义主题来创建独特的博客风格，也可以使用插件来添加额外的功能，如社交媒体分享、评论系统等。
 
本博客即基于[Hexo](https://hexo.io/zh-cn/)框架和[Fluid](https://github.com/fluid-dev/hexo-theme-fluid)主题一通爆改搭建而成。下面介绍从零开始搭建此博客的流程，仅供参考。

## 准备工作
- 拥有一台Windows / Mac / Linux系统的主机
- 拥有可用的文本编辑器（最好支持Markdown语法）
- 能够访问互联网
- 可用的[Github](https://github.com)账号

## 搭建步骤
### 1. 安装node环境
Hexo渲染静态页面需要依赖node.js，因此需要先安装此环境。可进入[node.js](https://nodejs.org/zh-cn)官方网站进行下载安装。  
![](1.jpg)
安装完成后进入终端命令行输入`node -v`命令查看node版本号。  
windows系统进入命令行方法：按下<kbd>win</kbd>+<kbd>R</kbd>键，在弹出的窗口中输入`cmd`,并敲回车。
![](2.jpg)
若显示正常，则node.js环境已经成功安装。
### 2. 安装Hexo
首先安装cnpm包管理器，打开终端执行命令：
``` bash
npm install -g cnpm
```
安装成功后即可开始安装Hexo框架，打开终端执行以下命令：
``` bash
cnpm install -g hexo-cli
```
默认会安装最新版本，安装完成后可输入`hexo -v`命令查看hexo版本号，验证是否安装成功。
### 3. 启动Hexo
在本地新建一个文件夹，用作博客项目的文件夹。如`D:\Hexo`。
建好文件夹后打开命令行，需要先切换至该目录。Linux系统下使用`cd 路径`命令来切换目录。Windows系统下首先输入盘符来切换磁盘，再使用`cd 路径`来切换目录。  
在该目录下使用`hexo init`命令来初始化一个博客项目。
指令示例如下：
``` bash
# Linux系统：
cd /root/hexo
hexo init

# Windows系统：
D:
cd D:\Hexo
hexo init
```
指令输入完毕后，应当看到该文件夹下生成了博客项目的目录结构。目录结构的大致说明如下：
> .  
├── _config.yml  
├── package.json  
├── scaffolds  
├── source  
|   ├── _drafts  
|   └── _posts  
└── themes

- _config.yml  
网站的配置信息，您可以在此配置大部分的参数。

- package.json  
应用程序的信息。EJS, Stylus 和 Markdown renderer 已默认安装，您可以自由移除。

- scaffolds  
模版文件夹。当您新建文章时，Hexo 会根据 scaffold 来建立文件。  
Hexo的模板是指在新建的文章文件中默认填充的内容。例如，如果您修改scaffold/post.md中的Front-matter内容，那么每次新建一篇文章时都会包含这个修改。

- source  
资源文件夹是存放用户资源的地方。除 _posts 文件夹之外，开头命名为 _ (下划线)的文件 / 文件夹和隐藏的文件将会被忽略。Markdown 和 HTML 文件会被解析并放到 public 文件夹，而其他文件会被拷贝过去。

- themes  
主题 文件夹。Hexo 会根据主题来生成静态页面。

在该目录命令行输入以下指令，即可刷新配置后，生成相应的静态网站，并开启本地4000端口的服务器。
``` bash
hexo clean
hexo g
hexo s
```
![](3.jpg)
看到如上图所示的提示信息，即代表博客已经在本地服务器启动。  
打开浏览器键入`http://localhost:4000/`回车，即可看到默认主题下的博客界面。
![](4.jpg)
自此，博客已经成功安装并启动成功。  
关于Hexo的更多指令，可以参考[Hexo指令官方文档](https://hexo.io/zh-cn/docs/commands)，文档中对各个指令有更为详细的介绍。

## 部署博客
待更新......