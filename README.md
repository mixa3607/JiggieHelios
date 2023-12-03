# JiggieHelios
![license](https://img.shields.io/github/license/mixa3607/JiggieHelios?style=flat-square)
![workflow](https://img.shields.io/github/actions/workflow/status/mixa3607/JiggieHelios/push.yml?style=flat-square)
![latest release](https://img.shields.io/github/v/release/mixa3607/JiggieHelios?style=flat-square)

ToolBox for https://jiggie.fun puzzles

# Features
- Capture all traffic to jcap
- Convert jcap to json
- Render jcap to timelapse (ffmpeg+skiasharp)
- Bot (WIP)
- Easily extensible codebase
- Crossplatform
- Ready2run (*if ffmpeg in path)

# Download
Latest build in [Actions](../../actions)

Releases in [Releases](../../releases)

# HowTo
## Basic usage
Capture puzzle solving to ./jcaps directory and wait 30s after finish then convert to video with duration 1:30
Result sample see at [samples/ee8nLJ](samples/ee8nLJ) 
```
./JiggieHelios.Cli capture -i ee8nLJ --post-delay 00:00:30 -o ./jcaps
./JiggieHelios.Cli jcap2video --target-dur 00:01:30 -i ./jcaps/ee8nLJ_2023.12.02-12.36.39.jcap
```

## Full help:
```console
$ ./JiggieHelios.Cli --help
Usage -  <action> -options

GlobalOption              Description
Help (-?, -h, --help)     Shows this help
Version (-v, --version)   Show version info

Actions

  Capture -options -

    Option                             Description
    WaitComplete* (--wait)             Finish capturing after puzzle completed
    PostCompleteDelay (--post-delay)   Delay before complete capturing after puzzle completed
    RoomId* (-i)                       Room id
    OutDirectory (-o)                  Output directory [Default='./']
    DownloadImages (-d)                Download images [Default='True']
    UserColor (--color)                [Default='#ffa5a5']
    UserLogin (--login)                [Default='HELIOS-cap']

  Jcap2Json -options -

    Option           Description
    JcapFile* (-i)   Jcap file
    OutFile (-o)     Output file. If - print to log stream

  Jcap2Video -options -

    Option                            Description
    JcapFile* (-i)                    Jcap file
    OutFile (-o)                      output file
    Threads (-t)                      [Default='5']
    ImagesDirectory (--img-dir)
    FramesPerJob (--frames-per-job)   [Default='300']
    Fps (--fps)                       [Default='30']
    SpeedMultiplier (--speedx)
    TargetDuration (--target-dur)     Dynamically calculate speedx. If set speedx will be ignored
    CanvasSize (--canvas-size)        [Default='1920x0']
    CanvasFill (--canvas-fill)        [Default='#5f9ea0']
    FfmpegInArgs (--ffmpeg-in)        [Default='']
    FfmpegOutArgs (--ffmpeg-out)      [Default='-c:v libx264 -vf scale=1920:-2,format=yuv420p']

  Bot -options -

    Option                             Description
    WaitComplete (--wait)              Exit after puzzle completed
    PostCompleteDelay (--post-delay)   Delay before exit after puzzle completed
    RoomId* (-i)                       Room id
    Secret* (-s)                       Room secret
    Admin* (-a)                        Room administrator
    StateFile (--state)                Bot state file [Default='bot.json']
    UserColor (--color)                [Default='#ffa5a5']
    UserLogin (--login)                [Default='HELIOS-bot']
```

## `appsettings.json` and `appsettings.<>.json`
Default values and some settings can be configured with appsettings files. By default used `appsettings.json` and `appsettings.Production.json`. `Production` is value of `DOTNET_ENVIRONMENT` env variable. 
```sh
> $env:DOTNET_ENVIRONMENT="AnyEnvName" #for windows
> export DOTNET_ENVIRONMENT=AnyEnvName #for NIX
```

FFmpeg bin can be set in appsettings if not in PATH. See `appsettings.Custom1.json` file.
```json
  "ffmpeg": {
    "BinaryFolder": "path/to/ffmpeg/bin",
    "TemporaryFilesFolder":"path/to/tmp/files"
  }
```

With appsettings default values for cli actions may be overriden.
```json
  "Cli": {
    "DefaultValues": {
      "Jcap2Video": {                    //action name
        "Threads": 36                      //arg value
      },
      "Capture": {                       //action name
        "WaitComplete": true,              //arg value
        "PostCompleteDelay": "00:00:30",   //arg value
        "OutDirectory": "./jcaps",         //arg value
        "DownloadImages": true,            //arg value
        "UserColor": "#13a10e"             //arg value
      }
    }
  }
```