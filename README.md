# Night in the Woods Font Patcher

A BepInEx plugin for Night in the Woods that allows custom character sprites to be patched into the game's font system. This is useful for adding support for special characters (e.g., 'Ç', 'Ş') with custom animations and width adjustments.

---

## Features
- Load custom character sprites from JSON configuration
- Seamless integration with the `SpriteFont` system in Night in the Woods

---

## Requirements
- [BepInEx 6.x](https://builds.bepinex.dev/projects/bepinex_be)
- Night in the Woods (PC version)

---

## Installation

1. **Install BepInEx:**
   - Download and extract BepInEx 6.x into the root folder of your Night in the Woods installation.

2. **Get the Plugin:**
   - Place the compiled `FontPatcher.dll` into the following directory:
     ```
     <Game Directory>\BepInEx\plugins\FontPatcher\
     ```

3. **Configuration:**
   - Create the following folder structure for your configuration and assets:
     ```
     <Game Directory>\BepInEx\plugins\FontPatcher\config\
     <Game Directory>\BepInEx\plugins\FontPatcher\assets\chars\
     ```

   - Place your character configuration file as:
     ```
     <Game Directory>\BepInEx\plugins\FontPatcher\config\character_mappings.json
     ```

   - Example `character_mappings.json`:
     ```json
     {
       "Characters": [
         {
           "Character": "Ç",
           "SpriteName": "char_cedilla",
           "Width": 0.5
         },
         {
           "Character": "Ş",
           "SpriteName": "char_scedilla",
           "Width": 0.5
         }
       ]
     }
     ```

   - Place corresponding sprite assets in:
     ```
     <Game Directory>\BepInEx\plugins\FontPatcher\assets\chars\char_cedilla\
     <Game Directory>\BepInEx\plugins\FontPatcher\assets\chars\char_scedilla\
     ```
     
     Each folder should contain 3 frame images (64x64px):
     ```
     frame_0.png
     frame_1.png
     frame_2.png
     ```

---

## Running the Plugin

1. Launch Night in the Woods.  
2. BepInEx will initialize, and you should see logs related to the Font Patcher in the console.
3. Custom characters defined in your configuration should now appear in the game dialogue. (If the dialogue is patched.)

---

## Logs and Debugging

- Logs are saved in:
  ```
     <Game Directory>\BepInEx\LogOutput.log
  ```
- Look for entries related to `NITW Font Patcher` for information on character loading and potential errors.

---

## Screenshots
![Screenshot_2](https://github.com/user-attachments/assets/38804d54-3030-439a-ab8a-d44d945a4a80)
![Screenshot_1](https://github.com/user-attachments/assets/3582d287-bd2a-47ae-bd89-202d0a5fd75e)
![Sequence 01](https://github.com/user-attachments/assets/38f00061-0035-4324-9849-b9a9c569cba1)

---

## Building from Source

To build the NITW Font Patcher from source, follow these steps:

1. **Clone the Repository:**
    ```sh
    git clone https://github.com/SkipperSkipTR/NITW_FontPatcher.git
    cd NITW_FontPatcher
    ```

2. **Setup Development Environment:**
   - Ensure you have the following installed:
     - [.NET SDK (v6.0 or later)](https://dotnet.microsoft.com/download)
     - [BepInEx Plugin Templates](https://docs.bepinex.dev/master/articles/dev_guide/plugin_tutorial/1_setup.html)

3. **Add References:**
    - Copy the required DLLs from your game installation into a `libs` folder within the project directory:
      ```
      NITW-FontPatcher/libs/
      ├── Newtonsoft.Json.dll
      ├── Assembly-CSharp.dll
      └── UnityEngine.dll
      ```

4. **Build the Plugin:**
    ```sh
    dotnet build -c Release
    ```

    The compiled DLL can be found at:
    ```
    NITW-FontPatcher/bin/Release/net6.0/FontPatcher.dll
    ```

5. **Install the Plugin:**
   - Copy the compiled `FontPatcher.dll` into the following directory:
     ```
     <Game Directory>\BepInEx\plugins\FontPatcher\
     ```

6. **Run the Game:**
   - Launch Night in the Woods, and the custom characters should now be patched into the game's font system.

---

## Contributing

1. Fork this repository.
2. Create your feature branch (`git checkout -b feature/AmazingFeature`).
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4. Push to the branch (`git push origin feature/AmazingFeature`).
5. Open a pull request.
