# Esp8266Dev
COM wrapper for communication between VS Code and ESP8266/NodeMCU Lua

### Typical using
- Integration with your favorite text editor or developing tool and habits.
- Automation sequences.

### About project
- I needed to make the development for the NodeMCU / Lua platform *more enjoyable*.
- I was inspired by [ESPlorer] (https://esp8266.ru/esplorer).

### How it works
The application ```EspComConsole.exe``` connects to the ESP COM port and makes it available to others over *named pipe*. It also provides terminal input.

The application ```EspComCom.exe``` provides a command line interface between EspComConsole and another application, such as a text editor.

The application ```EspComDirect.exe``` provides direct upload to ESP outside ```EspComConsole.exe```.

### Using

#### 1. Start the EspComConsole
```EspComConsole.exe /PIPE:esppipe```

Example:
```
    Microsoft Windows [Version 10.0.18363.592]
    (c) 2019 Microsoft Corporation. Všechna práva vyhrazena.

    C:\Esp8266Dev>EspComConsole /PIPE:esppipe
    EspComConsole - version 1.0.7328.19274

    Valid port(s): COM4
    Selected port: COM4

    Pipe server 'esppipe' waiting for connection.
```
```
    help
```
```
    Help of EspComConsole
    
    clear      - Clears the screen.
    cpos       - Console position. (0=normal; 1=top.)
    exit       - Quits the EspComConsole.EXE program.
    help       - This help.
    ls         - List of file.
    lscom      - List of communication ports.
    rm         - Remove file. Parameter <filename>
    type       - Displays file on screen. Parameter <filename>
    upload     - Upload file(s). Parameter [filename]
```

#### 2. Upload file over EspComConsole
Type ```upload <fileName>``` in console (```EspComConsole.exe```). If you want to use the open file dialog box, type only ```upload```.

Example:
```
    upload test1.lua
    Upload: "test1.lua"
    ---[ START ]---
    file.remove('test1.lua')
    file.open('test1.lua', 'w+')
    w = file.writeline
    ....................................
    file.close()
    ---[ END ]---
    36 line(s) sended
```

#### 3. Upload file over EspComCom
Type ```EspComCom /PIPE:<pipename> /UPLOAD:<fileName>``` in command line.

Example:
```
    C:\esp8266Dev>EspComCom /PIPE:esppipe /UPLOAD:test2.lua
    EspComCom - version 1.0.7328.25226

    Connect to pipe 'esppipe' successed.
    Ok
    -- 3. Testovací soubor pro EspComCom

    C:\Esp8266Dev>
```

#### 4. Run Lua script over EspComCom
Type ```EspComCom /PIPE:<pipename> /RUN:<fileName>``` in command line.

Example:
```
    C:\Esp8266Dev>EspComCom /PIPE:esppipe /dofile:test2.lua
    EspComCom - version 1.0.7328.25226

    Connect to pipe 'esppipe' successed.
    Ok
    
    C:\Esp8266Dev>     
```
#### 5. Run command over EspComCom
Type ```EspComCom /PIPE:<pipename> /CMD:<command>``` in command line.

Example:
```
    C:\dev\PARI\Esp8266Dev\install>EspComCom /PIPE:esppipe /CMD:a=30;print(a*a)
    EspComCom - version 1.0.7328.25226

    Connect to pipe 'esppipe' successed.
    Ok

    C:\dev\PARI\Esp8266Dev\install>
```
Result in console (```EspComConsole.exe```).
```
Pipe client connected.
Command: CMD
a=30;print(a*a)
Pipe server 'esppipe' waiting for connection.
900
```

### Installation
Copy contains of ```install``` directory to your own directory and enjoy it!

You can also customize applications by editing configuration files (```.exe.config```) or improving this project.
