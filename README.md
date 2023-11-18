# cg4002-viz
## CG4002 B08 AY23/24 SEM 1 | Visualizer

Download Vuforia package manually. Make sure you have the file under cg4002-viz/Packages/com.ptc.vuforia.engine-10.18.4.tgz
<br/>
This file was >100MB so couldn't fit on github.

Open in Unity and build to Android phone.

`master` branch holds the latest version used (week 13).
<br/>
`wk13-stereo` branch holds the version used in week 13 demo.
<br/>
`wk12-stereo` branch holds the version used in week 12 demo.
<br/>
`wk9-stereo` branch holds the version used in week 9 demo.
<br/>
`fixed_dev` branch holds the version before conversion to stereoscopic view.
<br/>
`stereoscopic` branch holds an old stereoscopic version, used as reference.
<br/>
`development` branch not in use anymore cos i messed it up (oops).
<br/>

Important files:
<br/>
1) HudController.cs
- Contains logic for rendering HUD UI elements and AR effects.
2) MqttUnityClient.cs
- Connects to MQTT broker. Relays data between MQTT broker and HudController.
3) GameEngine.cs
- Contains the game logic used for week 6 subcomponent demo. Not used in gameplay mode anymore. Still used in debug mode for dev testing.
4) ChangeScene.cs
- Controls changing scenes between main menu, gameplay mode and debug mode.
5) QR_CODE_NEW_GREY.png
- The QR code image used as the opponent marker.




