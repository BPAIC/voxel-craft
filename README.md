# Voxel Craft

This project is a small voxel-based game built with Godot **4.4+**.

## Running the project

1. Install [Godot 4.4 or later](https://godotengine.org/).
2. Open `project.godot` with the Godot editor.
3. Press **Play** to run.

## Coding guidelines

* All GDScript files use **tabs** for indentation. An `.editorconfig` is provided with `indent_style = tab` to help keep this consistent.
* Use `preload()` for resources that are reused to avoid loading them multiple times.
* Keep variable names descriptive; for example use `look_sensitivity` for camera sensitivity.

Following these conventions will help avoid parsing issues in Godot and keep the codebase consistent.
