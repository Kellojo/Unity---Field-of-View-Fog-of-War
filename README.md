# Unity---Field-of-View-Fog-of-War
This code snippet can be used to achieve a stealth game like field of view. Thanks to Sebastian Lague (https://github.com/SebLague) for the implementation (https://github.com/SebLague/Field-of-View) of the field of view mechanic.



## Showcase
![](https://i.imgur.com/7AqKA66.gif)
![Added Peripheral Vision](https://i.imgur.com/wkc6zrS.gif)

## Features
- Field of view visualization
- Edge revolsing
- Tweakable field of view (radius and width)
- Peripheral vision  (tweakable radius and width)

## How to use
1. Add the "Field of View" prefab to your scene.
2. Setup the "Target Mask" on the Field of View Component
3. Setup the "Obstacle Mask" on the Field of View Component
4. Add the "Hideable" component to the gameObjects that should be affected by the field of view


## How to implement your own behaviour
Any MonoBehaviour that should be affected by the field of view needs to implement the IHideable interface which gives you the option to implement the "OnFOVEnter" and "OnFOVLeave" methods yourself.
