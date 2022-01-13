Hyper-V Launcher
----------------

Hyper-V Launcher is an application which makes it easier to access and manage your local Hyper-V Virtual Machines. 

Main Features
-------------

* Create Desktop shortcuts for Virtual Machines
* Create Start Menu shortcuts for Virtual Machines
* Launch Virtual Machines from the System Tray
* Automatically pause or shut down a Virtual Machine when its window is closed
* Automatically create a shortcut when a Virtual Machine is created in Hyper-V
* Shortcuts are automatically cleaned up if a Virtual Machine is deleted

![Main Window](./Images/MainWindow.png?raw=true)

How to Install and Run Hyper-V Launcher
---------------------------------------
#### Download

The Windows Installer can be downloaded from [here](https://github.com/andrezammit/hypervlauncher/releases).

#### Usage 

Hyper-V Launcher is configured through its main application which can be started from the Start Menu. Shortcuts and settings are managed through this application.

#### Creating Shortcuts

![Virtual Machines Page](./Images/VirtualMachines.png?raw=true)

The Virtual Machines page shows all available Hyper-V Virtual Machines available on the system. Select one and click on "Create Shortcut".

![Virtual Machines Page](./Images/CreateShortcut.png?raw=true)

The shortcut dialog allows you to change the shortcut's name and select whether Desktop and Start Menu shortcuts should be created.
You can also select an action which should be executed after the Virtual Machine window is closed.

#### Launching Shortcuts

![Main Window](./Images/MainWindow.png?raw=true)

Created shortcuts can be launched from the Hyper-V Launcher window itself through the Shortcuts page. 

![System Tray Menu](./Images/TrayApp.png?raw=true)

They are also available from the System Tray application context menu.

#### Automatic Virtual Machine Detection

If enabled through the Settings page, Hyper-V Launcher may detect any Virtual Machines which is created or deleted from the system.

![Notifications](./Images/Notifications.png?raw=true)

When a new Virtual Machine is detected, a Windows notification is presented either allowing you to create a shortcut or notifying you that a shortcut was already created.

When a Virtual Machine is deleted, Hyper-V Launcher automatically cleans up all its configured shortcuts.

License
-------

MIT License

Copyright (c) 2022 Andre Zammit

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
