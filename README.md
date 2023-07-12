# Welcome to Annihilis
Annihilis is a Quake-style shooter made in the Unity engine. The custom made movement system, though simple, is heavily refined to feel fast, fluid, and satisfying, almost as if the player were gliding and bouncing through reality. The game's sounds and explosion particles come from various free sources. The rest of the game, including its programming (C#) and models, are done by me.

This project's scripts can be found in the Assets/Script folder. If you would like to run the project for yourself, you can use version 2021.3.18 of the Unity Editor to open the project.

A quick overview of some of my accomplishments within this project can be read on my portfolio here: https://bichael.bike/projects/annihilis/scripting.html

You can also download a playable build of this project on my Itch.io page: https://onemanoccultband.itch.io/annihilis

# Incase you're unfamiliar with Unity projects...
Everything placed into a Unity scene is a GameObject. All GameObjects have a Transform and can have any number of Components attached it. A Component can be something that comes built into the engine, such as colliders and cameras, or a script written by the user in C#. For a script to work as a component for GameObjects, it must be derived from the MonoBehavior class.

The most important methods to know about are **Update**, **Start**, and **Awake**. 

**Update** is called every frame. The time between frames varies, so you will often see Time.deltaTime, which stores the seconds since the last frame, used with time dependent values. Scripts can be "Disabled" which keeps their Update method from being called.

**Start** is called before the script's first Update call. A script that is disabled won't call the Start method until it is enabled.

**Awake** is called when the script is loaded, irregardless of whether or not the script is enabled. It is always called before *any* Start method in *any* script is called, so it is useful for initialization before scripts need to talk to one another.
