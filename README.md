# Patchland
Piece for Zubin Kanga which uses a ROLI Seaboard expressive keyboard to control motion in a 3D environment created in Unity. The two are connected using a Pure Data patch and the intention for the final work is that the environment should be a performance score which is also visible to the audience and the sound will (cross fingers - still need to learn about this) use ambisonics.

As of August 2025, I am using LLMs (primarily Claude.ai, but also testing out ChatGPT) to help me develop code for this project, primarily in the new-to-me areas of external library development for Pure Data using C++ (which I learned a little of about 30 years ago, but it's all dribbled out my ears since then), script development for Unity using C# (which I've never used before) and to bridge gaps in my knowledge of Pure Data (which I kind of know, but use rarely so the LLM acts as a more context-specific Google to help me find the right objects and introduce me to new ones).

The project files consist of three principle elements:
1. externals/mpe-poly contains the polyphonic MPE Pure Data library files
2. pd contains Pure Data patches used in the piece
3. unity/Patchland contains scripts and critical files for the Unity 6.3 project - note that this contains no assets, etc. as these are too big to include in this free GitHub account so I have these backed up manually.

2026-10-31 Repo formerly titled "seaboard-vr"
